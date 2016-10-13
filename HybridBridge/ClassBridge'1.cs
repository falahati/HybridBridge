using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HybridBridge.Exceptions;
using HybridBridge.InstancePool;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HybridBridge
{
    /// <summary>
    ///     A generic handler to be used to make C# classes accessible from the
    ///     Javascript side
    /// </summary>
    /// <typeparam name="T">
    ///     The generic class to be reflected on the Javascript side
    /// </typeparam>
    public class ClassBridge<T> : IBridgeHandler, IDisposable where T : class
    {
        // ReSharper disable once StaticMemberInGenericType
        /// <summary>
        ///     A cached list of public constructors for this type
        /// </summary>
        protected static Dictionary<ConstructorInfo, ParameterInfo[]> Constructors;

        // ReSharper disable once StaticMemberInGenericType
        /// <summary>
        ///     A cached list of public events for this type
        /// </summary>
        protected static List<EventInfo> Events;

        // ReSharper disable once StaticMemberInGenericType
        /// <summary>
        ///     A cached list of fields for this type
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

        // ReSharper disable once StaticMemberInGenericType
        /// <summary>
        ///     A cached list of public properties for this type
        /// </summary>
        protected static List<PropertyInfo> Properties;

        // ReSharper disable once StaticMemberInGenericType
        /// <summary>
        ///     A list of public and not generic sub classes
        /// </summary>
        protected static List<IBridgeHandler> SubClasses;

        // ReSharper disable once StaticMemberInGenericType
        /// <summary>
        ///     A cached list of public sub enumerations types for this type
        /// </summary>
        protected static List<IBridgeHandler> SubEnumerations;

        /// <summary>
        ///     A list containing all registered instances of the generic type to be accessible from the other side
        /// </summary>
        protected readonly InstanceDictionary<List<string>> Instances = new InstanceDictionary<List<string>>();

        /// <summary>
        ///     A list containing all generated delegates for each instance's events
        /// </summary>
        protected readonly Dictionary<T, Dictionary<EventInfo, Delegate>> InstancesEventsDelegates =
            new Dictionary<T, Dictionary<EventInfo, Delegate>>();

        /// <summary>
        ///     A list containing all generated delegates for each static events
        /// </summary>
        protected readonly Dictionary<EventInfo, Delegate> StaticEventsDelegates = new Dictionary<EventInfo, Delegate>();

        /// <summary>
        ///     Indicates if the class already disposed
        /// </summary>
        protected bool Disposed;

        /// <summary>
        ///     Creates a new instance of this class
        /// </summary>
        /// <exception cref="InvalidGenericTypeException">Indicates that the passed generic type is not a valid class</exception>
        public ClassBridge()
        {
            if (!GenericType.IsClass)
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
                                    info.IsPublic && !info.IsGenericMethod)
                        .ToDictionary(info => info, info => info.GetParameters());
                }
                if (Constructors == null)
                {
                    Constructors = GenericType.GetConstructors()
                        .Where(
                            info =>
                                    info.IsPublic && !info.IsGenericMethod)
                        .ToDictionary(info => info, info => info.GetParameters());
                }
                if (Properties == null)
                {
                    Properties =
                        GenericType.GetProperties()
                            .Where(
                                info =>
                                    Methods.ContainsKey(info.GetGetMethod()) ||
                                    Methods.ContainsKey(info.GetSetMethod()))
                            .ToList();
                }
                if (Fields == null)
                {
                    Fields = GenericType.GetFields().Where(info => info.IsPublic).ToList();
                }
                if (Events == null)
                {
                    Events =
                        GenericType.GetEvents()
                            .Where(
                                info =>
                                    Methods.ContainsKey(info.GetAddMethod()) &&
                                    Methods.ContainsKey(info.GetRemoveMethod()))
                            .ToList();
                }
                foreach (
                    var info in
                    Events.Where(info => info.GetAddMethod().IsStatic || info.GetRemoveMethod().IsStatic))
                {
                    var eventInvokerReturn = info.EventHandlerType.GetMethod("Invoke").ReturnType;
                    if ((eventInvokerReturn == typeof(void)) || (eventInvokerReturn == typeof(object)))
                    {
                        var del = ClassBridge.CreateProxyDelegateForEvent(info, null, RaiseEvent);
                        StaticEventsDelegates.Add(info, del);
                        info.AddEventHandler(null, del);
                    }
                    else
                    {
                        var del = ClassBridge.CreateProxyDelegateForEvent(info, null,
                            (instance, eventName, isVoid, eventArgs) =>
                                ClassBridge.NormalizeVariable(RaiseEvent(instance, eventName, isVoid, eventArgs),
                                    eventInvokerReturn, false));
                        StaticEventsDelegates.Add(info, del);
                        info.AddEventHandler(null, del);
                    }
                }
                if (SubEnumerations == null)
                {
                    SubEnumerations =
                        GenericType.GetNestedTypes(BindingFlags.Public)
                            .Where(type => type.IsEnum)
                            .Select(EnumBridge.FromType)
                            .ToList();
                    foreach (var subEnum in SubEnumerations)
                    {
                        subEnum.PushJavascript += (sender, args) => OnPushJavascript(args);
                    }
                }
                if (SubClasses == null)
                {
                    SubClasses =
                        GenericType.GetNestedTypes(BindingFlags.Public)
                            .Where(type => type.IsClass && !typeof(Delegate).IsAssignableFrom(type))
                            .Select(ClassBridge.FromType)
                            .ToList();
                    foreach (var subClasses in SubClasses)
                    {
                        subClasses.PushJavascript += (sender, args) => OnPushJavascript(args);
                    }
                }
            }
        }

        /// <summary>
        ///     Returns an array containing all registered instances
        /// </summary>
        public virtual T[] RegisteredInstances => Instances.Keys.Select(o => o as T).Where(t => t != null).ToArray();

        /// <summary>
        ///     Returns a string containing the name of the proxy class accessible from the Javascript side
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
            ConstructorInfo constructorInfo = null;
            var methodParameters = new object[0];
            FieldInfo fieldInfo = null;
            object fieldValue = null;
            object classInstance = null;
            var methodParts = method.Split('/');
            if (methodParts.Length > 1)
            {
                method = string.Join("/", methodParts, 1, methodParts.Length - 1);
                classInstance =
                    Instances.FirstOrDefault(
                        instance => GlobalPool.GetInstanceId(instance.Key) == methodParts[0]).Key;
            }
            lock (Lock)
            {
                if (string.IsNullOrWhiteSpace(method))
                {
                    if ((Constructors != null) && parameters.ContainsKey("arguments"))
                    {
                        var constructorParameters = parameters["arguments"] as JArray;
                        foreach (var pair in Constructors)
                        {
                            var matchedArguments = 0;
                            try
                            {
                                var param = new List<object>();
                                for (var i = 0; i < pair.Value.Length; i++)
                                {
                                    if ((constructorParameters != null) && (i < constructorParameters.Count))
                                    {
                                        param.Add(ClassBridge.NormalizeVariable(constructorParameters.ToArray()[i],
                                            pair.Value[i].ParameterType, true));
                                        matchedArguments++;
                                    }
                                    else
                                    {
                                        if (pair.Value[i].IsOptional)
                                        {
                                            param.Add(pair.Value[i].DefaultValue);
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }
                                if ((param.Count != pair.Value.Length) ||
                                    (matchedArguments != (constructorParameters?.Count ?? 0)))
                                {
                                    continue;
                                }
                                methodParameters = param.ToArray();
                            }
                            catch (InvalidCastException)
                            {
                                // ignore
                            }
                            constructorInfo = pair.Key;
                        }
                    }
                }
                else
                {
                    if (Methods != null)
                    {
                        foreach (var pair in Methods.Where(pair => pair.Key.Name.Equals(method)))
                        {
                            var matchedArguments = 0;
                            try
                            {
                                var param = new List<object>();
                                for (var i = 0; i < pair.Value.Length; i++)
                                {
                                    if (i < parameters.Count)
                                    {
                                        if (pair.Value[i].Name == parameters.Keys.ToArray()[i])
                                        {
                                            param.Add(ClassBridge.NormalizeVariable(parameters.Values.ToArray()[i],
                                                pair.Value[i].ParameterType, true));
                                            matchedArguments++;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (pair.Value[i].IsOptional)
                                        {
                                            param.Add(pair.Value[i].DefaultValue);
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }
                                if ((param.Count != pair.Value.Length) || (matchedArguments != parameters.Count))
                                {
                                    continue;
                                }
                                methodParameters = param.ToArray();
                            }
                            catch (InvalidCastException)
                            {
                                // ignore
                            }
                            methodInfo = pair.Key;
                        }
                    }
                    if ((methodInfo == null) && (Fields != null))
                    {
                        foreach (
                            var field in
                            Fields.Where(info => info.Name.Equals(method, StringComparison.OrdinalIgnoreCase)))
                        {
                            if (parameters.Keys.Contains("value"))
                            {
                                try
                                {
                                    fieldValue = ClassBridge.NormalizeVariable(parameters["value"], field.FieldType,
                                        true);
                                }
                                catch (InvalidCastException)
                                {
                                    // ignore
                                }
                            }
                            fieldInfo = field;
                        }
                    }
                }
            }
            if (constructorInfo != null)
            {
                var newObject = constructorInfo.Invoke(methodParameters) as T;
                if (newObject != null)
                {
                    AddInstance(newObject);
                    var instanceId = GlobalPool.GetInstanceId(newObject);
                    if (!string.IsNullOrWhiteSpace(instanceId))
                    {
                        hasResult = true;
                        return instanceId;
                    }
                }
                return null;
            }
            if (methodInfo != null)
            {
                if (methodInfo.ReturnType != typeof(void))
                {
                    hasResult = true;
                    return methodInfo.Invoke(classInstance, methodParameters.ToArray());
                }
                // Let's not block the UI/Javascript thread if there is no result for the requested method
                Task.Factory.StartNew(() => methodInfo.Invoke(classInstance, methodParameters.ToArray()));
                return null;
            }
            if (fieldInfo != null)
            {
                if (fieldValue != null)
                {
                    fieldInfo.SetValue(classInstance, fieldValue);
                    return null;
                }
                hasResult = true;
                return fieldInfo.GetValue(classInstance);
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
            builder.Append(ClassBridge.GenerateProxyClass(Identification, false));
            lock (Lock)
            {
                if (Fields != null)
                {
                    foreach (var field in Fields)
                    {
                        builder.Append(ClassBridge.GenerateProxyField(Identification, field));
                    }
                }
                if (Methods != null)
                {
                    if (Properties != null)
                    {
                        foreach (var property in Properties)
                        {
                            var get = property.GetGetMethod();
                            if ((get != null) && !Methods.ContainsKey(get))
                            {
                                get = null;
                            }
                            var set = property.GetSetMethod();
                            if ((set != null) && !Methods.ContainsKey(set))
                            {
                                set = null;
                            }
                            builder.Append(ClassBridge.GenerateProxyProperty(Identification, property, get, set));
                        }
                    }
                    foreach (
                        var method in
                        Methods.Where(
                            pair =>
                                (Events == null) ||
                                !Events.Any(
                                    info => (pair.Key == info.GetAddMethod()) || (pair.Key == info.GetRemoveMethod()))))
                    {
                        builder.Append(ClassBridge.GenerateProxyMethod(Identification, method.Key, method.Value));
                    }
                }
                if (Events != null)
                {
                    foreach (var info in Events)
                    {
                        var isStatic = info.GetAddMethod().IsStatic || info.GetRemoveMethod().IsStatic;
                        builder.Append(ClassBridge.GenerateProxyField(Identification, info.Name, isStatic, true,
                            new object[0]));
                        builder.Append(ClassBridge.GenerateProxyEventMethods(Identification, info, isStatic));
                    }
                }
            }
            foreach (var instance in Instances.Where(pair => pair.Key != null).ToArray())
            {
                var instanceId = GlobalPool.GetInstanceId(instance.Key);
                if (instanceId != null)
                {
                    builder.AppendLine(ClassBridge.GenerateInstanceChange(Identification, instanceId, false));
                    foreach (var variableName in instance.Value.Where(s => !string.IsNullOrWhiteSpace(s)))
                    {
                        builder.AppendLine(ClassBridge.GenerateInstanceVariable(Identification, instanceId, variableName));
                    }
                }
            }
            OnPushJavascript(new FireJavascriptEventArgs(builder.ToString(), bridge));
            lock (Lock)
            {
                if (SubEnumerations != null)
                {
                    foreach (var subEnum in SubEnumerations)
                    {
                        subEnum.Initialize(bridge);
                    }
                }
                if (SubClasses != null)
                {
                    foreach (var subClass in SubClasses)
                    {
                        subClass.Initialize(bridge);
                    }
                }
            }
        }


        /// <summary>
        ///     The method to dispose this class
        /// </summary>
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     The method to dispose this class
        /// </summary>
        ~ClassBridge()
        {
            Dispose(false);
        }

        /// <summary>
        ///     The method to dispose this class
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
                return;
            Disposed = true;
            if (disposing)
            {
                foreach (var eventDelegate in StaticEventsDelegates)
                {
                    eventDelegate.Key.RemoveEventHandler(null, eventDelegate.Value);
                }
                StaticEventsDelegates.Clear();
                foreach (var instance in Instances.ToArray())
                {
                    var strongTypeInstance = instance.Key as T;
                    if (strongTypeInstance != null)
                    {
                        RemoveInstance(strongTypeInstance);
                    }
                }
                Instances.Clear();
            }
        }

        /// <summary>
        ///     Returns an instance of the generic type from global pool using instance id
        /// </summary>
        /// <param name="instanceId">The identification string of the instance</param>
        /// <returns>The registered instance</returns>
        public static T GetInstance(string instanceId)
        {
            return GlobalPool.GetInstance<T>(instanceId);
        }

        /// <summary>
        ///     Handles the events raised by public events of the generic class and its instances
        /// </summary>
        /// <param name="instance">The instance that raised the event</param>
        /// <param name="eventName">The name of the raised event</param>
        /// <param name="isVoid">Indicates if the raised event has a return value</param>
        /// <param name="eventArgs">An array containing the parameters of the raised event</param>
        /// <returns>Returns the result of raising registered delegates</returns>
        protected virtual object RaiseEvent(object instance, string eventName, bool isVoid, object[] eventArgs)
        {
            if (instance != null)
            {
                if (Instances.ContainsKey(instance))
                {
                    var instanceId = GlobalPool.GetInstanceId(instance);
                    if (instanceId != null)
                    {
                        return
                            OnPushJavascript(
                                new PushJavascriptEventArgs(
                                    // ReSharper disable once UseStringInterpolation
                                    string.Format("{0}.__instances[\"{1}\"].raise_{2}({3});",
                                        Identification, instanceId, eventName,
                                        string.Join(", ", eventArgs.Select(JsonConvert.SerializeObject))))
                                {
                                    FireAway = isVoid
                                });
                    }
                }
                return null;
            }
            return
                OnPushJavascript(
                    // ReSharper disable once UseStringInterpolation
                    new PushJavascriptEventArgs(string.Format("{0}.raise_{1}({2});", Identification, eventName,
                        string.Join(", ", eventArgs.Select(JsonConvert.SerializeObject)))) {FireAway = isVoid});
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

        /// <summary>
        ///     Registers an instance of the generic type and adds it to the list of registered instances
        /// </summary>
        /// <param name="instance">The instance of the generic type to ad</param>
        /// <returns>Returns <see langword="this" /> instance to be used for other operations</returns>
        public virtual ClassBridge<T> AddInstance(T instance)
        {
            return AddInstance(instance, null);
        }

        /// <summary>
        ///     Registers an instance of the generic type and adds it to the list of registered instances
        /// </summary>
        /// <param name="instance">The instance of the generic type to ad</param>
        /// <param name="variableName">The name of the variable that is accessible from the Javascript side</param>
        /// <returns>Returns <see langword="this" /> instance to be used for other operations</returns>
        public virtual ClassBridge<T> AddInstance(T instance, string variableName)
        {
            if (instance != null)
            {
                if (!Instances.ContainsKey(instance))
                {
                    Instances.Add(instance, new List<string>());
                    if (!InstancesEventsDelegates.ContainsKey(instance))
                    {
                        InstancesEventsDelegates.Add(instance, new Dictionary<EventInfo, Delegate>());
                    }
                    foreach (
                        var info in
                        Events.Where(info => !info.GetAddMethod().IsStatic && !info.GetRemoveMethod().IsStatic))
                    {
                        var eventDelegate =
                            InstancesEventsDelegates[instance].FirstOrDefault(pair => pair.Key == info).Value;
                        if (eventDelegate == null)
                        {
                            var eventInvokerReturn = info.EventHandlerType.GetMethod("Invoke").ReturnType;
                            if ((eventInvokerReturn == typeof(void)) || (eventInvokerReturn == typeof(object)))
                            {
                                eventDelegate = ClassBridge.CreateProxyDelegateForEvent(info, instance, RaiseEvent);
                            }
                            else
                            {
                                eventDelegate = ClassBridge.CreateProxyDelegateForEvent(info, instance,
                                    (o, s, arg3, arg4) =>
                                        ClassBridge.NormalizeVariable(RaiseEvent(o, s, arg3, arg4), eventInvokerReturn,
                                            false));
                            }
                            InstancesEventsDelegates[instance].Add(info, eventDelegate);
                        }
                        info.AddEventHandler(instance, eventDelegate);
                    }
                    OnPushJavascript(
                        new FireJavascriptEventArgs(ClassBridge.GenerateInstanceChange(Identification,
                            GlobalPool.GetInstanceId(instance), false)));
                }
            }
            if (!string.IsNullOrWhiteSpace(variableName))
            {
                foreach (
                    var key in
                    Instances.Keys.Where(key => key != instance)
                        .Where(key => Instances[key].Contains(variableName))
                        .ToArray())
                {
                    RemoveInstance(variableName, key as T);
                }
                if (instance != null)
                {
                    if (!Instances[instance].Contains(variableName))
                    {
                        Instances[instance].Add(variableName);
                        OnPushJavascript(
                            new FireJavascriptEventArgs(ClassBridge.GenerateInstanceVariable(Identification,
                                GlobalPool.GetInstanceId(instance), variableName)));
                    }
                }
                else
                {
                    OnPushJavascript(
                        new FireJavascriptEventArgs(ClassBridge.GenerateInstanceVariable(Identification, null,
                            variableName)));
                }
            }
            return this;
        }

        /// <summary>
        ///     Removes the requested instances by variable name
        /// </summary>
        /// <param name="variableNames">The variable name of the instances to be removed from Javascript side</param>
        /// <returns>Returns <see langword="this" /> instance to be used for other operations</returns>
        public virtual ClassBridge<T> RemoveInstance(params string[] variableNames)
        {
            foreach (var variableName in variableNames)
            {
                foreach (
                    var key in
                    Instances.Keys.Where(key => Instances[key].Contains(variableName))
                        .ToArray())
                {
                    RemoveInstance(variableName, key as T);
                }
            }
            return this;
        }

        /// <summary>
        ///     Removes the requested instances
        /// </summary>
        /// <param name="instances">The instances to be removed from the Javascript side</param>
        /// <returns>Returns <see langword="this" /> instance to be used for other operations</returns>
        public virtual ClassBridge<T> RemoveInstance(params T[] instances)
        {
            foreach (var instance in instances.Where(instance => Instances.ContainsKey(instance)))
            {
                foreach (var variable in Instances[instance].ToArray())
                {
                    AddInstance(null, variable);
                }
                if (InstancesEventsDelegates.ContainsKey(instance))
                {
                    foreach (var eventDelegate in InstancesEventsDelegates[instance])
                    {
                        eventDelegate.Key.RemoveEventHandler(instance, eventDelegate.Value);
                    }
                    InstancesEventsDelegates.Remove(instance);
                }
                Instances.Remove(instance);
                OnPushJavascript(
                    new FireJavascriptEventArgs(ClassBridge.GenerateInstanceChange(Identification,
                        GlobalPool.GetInstanceId(instance), true)));
            }
            return this;
        }

        /// <summary>
        ///     Removed the requested instance and its variable
        /// </summary>
        /// <param name="variableName">The variable name of the instance to be removed from Javascript side</param>
        /// <param name="instance">The instance to be removed from the Javascript side</param>
        /// <returns>Returns <see langword="this" /> instance to be used for other operations</returns>
        public virtual ClassBridge<T> RemoveInstance(string variableName, T instance)
        {
            if (Instances.ContainsKey(instance))
            {
                if (Instances[instance].Contains(variableName))
                {
                    Instances[instance].Remove(variableName);
                    AddInstance(null, variableName);
                }
                if (Instances[instance].Count == 0)
                {
                    RemoveInstance(instance);
                }
            }
            return this;
        }
    }
}