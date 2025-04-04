/// <summary>
/// Middleware for logging incoming requests and outgoing responses.
/// </summary>
public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;


    /// <summary>
    /// Constructs a new instance of the RequestResponseLoggingMiddleware class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger to use for logging.</param>
    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }


    /// <summary>
    /// Middleware method to log incoming requests and outgoing responses.
    /// </summary>
    /// <param name="context">The HTTP context of the request.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task Invoke(HttpContext context)
    {
        _logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path}");

        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
        _logger.LogInformation($"Response: {context.Response.StatusCode} {responseText}");
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        await responseBody.CopyToAsync(originalBodyStream);
    }
}
