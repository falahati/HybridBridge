using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HybridBridge.Exceptions;
using HybridBridge.HttpTools;
using Newtonsoft.Json;

namespace HybridBridge
{
    /// <summary>
    ///     BridgeController is an abstract class containing a list of
    ///     registered <see cref="IBridgeHandler" /> instances. This class
    ///     should be extended for different environments.
    /// </summary>
    public abstract class BridgeController : IList<IBridgeHandler>
    {
        /// <summary>
        ///     The underlying list of <see cref="IBridgeHandler" /> instances
        /// </summary>
        protected readonly List<IBridgeHandler> Handlers = new List<IBridgeHandler>();

        private string _protocol;

        /// <summary>
        ///     Creates a new instance of BridgeController class
        /// </summary>
        /// <param name="customProtocol">
        ///     The protocol to use for communication with web browser, or <see langword="null" /> to use
        ///     default
        /// </param>
        /// <param name="collection">List of all <see cref="IBridgeHandler" /> classes</param>
        protected BridgeController(string customProtocol, IEnumerable<IBridgeHandler> collection)
        {
            Protocol = customProtocol;
            if (collection != null)
                AddRange(collection);
        }

        /// <summary>
        ///     Creates a new instance of BridgeController class
        /// </summary>
        /// <param name="customProtocol">
        ///     The protocol to use for communication with web browser, or <see langword="null" /> to use
        ///     default
        /// </param>
        protected BridgeController(string customProtocol) : this(customProtocol, null)
        {
        }

        /// <summary>
        ///     Creates a new instance of BridgeController class
        /// </summary>
        /// <param name="collection">List of all <see cref="IBridgeHandler" /> classes</param>
        protected BridgeController(IEnumerable<IBridgeHandler> collection) : this(null, collection)
        {
        }

        /// <summary>
        ///     Creates a new instance of BridgeController class
        /// </summary>
        protected BridgeController() : this(null, null)
        {
        }

        /// <summary>
        ///     Sets or gets the time that we should wait for the result of any Javascript invocation, this value doesn't applies
        ///     to callbacks
        /// </summary>
        public virtual TimeSpan JavascriptTimeout { get; set; } = TimeSpan.FromSeconds(3);

        /// <summary>
        ///     The protocol to use for communication with web browser
        /// </summary>
        public string Protocol
        {
            get { return _protocol; }
            protected set { _protocol = NormalizeCustomProtocol(value); }
        }

        /// <summary>
        ///     Returns the <see cref="IEnumerator" /> object for this class
        /// </summary>
        /// <returns><see cref="IEnumerator" /> object for this class</returns>
        public virtual IEnumerator<IBridgeHandler> GetEnumerator()
        {
            return Handlers.GetEnumerator();
        }

        /// <summary>
        ///     Returns the <see cref="IEnumerator" /> object for this class
        /// </summary>
        /// <returns><see cref="IEnumerator" /> object for this class</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        /// <summary>
        ///     Adds a new <see cref="IBridgeHandler" /> object to the list
        /// </summary>
        /// <param name="item">The <see cref="IBridgeHandler" /> object</param>
        public void Add(IBridgeHandler item)
        {
            Insert(Count, item);
        }

        /// <summary>
        ///     Clears the list by removing all registered <see cref="IBridgeHandler" /> objects
        /// </summary>
        public void Clear()
        {
            for (var index = Count - 1; index <= 0; index--)
                RemoveAt(index);
        }

        /// <summary>
        ///     Goes through the list of registered <see cref="IBridgeHandler" /> objects and checks for existing of the passed
        ///     instance
        /// </summary>
        /// <param name="item"><see cref="IBridgeHandler" /> instance to search for</param>
        /// <returns><see langword="true" /> if the passed instance was in the list, otherwise <see langword="false" /></returns>
        public virtual bool Contains(IBridgeHandler item)
        {
            return Handlers.Contains(item);
        }

