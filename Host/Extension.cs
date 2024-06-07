using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Runtime;
using Orleans.Storage;
using System.Text.Json;

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

    public class FileStorage : IGrainStorage
    {
        private void _ensureFileExists(string filePath)
        {
            if (!File.Exists(filePath))
            {
                var directory = Path.GetDirectoryName(filePath);
                Directory.CreateDirectory(directory!);
                File.Create(filePath).Close();
            }
        }

        private string _getFilePath(GrainId grainId, string stateName)
        {
            return Path.Combine(_options.Value.StorePath!, grainId.Type.ToString()!, grainId.Key.ToString()!, stateName + ".json");
        }
        private readonly IOptions<FileConfigurationOptions> _options;
        public FileStorage(IOptions<FileConfigurationOptions> options) => _options = options;
        public Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
        {
            var path = _getFilePath(grainId, stateName);
            File.Delete(path);
            return Task.CompletedTask;
        }

        public async Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
        {
            var path = _getFilePath(grainId, stateName);
            _ensureFileExists(path);
            var data = await File.ReadAllTextAsync(path);
            if (string.IsNullOrEmpty(data)) return;
            grainState.State = JsonSerializer.Deserialize<T>(data)!;
        }

        public async Task WriteStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
        {
            var path = _getFilePath(grainId, stateName);
            _ensureFileExists(path);
            var data = JsonSerializer.Serialize(grainState.State);
            await File.WriteAllTextAsync(path, data);
        }
    }

}
