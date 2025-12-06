using System;
using System.Collections.Generic;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.Ratings
{
    // We inherit from BasePlugin, which is standard
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        public static Plugin Instance { get; private set; }

        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
        }

        public override string Name => "Jellyfin Ratings";
        public override Guid Id => Guid.Parse("93A5D7C8-2F1E-4B0A-9C3D-5E7F1A2B4C6D"); // A unique ID

        // FIX: Explicitly override the version property to ensure it displays correctly in the dashboard
        // Otherwise, it might fallback to 0.0.0.0 in some environments
        public override Version Version => new Version(1, 0, 3);

        // (Optional) This is required for the HTML settings page in the dashboard
        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "ratings", // This defines the URL slug
                    EmbeddedResourcePath = GetType().Namespace + ".Configuration.configPage.html"
                }
            };
        }
    }
}