        /// <summary>
        ///     Copies the list of registered <see cref="IBridgeHandler" /> object to an array
        /// </summary>
        /// <param name="array">Array to copy the list to</param>
        /// <param name="arrayIndex">Index of first element in the destination array</param>
        public virtual void CopyTo(IBridgeHandler[] array, int arrayIndex)
        {
            Handlers.CopyTo(array, arrayIndex);
        }

        /// <summary>
        ///     Removes the passed instance of <see cref="IBridgeHandler" /> from the list of the registered handlers
        /// </summary>
        /// <param name="item">The <see cref="IBridgeHandler" /> instance to remove from the list</param>
        /// <returns>
        ///     <see langword="true" /> if desired instance removed from the list, otherwise <see langword="false" />
        /// </returns>
        public bool Remove(IBridgeHandler item)
        {
            var index = IndexOf(item);
            if (index < 0)
                return false;
            RemoveAt(index);
            return true;
        }

        /// <summary>
        ///     Returns the number of registered <see cref="IBridgeHandler" /> objects
        /// </summary>
        public virtual int Count => Handlers.Count;

        /// <summary>
        ///     Indicates if the list is read only
        /// </summary>
        public virtual bool IsReadOnly => false;

        /// <summary>
        ///     Returns the index of the passed <see cref="IBridgeHandler" /> instance in the list
        /// </summary>
        /// <param name="item"><see cref="IBridgeHandler" /> instance to search for</param>
        /// <returns>Index of the passed instance, otherwise -1</returns>
        public virtual int IndexOf(IBridgeHandler item)
        {
            return Handlers.IndexOf(item);
        }

        /// <summary>
        ///     Inserts and registers the passed instance of <see cref="IBridgeHandler" />
        /// </summary>
        /// <param name="index">Index to which the new instance should be added</param>
        /// <param name="item">The instance of <see cref="IBridgeHandler" /></param>
        /// <exception cref="RedundantHandlerException">An instance of the same type in already in list</exception>
        public virtual void Insert(int index, IBridgeHandler item)
        {
            if (Get(item.GetType()) != null)
                throw new RedundantHandlerException();
            Handlers.Insert(index, item);
            item.PushJavascript += BridgeHandler_PushJavascript;
        }

        /// <summary>
        ///     Removed a <see cref="IBridgeHandler" /> instance at the specific index
        /// </summary>
        /// <param name="index">Index of the instance to remove</param>
        public virtual void RemoveAt(int index)
        {
            var item = this[index];
            Handlers.RemoveAt(index);
            item.PushJavascript -= BridgeHandler_PushJavascript;
        }

        /// <summary>
        ///     Set or get an <see cref="IBridgeHandler" /> instance by index
        /// </summary>
        /// <param name="index">Index of the <see cref="IBridgeHandler" /> instance</param>
        public virtual IBridgeHandler this[int index]
        {
            get { return Handlers[index]; }
            set
            {
                if (this[index] != value)
                {
                    RemoveAt(index);
                    Insert(index, value);
                }
            }
        }

        /// <summary>
        ///     Makes sure that the passed custom protocol is acceptable and makes the necessarily changes
        /// </summary>
        protected virtual string NormalizeCustomProtocol(string customProtocol = null)
        {
            var goodCharecters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
            var protocolChars =
                customProtocol?.ToLower().Trim().ToCharArray().Where(c => goodCharecters.Contains(c)).ToArray();
            if ((protocolChars == null) || !protocolChars.Any())
            {
                return $"hybrid{Guid.NewGuid():N}:";
            }
            return new string(protocolChars) + ":";
        }

        /// <summary>
        ///     Returns a registered <see cref="IBridgeHandler" /> by type
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IBridgeHandler" /> to return</typeparam>
        /// <returns>The <see cref="IBridgeHandler" /> instance of the specified type</returns>
        public virtual T Get<T>()
        {
            return (T) Get(typeof(T));
        }

