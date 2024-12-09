using DFC.HTTP.Standard;
using DFC.Swagger.Standard;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.Transfer.Cosmos.Helper;
using NCS.DSS.Transfer.Cosmos.Provider;
using NCS.DSS.Transfer.GetTransferByIdHttpTrigger.Service;
using NCS.DSS.Transfer.GetTransferHttpTrigger.Service;
using NCS.DSS.Transfer.Models;
using NCS.DSS.Transfer.PatchTransferHttpTrigger.Service;
using NCS.DSS.Transfer.PostTransferHttpTrigger.Service;
using NCS.DSS.Transfer.Validation;

namespace NCS.DSS.Transfer
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWebApplication()
                .ConfigureServices((context, services) =>
                {
                    var configuration = context.Configuration;
                    services.AddOptions<TransferConfigurationSettings>()
                        .Bind(configuration);

                    services.AddApplicationInsightsTelemetryWorkerService();
                    services.ConfigureFunctionsApplicationInsights();
                    services.AddLogging();
                    services.AddTransient<IGetTransferHttpTriggerService, GetTransferHttpTriggerService>();
                    services.AddTransient<IGetTransferByIdHttpTriggerService, GetTransferByIdHttpTriggerService>();
                    services.AddTransient<IPostTransferHttpTriggerService, PostTransferHttpTriggerService>();
                    services.AddTransient<IPatchTransferHttpTriggerService, PatchTransferHttpTriggerService>();
                    services.AddTransient<ICosmosDBProvider, CosmosDBProvider>();
                    services.AddTransient<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
                    services.AddTransient<IHttpRequestHelper, HttpRequestHelper>();
                    services.AddTransient<IResourceHelper, ResourceHelper>();
                    services.AddTransient<IValidate, Validate>();
                    services.AddTransient<IDynamicHelper, DynamicHelper>();

                    services.AddSingleton(sp =>
                    {
                        var config = sp.GetRequiredService<IOptions<TransferConfigurationSettings>>().Value;
                        config.TransferConnectionString = $"AccountEndpoint={config.Endpoint}/;AccountKey={config.Key};Database={config.CustomerDatabaseId};";

                        var options = new CosmosClientOptions()
                        {
                            ConnectionMode = ConnectionMode.Gateway
                        };

                        return new CosmosClient(config.TransferConnectionString, options);
                    });

                    services.AddSingleton(sp =>
                    {
                        var config = sp.GetRequiredService<IOptions<TransferConfigurationSettings>>().Value;
                        config.ServiceBusConnectionString =
                            $"Endpoint={config.BaseAddress};SharedAccessKeyName={config.KeyName};SharedAccessKey={config.AccessKey}";

                        return new Azure.Messaging.ServiceBus.ServiceBusClient(config.ServiceBusConnectionString);
                    });
                })
                .ConfigureLogging(logging =>
                {
                    logging.Services.Configure<LoggerFilterOptions>(options =>
                    {
                        LoggerFilterRule defaultRule = options.Rules.FirstOrDefault(rule => rule.ProviderName
                            == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
                        if (defaultRule is not null)
                        {
                            options.Rules.Remove(defaultRule);
                        }
                    });
                })
                .Build();

            await host.RunAsync();
        }
    }
}