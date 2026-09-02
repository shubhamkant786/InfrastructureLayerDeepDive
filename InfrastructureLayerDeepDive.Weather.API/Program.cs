using InfrastructureLayerDeepDive.Weather.Infrastructure;
using InfrastructureLayerDeepDive.Weather.Infrastructure.Caching.DistributedRedis;
using InfrastructureLayerDeepDive.Weather.Infrastructure.Caching.InMemory;
using InfrastructureLayerDeepDive.Weather.Infrastructure.Dependencies.GraphqlDependency;
using InfrastructureLayerDeepDive.Weather.Infrastructure.Dependencies.RestDependency;
using InfrastructureLayerDeepDive.Weather.Infrastructure.Mesaging.Azure.BlobStorage;
using InfrastructureLayerDeepDive.Weather.Infrastructure.Mesaging.Azure.EventHubs;
using InfrastructureLayerDeepDive.Weather.Infrastructure.Mesaging.Azure.ServiceBus;
using InfrastructureLayerDeepDive.Weather.Infrastructure.Mesaging.IBM.MQ;
using InfrastructureLayerDeepDive.Weather.Infrastructure.Mesaging.Kafka;
using InfrastructureLayerDeepDive.Weather.Infrastructure.Notifications.Email;
using InfrastructureLayerDeepDive.Weather.Infrastructure.Notifications.Push;
using InfrastructureLayerDeepDive.Weather.Infrastructure.Notifications.Sms;
using InfrastructureLayerDeepDive.Weather.Infrastructure.Repositories.Cosmos;
using InfrastructureLayerDeepDive.Weather.Infrastructure.Repositories.Oracle;
using InfrastructureLayerDeepDive.Weather.Infrastructure.Repositories.Postgres;
using InfrastructureLayerDeepDive.Weather.Infrastructure.Repositories.SqlServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Asn1.X509;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var cosmosConfiguration = builder.Configuration.GetSection("CosmosDb").Get<CosmosDbOptions>();
var sqlServerConfiguration = builder.Configuration.GetSection("SqlServerDb").Get<SqlServerDbOptions>();
var oracleConfiguration = builder.Configuration.GetSection("OracleDb").Get<OracleDbOptions>();
var postgresConfiguration = builder.Configuration.GetSection("PostgresDb").Get<PostgresDbOptions>();

var sqlServerConnectionString = $"Server={sqlServerConfiguration.Host},{sqlServerConfiguration.Port};" +
    $"Authentication=Active Directory Service Principal; Encrypt=True;" +
    $"Database={sqlServerConfiguration.Database};" +
    $"User Id={sqlServerConfiguration.Username};" +
    $"Password={sqlServerConfiguration.Password};";
var oracleConnectionString = $"Data Source=(DESCRIPTION=(ADDRESS_LIST=(" +
                $"ADDRESS=(PROTOCOL=TCP)" +
                $"(HOST={oracleConfiguration.Host})" +
                $"(PORT={oracleConfiguration.Port})))" +
                $"(CONNECT_DATA=(SERVER=DEDICATED)" +
                $"(SERVICE_NAME={oracleConfiguration.ServiceName})));" +
                $"User Id ={oracleConfiguration.User};" +
                $"Password ={oracleConfiguration.Password};" +
                $"Connection Timeout=120;";
var postgresConnectionString = $"Host={postgresConfiguration.Host};" +
    $"Port={postgresConfiguration.Port};" +
    $"Database={postgresConfiguration.Database};" +
    $"User ID={postgresConfiguration.User};" +
    $"Password={postgresConfiguration.Password};" +
    $"Timeout=120;Ssl Mode=Require;";
builder.Services.AddWeatherInfrastructureRepo(sqlServerConnectionString, oracleConnectionString, postgresConnectionString);

