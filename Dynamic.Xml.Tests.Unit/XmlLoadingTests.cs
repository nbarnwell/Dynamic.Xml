using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
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

    public class DynamicXContainer : DynamicObject
    {
        public static dynamic Parse(string xml)
        {
            return new DynamicXContainer(XDocument.Parse(xml));
        }

        private readonly XContainer _container;

        private DynamicXContainer(XContainer container)
        {
            _container = container;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (binder.Name.StartsWith("Has"))
            {
                string elementName = binder.Name.Substring(3);
                result = GetAttributes(elementName).Any() || GetElements(elementName).Any();
            }
            else if (binder.Name.StartsWith("Every"))
            {
                string elementName = binder.Name.Substring(5);
                result = GetElements(elementName);
            }
            else
            {
                var attribute = GetAttributes(binder.Name).FirstOrDefault();

                if (attribute != null)
                {
                    result = attribute.Value;
                }
                else
                {
                    result = GetElements(binder.Name).FirstOrDefault();
                    if (result == null)
                    {
                        throw new XmlException(string.Format("There is no \"{0}\" attribute or element in the following XML:\r\n\r\n{1}", binder.Name, _container));
                    }
                }
            }

            return result != null;
        }

        private IEnumerable<XAttribute> GetAttributes(string attributeName)
        {
            var element = _container as XElement;

            if (element == null) return Enumerable.Empty<XAttribute>();

            return element.Attributes()
                .Where(a => a.Name.ToString().Equals(attributeName, StringComparison.InvariantCultureIgnoreCase))
                .Select(a => a);
        }

        private IEnumerable<DynamicXContainer> GetElements(string elementName)
        {
            return _container.Elements()
                .Where(e => e.Name.ToString().Equals(elementName, StringComparison.InvariantCultureIgnoreCase))
                .Select(e => new DynamicXContainer(e));
        }

    }
}