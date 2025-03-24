using DFC.Swagger.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using System.Reflection;

namespace NCS.DSS.Transfer.APIDefinition
{
    public class ApiDefinition
    {
        private const string APITitle = "Transfers";
        private const string APIDefinitionName = "API-Definition";
        private const string APIDefRoute = APITitle + "/" + APIDefinitionName;
        private const string APIDescription = "Basic details of a National Careers Service " + APITitle + " Resource";
        private const string ApiVersion = "2.0.0";
        
        private readonly ISwaggerDocumentGenerator _swaggerDocumentGenerator;

        public ApiDefinition(ISwaggerDocumentGenerator swaggerDocumentGenerator)
        {
            this._swaggerDocumentGenerator = swaggerDocumentGenerator;
        }

        [Function(APIDefinitionName)]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = APIDefRoute)] HttpRequest req)
        {
            var swagger = _swaggerDocumentGenerator.GenerateSwaggerDocument(req, APITitle, APIDescription,
                APIDefinitionName, ApiVersion, Assembly.GetExecutingAssembly());

            if (string.IsNullOrEmpty(swagger))
            {
                return new NoContentResult();
            }

            return new OkObjectResult(swagger);
        }
    }
}