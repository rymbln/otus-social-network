using System.Net;
using System.Text;

namespace OtusSocialNetwork.Middlewares;

public class DefaultPageMiddleware
{
    private readonly RequestDelegate _next;
    public DefaultPageMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, IConfiguration config)
    {
        var serviceName = config["ServiceName"] ?? string.Empty;
        if (context.Request.Path == "/" || context.Request.Path == "/index.html")
        {
            await ReturnIndexPage(context, serviceName);
            return;
        }
       
        await _next.Invoke(context);
    }

    private static async Task ReturnIndexPage(HttpContext context, string serviceName)
    {
        context.Response.StatusCode = (int)HttpStatusCode.OK;
        context.Response.ContentType = "text/html";

        byte[] buffer = Encoding.UTF8.GetBytes(
            $"<!DOCTYPE html><html><head><meta charset='utf-8' /><title>{serviceName}</title></head><body><h1>{serviceName}</h1></body></html>");

        context.Response.ContentLength = buffer.Length;

        using (var stream = context.Response.Body)
        {
            await stream.WriteAsync(buffer, 0, buffer.Length);
            await stream.FlushAsync();
        }
    }
}

public static class DefaultPageExtensions
{
    public static IApplicationBuilder UseDefaultPage(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<DefaultPageMiddleware>();
    }
}

