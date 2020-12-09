using DFC.Swagger.Standard;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NCS.DSS.Transfer;
using NCS.DSS.Transfer.Cosmos.Helper;
using NCS.DSS.Transfer.GetTransferByIdHttpTrigger.Service;
using NCS.DSS.Transfer.GetTransferHttpTrigger.Service;
using NCS.DSS.Transfer.Helpers;
using NCS.DSS.Transfer.PatchTransferHttpTrigger.Service;
using NCS.DSS.Transfer.PostTransferHttpTrigger.Service;
using NCS.DSS.Transfer.Validation;

[assembly: FunctionsStartup(typeof(Startup))]
namespace NCS.DSS.Transfer
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging();
            builder.Services.AddTransient<IGetTransferHttpTriggerService, GetTransferHttpTriggerService>();
            builder.Services.AddTransient<IGetTransferByIdHttpTriggerService, GetTransferByIdHttpTriggerService>();
            builder.Services.AddTransient<IPostTransferHttpTriggerService, PostTransferHttpTriggerService>();
            builder.Services.AddTransient<IPatchTransferHttpTriggerService, PatchTransferHttpTriggerService>();
            builder.Services.AddTransient<IResourceHelper, ResourceHelper>();
            builder.Services.AddTransient<IValidate, Validate>();
            builder.Services.AddTransient<IHttpRequestMessageHelper, HttpRequestMessageHelper>();
            builder.Services.AddTransient<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
        }
    }
}
