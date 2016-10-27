namespace IFS.Web.Core.Upload.Http {
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;

    public sealed class SharedUploadRouteHandler {
        public Task ExecuteAsync(HttpContext context) {
            UploadHandler routeHandler = context.RequestServices.GetRequiredService<UploadHandler>();

            return routeHandler.ExecuteAsync(context);
        }
    }
}