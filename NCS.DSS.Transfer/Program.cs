using DFC.HTTP.Standard;
using DFC.Swagger.Standard;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCS.DSS.Transfer.Cosmos.Helper;
using NCS.DSS.Transfer.GetTransferByIdHttpTrigger.Service;
using NCS.DSS.Transfer.GetTransferHttpTrigger.Service;
using NCS.DSS.Transfer.PatchTransferHttpTrigger.Service;
using NCS.DSS.Transfer.PostTransferHttpTrigger.Service;
using NCS.DSS.Transfer.Validation;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddLogging();
        services.AddTransient<IGetTransferHttpTriggerService, GetTransferHttpTriggerService>();
        services.AddTransient<IGetTransferByIdHttpTriggerService, GetTransferByIdHttpTriggerService>();
        services.AddTransient<IPostTransferHttpTriggerService, PostTransferHttpTriggerService>();
        services.AddTransient<IPatchTransferHttpTriggerService, PatchTransferHttpTriggerService>();
        services.AddTransient<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
        services.AddTransient<IHttpRequestHelper, HttpRequestHelper>();
        services.AddTransient<IResourceHelper, ResourceHelper>();
        services.AddTransient<IValidate, Validate>();
        services.AddTransient<IDynamicHelper, DynamicHelper>();
    })
    .ConfigureLogging(logging =>
    {
        // The Application Insights SDK adds a default logging filter that instructs ILogger to capture only Warning and more severe logs. Application Insights requires an explicit override.
        // For more information, see https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide?tabs=windows#application-insights
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

host.Run();
