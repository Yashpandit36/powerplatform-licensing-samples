namespace sample.gateway
{
    using System.Linq;
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Identity.Client;
    using sample.gateway.Discovery;

    public static class StartupExtensions
    {

        /// <summary>
        /// Adds logging services to the DI container.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static IServiceCollection AddLoggers(this IServiceCollection services, ILogger logger, ILoggerFactory loggerFactory)
        {
            services.TryAddSingleton<ILoggerFactory>(loggerFactory);
            services.TryAddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.TryAddSingleton<ILogger>(logger);

            return services;
        }

        public static IServiceCollection AddNeptuneDiscovery(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<INeptuneDiscovery, NeptuneDiscovery>();

            // these should be set in the LaunchSettings.json or at runtime in Environment Variables
            string coreEnvironment = Environment.GetEnvironmentVariable("CS_ENVIRONMENT");
            string coreClusterCategory = Environment.GetEnvironmentVariable("CS_CATEGORY");
            string coreClusterType = Environment.GetEnvironmentVariable("CS_TYPE");

            EvaluateConfigurationSet<GatewayConfig>(services, configuration, GatewayConfig.SectionName);
            EvaluateConfigurationSet<PowerPlatformEndpointsSettings>(services, configuration, PowerPlatformEndpointsSettings.SectionName);

            services.PostConfigure<GatewayConfig>(options =>
            {
                options.Environment = coreEnvironment;
                options.ClusterCategory = Enum.Parse<ClusterCategory>(coreClusterCategory);
                options.ClusterType = Enum.Parse<ClusterType>(coreClusterType);
            });

            return services;
        }

        private static void EvaluateConfigurationSet<T>(IServiceCollection services, IConfiguration configuration, string sectionName) where T : class
        {
            IConfigurationSection configSettingsOptions = configuration.GetSection(sectionName);

            if (configSettingsOptions == default || !configSettingsOptions.Exists())
            {
                throw new InvalidOperationException("Configuration is not set.");
            }

            services.Configure<T>(configSettingsOptions);
        }

        public static IServiceCollection AddAuthentication(this IServiceCollection services)
        {
            services.AddSingleton<IMsalHttpClientFactory, ConfidentialClientHttpClientFactory>();
            services.AddSingleton<IMsalCredentialFactory, MsalCredentialFactory>();
            services.AddSingleton<IMicrosoftAuthentication, MicrosoftAuthentication>();

            return services;
        }

        public static Type[] LoadVerbs()
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetCustomAttribute<VerbAttribute>() != null).ToArray();
        }

        public static int VerbRunner(this ICommandOptions obj, IHost host, ILogger appLogger)
        {
            IConfiguration config = host.Services.GetRequiredService<IConfiguration>();
            string coreSettings = config.GetValue<string>("Authentication:AzureEnvironment");
            appLogger.LogInformation($"Running in config {coreSettings}");

            switch (obj)
            {
                case CommandEvaluateRoleAssignmentOptions opts:
                    return opts.RunGenerateAndReturnExitCode(config, appLogger, host.Services);
                case CommandEvaluateEntitlementOptions opts:
                    return opts.RunGenerateAndReturnExitCode(config, appLogger, host.Services);
                case CommandAllocationPutOptions opts:
                    return opts.RunGenerateAndReturnExitCode(config, appLogger, host.Services);
                case CommandAllocationGetOptions opts:
                    return opts.RunGenerateAndReturnExitCode(config, appLogger, host.Services);
                case CommandCapacityGetOptions opts:
                    return opts.RunGenerateAndReturnExitCode(config, appLogger, host.Services);
                case CommandSdkCapacityGetOptions opts:
                    return opts.RunGenerateAndReturnExitCode(config, appLogger, host.Services);
            }

            return -1;
        }
    }
}