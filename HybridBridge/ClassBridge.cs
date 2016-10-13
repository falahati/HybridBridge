using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using HybridBridge.InstancePool;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HybridBridge
{
    /// <summary>
    ///     Contains static, reusable and non generic methods for the
    ///     ClassBridge class
    /// </summary>
    public static class ClassBridge
    {
        internal static string GenerateNameSpace(Type type)
        {
            return GenerateNameSpace(type.FullName.Replace('+', '.'), type.Name);
        }

        internal static string GenerateNameSpace(string identification, string className)
        {
            var allNs = string.Empty;
            var currentNs = string.Empty;
            var nameSpaceSplited = identification.Split('.').Where(s => !s.Equals(className)).ToArray();
            if (nameSpaceSplited.Length == 0)
            {
                return string.Empty;
            }
            foreach (var s in nameSpaceSplited)
            {
                if (string.IsNullOrEmpty(currentNs))
                {
                    allNs = allNs + $"if (typeof {s} === \"undefined\") {{\r\n   var {s} = {{}};\r\n}}\r\n";
                }
                else
                {
                    allNs = allNs + $"if (typeof {currentNs}{s} === \"undefined\") {{\r\n   {currentNs}{s} = {{}};\r\n}}\r\n";
                }
                currentNs += s + ".";
            }
            return allNs;
        }

        internal static Delegate CreateProxyDelegateForEvent(EventInfo eventInfo, object state,
            Func<object, string, bool, object[], object> methodToCall)
        {
            var eventHandler = eventInfo.EventHandlerType;
            var eventInvoker = eventHandler.GetMethod("Invoke");
            var eventInvokerParameters = eventInvoker.GetParameters();
            var eventInvokerReturn = eventInvoker.ReturnType;
            var isEventInvokerVoid = eventInvokerReturn == typeof (void);
            var expressionParameters = new List<ParameterExpression>();
            var methodArguments = new List<Expression>();
            foreach (
                var parameter in
                    eventInvokerParameters.Select(
                        parameterInfo => Expression.Parameter(parameterInfo.ParameterType, parameterInfo.Name)))
            {
                expressionParameters.Add(parameter);
                methodArguments.Add(Expression.Convert(parameter, typeof (object)));
            }
            methodArguments =
                new Expression[]
                {
                    Expression.Constant(state, typeof (object)),
                    Expression.Constant(eventInfo.Name, typeof (string)),
                    Expression.Constant(isEventInvokerVoid, typeof (bool))
                }.Concat(new Expression[] {Expression.NewArrayInit(typeof (object), methodArguments)}).ToList();
            var methodCall = methodToCall.Target == null
                ? Expression.Call(methodToCall.Method, methodArguments)
                : Expression.Call(Expression.Constant(methodToCall.Target), methodToCall.Method, methodArguments);
            var finalExpression = isEventInvokerVoid
                ? (Expression) Expression.Block(methodCall, Expression.Empty())
                : Expression.Convert(methodCall, eventInvokerReturn);
            var lambdaDelegate = Expression.Lambda(finalExpression, expressionParameters.ToArray()).Compile();
            return Delegate.CreateDelegate(eventHandler, lambdaDelegate,
                lambdaDelegate.GetType()
                    .GetMethod("Invoke", eventInvokerParameters.Select(parameter => parameter.ParameterType).ToArray()));
        }

        internal static string GenerateProxyMethod(string identification, MethodInfo method,
            ParameterInfo[] parameters,
            bool isAsync = false)
        {
            var result = GenerateProxyMethod(identification, method.Name,
                parameters.Select(info => new InternalParameterInfo(info)).ToArray(), method.IsStatic,
                method.ReturnType == typeof (void), isAsync);
            if (isAsync != true)
            {
                result += GenerateProxyMethod(identification, method, parameters, true);
            }
            return result;
        }

        internal static string GenerateInstanceVariable(string indentification, string instanceId, string variable)
        {
            return instanceId == null
                ? string.Format("var {0} = ({0} === undefined) ? undefined : null;", variable)
                // ReSharper disable once UseStringInterpolation
                : string.Format("var {0} = {1}.__fromInstanceId(\"{2}\");", variable, indentification, instanceId);
        }

        internal static string GenerateInstanceChange(string indentification, string instanceId, bool remove)
        {
            return
                string.Format(
                    remove
                        ? "\r\n{0}.__instances[\"{1}\"] = null;"
                        : "\r\n{0}.__instances[\"{1}\"] = {0}.__fromInstanceId(\"{1}\");", indentification,
                    instanceId);
        }

        internal static string GenerateProxyEventMethods(string identification, EventInfo eventInfo,
            bool isStatic)
        {
            var result = GenerateProxyMethod(identification, "add_" + eventInfo.Name,
                new[] {new InternalParameterInfo("callback")}, string.Format(@"    if (!{0}.{1}) {{
        {0}.{1} = var_callback;
    }} else if (Object.prototype.toString.call({0}.{1}) === ""[object Array]"") {{
        {0}.{1}.push(var_callback);
    }} else {{
        {0}.{1} = [{0}.{1}, var_callback];
    }}
    return true;", isStatic ? identification : "this", eventInfo.Name), isStatic);

            result += GenerateProxyMethod(identification, "remove_" + eventInfo.Name,
                new[] {new InternalParameterInfo("callback")}, string.Format(@"    if (!!{0}.{1}) {{
        if (Object.prototype.toString.call({0}.{1}) === ""[object Array]"") {{
            var index = {0}.{1}.indexOf(var_callback);
            if (index > -1) {{
                {0}.{1}.splice(index, 1);
                return true;
            }}
        }} else if ({0}.{1} == var_callback) {{
            {0}.{1} = [];
            return true;
        }}
    }}
    return false;", isStatic ? identification : "this", eventInfo.Name), isStatic);

            result += GenerateProxyMethod(identification, "raise_" + eventInfo.Name, new InternalParameterInfo[0],
                string.Format(@"    var result = undefined;
    if (!!{0}.{1}) {{
        if (Object.prototype.toString.call({0}.{1}) === ""[object Array]"") {{
            for (index = 0; index < {0}.{1}.length; ++index) {{
                result = {0}.{1}[index].apply({0}, Array.prototype.slice.call(arguments));
            }}
        }} else {{
            result = {0}.{1}.apply({0}, Array.prototype.slice.call(arguments));
        }}
    }}
    return JSON.stringify(result);", isStatic ? identification : "this", eventInfo.Name), isStatic);
            return result;
        }

        internal static string GenerateProxyMethod(string identification, string methodName,
            InternalParameterInfo[] parameters, string customBody, bool isStatic, bool isAsync = false)
        {
            // ReSharper disable once UseStringInterpolation
            var result = string.Format(@"
{0}.{1} = function ({2}) {{
",
                isStatic ? identification : identification + ".prototype",
                methodName + (isAsync ? @"Async" : ""),
                string.Join(", ",
                    parameters.Select(info => "var_" + info.Name)
                        .Concat(isAsync ? new[] {"callback"} : new string[0])
                        .ToArray()));
            if (!isStatic)
            {
                result +=
                    $@"    if (!(this instanceof {identification
                        }) || !this.__instanceId) {{
        return undefined;
    }}
";
            }
            foreach (var parameter in parameters)
            {
                if (parameter.IsOptional)
                {
                    var valueJson = JsonConvert.SerializeObject(parameter.DefaultValue);
                    result += string.Format(@"    if (var_{0} === undefined) {{
        var_{0} = {1};
    }}
",
                        parameter.Name, valueJson);
                }
            }
            if (isAsync)
            {
                result += @"    if (callback === undefined) {
        callback = function() { };
    }
";
            }

            result += customBody;
            result += @"
};";
            return result;
        }

        internal static string GenerateProxyMethod(string identification, string methodName,
            InternalParameterInfo[] parameters, bool isStatic, bool isVoid, bool isAsync = false)
        {
            // ReSharper disable once UseStringInterpolation
            var functionBody = string.Format("    {0}HybridBridge.__call({1}, \"{2}\", {{{3}}}, {4});",
                !isVoid && !isAsync ? "return " : "",
                !isStatic ? $"\"{identification}/\" + this.__instanceId" : $"\"{identification}\"",
                methodName,
                string.Join(", ",
                    parameters.Select(info => string.Format("\"{0}\": var_{0}", info.Name)).ToArray()),
                isAsync ? "callback" : "null");
            return GenerateProxyMethod(identification, methodName, parameters, functionBody, isStatic, isAsync);
        }


        internal static string GenerateProxyField(string identification, FieldInfo field)
        {
            var isConst = field.IsLiteral && !field.IsInitOnly;
            return GenerateProxyField(identification, field.Name, field.IsStatic, isConst,
                isConst ? field.GetValue(null) : null);
        }

        internal static string GenerateProxyField(string identification, string fieldName,
            bool isStatic, bool isConst, object value = null)
        {
            if (isConst)
            {
                // ReSharper disable once UseStringInterpolation
                return string.Format("\r\n{0}.{1} = {2};", isStatic ? identification : identification + ".prototype",
                    fieldName,
                    JsonConvert.SerializeObject(value));
            }
            // ReSharper disable once UseStringInterpolation
            var result = string.Format("\r\nObject.defineProperty({0}, \"{1}\", {{",
                isStatic ? identification : identification + ".prototype",
                fieldName);

            result += string.Format(@"
{0}    get: function () {{
{0}        return HybridBridge.__call({1}, ""{2}"", {{}}, null);
{0}    }}",
                !isStatic ? "    " : "",
                !isStatic ? $"\"{identification}/\" + this.__instanceId" : $"\"{identification}\"",
                fieldName);

            result += string.Format(@",
{0}    set: function (value) {{
{0}        HybridBridge.__call({1}, ""{2}"", {{""value"": value}}, null);
{0}    }}",
                !isStatic ? "    " : "",
                !isStatic ? $"\"{identification}/\" + this.__instanceId" : $"\"{identification}\"",
                fieldName);
            // ReSharper disable once UseStringInterpolation
            result += string.Format("\r\n{0}}});", !isStatic ? "    " : "");
            return result;
        }

        internal static string GenerateProxyClass(string identification, bool isStatic)
        {
            if (isStatic)
            {
                return $@"
{identification} = function () {{
    return undefined;
}};";
            }
            var result = string.Format(@"
{0} = function () {{
    var instanceId = HybridBridge.__call(""{0}"", """", {{""arguments"": Array.prototype.slice.call(arguments)}}, null);
    return {0}.__fromInstanceId(instanceId);
}};", identification);
            result += GenerateProxyField(identification, "__instanceId", false, true);
            result += GenerateProxyField(identification, "__instances", true, true, new Dictionary<string, object>());
            result += GenerateProxyMethod(identification, "__fromInstanceId",
                new[] {new InternalParameterInfo("instanceId")},
                string.Format(@"    if (!(typeof var_instanceId === ""string"" || var_instanceId instanceof String)) {{
        return undefined;
    }}
    if (!!{0}.__instances[var_instanceId]) {{
        return {0}.__instances[var_instanceId];
    }}
    var newObject =  Object.create({0}.prototype);
    newObject.__instanceId = var_instanceId;
    {0}.__instances[var_instanceId] = newObject;
    return newObject;", identification), true);
            return result;
        }

        internal static string GenerateProxyProperty(string identification, PropertyInfo property,
            MethodInfo get, MethodInfo set)
        {
            return GenerateProxyProperty(identification, property.Name, get?.Name, set?.Name,
                get?.IsStatic == true || set?.IsStatic == true);
        }

        internal static string GenerateProxyProperty(string identification, string propertyName,
            string getName, string setName, bool isStatic)
        {
            if (getName == null && setName == null)
            {
                return string.Empty;
            }
            // ReSharper disable once UseStringInterpolation
            var result = string.Format(@"
Object.defineProperty({0}, ""{1}"", {{",
                isStatic ? identification : identification + ".prototype",
                propertyName);

            if (getName != null)
            {
                // ReSharper disable once UseStringInterpolation
                result += string.Format(@"
    get: function () {{
        return {0}.{1}();
    }}",
                    isStatic ? identification : "this", getName);
            }
            else
            {
                result += @"
    get: function () {
        return undefined;
    }";
            }
            if (setName != null)
            {
                // ReSharper disable once UseStringInterpolation
                result += string.Format(@",
    set: function (value) {{
        {0}.{1}(value);
    }}",
                    isStatic ? identification : "this", setName);
            }
            else
            {
                result += @",
    set: function (value) {
        throw new InvalidOperationException();
    }";
            }
            result += @"
});";
            return result;
        }

        /// <summary>
        ///     Tries to normalize variable by searching InstancePool and converting of variable to the desired <see cref="Type" />
        /// </summary>
        /// <param name="variable">The variable to normalize</param>
        /// <returns>
        ///     New <see langword="object" /> created or casted from the variable in the desired <see cref="Type" />, or
        ///     <see langword="null" />
        /// </returns>
        public static T NormalizeVariable<T>(object variable)
        {
            return (T) (NormalizeVariable(variable, typeof (T), false) ?? default(T));
        }

        /// <summary>
        ///     Tries to normalize variable by searching InstancePool and converting of variable to the desired <see cref="Type" />
        /// </summary>
        /// <param name="variable">The variable to normalize</param>
        /// <param name="normalizeType">The <see cref="Type" /> to convert variable to</param>
        /// <param name="throwOnException">
        ///     A boolean value indicating if the method should throw an exception or just returns
        ///     <see langword="null" /> in case of errors
        /// </param>
        /// <returns>
        ///     New <see langword="object" /> created or casted from the variable in the desired <see cref="Type" />, or
        ///     <see langword="null" />
        /// </returns>
        /// <exception cref="InvalidCastException">
        ///     The passed variable can not be converted to the desired <see cref="Type" />
        /// </exception>
        public static object NormalizeVariable(object variable, Type normalizeType, bool throwOnException)
        {
            if (variable != null)
            {
                try
                {
                    var javaScriptClass = variable as JObject;
                    if (javaScriptClass != null)
                    {
                        var instanceIdProperty =
                            javaScriptClass.Property("__instanceId")?.Value?.ToString(Formatting.None).Trim('"', ' ');
                        if (!string.IsNullOrWhiteSpace(instanceIdProperty))
                        {
                            var instance = GlobalPool.GetInstance(instanceIdProperty);
                            if (instance != null && (normalizeType == null || normalizeType.IsInstanceOfType(instance)))
                            {
                                return instance;
                            }
                        }
                        return normalizeType == null
                            ? variable
                            : javaScriptClass.ToObject(normalizeType);
                    }
                    return normalizeType == null || normalizeType.IsInstanceOfType(variable)
                        ? variable
                        : Convert.ChangeType(variable, normalizeType, null);
                }
                catch (InvalidCastException)
                {
                    if (throwOnException)
                    {
                        throw;
                    }
                }
                catch (ArgumentException e)
                {
                    if (throwOnException)
                    {
                        throw new InvalidCastException(e.Message, e);
                    }
                }
            }
            return normalizeType != null && normalizeType.IsValueType ? Activator.CreateInstance(normalizeType) : null;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="ClassBridge" /> class with the specified <see cref="Type" /> as the class's
        ///     generic type
        /// </summary>
        /// <param name="genericType">The type to create a <see cref="ClassBridge" /> instance for</param>
        /// <returns>Newly created <see cref="ClassBridge" /> instance</returns>
        public static IBridgeHandler FromType(Type genericType)
        {
            return Activator.CreateInstance(typeof (ClassBridge<>).MakeGenericType(genericType)) as IBridgeHandler;
        }

        internal class InternalParameterInfo
        {
            public InternalParameterInfo(ParameterInfo info)
            {
                Name = info.Name;
                IsOptional = info.IsOptional;
                DefaultValue = info.DefaultValue;
                IsParam = info.ParameterType.IsArray &&
                          info.GetCustomAttributes(typeof (ParamArrayAttribute), false).Length > 0;
            }

            public InternalParameterInfo(string name)
            {
                Name = name;
                IsParam = false;
                IsOptional = false;
                DefaultValue = null;
            }

            public InternalParameterInfo(string name, bool isParam) : this(name)
            {
                IsParam = isParam;
            }

            public InternalParameterInfo(string name, bool isOptional, object defaultValue) : this(name)
            {
                IsOptional = isOptional;
                DefaultValue = defaultValue;
            }

            public string Name { get; }
            public bool IsOptional { get; }
            public object DefaultValue { get; }

            public bool IsParam { get; private set; }
        }
    }
}