namespace IFS.Web.Core.Upload.Http {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Humanizer;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Primitives;
    using Microsoft.Net.Http.Headers;

    using Models;

    public sealed class UploadHandler {
        private readonly IUploadManager _uploadManager;
        private readonly IUploadProgressManager _uploadProgressManager;
        private readonly IOptions<FileStoreOptions> _fileStoreOptions;

        private readonly ILogger<UploadHandler> _logger;

        public UploadHandler(IUploadManager uploadManager, IUploadProgressManager uploadProgressManager, IOptions<FileStoreOptions> fileStoreOptions, ILogger<UploadHandler> logger) {
            this._uploadManager = uploadManager;
            this._fileStoreOptions = fileStoreOptions;
            this._logger = logger;
            this._uploadProgressManager = uploadProgressManager;
        }

        public async Task ExecuteAsync(HttpContext context) {
            FileIdentifier identifier = FileIdentifier.FromString(context.GetRouteValue("fileidentifier").ToString());

            this._logger.LogInformation(LogEvents.NewUpload, "New upload of file with id {0}", identifier);

            // We have already the ID, so we can set some progress
            UploadProgress progress = new UploadProgress {
                Current = 0,
                StartTime = DateTime.UtcNow,
                Total = context.Request.ContentLength ?? -1
            };
            this._uploadProgressManager.SetProgress(identifier, progress);

            // Initialize reading request
            MediaTypeHeaderValue contentType = GetContentType(context);
            string boundary = GetBoundary(contentType);

            MultipartReader reader = new MultipartReader(boundary, context.Request.Body);
            reader.BodyLengthLimit = (long?) this._fileStoreOptions.Value?.MaximumFileSize.Megabytes().Bytes;

            // Delegate actual request parsing
            try {
                using (context.RequestAborted.Register(context.Abort)) {
                    await this._uploadManager.StoreAsync(identifier, reader, context.RequestAborted);
                }

                PrepForReExecute(context, new UploadErrorsModel());
            } catch (Exception ex) {
                UploadErrorsModel errors = new UploadErrorsModel {
                    Errors = new[] {
                        ex.Message
                    }
                };

                this._logger.LogError(LogEvents.UploadFailed, "Detected failed upload - passing error to child handler: {0}", ex);

                PrepForReExecute(context, errors);
            }

            await ReExecuteAsync(context);
        }

        private static async Task ReExecuteAsync(HttpContext context) {
            IRouter router = context.GetRouteData().Routers[0];

            RouterMiddleware routerMiddleware = new RouterMiddleware(_ => Task.CompletedTask, context.RequestServices.GetRequiredService<ILoggerFactory>(), router);

            await routerMiddleware.Invoke(context);
        }

        private static void PrepForReExecute(HttpContext context, UploadErrorsModel uploadErrorsModel) {
            // Clear request
            context.Request.Method = "POST";
            context.Request.Body = Stream.Null;

            var query = new Dictionary<string, StringValues> {
                [nameof(uploadErrorsModel.Errors)] = uploadErrorsModel.Errors
            };
            context.Request.QueryString = QueryString.Create(query);
            context.Request.ContentType = "text/plain";

            // Set route data
            var routeData = new {
                controller = "Upload",
                action = "Frame",
                id = context.GetRouteValue("fileidentifier")
            };

            foreach (IRouter router in context.GetRouteData().Routers.Reverse()) {
                VirtualPathContext virtualPathContext = new VirtualPathContext(context, new RouteValueDictionary(), new RouteValueDictionary(routeData));
                VirtualPathData data = router.GetVirtualPath(virtualPathContext);

                if (data != null) {
                    context.Request.Path = new PathString(data.VirtualPath);
                   
                    return;
                }
            }
        }

        private static string GetBoundary(MediaTypeHeaderValue contentType) {
            string boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary);

            if (string.IsNullOrWhiteSpace(boundary)) {
                throw new InvalidOperationException("Missing content-type boundary.");
            }

            return boundary;
        }

        private static MediaTypeHeaderValue GetContentType(HttpContext context) {
            return MediaTypeHeaderValue.Parse(context.Request.ContentType);
        }
    }
}