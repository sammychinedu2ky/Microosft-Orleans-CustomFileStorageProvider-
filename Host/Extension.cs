using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Storage;

namespace Host
{
    public class FileConfigurationOptions
    {
        public string? StorePath { get; set; }
        public const string MyFileConfiguration = "FileConfiguration";
    }

    public static class FileStorageExtensions
    {
        public static IServiceCollection AddFileStorage(this IServiceCollection services, Action<FileConfigurationOptions> configureOptions)
        {
            services.AddOptions<FileConfigurationOptions>()
                       .Configure(configureOptions);
            return services.AddKeyedSingleton<IGrainStorage, FileStorage>("fileStateStore");

        }

        public static IServiceCollection AddFileStorage(this IServiceCollection services)
        {
            services.AddOptions<FileConfigurationOptions>()
               .Configure<IConfiguration>((settings, configuration) =>
               {
                   configuration.GetSection(FileConfigurationOptions.MyFileConfiguration).Bind(settings);
               });
            return services.AddKeyedSingleton<IGrainStorage, FileStorage>("fileStateStore");
        }

    }
}
