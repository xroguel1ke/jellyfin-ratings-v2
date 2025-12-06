using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using System;

namespace Jellyfin.Plugin.Ratings
{
    public class RatingsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RatingsMiddleware> _logger;

        public RatingsMiddleware(RequestDelegate next, ILogger<RatingsMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";

            // Prüfen, ob es sich um eine Seite handelt, auf der wir das Skript brauchen
            bool isPageRequest = path == "/" || 
                                 path.StartsWith("/web/index.html") || 
                                 (path.StartsWith("/web/") && !path.Contains(".")); 

            if (path.EndsWith(".js") || path.EndsWith(".css") || path.EndsWith(".png") || path.EndsWith(".woff2"))
            {
                await _next(context);
                return;
            }

            if (!isPageRequest)
            {
                await _next(context);
                return;
            }

            // Kompression verhindern, damit wir den Body lesen können
            context.Request.Headers.Remove("Accept-Encoding");

            var originalBodyStream = context.Response.Body;

            using (var responseBody = new MemoryStream())
            {
                context.Response.Body = responseBody;

                await _next(context);

                if (context.Response.ContentType != null && 
                    context.Response.ContentType.ToLower().Contains("text/html"))
                {
                    context.Response.Headers.Remove("If-Modified-Since");
                    context.Response.Headers.Remove("ETag");
                    
                    responseBody.Seek(0, SeekOrigin.Begin);
                    var responseText = await new StreamReader(responseBody).ReadToEndAsync();

                    // FIX: Wir nutzen den LOKALEN Pfad, nicht CDN.
                    // Nur so läuft die Anfrage durch den RatingsController, der den API Key einsetzt.
                    var scriptTag = "<script src=\"/Plugins/Jellyfin.Plugin.Ratings/ratings.js\" defer></script>";

                    if (responseText.Contains("</body>") && !responseText.Contains("ratings.js"))
                    {
                        responseText = responseText.Replace("</body>", scriptTag + "</body>");
                        context.Response.Headers.Remove("Content-Length");
                        context.Response.Headers.Remove("Content-Encoding"); 
                    }

                    var modifiedBytes = Encoding.UTF8.GetBytes(responseText);
                    await originalBodyStream.WriteAsync(modifiedBytes, 0, modifiedBytes.Length);
                }
                else
                {
                    responseBody.Seek(0, SeekOrigin.Begin);
                    await responseBody.CopyToAsync(originalBodyStream);
                }
            }
        }
    }
}
