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
        public override Guid Id => Guid.Parse("A8972C41-7A69-4F5B-B3C8-E0D9F7C4B1A6"); // NEUE UNBLOCKIERTE GUID

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
