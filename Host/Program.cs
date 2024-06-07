using Host;
using Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Orleans.Concurrency;
using Orleans.Runtime;

IHostBuilder builder = new HostBuilder()
    .UseOrleans(silo =>
    {
        silo.UseLocalhostClustering();
        silo.Services.AddFileStorage();
    });

builder.ConfigureAppConfiguration((context, config) =>
{
    config.AddJsonFile("path_to_config_file.json", optional: true);
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
