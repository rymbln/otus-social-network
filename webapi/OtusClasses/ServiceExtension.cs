using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using OtusClasses.Sagas;
using OtusClasses.Sagas.Events;
using OtusClasses.Settings;

using Rebus.Config;
using Rebus.Routing.TypeBased;

namespace OtusClasses;

public static class ServiceRegistration
{

    public static void AddSagas(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRebus(rebus =>
                   rebus
                   .Routing(r => r.TypeBased().MapAssemblyOf<SendMessageSaga>(configuration["RabbitMqSettings:SagaQueue"]))
                   .Transport(t => t.UseRabbitMq(configuration["RabbitMqSettings:SagaUri"], configuration["RabbitMqSettings:SagaQueue"]))
                   .Sagas(s => s.StoreInPostgres(configuration["DatabaseSettings:ConnStr"], "sagas", "saga_indexes")),
                   onCreated: async bus =>
                   {
                       await bus.Subscribe<SaveMessageEvent>();
                   }
                   );
        services.AutoRegisterHandlersFromAssemblyOf<SendMessageSaga>();
    }


}