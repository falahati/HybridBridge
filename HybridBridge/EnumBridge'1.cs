using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HybridBridge.Exceptions;
using Newtonsoft.Json.Linq;

namespace HybridBridge
{
    /// <summary>
    ///     A generic handler to be used to make C# enumerable types accessible
    ///     from the Javascript side
    /// </summary>
    /// <typeparam name="T">
    ///     The enumerable type to be reflected on the Javascript side
    /// </typeparam>
    public class EnumBridge<T> : IBridgeHandler where T : struct, IConvertible
    {
        // ReSharper disable once StaticMemberInGenericType
        /// <summary>
        ///     A cached list of enumerable fields for this type
        /// </summary>
        protected static List<FieldInfo> Fields;

        // ReSharper disable once StaticMemberInGenericType
        /// <summary>
        ///     A static object to be used as thread lock when working with other static fields
        /// </summary>
        protected static readonly object Lock = new object();

        // ReSharper disable once StaticMemberInGenericType
        /// <summary>
        ///     A cached list of public and non generic methods for this type
        /// </summary>
        protected static Dictionary<MethodInfo, ParameterInfo[]> Methods;

        /// <summary>
        ///     Creates a new instance of this class
        /// </summary>
        /// <exception cref="InvalidGenericTypeException">Indicates that the passed generic type is not a enumerable</exception>
        public EnumBridge()
        {
            if (!GenericType.IsEnum)
            {
                throw new InvalidGenericTypeException();
            }
            lock (Lock)
            {
                if (Methods == null)
                {
                    Methods = GenericType.GetMethods()
                        .Where(
                            info =>
                                    info.IsStatic && info.IsPublic && !info.IsGenericMethod)
                        .ToDictionary(info => info, info => info.GetParameters());
                }
                if (Fields == null)
                {
                    Fields = GenericType.GetFields().Where(info => info.IsStatic).ToList();
                }
            }
        }

        /// <summary>
        ///     Returns a string containing the name of the proxy enumerable object accessible from the Javascript side
        /// </summary>
        public static string Identification { get; } = typeof(T).FullName.Replace('+', '.');

        /// <summary>
        ///     Returns the generic type used to create this class
        /// </summary>
        public static Type GenericType { get; } = typeof(T);

        /// <summary>
        ///     Handles the passed request and returns the result
        /// </summary>
        /// <param name="method">The method name to handle</param>
        /// <param name="parameters">The method parameters</param>
        /// <param name="hasResult">A boolean value indicting if the handling process resulted in a value</param>
        /// <returns>Returns the value that created from the handling of the request</returns>
        public virtual object InterceptRequest(string method, Dictionary<string, object> parameters, out bool hasResult)
        {
            hasResult = false;
            MethodInfo methodInfo = null;
            ParameterInfo[] parametersInfo = null;
            lock (Lock)
            {
                if (Methods != null)
                {
                    methodInfo =
                        Methods.FirstOrDefault(
                            pair => pair.Key.Name.Equals(method, StringComparison.OrdinalIgnoreCase)).Key;
                    parametersInfo = Methods[methodInfo];
                }
            }
            if ((methodInfo != null) && (parametersInfo != null))
            {
                var methodParameters = parametersInfo.Where(
                        parameterInfo => parameters.Keys.Contains(parameterInfo.Name))
                    .Select(
                        parameterInfo => parameters[parameterInfo.Name] is JObject
                            ? ((JObject) parameters[parameterInfo.Name]).ToObject(parameterInfo.ParameterType)
                            : Convert.ChangeType(parameters[parameterInfo.Name], parameterInfo.ParameterType, null))
                    .ToList();
                hasResult = methodInfo.ReturnType != typeof(void);
                return methodInfo.Invoke(null, methodParameters.ToArray());
            }
            return null;
        }

        /// <summary>
        ///     Checks the request and returns true if this request can be handled by this handler
        /// </summary>
        /// <param name="handler">The name of requested handler</param>
        /// <param name="method">The method name to handle</param>
        /// <returns>true, if this handler can handle this request, false otherwise</returns>
        public virtual bool ShouldInterceptRequest(string handler, string method)
        {
            return handler.Equals(Identification, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     Event that gets raised when handler needs to push some Javascript code
        /// </summary>
        public event PushJavascriptEvent PushJavascript;

        /// <summary>
        ///     Initialize the handler and generates the needed Javascript code
        /// </summary>
        /// <param name="bridge">The <see cref="BridgeController" /> object requesting initialization</param>
        public virtual void Initialize(BridgeController bridge)
        {
            var builder = new StringBuilder();
            builder.Append(ClassBridge.GenerateNameSpace(GenericType));
            builder.Append(ClassBridge.GenerateProxyClass(Identification, true));
            lock (Lock)
            {
                if (Methods != null)
                {
                    if (Fields != null)
                    {
                        foreach (var field in Fields)
                        {
                            builder.Append(ClassBridge.GenerateProxyField(Identification, field));
                        }
                    }
                    foreach (var method in Methods)
                    {
                        builder.Append(ClassBridge.GenerateProxyMethod(Identification, method.Key, method.Value));
                    }
                }
            }
            OnPushJavascript(new FireJavascriptEventArgs(builder.ToString(), bridge));
        }

        /// <summary>
        ///     Raises the <see cref="PushJavascript" /> event using provided arguments
        /// </summary>
        /// <param name="eventArgs">The arguments to be used to raise <see cref="PushJavascript" /> event</param>
        /// <returns>Returns the result of raising <see cref="PushJavascript" /> event</returns>
        protected virtual object OnPushJavascript(PushJavascriptEventArgs eventArgs)
        {
            return PushJavascript?.Invoke(this, eventArgs);
        }
    }
}