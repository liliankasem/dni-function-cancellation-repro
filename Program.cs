using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;

Console.WriteLine($"Azure Functions .NET Worker (PID: { Environment.ProcessId }).");

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Build().Run();
