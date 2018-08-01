using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCS.DSS.Transfer.Cosmos.Helper;
using NCS.DSS.Transfer.Helpers;
using NCS.DSS.Transfer.Models;
using NCS.DSS.Transfer.PostTransferHttpTrigger.Service;
using NCS.DSS.Transfer.Validation;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace NCS.DSS.Transfer.Tests
{
    [TestFixture]
    public class PostTransferHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidInteractionId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private ILogger _log;
        private HttpRequestMessage _request;
        private IResourceHelper _resourceHelper;
        private IValidate _validate;
        private IHttpRequestMessageHelper _httpRequestMessageHelper;
        private IPostTransferHttpTriggerService _postTransferHttpTriggerService;
        private Models.Transfer _transfer;

        [SetUp]
        public void Setup()
        {
            _transfer = Substitute.For<Models.Transfer>();

            _request = new HttpRequestMessage()
            {
                Content = new StringContent(string.Empty),
                RequestUri =
                    new Uri($"http://localhost:7071/api/Customers/7E467BDB-213F-407A-B86A-1954053D3C24/" +
                            $"Interactions/aa57e39e-4469-4c79-a9e9-9cb4ef410382/" +
                            $"Transfer")
            };

            _log = Substitute.For<ILogger>();
            _resourceHelper = Substitute.For<IResourceHelper>();
            _httpRequestMessageHelper = Substitute.For<IHttpRequestMessageHelper>();
            _validate = Substitute.For<IValidate>();
            _postTransferHttpTriggerService = Substitute.For<IPostTransferHttpTriggerService>();
            _httpRequestMessageHelper.GetTouchpointId(_request).Returns(new Guid());
        }

        [Test]
        public async Task PostTransferHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            _httpRequestMessageHelper.GetTouchpointId(_request).Returns((Guid?)null);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostTransferHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Act
            var result = await RunFunction(InValidId, ValidInteractionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostTransferHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, InValidId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostTransferHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenTransferHasFailedValidation()
        {
            _httpRequestMessageHelper.GetTransferFromRequest<Models.Transfer>(_request).Returns(Task.FromResult(_transfer).Result);

            var validationResults = new List<ValidationResult> { new ValidationResult("interaction Id is Required") };
            _validate.ValidateResource(Arg.Any<Models.Transfer>(), true).Returns(validationResults);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PostTransferHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenTransferRequestIsInvalid()
        {
            _httpRequestMessageHelper.GetTransferFromRequest<Models.Transfer>(_request).Throws(new JsonException());

            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PostTransferHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            _httpRequestMessageHelper.GetTransferFromRequest<Models.Transfer>(_request).Returns(Task.FromResult(_transfer).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(false);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PostTransferHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            _httpRequestMessageHelper.GetTransferFromRequest<Models.Transfer>(_request).Returns(Task.FromResult(_transfer).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesInteractionExist(Arg.Any<Guid>()).Returns(false);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PostTransferHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToCreateTransferRecord()
        {
            _httpRequestMessageHelper.GetTransferFromRequest<Models.Transfer>(_request).Returns(Task.FromResult(_transfer).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);
            _resourceHelper.DoesInteractionExist(Arg.Any<Guid>()).Returns(true);

            _postTransferHttpTriggerService.CreateAsync(Arg.Any<Models.Transfer>()).Returns(Task.FromResult<Models.Transfer>(null).Result);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostTransferHttpTrigger_ReturnsStatusCodeCreated_WhenRequestIsInValid()
        {
            _httpRequestMessageHelper.GetTransferFromRequest<Models.Transfer>(_request).Returns(Task.FromResult(_transfer).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);
            _resourceHelper.DoesInteractionExist(Arg.Any<Guid>()).Returns(true);

            _postTransferHttpTriggerService.CreateAsync(Arg.Any<Models.Transfer>()).Returns(Task.FromResult<Models.Transfer>(null).Result);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostTransferHttpTrigger_ReturnsStatusCodeCreated_WhenRequestIsValid()
        {
            _httpRequestMessageHelper.GetTransferFromRequest<Models.Transfer>(_request).Returns(Task.FromResult(_transfer).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);
            _resourceHelper.DoesInteractionExist(Arg.Any<Guid>()).Returns(true);

            _postTransferHttpTriggerService.CreateAsync(Arg.Any<Models.Transfer>()).Returns(Task.FromResult<Models.Transfer>(_transfer).Result);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId, string interactionId)
        {
            return await PostTransferHttpTrigger.Function.PostTransferHttpTrigger.Run(
                _request, _log, customerId, interactionId, _resourceHelper, _httpRequestMessageHelper, _validate, _postTransferHttpTriggerService).ConfigureAwait(false);
        }

    }
}