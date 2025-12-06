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

            // Prüfen, ob es sich um die Hauptseite handelt
            // Wir prüfen auf "/" oder "/web/" oder "/web/index.html"
            bool isPageRequest = path == "/" || 
                                 path.StartsWith("/web/index.html") || 
                                 (path.StartsWith("/web/") && !path.Contains(".")); 

            // Schnell-Ausstieg bei statischen Assets (Bilder, JS, CSS)
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

            // WICHTIG: Wir löschen diesen Header, damit der Server uns KLARTEXT schickt.
            context.Request.Headers.Remove("Accept-Encoding");

            var originalBodyStream = context.Response.Body;

            using (var responseBody = new MemoryStream())
            {
                context.Response.Body = responseBody;

                await _next(context);

                // --- HIER WAR VORHER DER ABBRUCH BEI KOMPRESSION ---
                // Wir prüfen jetzt nur noch grob und versuchen es trotzdem, falls möglich.
                
                // Content-Type Check (nur HTML anfassen)
                if (context.Response.ContentType != null && 
                    context.Response.ContentType.ToLower().Contains("text/html"))
                {
                    // Cache-Header killen, damit der Browser beim nächsten Mal sicher neu lädt
                    context.Response.Headers.Remove("If-Modified-Since");
                    context.Response.Headers.Remove("ETag");
                    
                    responseBody.Seek(0, SeekOrigin.Begin);
                    
                    // Wir lesen den Stream. Wenn er komprimiert ist, sehen wir hier nur Hieroglyphen.
                    // Aber da wir oben "Accept-Encoding" entfernt haben, SOLLTE es Text sein.
                    var responseText = await new StreamReader(responseBody).ReadToEndAsync();

                    // Einfügen des Scripts
                    var scriptTag = "<script src=\"/Plugins/Jellyfin.Plugin.Ratings/ratings.js\" defer></script>";

                    if (responseText.Contains("</body>") && !responseText.Contains("ratings.js"))
                    {
                        _logger.LogInformation("RatingsPlugin: Injection erfolgreich für {0}", path);
                        responseText = responseText.Replace("</body>", scriptTag + "</body>");
                        
                        // Header anpassen
                        context.Response.Headers.Remove("Content-Length");
                        // Falls der Server trotz Verbot komprimiert hat, müssen wir den Header entfernen,
                        // weil wir den Inhalt ja dekomprimiert (gelesen) und verändert haben.
                        context.Response.Headers.Remove("Content-Encoding"); 
                    }

                    var modifiedBytes = Encoding.UTF8.GetBytes(responseText);
                    await originalBodyStream.WriteAsync(modifiedBytes, 0, modifiedBytes.Length);
                }
                else
                {
                    // Kein HTML -> Original zurück
                    responseBody.Seek(0, SeekOrigin.Begin);
                    await responseBody.CopyToAsync(originalBodyStream);
                }
            }
        }
    }
}
