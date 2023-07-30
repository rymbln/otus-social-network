
using OtusChatCounters.Database;

using OtusClasses;
using OtusClasses.Settings;

using Rebus.Config;

namespace OtusChatCounters;

public class Program
{
    public static void Main(string[] args)
    {
        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").AddEnvironmentVariables().Build();
        var builder = WebApplication.CreateBuilder(args);

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

        // Add services to the container.
        builder.Services.Configure<TarantoolSettings>(config.GetSection("TarantoolSettings"));
        builder.Services.AddSingleton<ITarantoolService, TarantoolService>();
        builder.Services.AddSagas(config);
        builder.Services.AutoRegisterHandlersFromAssemblyOf<Program>();

        var app = builder.Build();

        app.Run();
    }
}