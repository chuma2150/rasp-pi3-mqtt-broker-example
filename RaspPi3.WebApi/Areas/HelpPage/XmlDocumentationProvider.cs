using RaspPi3.WebApi.Areas.HelpPage.ModelDescriptions;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Xml.XPath;

namespace RaspPi3.WebApi.Areas.HelpPage
{
    /// <summary>
    /// A custom <see cref="IDocumentationProvider"/> that reads the API documentation from an XML documentation file.
    /// </summary>
    public class XmlDocumentationProvider : IDocumentationProvider, IModelDocumentationProvider
    {
        private readonly XPathNavigator _documentNavigator;
        private const string TypeExpression = "/doc/members/member[@name='T:{0}']";
        private const string MethodExpression = "/doc/members/member[@name='M:{0}']";
        private const string PropertyExpression = "/doc/members/member[@name='P:{0}']";
        private const string FieldExpression = "/doc/members/member[@name='F:{0}']";
        private const string ParameterExpression = "param[@name='{0}']";

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlDocumentationProvider"/> class.
        /// </summary>
        /// <param name="documentPath">The physical path to XML document.</param>
        public XmlDocumentationProvider(string documentPath)
        {
            if (documentPath == null)
            {
                throw new ArgumentNullException(nameof(documentPath));
            }
            var xpath = new XPathDocument(documentPath);
            _documentNavigator = xpath.CreateNavigator();
        }

        public string GetDocumentation(HttpControllerDescriptor controllerDescriptor)
        {
            var typeNode = GetTypeNode(controllerDescriptor.ControllerType);
            return GetTagValue(typeNode, "summary");
        }

        public virtual string GetDocumentation(HttpActionDescriptor actionDescriptor)
        {
            var methodNode = GetMethodNode(actionDescriptor);
            return GetTagValue(methodNode, "summary");
        }

        public virtual string GetDocumentation(HttpParameterDescriptor parameterDescriptor)
        {
            if (parameterDescriptor is not ReflectedHttpParameterDescriptor reflectedParameterDescriptor)
            {
                return null;
            }

            var methodNode = GetMethodNode(reflectedParameterDescriptor.ActionDescriptor);
            if (methodNode == null)
            {
                return null;
            }

            var parameterName = reflectedParameterDescriptor.ParameterInfo.Name;
            var parameterNode = methodNode.SelectSingleNode(String.Format(CultureInfo.InvariantCulture, ParameterExpression, parameterName));
            return parameterNode?.Value.Trim();
        }

        public string GetResponseDocumentation(HttpActionDescriptor actionDescriptor)
        {
            var methodNode = GetMethodNode(actionDescriptor);
            return GetTagValue(methodNode, "returns");
        }

        public string GetDocumentation(MemberInfo member)
        {
            var memberName = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", GetTypeName(member.DeclaringType), member.Name);
            var expression = member.MemberType == MemberTypes.Field ? FieldExpression : PropertyExpression;
            var selectExpression = String.Format(CultureInfo.InvariantCulture, expression, memberName);
            var propertyNode = _documentNavigator.SelectSingleNode(selectExpression);
            return GetTagValue(propertyNode, "summary");
        }

        public string GetDocumentation(Type type)
        {
            var typeNode = GetTypeNode(type);
            return GetTagValue(typeNode, "summary");
        }

        private XPathNavigator GetMethodNode(HttpActionDescriptor actionDescriptor)
        {
            if (actionDescriptor is not ReflectedHttpActionDescriptor reflectedActionDescriptor)
            {
                return null;
            }

            var selectExpression = string.Format(CultureInfo.InvariantCulture, MethodExpression, GetMemberName(reflectedActionDescriptor.MethodInfo));
            return _documentNavigator.SelectSingleNode(selectExpression);
        }

        private static string GetMemberName(MethodInfo method)
        {
            var name = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", GetTypeName(method.DeclaringType), method.Name);
            var parameters = method.GetParameters();
            if (parameters.Length == 0)
            {
                return name;
            }

            var parameterTypeNames = parameters.Select(param => GetTypeName(param.ParameterType)).ToArray();
            name += string.Format(CultureInfo.InvariantCulture, "({0})", string.Join(",", parameterTypeNames));

            return name;
        }

        private static string GetTagValue(XPathNavigator parentNode, string tagName)
        {
            var node = parentNode?.SelectSingleNode(tagName);
            return node?.Value.Trim();
        }

        private XPathNavigator GetTypeNode(Type type)
        {
            var controllerTypeName = GetTypeName(type);
            var selectExpression = String.Format(CultureInfo.InvariantCulture, TypeExpression, controllerTypeName);
            return _documentNavigator.SelectSingleNode(selectExpression);
        }

        private static string GetTypeName(Type type)
        {
            var name = type.FullName;
            if (type.IsGenericType)
            {
                // Format the generic type name to something like: Generic{System.Int32,System.String}
                var genericType = type.GetGenericTypeDefinition();
                var genericArguments = type.GetGenericArguments();
                var genericTypeName = genericType.FullName;

                // Trim the generic parameter counts from the name
                genericTypeName = genericTypeName.Substring(0, genericTypeName.IndexOf('`'));
                var argumentTypeNames = genericArguments.Select(t => GetTypeName(t)).ToArray();
                name = string.Format(CultureInfo.InvariantCulture, "{0}{{{1}}}", genericTypeName, string.Join(",", argumentTypeNames));
            }
            if (type.IsNested)
            {
                // Changing the nested type name from OuterType+InnerType to OuterType.InnerType to match the XML documentation syntax.
                name = name.Replace("+", ".");
            }

            return name;
        }
    }
}
