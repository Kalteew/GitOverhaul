using System.Net;
using System.Text.Json;

namespace GitOverhaul.Api.Middleware;

public class ErrorMiddleware(RequestDelegate next, ILogger<ErrorMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try {
            await next(context);
        }
        catch (Exception ex) {
            logger.LogError(ex, "🔥 Une erreur non gérée a été interceptée");

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var errorResponse = new
            {
                status = 500,
                message = "Une erreur est survenue sur le serveur.",
                details = ex.Message,
            };

            var json = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(json);
        }
    }
}