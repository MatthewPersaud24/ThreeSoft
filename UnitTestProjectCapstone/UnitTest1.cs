using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.ComponentModel.Annotations;
using ThreeSoft.Models;

namespace UnitTestProjectCapstone
{
    [TestClass]
    public class UnitTest1
    {
        private static bool ValidateModel(object model, out List<ValidationResult> results)
        {
            var context = new ValidationContext(model, null, null);
            results = new List<ValidationResult>();
            return Validator.TryValidateObject(model, context, results, true);
        }


        [TestMethod]
        public void TestMethod1()
        {
        }

        [TestMethod]
        public void Username_IsRequired()
        {
            // Arrange
            var model = new LoginViewModel { Password = "password" };

            // Act
            var isValid = ValidateModel(model, out var results);

            // Assert
            Assert.IsFalse(isValid);
            Assert.IsTrue(results.Exists(v => v.ErrorMessage == "Please enter a username."));
        }

        [TestMethod]
        public void Password_IsRequired()
        {
            // Arrange
            var model = new LoginViewModel { Username = "username" };

            // Act
            var isValid = ValidateModel(model, out var results);

            // Assert
            Assert.IsFalse(isValid);
            Assert.IsTrue(results.Exists(v => v.ErrorMessage == "Please enter a password."));
        }

        [TestMethod]
        public void Username_MaxLength_255()
        {
            // Arrange
            var model = new LoginViewModel
            {
                Username = new string('a', 256),
                Password = "password"
            };

            // Act
            var isValid = ValidateModel(model, out var results);

            // Assert
            Assert.IsFalse(isValid);
            Assert.IsTrue(results.Exists(v => v.MemberNames.Contains("Username") && v.ErrorMessage.Contains("The field Username must be a string or array type with a maximum length of '255'.")));
        }

        [TestMethod]
        public void Password_MaxLength_255()
        {
            // Arrange
            var model = new LoginViewModel
            {
                Username = "username",
                Password = new string('a', 256)
            };

            // Act
            var isValid = ValidateModel(model, out var results);

            // Assert
            Assert.IsFalse(isValid);
            Assert.IsTrue(results.Exists(v => v.MemberNames.Contains("Password") && v.ErrorMessage.Contains("The field Password must be a string or array type with a maximum length of '255'.")));
        }

        [TestMethod]
        public void LoginModel_IsValid()
        {
            // Arrange
            var model = new LoginViewModel
            {
                Username = "username",
                Password = "password",
                ReturnUrl = "/home",
                RememberMe = true
            };

            // Act
            var isValid = ValidateModel(model, out var results);

            // Assert
            Assert.IsTrue(isValid);
            Assert.AreEqual(0, results.Count);
        }


    }
}
