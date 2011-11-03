using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Dynamic.Xml.Tests.Unit
{
    [TestFixture]
    public class XmlLoadingTests
    {
        [Test]
        public void Loading_GivenValidXml_ShouldReturnDynamicXDocument()
        {
            // Arrange
            
            // Act
            var document = DynamicXContainer.Parse(CreateTemplateXml());

            // Assert
            Assert.NotNull(document);
            Assert.IsAssignableFrom<DynamicXContainer>(document);
        }

        [Test]
        public void InterrogatingProperties_GivenValidXml_ShouldNotReturnNull()
        {
            // Arrange
            var document = DynamicXContainer.Parse(CreateTemplateXml());

            // Act
            var result = document.Document;

            // Assert
            Assert.NotNull(result, "Did not return root element from xml.");
        }

        [Test]
        public void QueryingProperties_GivenValidXml_ShouldReturnAttributeValue()
        {
            // Arrange
            var document = DynamicXContainer.Parse(CreateTemplateXml());

            // Act
            var result = document.Document.Type;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual("Simple", result);
        }

        [Test]
        public void QueryingPropertiesOfProperties_GivenValidXml_ShouldReturnAttributeValue()
        {
            // Arrange
            var document = DynamicXContainer.Parse(CreateTemplateXml());

            // Act
            var result = document.Document.Response.Data;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual("Hello", result);
        }

        [Test]
        public void QueryingExistenceOfProperty_GivenXmlContainingAValue_ShouldReturnTrue()
        {
            // Arrange
            var document = DynamicXContainer.Parse(CreateTemplateXml());

            // Act
            bool result = document.HasDocument;

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void QueryingExistenceOfProperty_GivenXmlNotContainingAValue_ShouldReturnFalse()
        {
            // Arrange
            var document = DynamicXContainer.Parse(CreateTemplateXml());

            // Act
            bool result = document.HasRequest;

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void QueryingForEveryInstanceOfAGivenProperty_GivenValidXmlWithElementsForThatProperty_ShouldReturnEnumerable()
        {
            // Arrange
            var document = DynamicXContainer.Parse(CreateTemplateXml());

            // Act
            IEnumerable<dynamic> result = document.Document.EveryResponse;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(2, result.Count());
        }

        private static string CreateTemplateXml()
        {
            return @"<Document Type=""Simple""><Response Data=""Hello"" /><Response Data=""Goodbye"" /></Document>";
        }
    }
}