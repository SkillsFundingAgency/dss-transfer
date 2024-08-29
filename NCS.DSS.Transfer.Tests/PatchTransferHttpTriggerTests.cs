using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Transfer.Cosmos.Helper;
using NCS.DSS.Transfer.Models;
using NCS.DSS.Transfer.PatchTransferHttpTrigger.Service;
using NCS.DSS.Transfer.Validation;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;

namespace NCS.DSS.Transfer.Tests
{
    [TestFixture]
    public class PatchTransferHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidInteractionId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private const string ValidTransferId = "d5369b9a-6959-4bd3-92fc-1583e72b7e51";

        private Mock<IPatchTransferHttpTriggerService> _patchTransferHttpTriggerService;
        private Mock<IHttpRequestHelper> _httpRequestMessageHelper;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<IDynamicHelper> _dynamicHelper;
        private Mock<ILogger<PatchTransferHttpTrigger.Function.PatchTransferHttpTrigger>> _log;

        private IValidate _validate;
        private Models.Transfer _transfer;
        private TransferPatch _transferPatch;
        private HttpRequest _request;
        private PatchTransferHttpTrigger.Function.PatchTransferHttpTrigger _function;

        [SetUp]
        public void Setup()
        {
            _transfer = new Models.Transfer() { LastModifiedTouchpointId = "0000000001" };
            _transferPatch = new TransferPatch() { LastModifiedTouchpointId = "0000000001", TargetTouchpointId = "0000000002" };
            _request = new DefaultHttpContext().Request;

            _patchTransferHttpTriggerService = new Mock<IPatchTransferHttpTriggerService>();
            _httpRequestMessageHelper = new Mock<IHttpRequestHelper>();
            _resourceHelper = new Mock<IResourceHelper>();
            _validate = new Validate();
            _dynamicHelper = new Mock<IDynamicHelper>();
            _log = new Mock<ILogger<PatchTransferHttpTrigger.Function.PatchTransferHttpTrigger>>();

            _function = new PatchTransferHttpTrigger.Function.PatchTransferHttpTrigger(
                _patchTransferHttpTriggerService.Object,
                _httpRequestMessageHelper.Object,
                _resourceHelper.Object,
                _validate,
                _dynamicHelper.Object,
                _log.Object);
        }

        [Test]
        public async Task PatchTransferHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns((string)null);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidTransferId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task PatchTransferHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidTransferId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PatchTransferHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");

            // Act
            var result = await RunFunction(ValidCustomerId, InValidId, ValidTransferId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PatchTransferHttpTrigger_ReturnsStatusCodeBadRequest_WhenTransferIdIsInvalid()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, InValidId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PatchTransferHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenTransferHasFailedValidation()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<TransferPatch>(_request)).Returns(Task.FromResult(_transferPatch));
            var val = new Mock<IValidate>();
            var validationResults = new List<ValidationResult> { new ValidationResult("interaction Id is Required") };
            val.Setup(x => x.ValidateResource(It.IsAny<TransferPatch>(), false)).Returns(validationResults);
            _function = new PatchTransferHttpTrigger.Function.PatchTransferHttpTrigger(
                _patchTransferHttpTriggerService.Object,
                _httpRequestMessageHelper.Object,
                _resourceHelper.Object,
                val.Object,
                _dynamicHelper.Object,
                _log.Object);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidTransferId);

            // Assert
            Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
        }

        [Test]
        public async Task PatchTransferHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenTransferRequestIsInvalid()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<Models.Transfer>(_request)).Throws(new JsonException());

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidTransferId);

            // Assert
            Assert.That(result, Is.InstanceOf<UnprocessableEntityResult>());
        }

        [Test]
        public async Task PatchTransferHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<TransferPatch>(_request)).Returns(Task.FromResult(_transferPatch));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidTransferId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task PatchTransferHttpTrigger_ReturnsStatusCodeNoContent_WhenTransferDoesNotExist()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<TransferPatch>(_request)).Returns(Task.FromResult(_transferPatch));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _patchTransferHttpTriggerService.Setup(x => x.GetTransferForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<Models.Transfer>(null));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidTransferId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task PatchTransferHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<TransferPatch>(_request)).Returns(Task.FromResult(_transferPatch));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesInteractionResourceExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(false);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidTransferId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task PatchTransferHttpTrigger_ReturnsStatusCodeOk_WhenTransferDoesNotExist()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<TransferPatch>(_request)).Returns(Task.FromResult(_transferPatch));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesInteractionResourceExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);
            _patchTransferHttpTriggerService.Setup(x => x.GetTransferForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<Models.Transfer>(null));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidTransferId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task PatchTransferHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToUpdateTransferRecord()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<TransferPatch>(_request)).Returns(Task.FromResult(_transferPatch));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesInteractionResourceExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);
            _patchTransferHttpTriggerService.Setup(x => x.GetTransferForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(_transfer));
            _patchTransferHttpTriggerService.Setup(x => x.UpdateAsync(It.IsAny<Models.Transfer>(), It.IsAny<TransferPatch>())).Returns(Task.FromResult<Models.Transfer>(null));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidTransferId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PatchTransferHttpTrigger_ReturnsStatusCodeOK_WhenRequestIsNotValid()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<TransferPatch>(_request)).Returns(Task.FromResult(_transferPatch));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesInteractionResourceExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);
            _patchTransferHttpTriggerService.Setup(x => x.GetTransferForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(_transfer));
            _patchTransferHttpTriggerService.Setup(x => x.UpdateAsync(It.IsAny<Models.Transfer>(), It.IsAny<TransferPatch>())).Returns(Task.FromResult<Models.Transfer>(null));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidTransferId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PatchTransferHttpTrigger_ReturnsStatusCodeOK_WhenRequestIsValid()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<TransferPatch>(_request)).Returns(Task.FromResult(_transferPatch));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesInteractionResourceExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);
            _patchTransferHttpTriggerService.Setup(x => x.GetTransferForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(_transfer));
            _patchTransferHttpTriggerService.Setup(x => x.UpdateAsync(It.IsAny<Models.Transfer>(), It.IsAny<TransferPatch>())).Returns(Task.FromResult(_transfer));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidTransferId);
            var resultResponse = result as JsonResult;

            // Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
            Assert.That(resultResponse.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
        }

        private async Task<IActionResult> RunFunction(string customerId, string interactionId, string transferId)
        {
            return await _function.Run(
                _request, customerId, interactionId, transferId).ConfigureAwait(false);
        }

    }
}