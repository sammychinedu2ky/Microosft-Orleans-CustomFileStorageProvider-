using Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IHost host = new HostBuilder()
     .UseOrleansClient(clientBuilder =>
     {
         clientBuilder.UseLocalhostClustering();
     })
     .Build();
await host.StartAsync();

IClusterClient client = host.Services.GetRequiredService<IClusterClient>();
var grain1 = client.GetGrain<IPersonGrain>("sam1");
var grain2 = client.GetGrain<IPersonGrain>("sam1");
Task.Run(async () =>
{
    try
    {
        
    await grain2.AddName(new string('a', 100000));
        Console.WriteLine("a done");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"from a");
        Console.WriteLine(ex.Message);
    }
});
Task.Run(async () =>
{
    try
    {

        await grain2.AddName(new string('b', 1000000));
        Console.WriteLine("b done");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"from b");
        Console.WriteLine(ex.Message);
    }
});
await Task.Delay(1000);
await grain2.AddName(new string('c', 1000000));
Console.WriteLine("c done");
var name1 = await grain1.GetName();
var name2 = await grain2.GetName();

Console.ReadLine();