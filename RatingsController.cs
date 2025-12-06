using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.Ratings
{
    [Route("Plugins/Jellyfin.Plugin.Ratings")]
    public class RatingsController : Controller
    {
        [HttpGet("ratings.js")]
        [Produces("application/javascript")]
        public ActionResult GetScript()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Jellyfin.Plugin.Ratings.ratings.js";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null) return NotFound();

                using (var reader = new StreamReader(stream))
                {
                    var scriptContent = reader.ReadToEnd();

                    // --- HIER PASSIERT DIE MAGIE ---
                    
                    // 1. Wir holen den Key aus der gespeicherten Config
                    var config = Plugin.Instance?.Configuration;
                    var userApiKey = config?.MdblistApiKey;

                    // 2. Fallback, falls der User noch nichts eingetragen hat
                    if (string.IsNullOrWhiteSpace(userApiKey))
                    {
                        userApiKey = ""; // Leer lassen oder einen Demo-Key nutzen
                    }

                    // 3. Wir suchen den Platzhalter im JS und ersetzen ihn
                    // Der Platzhalter muss EXAKT so heißen wie in Schritt 1 definiert
                    scriptContent = scriptContent.Replace("__MDBLIST_API_KEY_PLACEHOLDER__", userApiKey);

                    return Content(scriptContent, "application/javascript");
                }
            }
        }
    }
}
