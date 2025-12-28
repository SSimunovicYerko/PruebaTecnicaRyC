using Microsoft.AspNetCore.Mvc;

namespace ClientesAPI.Middlewares;

public sealed class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var traceId = context.TraceIdentifier;

            _logger.LogError(ex,
                "Unhandled exception. TraceId={TraceId} Method={Method} Path={Path}",
                traceId, context.Request.Method, context.Request.Path);

            var problem = new ProblemDetails
            {
                Title = "Error inesperado",
                Status = StatusCodes.Status500InternalServerError,
                Detail = _env.IsDevelopment() ? ex.ToString() : "Ocurrió un error inesperado.",
                Instance = context.Request.Path
            };

            problem.Extensions["traceId"] = traceId;

            context.Response.StatusCode = problem.Status.Value;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
