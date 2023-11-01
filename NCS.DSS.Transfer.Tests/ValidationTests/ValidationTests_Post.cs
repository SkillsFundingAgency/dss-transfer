using NCS.DSS.Transfer.Validation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCS.DSS.Transfer.Tests.ValidationTests
{
    [TestFixture]
    public class ValidationTests_Post
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
            var transfer = new Models.Transfer
            {
                TargetTouchpointId = "000000000A", 
                Context = "Some context data"
            };

            var result = _validate.ValidateResource(transfer, true);

            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenOriginatingTouchpointIdIsValid()
        {
            var transfer = new Models.Transfer
            {
                TargetTouchpointId = "0000000001",
                Context = "Some context data"
            };

            var result = _validate.ValidateResource(transfer, true);

            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenContextIsInvalid()
        {
            var transfer = new Models.Transfer
            {
                TargetTouchpointId = "0000000001",
                Context = "TestContextString[]%$"
            };

            var result = _validate.ValidateResource(transfer, true);

            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenContextIsValid()
        {
            var transfer = new Models.Transfer
            {
                TargetTouchpointId = "0000000001",
                Context = "Some context data"
            };

            var result = _validate.ValidateResource(transfer, true);

            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }
    }
}
