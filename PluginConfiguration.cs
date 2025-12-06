using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.Ratings
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        // Leerer Standardwert, damit das Feld im Dashboard sauber ist
        public string MdblistApiKey { get; set; } = ""; 

        public PluginConfiguration()
        {
            // Standardwerte
        }
    }
}
