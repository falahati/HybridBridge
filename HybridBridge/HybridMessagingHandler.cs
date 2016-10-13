using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybridBridge.InstancePool;

namespace HybridBridge
{
    /// <summary>
    ///     A class for event driven communicate with Javascript side
    /// </summary>
    public class HybridMessagingHandler : IBridgeHandler, IDisposable
    {
        /// <summary>
        ///     A static object to be used as thread lock when working with other static fields
        /// </summary>
        protected readonly object Lock = new object();

        /// <summary>
        ///     A list of all registered delegates
        /// </summary>
        protected readonly Dictionary<string, List<Delegate>> Subscriptions = new Dictionary<string, List<Delegate>>();

        /// <summary>
        ///     Indicates if the class already disposed
        /// </summary>
        protected bool Disposed;

        /// <summary>
        ///     An instance of the <see cref="ClassBridge" /> class for <see cref="ProxyClass" /> to be used as the communication
        ///     bridge
        /// </summary>
        protected ClassBridge<ProxyClass> HybridMessagingBridge;

        /// <summary>
        ///     An instance of the <see cref="ProxyClass" /> to be used for communication
        /// </summary>
        protected ProxyClass HybridMessagingProxy;

        /// <summary>
        ///     Creates a new instance of this class
        /// </summary>
        /// <param name="jsVariableName">
        ///     The name of the variable to be used from the Javascript side to access the HybridMessaging
        ///     object
        /// </param>
        public HybridMessagingHandler(string jsVariableName = "HybridMessaging")
        {
            HybridMessagingProxy = new ProxyClass(this);
            HybridMessagingBridge = new ClassBridge<ProxyClass>();
            HybridMessagingBridge.PushJavascript += OnPushJavascript;
            HybridMessagingBridge.AddInstance(HybridMessagingProxy, jsVariableName);
        }

        /// <summary>
        ///     Handles the passed request and returns the result
        /// </summary>
        /// <param name="method">The method name to handle</param>
        /// <param name="parameters">The method parameters</param>
        /// <param name="hasResult">A boolean value indicting if the handling process resulted in a value</param>
        /// <returns>Returns the value that created from the handling of the request</returns>
        public virtual object InterceptRequest(string method, Dictionary<string, object> parameters, out bool hasResult)
        {
            return HybridMessagingBridge.InterceptRequest(method, parameters, out hasResult);
        }

        /// <summary>
        ///     Checks the request and returns true if this request can be handled by this handler
        /// </summary>
        /// <param name="handler">The name of requested handler</param>
        /// <param name="method">The method name to handle</param>
        /// <returns>true, if this handler can handle this request, false otherwise</returns>
        public virtual bool ShouldInterceptRequest(string handler, string method)
        {
            return HybridMessagingBridge.ShouldInterceptRequest(handler, method);
        }

