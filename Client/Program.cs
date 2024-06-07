using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Interfaces;
using IHost host = new HostBuilder()
     .UseOrleansClient(clientBuilder =>
     {
         clientBuilder.UseLocalhostClustering();
     })
     .Build();
await host.StartAsync();

IClusterClient client = host.Services.GetRequiredService<IClusterClient>();
var grain1 = client.GetGrain<IPersonGrain>("sam1");
var grain2 = client.GetGrain<IPersonGrain>("sam2");
await grain1.AddName("bondi");
await grain2.AddName("bondi2");
var name1 = await grain1.GetName();
var name2 = await grain2.GetName();
Console.WriteLine(name1);
Console.WriteLine(name2);
Console.ReadLine();