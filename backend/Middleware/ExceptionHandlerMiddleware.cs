using System.Text;

namespace backend.Middleware
{
    //Middleware для обработки ошибок
    public sealed class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;
        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                context.Response.StatusCode = 500;
                var buffer = Encoding.UTF8.GetBytes(exception.ToString());
                await context.Response.Body.WriteAsync(buffer, 0, buffer.Length);
                _logger.LogError(0, exception, "Exception");
            }
        }
    }
}
