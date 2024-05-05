using DL91;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DL91Web8.Helpers
{
    internal sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogTool _logTool;

        public GlobalExceptionHandler(ILogTool logTool)
        {
            _logTool = logTool;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {

            var msg = "GlobalException:\r\nUrl:" + httpContext.Request.Path.Value + "\r\n";
            _logTool.Error(msg, exception);

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Server error"
            };

            httpContext.Response.StatusCode = problemDetails.Status.Value;

            await httpContext.Response
                .WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
