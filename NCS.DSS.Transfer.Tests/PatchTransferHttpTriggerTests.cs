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
using NCS.DSS.Transfer.PatchTransferHttpTrigger.Service;
using NCS.DSS.Transfer.Validation;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace NCS.DSS.Transfer.Tests
{
    [TestFixture]
    public class PatchTransferHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidInteractionId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private const string ValidTransferId = "d5369b9a-6959-4bd3-92fc-1583e72b7e51";
        private ILogger _log;
        private HttpRequestMessage _request;
        private IResourceHelper _resourceHelper;
        private IValidate _validate;
        private IHttpRequestMessageHelper _httpRequestMessageHelper;
        private IPatchTransferHttpTriggerService _patchTransferHttpTriggerService;
        private Models.Transfer _transfer;
        private TransferPatch _transferPatch;

        [SetUp]
        public void Setup()
        {
            _transfer = Substitute.For<Models.Transfer>();
            _transferPatch = Substitute.For<TransferPatch>();

            _request = new HttpRequestMessage()
            {
                Content = new StringContent(string.Empty),
                RequestUri = 
                    new Uri($"http://localhost:7071/api/Customers/7E467BDB-213F-407A-B86A-1954053D3C24/" +
                            $"Transfer/1e1a555c-9633-4e12-ab28-09ed60d51cb3")
            };

            _log = Substitute.For<ILogger>();
            _resourceHelper = Substitute.For<IResourceHelper>();
            _validate = Substitute.For<IValidate>();
            _httpRequestMessageHelper = Substitute.For<IHttpRequestMessageHelper>();
            _patchTransferHttpTriggerService = Substitute.For<IPatchTransferHttpTriggerService>();
        }

        [Test]
        public async Task PatchTransferHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidTransferId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchTransferHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, InValidId, ValidTransferId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchTransferHttpTrigger_ReturnsStatusCodeBadRequest_WhenTransferIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, InValidId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchTransferHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenTransferHasFailedValidation()
        {
            _httpRequestMessageHelper.GetTransferFromRequest<TransferPatch>(_request).Returns(Task.FromResult(_transferPatch).Result);

            var validationResults = new List<ValidationResult> { new ValidationResult("interaction Id is Required") };
            _validate.ValidateResource(Arg.Any<TransferPatch>()).Returns(validationResults);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidTransferId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PatchTransferHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenTransferRequestIsInvalid()
        {
            _httpRequestMessageHelper.GetTransferFromRequest<Models.Transfer>(_request).Throws(new JsonException());

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidTransferId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PatchTransferHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            _httpRequestMessageHelper.GetTransferFromRequest<TransferPatch>(_request).Returns(Task.FromResult(_transferPatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(false);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidTransferId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PatchTransferHttpTrigger_ReturnsStatusCodeNoContent_WhenTransferDoesNotExist()
        {
            _httpRequestMessageHelper.GetTransferFromRequest<TransferPatch>(_request).Returns(Task.FromResult(_transferPatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);

            _patchTransferHttpTriggerService.GetTransferForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult<Models.Transfer>(null).Result);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidTransferId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PatchTransferHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            _httpRequestMessageHelper.GetTransferFromRequest<TransferPatch>(_request).Returns(Task.FromResult(_transferPatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesInteractionExist(Arg.Any<Guid>()).Returns(false);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidTransferId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PatchTransferHttpTrigger_ReturnsStatusCodeOk_WhenTransferDoesNotExist()
        {
            _httpRequestMessageHelper.GetTransferFromRequest<TransferPatch>(_request).Returns(Task.FromResult(_transferPatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesInteractionExist(Arg.Any<Guid>()).Returns(true);

            _patchTransferHttpTriggerService.GetTransferForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult<Models.Transfer>(null).Result);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidTransferId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PatchTransferHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToUpdateTransferRecord()
        {
            _httpRequestMessageHelper.GetTransferFromRequest<TransferPatch>(_request).Returns(Task.FromResult(_transferPatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);
            _resourceHelper.DoesInteractionExist(Arg.Any<Guid>()).Returns(true);

            _patchTransferHttpTriggerService.GetTransferForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult(_transfer).Result);

            _patchTransferHttpTriggerService.UpdateAsync(Arg.Any<Models.Transfer>(), Arg.Any<TransferPatch>()).Returns(Task.FromResult<Models.Transfer>(null).Result);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidTransferId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchTransferHttpTrigger_ReturnsStatusCodeOK_WhenRequestIsNotValid()
        {
            _httpRequestMessageHelper.GetTransferFromRequest<TransferPatch>(_request).Returns(Task.FromResult(_transferPatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);
            _resourceHelper.DoesInteractionExist(Arg.Any<Guid>()).Returns(true);

            _patchTransferHttpTriggerService.GetTransferForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult(_transfer).Result);

            _patchTransferHttpTriggerService.UpdateAsync(Arg.Any<Models.Transfer>(), Arg.Any<TransferPatch>()).Returns(Task.FromResult<Models.Transfer>(null).Result);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidTransferId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchTransferHttpTrigger_ReturnsStatusCodeOK_WhenRequestIsValid()
        {
            _httpRequestMessageHelper.GetTransferFromRequest<TransferPatch>(_request).Returns(Task.FromResult(_transferPatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);
            _resourceHelper.DoesInteractionExist(Arg.Any<Guid>()).Returns(true);

            _patchTransferHttpTriggerService.GetTransferForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult(_transfer).Result);

            _patchTransferHttpTriggerService.UpdateAsync(Arg.Any<Models.Transfer>(), Arg.Any<TransferPatch>()).Returns(Task.FromResult(_transfer).Result);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidTransferId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId, string interactionId, string transferId)
        {
            return await PatchTransferHttpTrigger.Function.PatchTransferHttpTrigger.Run(
                _request, _log, customerId, interactionId, transferId, _resourceHelper, _httpRequestMessageHelper, _validate, _patchTransferHttpTriggerService).ConfigureAwait(false);
        }

    }
}