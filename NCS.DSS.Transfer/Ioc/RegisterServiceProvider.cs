using System;
using Microsoft.Extensions.DependencyInjection;
using NCS.DSS.Transfer.Cosmos.Helper;
using NCS.DSS.Transfer.GetTransferByIdHttpTrigger.Service;
using NCS.DSS.Transfer.GetTransferHttpTrigger.Service;
using NCS.DSS.Transfer.Helpers;
using NCS.DSS.Transfer.PatchTransferHttpTrigger.Service;
using NCS.DSS.Transfer.PostTransferHttpTrigger.Service;
using NCS.DSS.Transfer.Validation;


namespace NCS.DSS.Transfer.Ioc
{
    public class RegisterServiceProvider
    {
        public IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddTransient<IGetTransferHttpTriggerService, GetTransferHttpTriggerService>();
            services.AddTransient<IGetTransferByIdHttpTriggerService, GetTransferByIdHttpTriggerService>();
            services.AddTransient<IPostTransferHttpTriggerService, PostTransferHttpTriggerService>();
            services.AddTransient<IPatchTransferHttpTriggerService, PatchTransferHttpTriggerService>();
            services.AddTransient<IResourceHelper, ResourceHelper>();
            services.AddTransient<IValidate, Validate>();
            services.AddTransient<IHttpRequestMessageHelper, HttpRequestMessageHelper>();
            return services.BuildServiceProvider(true);
        }
    }
}
