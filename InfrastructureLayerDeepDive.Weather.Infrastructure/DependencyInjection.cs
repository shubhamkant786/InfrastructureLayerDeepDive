using InfrastructureLayerDeepDive.Weather.Application.InfrastructureContracts;
using InfrastructureLayerDeepDive.Weather.Application.Models;
using InfrastructureLayerDeepDive.Weather.Domain;
using InfrastructureLayerDeepDive.Weather.Infrastructure.Caching.DistributedRedis;
using InfrastructureLayerDeepDive.Weather.Infrastructure.Caching.InMemory;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Configuration;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure
{
    public static class DependencyInjection
    {
        public static void AddWeatherInfrastructureRepo(this IServiceCollection services
            , string? sqlServerConnectionString
            , string? oracleConnectionString
            , string? postgresConnectionString)
        {
            services.AddSingleton<CosmosDbContext>();

            services.AddDbContext<SqlServerDbContext>(o => o.UseSqlServer(sqlServerConnectionString, sqlOptions=>
               {
                   sqlOptions.UseNetTopologySuite();
                   sqlOptions.EnableRetryOnFailure(
                       maxRetryCount: 3,
                       maxRetryDelay: TimeSpan.FromSeconds(5),
                       errorNumbersToAdd: null);
               })
            
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
                .ConfigureWarnings(warnings =>
                {
                    warnings.Log(CoreEventId.PossibleUnintendedCollectionNavigationNullComparisonWarning);
                    warnings.Log(CoreEventId.PossibleUnintendedReferenceComparisonWarning);
                    warnings.Log(CoreEventId.RowLimitingOperationWithoutOrderByWarning);
                    warnings.Log(CoreEventId.FirstWithoutOrderByAndFilterWarning);
                    warnings.Log(CoreEventId.DistinctAfterOrderByWithoutRowLimitingOperatorWarning);
                    warnings.Ignore(CoreEventId.LazyLoadOnDisposedContextWarning);
                    warnings.Ignore(CoreEventId.DetachedLazyLoadingWarning);
                    warnings.Ignore(CoreEventId.RedundantAddServicesCallWarning);
                    warnings.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning);
                    warnings.Ignore(CoreEventId.NavigationBaseIncludeIgnored);
                    warnings.Ignore(CoreEventId.AmbiguousEndRequiredWarning);
                    warnings.Ignore(CoreEventId.NavigationBaseIncludeIgnored);
                    warnings.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning);
                    warnings.Ignore(RelationalEventId.MultipleCollectionIncludeWarning);
                })
            );
            services.AddDbContext<PostgresDbContext>(o => o.UseNpgsql(postgresConnectionString));
            services.AddDbContext<OracleDbContext>(o => o.UseOracle(oracleConnectionString));

            services.AddScoped<IWeatherCosmosRepository, WeatherCosmosRepository>();
            services.AddScoped<IWeatherRepository, SqlServerWeatherRepository>();
            services.AddScoped<IWeatherRepository, OracleWeatherRepository>();
            services.AddScoped<IWeatherRepository, PostgresWeatherRepository>();// pick the one you need, or use keyed services for multiple            
        }

        public static void AddWeatherInfrastructureMessaging(this IServiceCollection services)
        {
            services.AddSingleton<IMessageProducer<WeatherEntity>, EventHubWeatherProducer>();
            services.AddSingleton<IMessageConsumer<WeatherModel>, EventHubWeatherConsumer>();
            services.AddSingleton<IMessageProducer<WeatherEntity>, ServiceBusWeatherProducer>();
            services.AddSingleton<IMessageConsumer<WeatherModel>, ServiceBusWeatherConsumer>();
            services.AddSingleton<IMessageProducer<WeatherEntity>, KafkaWeatherProducer>();
            services.AddSingleton<IMessageConsumer<WeatherModel>, KafkaWeatherConsumer>();
            services.AddSingleton<IMessageProducer<WeatherEntity>, IbmMqWeatherProducer>();
            services.AddSingleton<IMessageConsumer<WeatherModel>, IbmMqWeatherConsumer>();            
            services.AddSingleton<IMessageProducer<WeatherEntity>, BlobStorageWeatherProducer>();
            services.AddSingleton<IMessageConsumer<WeatherEntity>, BlobStorageWeatherConsumer>();
        }

        public static void AddWeatherInfrastructureCacheServices(this IServiceCollection services)
        {
            // In-memory
            services.AddMemoryCache(options => options.SizeLimit = 1024);
            services.AddSingleton<ICacheService, InMemoryCacheService>();

            // OR Redis (pick one, or use keyed services if both are needed simultaneously)

            services.AddSingleton<ICacheService, RedisCacheService>();
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var redisOptions = sp.GetRequiredService<IOptions<RedisCacheOptions>>().Value;
                return ConnectionMultiplexer.Connect(redisOptions.ConnectionString);
            });
            services.AddSingleton<ICacheService, RedisConnectionMultiplexerCacheService>();
        }

        public static void AddWeatherInfrastructureNotificationServices(this IServiceCollection services)
        {
            services.AddScoped<INotificationService<EmailMessage>, SmtpEmailNotificationService>();
            services.AddScoped<INotificationService<SmsMessage>, TwilioSmsNotificationService>();
            services.AddSingleton<INotificationService<PushMessage>, FirebasePushNotificationService>();
        }
    }
}
