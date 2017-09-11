using Microsoft.Extensions.Configuration;

namespace Como.Cluster.FrontEnd
{
    public static class ServiceFabricConfigExtensions
    {
        public static IConfigurationBuilder AddServiceFabricConfig(this IConfigurationBuilder builder, string packageName)
        {
            return builder.Add(new ServiceFabricConfigSource(packageName));
        }
    }
}
