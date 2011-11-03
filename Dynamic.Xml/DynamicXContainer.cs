using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Dynamic.Xml
{
    public class DynamicXContainer : DynamicObject
    {
        private readonly XContainer _container;

        public static dynamic Parse(string xml)
        {
            return new DynamicXContainer(XDocument.Parse(xml));
        }

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