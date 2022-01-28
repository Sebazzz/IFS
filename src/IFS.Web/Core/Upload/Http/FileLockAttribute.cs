// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : FileLockAttribute.cs
//  Project         : IFS.Web
// ******************************************************************************

using System;
using System.Linq;
using System.Threading;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

using IFS.Web.Models;

namespace IFS.Web.Core.Upload.Http {
    /// <summary>
    /// Acquires a file lock for the duration of the action method
    /// </summary>
    public sealed class FileLockAttribute : ActionFilterAttribute, IExceptionFilter {
        private static readonly string FileLockItemKey = nameof(FileLockAttribute) + Guid.NewGuid();

        public override void OnActionExecuted(ActionExecutedContext context) {
            DisposeLock(context.HttpContext);
        }

        public override void OnActionExecuting(ActionExecutingContext context) {
            FileIdentifier fileIdentifier = context.ActionArguments.Values.OfType<FileIdentifier>().Single();
            CancellationToken cancellationToken = context.HttpContext.RequestAborted;

            IUploadFileLock locker = context.HttpContext.RequestServices.GetRequiredService<IUploadFileLock>();

            context.HttpContext.Items[FileLockItemKey] = locker.Acquire(fileIdentifier, cancellationToken);
        }

        public void OnException(ExceptionContext context) {
            DisposeLock(context.HttpContext);
        }

        private static void DisposeLock(HttpContext httpContext) {
            if (httpContext.Items[FileLockItemKey] is IDisposable @lock) {
                @lock.Dispose();

                httpContext.Items.Remove(FileLockItemKey);
            }
        }
    }
}
