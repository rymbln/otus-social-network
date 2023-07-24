using MassTransit;
using MassTransit.RabbitMqTransport.Configuration;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using OtusSocialNetwork.Consumers;
using OtusSocialNetwork.Database;
using OtusSocialNetwork.DataClasses.Dtos;
using OtusSocialNetwork.DataClasses.Internals;
using OtusSocialNetwork.DataClasses.Notifications;
using OtusSocialNetwork.Filters;
using OtusSocialNetwork.Middlewares;
using OtusSocialNetwork.Rabbitmq;
using OtusSocialNetwork.Services;
using OtusSocialNetwork.SignalHub;
using OtusSocialNetwork.Tarantool;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").AddEnvironmentVariables().Build();
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<DatabaseSettings>(config.GetSection("DatabaseSettings"));
builder.Services.Configure<TarantoolSettings>(config.GetSection("TarantoolSettings"));
builder.Services.AddScoped<IDatabaseContext, DatabaseContext>();
builder.Services.AddSingleton<ITarantoolService, TarantoolService>();
builder.Services.AddSingleton<IPasswordService, PasswordService>();
builder.Services.AddScoped<IAuthenticatedUserService, AuthenticatedUserService>();
// TODO: Delete
builder.Services.AddSingleton<TimerManager>();
builder.Services.AddHttpContextAccessor();
//builder.Services.AddControllers(options =>
//{
//    options.Filters.Add(typeof(ValidateModelStateAttribute));
//});
//    .AddJsonOptions(options =>
//{
//    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
//    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
//});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder => builder
        .WithOrigins("http://localhost:4200")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});

builder.Services.AddSignalR();

builder.Services.AddControllers();

builder.Services.Configure<JWTSettings>(config.GetSection("JWTSettings"));
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(o =>
    {
        o.RequireHttpsMetadata = false;
        o.SaveToken = true;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = config["JWTSettings:Issuer"],
            ValidAudience = config["JWTSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWTSettings:Key"])),
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.Zero,
        };
        o.Events = new JwtBearerEvents()
        {
            OnAuthenticationFailed = c =>
            {
                c.NoResult();
                c.Response.StatusCode = 401;
                c.Response.ContentType = "text/plain";
                return c.Response.WriteAsync(c.Exception.ToString());
            },
            // OnChallenge = context =>
            // {
            //     context.HandleResponse();
            //     // context.Response.StatusCode = 401;
            //     // context.Response.ContentType = "application/json";
            //     var result = JsonConvert.SerializeObject(new Response<string>("You are not Authorized"));
            //     return context.Response.WriteAsync(result);
            // },
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                // If the request is for our hub...
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/ws/feed/news") || path.StartsWithSegments("/ws/feed/chat")))
                {
                    // Read the token out of the query string
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            },
            OnForbidden = context =>
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";
                var result = JsonSerializer.Serialize("You are not authorized to access this resource");
                return context.Response.WriteAsync(result);
            },
        };
    });

//builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Otus.Social.Network",
        Description = "",
        Contact = new OpenApiContact
        {
            Name = "Ivan Trushin",
            Email = "ivan.v.trushin@yandex.ru",
        }
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "Input your Bearer token in this format - Bearer {your token here} to access this API",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer",
                            },
                            Scheme = "Bearer",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        }, new List<string>()
                    },
                });
});

var rabbitMqSettings = builder.Configuration.GetSection(nameof(RabbitMqSettings)).Get<RabbitMqSettings>();
builder.Services.AddMassTransit(mt =>
{
    //mt.SetKebabCaseEndpointNameFormatter();
    mt.AddConsumers(Assembly.GetExecutingAssembly());

    mt.UsingRabbitMq((context, busFactoryConfigurator) =>
    {
        busFactoryConfigurator.Host(rabbitMqSettings.Uri);
        //busFactoryConfigurator.ConfigureEndpoints(context);

        busFactoryConfigurator.ReceiveEndpoint(nameof(NotificationFeedAddConsumer), e =>
        {
            e.ConfigureConsumeTopology = false;
            e.ConfigureConsumer(context, typeof(NotificationFeedAddConsumer));
            e.Bind<INotificationFeedAdd>();
        });
        busFactoryConfigurator.ReceiveEndpoint(nameof(NotificationFeedUpdateConsumer), e =>
        {
            e.ConfigureConsumeTopology = false;
            e.ConfigureConsumer(context, typeof(NotificationFeedUpdateConsumer));
            e.Bind<INotificationFeedUpdate>();
        });
        busFactoryConfigurator.ReceiveEndpoint(nameof(NotificationFeedReloadConsumer), e =>
        {
            e.ConfigureConsumeTopology = false;
            e.ConfigureConsumer(context, typeof(NotificationFeedReloadConsumer));
            e.Bind<INotificationFeedReload>();
        });
        busFactoryConfigurator.ReceiveEndpoint(nameof(NotificationFeedDeleteConsumer), e =>
        {
            e.ConfigureConsumeTopology = false;
            e.ConfigureConsumer(context, typeof(NotificationFeedDeleteConsumer));
            e.Bind<INotificationFeedDelete>();
        });
        busFactoryConfigurator.ReceiveEndpoint($"{nameof(PushFeedUpdateConsumer)}_{rabbitMqSettings.Consumer}", e =>
        {
            e.ConfigureConsumeTopology = false;
            e.ConfigureConsumer(context, typeof(PushFeedUpdateConsumer));
            e.Bind<IPushFeedUpdate>();
        });
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

app.UseAuthorization();

app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseDefaultPage();



//app.UseRouting();
app.MapControllers();
app.MapHub<PostHub>("/ws/feed/news");
app.MapHub<ChatHub>("/ws/feed/chat");
//app.MapHub<ChartHub>("/chart");

app.Run();
