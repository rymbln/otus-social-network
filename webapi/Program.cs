using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using OtusSocialNetwork.Database;
using OtusSocialNetwork.DataClasses.Dtos;
using OtusSocialNetwork.DataClasses.Internals;
using OtusSocialNetwork.Filters;
using OtusSocialNetwork.Middlewares;
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
                    (path.StartsWithSegments("/post/feed/posted")))
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

builder.Services.AddMassTransit(busConfigurator =>
{ 
    busConfigurator.SetKebabCaseEndpointNameFormatter();
    busConfigurator.AddConsumers(Assembly.GetExecutingAssembly());
    busConfigurator.UsingRabbitMq((context, busFactoryConfigurator) =>
    {
        //busFactoryConfigurator.Host("rabbitmq", "/", h => { });
        busFactoryConfigurator.Host(config.GetValue<string>("RabbitEndpoint"), hostConfigurator => { });
        busFactoryConfigurator.ConfigureEndpoints(context);
    });
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

app.UseAuthorization();

app.UseMiddleware<ErrorHandlerMiddleware>();

//app.UseRouting();
app.MapControllers();
app.MapHub<PostHub>("/post/feed/posted");
//app.MapHub<ChartHub>("/chart");

app.Run();