//var eventHubConfiguration = builder.Configuration.GetSection("EventHub").Get<EventHubOptions>();
//builder.Services.Configure<EventHubOptions>(builder.Configuration.GetSection("EventHub"));
builder.Services.AddOptions<EventHubOptions>()
    .Bind(builder.Configuration.GetSection("EventHub"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<ServiceBusOptions>()
       .Bind(builder.Configuration.GetSection("ServiceBus"))
       .ValidateDataAnnotations();

builder.Services.AddOptions<KafkaOptions>()
       .Bind(builder.Configuration.GetSection("Kafka"))
       .ValidateDataAnnotations();

builder.Services.AddOptions<IbmMqOptions>()
       .Bind(builder.Configuration.GetSection("IbmMq"))
       .ValidateDataAnnotations();
builder.Services.Configure<BlobStorageOptions>(builder.Configuration.GetSection("BlobStorage"));
builder.Services.AddWeatherInfrastructureMessaging();

builder.Services.AddOptions<SmtpOptions>()
       .Bind(builder.Configuration.GetSection("Smtp"))
       .ValidateDataAnnotations();

builder.Services.AddOptions<TwilioOptions>()
       .Bind(builder.Configuration.GetSection("Sms"))
       .ValidateDataAnnotations();

builder.Services.AddOptions<FirebaseOptions>()
       .Bind(builder.Configuration.GetSection("Push"))
       .ValidateDataAnnotations();
builder.Services.AddWeatherInfrastructureNotificationServices();


builder.Services.Configure<InMemoryCacheOptions>(builder.Configuration.GetSection("InMemoryCache"));
builder.Services.Configure<RedisCacheOptions>(builder.Configuration.GetSection("RedisCache"));

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["RedisCache:Host"];
    options.InstanceName = builder.Configuration["RedisCache:InstanceName"];
    options.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions
    {
        Password = builder.Configuration["RedisCache:Password"],
        Ssl = bool.Parse(builder.Configuration["RedisCache:Ssl"] ?? "false"),
        AbortOnConnectFail = bool.Parse(builder.Configuration["RedisCache:AbortConnect"] ?? "true"),
        ConnectTimeout = int.Parse(builder.Configuration["RedisCache:ConnectTimeout"] ?? "5000")
    };
});

builder.Services.AddWeatherInfrastructureCacheServices();

builder.Services.Configure<RestClientOptions>(builder.Configuration.GetSection("RestClient"));
// REST            
var options = builder.Configuration.GetSection("CosmosDb").Get<RestClientOptions>();
var httpClientBuilder = builder.Services.AddHttpClient<IWeatherRestClient, WeatherRestClient>((sp, client) =>
{
    //var options = sp.GetRequiredService<IOptions<RestClientOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
    if (!string.IsNullOrWhiteSpace(options.ApiKeyName))
    {
        client.DefaultRequestHeaders.Add(options.ApiKeyName, options.ApiKeyValue);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("User-Agent", ".net application");
        
    }
});
if (options.IsCertificateValidationEnabled)
{
    var certificatePath = Path.Combine(AppContext.BaseDirectory, options.CertificateName);
    httpClientBuilder.ConfigurePrimaryHttpMessageHandler(() =>
    {

        X509Certificate2 clientCertificate = new(certificatePath, options.CertificatePassword);
        HttpClientHandler clientHandler = new();
        //clientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
        clientHandler.ClientCertificates.Add(clientCertificate);
        clientHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
        return clientHandler;
    });
}

// GraphQL
builder.Services.Configure<GraphqlClientOptions>(builder.Configuration.GetSection("GraphqlClient"));
builder.Services.AddHttpClient(nameof(GraphqlClientFactory));
builder.Services.AddSingleton<GraphqlClientFactory>();
//builder.Services.AddScoped<IWeatherGraphqlClient, WeatherGraphqlClient>(sp =>
//    new WeatherGraphqlClient(
//        await sp.GetRequiredService<GraphqlClientFactory>().CreateClient(),
//        sp.GetRequiredService<ILogger<WeatherGraphqlClient>>()));

// gRPC
//https://learn.microsoft.com/en-us/aspnet/core/grpc/clientfactory?view=aspnetcore-10.0
//builder.Services.Configure<GrpcClientOptions>(builder.Configuration.GetSection("GrpcClient"));
//builder.Services.AddGrpcClient<WeatherService.WeatherServiceClient>((sp, options) =>
//{
//    var grpcOptions = sp.GetRequiredService<IOptions<GrpcClientOptions>>().Value;
//    options.Address = new Uri(grpcOptions.Address);
//});
//builder.Services.AddScoped<IWeatherGrpcClient, WeatherGrpcClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
