
using LazyPropertiesSample;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddTransient<IEchoService, EchoService>();
services.AddTransient<SampleService>();

using var serviceProvider = services.BuildServiceProvider();
var sampleService = serviceProvider.GetRequiredService<SampleService>();
Console.WriteLine(sampleService.Service.Hello("Hello"));
Console.WriteLine(sampleService.Service.Hello("Hello"));
