using System.Net;
using InventoryHub.Exceptions;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (ProductNotFoundException ex)
        {
            await HandleProductNotFoundExceptionAsync(httpContext, ex);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private Task HandleProductNotFoundExceptionAsync(HttpContext context, ProductNotFoundException exception)
    {
        //Todo: Добавить TraceId
        _logger.LogWarning($"Product not found: {exception.ProductId}");

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.NotFound;

        var result = new
        {
            Error = "Product not found",
            ProductId = exception.ProductId
        };

        return context.Response.WriteAsJsonAsync(result);
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        //Todo: Добавить TraceId
        var errorId = Guid.NewGuid();
        _logger.LogError(exception,
            "An unexpected error occurred. Request: {Request}, ErrorId: {ErrorId}", 
            FormatRequest(context), errorId);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var error = new
        {
            ErrorId = errorId,
            ErrorMessage = "An unexpected error occurred"
        };

        return context.Response.WriteAsJsonAsync(error);
    }

    private static string FormatRequest(HttpContext context)
    {
        var request = context.Request;
        return $"{request.Method} {request.Path}{request.QueryString} from {context.Connection.RemoteIpAddress}";
    }
}