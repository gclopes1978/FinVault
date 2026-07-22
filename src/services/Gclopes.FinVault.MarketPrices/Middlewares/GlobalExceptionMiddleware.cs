namespace Gclopes.FinVault.MarketPrices.Middlewares;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception caught by GlobalExceptionMiddleware");

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var error = new
            {
                message = "An unexpected error occurred.",
                detail = ex.Message
            };

            await context.Response.WriteAsJsonAsync(error);
        }
    }
}
