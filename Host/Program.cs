using Host;
using Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Orleans.Runtime;
using Orleans.Storage;
using System.Text.Json;

IHostBuilder builder = new HostBuilder()
    .UseOrleans(silo =>
    {
        silo.UseLocalhostClustering();
        silo.Services.AddFileStorage();
    });

builder.ConfigureAppConfiguration((context, config) =>
{
    config.AddJsonFile("C:\\Users\\Swacblooms\\source\\repos\\Microosft-Orleans(CustomFileStorageProvider)\\Host\\appsettings.json", optional: true);
});
using IHost host = builder.Build();

await host.RunAsync();

public class PersonGrain : IGrainBase, IPersonGrain
{
    IPersistentState<PersonState> state;
    public IGrainContext GrainContext { get; }
    public PersonGrain(IGrainContext context, [PersistentState("personState", "fileStateStore")] IPersistentState<PersonState> state) => (GrainContext, this.state) = (context, state);
    public async Task AddName(string name)
    {
        var context = this.GrainContext;
        state.State.Name = name;
        await state.WriteStateAsync();
    }
    public Task<string> GetName()
    {
        return Task.FromResult($"My name is {state.State.Name}");
    }
}


public class PersonState
{
    public string? Name { get; set; }
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
