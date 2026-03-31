using System.Net;
using System.Text.Json;

namespace TaskManagerAPI.Middleware

{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleException(context, ex);                
            }
        }

        private static Task HandleException(HttpContext context, Exception ex)
        {
            var response = context.Response;

            response.ContentType = "application/json";
            response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var result = JsonSerializer.Serialize(new
            {
                message = ex.Message
            });

            return response.WriteAsync(result);
        }
    }
}
