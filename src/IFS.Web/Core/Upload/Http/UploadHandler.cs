namespace IFS.Web.Core.Upload.Http {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    using Humanizer;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Primitives;
    using Microsoft.Net.Http.Headers;

    using Models;

    public sealed class UploadHandler {
        private readonly IUploadManager _uploadManager;
        private readonly IUploadProgressManager _uploadProgressManager;
        private readonly IOptions<FileStoreOptions> _fileStoreOptions;
        private readonly LinkGenerator _linkGenerator;

        private readonly ILogger<UploadHandler> _logger;

        public UploadHandler(IUploadManager uploadManager, IUploadProgressManager uploadProgressManager, IOptions<FileStoreOptions> fileStoreOptions, ILogger<UploadHandler> logger, LinkGenerator linkGenerator) {
            this._uploadManager = uploadManager;
            this._fileStoreOptions = fileStoreOptions;
            this._logger = logger;
            this._linkGenerator = linkGenerator;
            this._uploadProgressManager = uploadProgressManager;
        }

        public async Task ExecuteAsync(HttpContext context) {
            FileIdentifier identifier = FileIdentifier.FromString(context.GetRouteValue("fileIdentifier")?.ToString() ?? throw new InvalidOperationException("No ID"));

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
            // ... after the request "completes" we re-execute to send the final response to the browser
            try {
                await using (context.RequestAborted.Register(context.Abort)) {
                    await this._uploadManager.StoreAsync(identifier, reader, context.RequestAborted);
                }

                PrepForReExecute(context, new UploadErrorsModel());
            }
            catch (UploadCryptoArgumentOrderException) {
                PrepForReExecute(context, UploadErrorsModel.CreateFromMessage("Invalid order of cryptographic parameters: file was uploaded before password."));
            } 
            catch (Exception ex) {
                UploadErrorsModel errors = UploadErrorsModel.CreateFromMessage(ex.Message);

                this._logger.LogError(LogEvents.UploadFailed, "Detected failed upload - passing error to child handler: {0}", ex);

                PrepForReExecute(context, errors);
            }

            await ReExecuteAsync(context);
        }

        private static async Task ReExecuteAsync(HttpContext context)
        {
            RequestDelegate? reExecutionPoint = ReExecuteMiddleware.GetReExecutionPoint(context);

            if (reExecutionPoint == null)
            {
                throw new InvalidOperationException($"No ReExecution point defined. Please use the {nameof(ApplicationBuilderExtensions.UseReExecution)} method to register middleware before the 'UseRouting' call");
            }

            await reExecutionPoint.Invoke(context);
        }

        private void PrepForReExecute(HttpContext context, UploadErrorsModel uploadErrorsModel) {
            // Clear request
            context.Request.Method = "POST";
            context.Request.Body = Stream.Null;

            if (uploadErrorsModel.Errors?.Length > 0) {
                Dictionary<string, StringValues> query = new Dictionary<string, StringValues> {
                    [nameof(uploadErrorsModel.Errors)] = uploadErrorsModel.Errors
                };
                context.Request.QueryString = QueryString.Create(query);
            } else {
                context.Request.QueryString = new QueryString();
            }

            context.Request.ContentType = "text/plain";

            // Set route data and path
            var routeData = new {
                id = context.GetRouteValue("fileIdentifier")
            };
            
            context.Request.PathBase = "/";
            context.Request.Path = this._linkGenerator.GetPathByAction("Frame", "Upload", routeData);
            context.SetEndpoint(null);
        }

        private static string GetBoundary(MediaTypeHeaderValue contentType) {
            string boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;

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