using OtusClasses.Sagas;
using OtusClasses.Sagas.Events;
using OtusClasses;
using OtusClasses.Settings;

using OtusDialogsGrpc.Database;
using OtusDialogsGrpc.Database.Interfaces;
using OtusDialogsGrpc.Services;

using Rebus.Config;
using Rebus.Routing.TypeBased;

namespace OtusDialogsGrpc;

public class Program
{
    public static void Main(string[] args)
    {
        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").AddEnvironmentVariables().Build();
        var builder = WebApplication.CreateBuilder(args);
        var rabbitMqSettings = builder.Configuration.GetSection(nameof(RabbitMqSettings)).Get<RabbitMqSettings>();
        var databaseSettings = builder.Configuration.GetSection(nameof(DatabaseSettings)).Get<DatabaseSettings>();


        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

        builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        builder.Services.Configure<TarantoolSettings>(config.GetSection("TarantoolSettings"));
        builder.Services.AddSingleton<ITarantoolService, TarantoolService>();
        builder.Services.AddSagas(config);
        builder.Services.AutoRegisterHandlersFromAssemblyOf<Program>();
        //builder.Services.AddRebus(rebus =>
        //        rebus
        //        .Routing(r => r.TypeBased().MapAssemblyOf<Program>(rabbitMqSettings.SagaQueue))
        //        .Transport(t => t.UseRabbitMq(rabbitMqSettings.SagaUri, rabbitMqSettings.SagaQueue))
        //        .Sagas(s => s.StoreInPostgres(databaseSettings.ConnStr, "sagas", "saga_indexes"))
        //         //onCreated: async bus =>
        //         //{
        //         //    await bus.Subscribe<SaveMessageEvent>();
        //         //}
        //        );
        //builder.Services.AutoRegisterHandlersFromAssemblyOf<Program>();

        // Add services to the container.
        //builder.Services.AddGrpc();
        builder.Services.AddGrpc();
    
        var app = builder.Build();


        // Configure the HTTP request pipeline.
        app.MapGrpcService<DialogsService>();
        app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

        app.Run("http://+:5147");

    }
}

   