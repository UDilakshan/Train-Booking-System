using System.Text.Json;
using RailwayReservation.Application.Common.Exceptions;

namespace RailwayReservation.Api.Common.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AppException ex)
        {
            context.Response.StatusCode = ex.StatusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(new ApiErrorResponse(false, new ApiErrorBody(ex.Code, ex.Message, ex.Details)), JsonOptions));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(new ApiErrorResponse(false, new ApiErrorBody("INTERNAL_ERROR", "An unexpected error occurred.")), JsonOptions));
        }
    }
}