        /// <summary>
        ///     Returns a registered <see cref="IBridgeHandler" /> by type
        /// </summary>
        /// <param name="type">The type of <see cref="IBridgeHandler" /> to return</param>
        /// <returns>The <see cref="IBridgeHandler" /> instance of the specified type</returns>
        public virtual IBridgeHandler Get(Type type)
        {
            return Handlers.FirstOrDefault(handler => handler.GetType() == type);
        }

        /// <summary>
        ///     Executes a Javascript string and returns the result
        /// </summary>
        /// <param name="jsCode">The code to execute</param>
        /// <returns>Result of the code execution</returns>
        public virtual object ExecuteJavascript(string jsCode)
        {
            var resetEvent = new ManualResetEvent(false);
            object returnValue = null;
            ExecuteJavascript(jsCode, o =>
            {
                returnValue = o;
                resetEvent.Set();
            });
            resetEvent.WaitOne(JavascriptTimeout);
            return returnValue;
        }

        /// <summary>
        ///     Executes a Javascript string and calls the passed callback for result
        /// </summary>
        /// <param name="jsCode">The code to execute</param>
        /// <param name="result">The callback to be called with the result of the execution</param>
        public virtual void ExecuteJavascript(string jsCode, Action<object> result)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Fires a Javascript string
        /// </summary>
        /// <param name="jsCode">The code to execute</param>
        public virtual void FireJavascript(string jsCode)
        {
            ExecuteJavascript(jsCode, null);
        }

        /// <summary>
        ///     Calls a Javascript method with the provided arguments and return the result
        /// </summary>
        /// <param name="methodName">The name of the method to call</param>
        /// <param name="arguments">The arguments to send to the method</param>
        /// <returns>Result of the code execution</returns>
        public virtual object CallJavascriptFunction(string methodName, params object[] arguments)
        {
            return
                ExecuteJavascript(
                    $"return {methodName}({string.Join(", ", arguments.Select(JsonConvert.SerializeObject))});");
        }

        /// <summary>
        ///     Calls a Javascript method with the provided arguments and return the result
        /// </summary>
        /// <param name="methodName">The name of the method to call</param>
        /// <param name="arguments">The arguments to send to the method</param>
        public virtual void CallJavascriptAction(string methodName, params object[] arguments)
        {
            FireJavascript($"{methodName}({string.Join(", ", arguments.Select(JsonConvert.SerializeObject))});");
        }

