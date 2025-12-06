using MediaBrowser.Controller; 
using MediaBrowser.Controller.Plugins;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.Ratings
{
    public class PluginServiceRegistrator : IPluginServiceRegistrator
    {
        public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
        {
            // Änderung auf AddTransient (besser für Filter)
            serviceCollection.AddTransient<IStartupFilter, RatingsStartupFilter>();
        }
    }
}
