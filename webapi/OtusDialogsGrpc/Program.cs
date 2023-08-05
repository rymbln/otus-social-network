using OtusClasses.Settings;

using OtusDialogsGrpc.Database;
using OtusDialogsGrpc.Database.Interfaces;
using OtusDialogsGrpc.Services;
using Prometheus;

namespace OtusDialogsGrpc;

public class Program
{
    public static void Main(string[] args)
    {
        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").AddEnvironmentVariables().Build();
        var builder = WebApplication.CreateBuilder(args);

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

        builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        builder.Services.Configure<TarantoolSettings>(config.GetSection("TarantoolSettings"));
        builder.Services.AddSingleton<ITarantoolDialogsService, TarantoolDialogsService>();

        // Add services to the container.
        //builder.Services.AddGrpc();
        builder.Services.AddGrpc();
    
        var app = builder.Build();


        // Configure the HTTP request pipeline.
        app.MapGrpcService<DialogsService>();
        app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
        app.UseRouting();
        app.UseGrpcMetrics();
        app.Run("http://+:5147");

    }
}

   