        /// <summary>
        ///     Executes the <see cref="BridgeController" /> javascript code
        /// </summary>
        protected virtual void Initialize()
        {
            FireJavascript(
                $@"
var HybridBridge = function () {{
    return undefined;
}};

HybridBridge.__call = function (handlerName, actionName, actionParameters, resultCallback) {{
    var ajaxRequest = new XMLHttpRequest();
    ajaxRequest.open(""GET"", ""{
                    Protocol
                    }"" + handlerName + ""/"" + actionName + ""?rand="" + Math.random() + ""&parameters="" + encodeURIComponent(JSON.stringify(actionParameters)), !!resultCallback);
    if (!!resultCallback) {{
        ajaxRequest.onreadystatechange = function () {{
            if (ajaxRequest.readyState === XMLHttpRequest.DONE) {{
                if (ajaxRequest.status === 200 && !!ajaxRequest.responseText.trim()) {{
                    var returnValue = JSON.parse(ajaxRequest.responseText);
                    resultCallback(returnValue);
                }} else {{
                    resultCallback(undefined);
                }}
            }}
        }};
        ajaxRequest.onerror = function () {{
            resultCallback(undefined);
        }};
        ajaxRequest.send(null);
    }} else {{
        ajaxRequest.send(null);
        if (ajaxRequest.status === 200 && !!ajaxRequest.responseText.trim()) {{
            var returnValue = JSON.parse(ajaxRequest.responseText);
            return returnValue;
        }}
    }}
    return undefined;
}};

HybridBridge.__fireReady = function() {{
    var name = ""bridgeready"";
    var event;
    if (document.createEvent) {{
        event = document.createEvent(""HTMLEvents"");
        event.initEvent(name, true, true);
        event.eventName = name;
        document.dispatchEvent(event);
    }} else {{
        event = document.createEventObject();
        event.eventType = name;
        event.eventName = name;
        document.fireEvent(""on"" + name, event);
    }}
}}");
        }

        /// <summary>
        ///     Handles a request and leaves the result behind
        /// </summary>
        /// <param name="url">The Url to handle</param>
        /// <returns><see langword="true" /> if the Url handled, otherwise <see langword="false" /></returns>
        protected virtual bool HandleRequestUrl(string url)
        {
            bool hasResult;
            object result;
            return HandleRequestUrl(url, out hasResult, out result);
        }

        /// <summary>
        ///     Handles a request
        /// </summary>
        /// <param name="url">The Url to handle</param>
        /// <param name="hasResult">An outgoing boolean parameter indicating if the handling of the passed Url generated a result</param>
        /// <param name="result">An outgoing object containing the result of the handling process</param>
        /// <returns><see langword="true" /> if the Url handled, otherwise <see langword="false" /></returns>
        protected virtual bool HandleRequestUrl(string url, out bool hasResult, out object result)
        {
            hasResult = false;
            result = null;
            if (!url.StartsWith(Protocol))
                return false;
            var resources = url.Substring(Protocol.Length).Split('?');
            var methodParameters = new Dictionary<string, object>();
            if (resources.Length > 0)
            {
                var methodAddress = resources[0].Split('/');
                if (methodAddress.Length > 1)
                {
                    methodAddress[1] = string.Join("/", methodAddress, 1, methodAddress.Length - 1);
                    var handler =
                        this.FirstOrDefault(
                            bridgeHandler =>
                                    bridgeHandler.ShouldInterceptRequest(methodAddress[0], methodAddress[1]));
                    if (handler != null)
                    {
                        if (resources.Length > 1)
                        {
                            var queryStrings = HttpUtility.ParseQueryString(resources[1]);
                            var parametersQueryArray = queryStrings.GetValues("parameters");
                            if ((parametersQueryArray != null) && (parametersQueryArray.Length > 0))
                                methodParameters =
                                    JsonConvert.DeserializeObject<Dictionary<string, object>>(
                                        parametersQueryArray.Last());
                        }
                        result = handler.InterceptRequest(methodAddress[1], methodParameters, out hasResult);
                    }
                }
            }
            return true;
        }

        /// <summary>
        ///     Generated the Javascript code for the <see cref="BridgeController" /> class as well as all the registered handlers
        /// </summary>
        protected virtual void PushJavascript()
        {
            Initialize();
            foreach (var handler in this)
                handler.Initialize(this);
            FireJavascript("HybridBridge.__fireReady();");
        }

        /// <summary>
        ///     Adds a list of <see cref="IBridgeHandler" /> instances to the list
        /// </summary>
        /// <param name="collection">A list of <see cref="IBridgeHandler" /> instances</param>
        public void AddRange(IEnumerable<IBridgeHandler> collection)
        {
            foreach (var item in collection)
                Add(item);
        }

        /// <summary>
        ///     Passes Javascript requests from the IBridgeHandler instances to the browser
        /// </summary>
        /// <param name="sender">The IBridgeHandler asking for the Javascript execution</param>
        /// <param name="eventArgs">The arguments of this event</param>
        /// <returns></returns>
        protected virtual object BridgeHandler_PushJavascript(IBridgeHandler sender, PushJavascriptEventArgs eventArgs)
        {
            if (!Contains(sender) || string.IsNullOrWhiteSpace(eventArgs.Script))
                return null;
            if ((eventArgs.Target != null) && (eventArgs.Target != this))
                return null;
            if (eventArgs.Callback != null)
            {
                ExecuteJavascript(eventArgs.Script, eventArgs.Callback);
                return null;
            }
            if (eventArgs.FireAway)
            {
                FireJavascript(eventArgs.Script);
                return null;
            }
            return ExecuteJavascript(eventArgs.Script);
        }
    }
}