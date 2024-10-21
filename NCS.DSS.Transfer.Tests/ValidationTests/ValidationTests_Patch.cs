using NCS.DSS.Transfer.Validation;
using NUnit.Framework;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Transfer.Tests.ValidationTests
{
    [TestFixture]
    public class ValidationTests_Patch
    {
        private IValidate _validate;

        [SetUp]
        public void Setup()
        {
            _validate = new Validate();
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenOriginatingTouchpointIdIsInvalid()
        {
            var transfer = new Models.TransferPatch
            {
                TargetTouchpointId = "000000000A",
                Context = "Some context data"
            };

            var result = _validate.ValidateResource(transfer, true);

            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenOriginatingTouchpointIdIsValid()
        {
            var transfer = new Models.TransferPatch
            {
                TargetTouchpointId = "0000000001",
                Context = "Some context data"
            };

            var result = _validate.ValidateResource(transfer, true);

            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenContextIsInvalid()
        {
            var transfer = new Models.TransferPatch
            {
                TargetTouchpointId = "0000000001",
                Context = "TestContextString[]%$"
            };

            var result = _validate.ValidateResource(transfer, true);

            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenContextIsValid()
        {
            var transfer = new Models.TransferPatch
            {
                TargetTouchpointId = "0000000001",
                Context = "Some context data"
            };

            var result = _validate.ValidateResource(transfer, true);

            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }
    }
}
