// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : ReExecuteMiddleware.cs
//  Project         : IFS.Web
// ******************************************************************************

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace IFS.Web.Core.Upload.Http
{
    public class ReExecuteMiddleware
    {
        private const string ReExecutionPoint = "__" + nameof(ReExecutionPoint);
        private readonly RequestDelegate _next;

        public ReExecuteMiddleware(RequestDelegate next)
        {
            this._next = next;
        }

        public Task InvokeAsync(HttpContext httpContext)
        {
            httpContext.Items[ReExecutionPoint] = this._next;
            return this._next.Invoke(httpContext);
        }

        internal static RequestDelegate? GetReExecutionPoint(HttpContext httpContext)
        {
            return httpContext.Items[ReExecutionPoint] as RequestDelegate;
        }
    }
}