        /// <summary>
        ///     Initialize the handler and generates the needed Javascript code
        /// </summary>
        /// <param name="bridge">The <see cref="BridgeController" /> object requesting initialization</param>
        public virtual void Initialize(BridgeController bridge)
        {
            var identification = typeof (ProxyClass).FullName.Replace('+', '.');
            var builder = new StringBuilder();
            HybridMessagingBridge.Initialize(bridge);
            builder.Append(ClassBridge.GenerateProxyField(identification, "__subscriptions", false, true,
                new Dictionary<string, object>()));
            builder.Append(ClassBridge.GenerateProxyMethod(identification, "Subscribe",
                new[]
                {
                    new ClassBridge.InternalParameterInfo("messageString"),
                    new ClassBridge.InternalParameterInfo("callback")
                }, @"
    var_messageString = var_messageString.toLowerCase().trim();
    if (this.__subscriptions[var_messageString] === undefined) {
        this.__subscriptions[var_messageString] = [];
    }
    var index = this.__subscriptions[var_messageString].indexOf(var_callback);
    if (index < 0) {
        this.__subscriptions[var_messageString].push(var_callback);
        return true;
    }
    return false;", false));
            builder.Append(ClassBridge.GenerateProxyMethod(identification, "UnSubscribe",
                new[]
                {
                    new ClassBridge.InternalParameterInfo("messageString"),
                    new ClassBridge.InternalParameterInfo("callback")
                }, @"
    var_messageString = var_messageString.toLowerCase().trim();
    if (this.__subscriptions[var_messageString] === undefined) {
        return false;
    }
    var index = this.__subscriptions[var_messageString].indexOf(var_callback);
    if (index < 0) {
        return false;
    }
    this.__subscriptions[var_messageString].splice(index, 1);
    if (this.__subscriptions[var_messageString].length < 1) {
        this.__subscriptions[var_messageString] = undefined;
    }
    return true;", false));
            builder.Append(ClassBridge.GenerateProxyMethod(identification, "__raise",
                new[]
                {
                    new ClassBridge.InternalParameterInfo("messageString"),
                    new ClassBridge.InternalParameterInfo("arguments", true, null)
                }, @"
    var result = undefined;
    var_messageString = var_messageString.toLowerCase().trim();
    for (var messageString in this.__subscriptions) {
        if (!messageString || !var_messageString || messageString == var_messageString) {
            for (index = 0; index < this.__subscriptions[messageString].length; ++index) {
                result = this.__subscriptions[messageString][index].apply(this, [ var_arguments ]);
            }
        }
    }
    return result;", false));
            var instanceId = GlobalPool.GetInstanceId(HybridMessagingProxy);
            if (!string.IsNullOrEmpty(instanceId))
            {
                builder.AppendFormat("\r\n{0}.__instances[\"{1}\"].add_NewMessage({0}.__instances[\"{1}\"].__raise);",
                    identification, instanceId);
            }
            OnPushJavascript(this, new FireJavascriptEventArgs(builder.ToString()));
        }

        /// <summary>
        ///     Event that gets raised when handler needs to push some Javascript code
        /// </summary>
        public event PushJavascriptEvent PushJavascript;

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
        ~HybridMessagingHandler()
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
                if (HybridMessagingBridge != null)
                {
                    if (HybridMessagingProxy != null)
                    {
                        HybridMessagingBridge.RemoveInstance(HybridMessagingProxy);
                    }
                    HybridMessagingBridge.PushJavascript -= OnPushJavascript;
                }
                HybridMessagingProxy = null;
                HybridMessagingBridge = null;
            }
        }

        /// <summary>
        ///     Raises the <see cref="PushJavascript" /> event using provided arguments
        /// </summary>
        /// <param name="sender">The IBridgeHandler instance that raised the event</param>
        /// <param name="eventArgs">The arguments to be used to raise <see cref="PushJavascript" /> event</param>
        /// <returns>Returns the result of raising <see cref="PushJavascript" /> event</returns>
        protected virtual object OnPushJavascript(IBridgeHandler sender, PushJavascriptEventArgs eventArgs)
        {
            return PushJavascript?.Invoke(this, eventArgs);
        }

        /// <summary>
        ///     Sends a message to the Javascript side
        /// </summary>
        /// <param name="messageString">Message identification, or string.Empty to send to all subscribers</param>
        public virtual void Send(string messageString)
        {
            HybridMessagingProxy.HandlerNewMessage(messageString, null);
        }


        /// <summary>
        ///     Sends a message to the Javascript side and waits for the response
        /// </summary>
        /// <param name="messageString">Message identification, or string.Empty to send to all subscribers</param>
        /// <typeparam name="TResult">The type of the expected result</typeparam>
        /// <returns>Returns the response of the last subscriber</returns>
        public virtual TResult Send<TResult>(string messageString)
        {
            var returnValue = HybridMessagingProxy.HandlerNewMessage(messageString, null);
            try
            {
                return ClassBridge.NormalizeVariable<TResult>(returnValue);
            }
            catch (Exception)
            {
                return default(TResult);
            }
        }

        /// <summary>
        ///     Sends a message to the Javascript side
        /// </summary>
        /// <param name="messageString">Message identification, or string.Empty to send to all subscribers</param>
        /// <param name="argument">The argument to be send to the message subscriber</param>
        /// <typeparam name="TArgument">The type of the argument to send</typeparam>
        public virtual void Send<TArgument>(string messageString, TArgument argument)
        {
            HybridMessagingProxy.HandlerNewMessage(messageString, argument);
        }

        /// <summary>
        ///     Sends a message to the Javascript side and waits for the response
        /// </summary>
        /// <param name="messageString">Message identification, or string.Empty to send to all subscribers</param>
        /// <param name="argument">The argument to be send to the message subscriber</param>
        /// <typeparam name="TResult">The type of the expected result</typeparam>
        /// <typeparam name="TArgument">The type of the argument to send</typeparam>
        /// <returns>Returns the response of the last subscriber</returns>
        public virtual TResult Send<TResult, TArgument>(string messageString, TArgument argument)
        {
            var returnValue = HybridMessagingProxy.HandlerNewMessage(messageString, argument);
            try
            {
                return ClassBridge.NormalizeVariable<TResult>(returnValue);
            }
            catch (Exception)
            {
                return default(TResult);
            }
        }

        /// <summary>
        ///     Registers an callback method for the specified message string
        /// </summary>
        /// <param name="messageString">The message identification string to subscribe to</param>
        /// <param name="callback">The callback function to be called for the message</param>
        /// <returns><see langword="true" /> if the callback registered successfully, otherwise <see langword="false" /></returns>
        protected virtual bool Subscribe(string messageString, Delegate callback)
        {
            if (messageString == null)
            {
                return false;
            }
            messageString = messageString.ToLower().Trim();
            lock (Lock)
            {
                if (!Subscriptions.ContainsKey(messageString))
                {
                    Subscriptions.Add(messageString, new List<Delegate>());
                }
                if (!Subscriptions[messageString].Contains(callback))
                {
                    Subscriptions[messageString].Add(callback);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     Unregisters a callback method for the specified message string
        /// </summary>
        /// <param name="messageString">The message identification string to unsubscribe from</param>
        /// <param name="callback">The callback function registered for the message</param>
        /// <returns>
        ///     <see langword="true" /> if the callback unregistered successfully, otherwise <see langword="false" />
        /// </returns>
        protected virtual bool Unsubscribe(string messageString, Delegate callback)
        {
            if (messageString == null)
            {
                return false;
            }
            messageString = messageString.ToLower().Trim();
            lock (Lock)
            {
                if (!Subscriptions.ContainsKey(messageString))
                {
                    return false;
                }
                if (!Subscriptions[messageString].Contains(callback))
                {
                    return false;
                }
                if (!Subscriptions[messageString].Remove(callback))
                {
                    return false;
                }
                if (Subscriptions[messageString].Count == 0)
                {
                    Subscriptions.Remove(messageString);
                }
            }
            return true;
        }

        /// <summary>
        ///     Registers a callback method for the specified message string
        /// </summary>
        /// <param name="messageString">The message identification string to subscribe to</param>
        /// <param name="callback">The callback function to be called for the message</param>
        /// <returns><see langword="true" /> if the callback registered successfully, otherwise <see langword="false" /></returns>
        public virtual bool Subscribe(string messageString, Action callback)
        {
            return Subscribe(messageString, (Delegate) callback);
        }

        /// <summary>
        ///     Registers a callback method for the specified message string
        /// </summary>
        /// <param name="messageString">The message identification string to subscribe to</param>
        /// <param name="callback">The callback function to be called for the message</param>
        /// <typeparam name="TArgument">The type of the message argument</typeparam>
        /// <returns><see langword="true" /> if the callback registered successfully, otherwise <see langword="false" /></returns>
        public virtual bool Subscribe<TArgument>(string messageString, Action<TArgument> callback)
        {
            return Subscribe(messageString, (Delegate) callback);
        }

        /// <summary>
        ///     Registers a callback method for the specified message string
        /// </summary>
        /// <param name="messageString">The message identification string to subscribe to</param>
        /// <param name="callback">The callback function to be called for the message</param>
        /// <typeparam name="TResult">The type of the callback result</typeparam>
        /// <typeparam name="TArgument">The type of the message argument</typeparam>
        /// <returns><see langword="true" /> if the callback registered successfully, otherwise <see langword="false" /></returns>
        public virtual bool Subscribe<TResult, TArgument>(string messageString, Func<TArgument, TResult> callback)
        {
            return Subscribe(messageString, (Delegate) callback);
        }

        /// <summary>
        ///     Registers a callback method for the specified message string
        /// </summary>
        /// <param name="messageString">The message identification string to subscribe to</param>
        /// <param name="callback">The callback function to be called for the message</param>
        /// <typeparam name="TResult">The type of the callback result</typeparam>
        /// <returns><see langword="true" /> if the callback registered successfully, otherwise <see langword="false" /></returns>
        public virtual bool Subscribe<TResult>(string messageString, Func<TResult> callback)
        {
            return Subscribe(messageString, (Delegate) callback);
        }

        /// <summary>
        ///     Unregisters a callback method for the specified message string
        /// </summary>
        /// <param name="messageString">The message identification string to unsubscribe from</param>
        /// <param name="callback">The callback function registered for the message</param>
        /// <returns>
        ///     <see langword="true" /> if the callback unregistered successfully, otherwise <see langword="false" />
        /// </returns>
        public virtual bool Unsubscribe(string messageString, Action callback)
        {
            return Unsubscribe(messageString, (Delegate) callback);
        }

        /// <summary>
        ///     Unregisters a callback method for the specified message string
        /// </summary>
        /// <param name="messageString">The message identification string to unsubscribe from</param>
        /// <param name="callback">The callback function registered for the message</param>
        /// <typeparam name="TArgument">The type of the message argument</typeparam>
        /// <returns>
        ///     <see langword="true" /> if the callback unregistered successfully, otherwise <see langword="false" />
        /// </returns>
        public virtual bool Unsubscribe<TArgument>(string messageString, Action<TArgument> callback)
        {
            return Unsubscribe(messageString, (Delegate) callback);
        }

        /// <summary>
        ///     Unregisters a callback method for the specified message string
        /// </summary>
        /// <param name="messageString">The message identification string to unsubscribe from</param>
        /// <param name="callback">The callback function registered for the message</param>
        /// <typeparam name="TResult">The type of the callback result</typeparam>
        /// <typeparam name="TArgument">The type of the message argument</typeparam>
        /// <returns>
        ///     <see langword="true" /> if the callback unregistered successfully, otherwise <see langword="false" />
        /// </returns>
        public virtual bool Unsubscribe<TResult, TArgument>(string messageString, Func<TArgument, TResult> callback)
        {
            return Unsubscribe(messageString, (Delegate) callback);
        }

        /// <summary>
        ///     Unregisters a callback method for the specified message string
        /// </summary>
        /// <param name="messageString">The message identification string to unsubscribe from</param>
        /// <param name="callback">The callback function registered for the message</param>
        /// <typeparam name="TResult">The type of the callback result</typeparam>
        /// <returns>
        ///     <see langword="true" /> if the callback unregistered successfully, otherwise <see langword="false" />
        /// </returns>
        public virtual bool Unsubscribe<TResult>(string messageString, Func<TResult> callback)
        {
            return Unsubscribe(messageString, (Delegate) callback);
        }

        /// <summary>
        ///     The method to be called when a new message received from the Javascript side
        /// </summary>
        /// <param name="messageString">The message identification string, or string.Empty to match all</param>
        /// <param name="arguments">The message arguments, or null</param>
        /// <returns>Returns the response of the last subscriber</returns>
        protected virtual object OnNewMessage(string messageString, object arguments)
        {
            messageString = messageString.ToLower().Trim();
            object result = null;
            Delegate[] delegates;
            lock (Lock)
            {
                delegates =
                    Subscriptions.Where(
                        pair =>
                            string.IsNullOrEmpty(messageString) || string.IsNullOrEmpty(pair.Key) ||
                            messageString.Equals(pair.Key, StringComparison.CurrentCulture))
                        .SelectMany(pair => pair.Value)
                        .ToArray();
            }
            foreach (var subscription in delegates)
            {
                var methodInfo = subscription.Method;
                var methodArguments = new List<object>();
                var methodResultType = methodInfo.ReturnType;
                try
                {
                    var methodParameters = methodInfo.GetParameters();
                    var failed = false;
                    if (methodParameters.Length > 0)
                    {
                        foreach (var methodParameter in methodParameters)
                        {
                            if (methodArguments.Count == 0 && !(methodParameter.IsOptional && arguments == null))
                            {
                                methodArguments.Add(arguments == null
                                    ? null
                                    : ClassBridge.NormalizeVariable(arguments, methodParameter.ParameterType, true));
                            }
                            else if (methodParameter.IsOptional)
                            {
                                methodArguments.Add(methodParameter.DefaultValue);
                            }
                            else
                            {
                                failed = true;
                                break;
                            }
                        }
                        if (failed)
                        {
                            continue;
                        }
                    }
                }
                catch (Exception)
                {
                    continue;
                }
                if (methodInfo == null)
                {
                    continue;
                }
                if (methodResultType == typeof (void))
                {
                    // If we don't need the result of this method, we better run it in a new thread, so the UI thread don't get blocked
                    Task.Factory.StartNew(() => methodInfo.Invoke(subscription.Target, methodArguments.ToArray()));
                }
                else
                {
                    var methodResult = methodInfo.Invoke(subscription.Target, methodArguments.ToArray());
                    result = methodResult == null
                        ? null
                        : ClassBridge.NormalizeVariable(methodResult, methodResultType, true);
                }
            }
            return result;
        }

        /// <summary>
        ///     The class to be reflected on Javascript side
        /// </summary>
        protected class ProxyClass
        {
            /// <summary>
            ///     The delegate used by NewMessageEvent event
            /// </summary>
            /// <param name="messageString">The message identification string</param>
            /// <param name="arguments">The message arguments</param>
            public delegate object NewMessageEvent(string messageString, object arguments);

            /// <summary>
            ///     The <see cref="HybridMessagingHandler" /> instance that created this class
            /// </summary>
            protected readonly HybridMessagingHandler Handler;

            /// <summary>
            ///     Creates a new instance of this class
            /// </summary>
            /// <param name="handler">The <see cref="HybridMessagingHandler" /> class that owns this istance</param>
            public ProxyClass(HybridMessagingHandler handler)
            {
                Handler = handler;
            }

            /// <summary>
            ///     The event to be called when a new message arrives from C# side
            /// </summary>
            public event NewMessageEvent NewMessage;

            internal object HandlerNewMessage(string messageString, object arguments)
            {
                return OnNewMessage(messageString, arguments);
            }

            /// <summary>
            ///     The method to raise <see cref="NewMessage" /> event for sending messages to Javascript subscribers
            /// </summary>
            /// <param name="messageString">The message identification string</param>
            /// <param name="arguments">The message arguments</param>
            /// <returns>The response from the Javascript side</returns>
            protected virtual object OnNewMessage(string messageString, object arguments)
            {
                return NewMessage?.Invoke(messageString, arguments);
            }

            /// <summary>
            ///     The method to be called from Javascript side for C# subscribers
            /// </summary>
            /// <param name="messageString">The message identification string</param>
            /// <param name="arguments">The message arguments</param>
            /// <returns>The response from the C# side</returns>
            public virtual object Send(string messageString, object arguments = null)
            {
                return Handler.OnNewMessage(messageString, arguments);
            }
        }
    }
}