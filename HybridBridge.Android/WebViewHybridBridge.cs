using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Android.Graphics;
using Android.Webkit;
using Newtonsoft.Json;
using Object = Java.Lang.Object;
using String = Java.Lang.String;

namespace HybridBridge.Android
{
    /// <summary>
    ///     WebViewHybridBridge is a class containing a list of registered
    ///     <see cref="IBridgeHandler" /> instances for android WebView object.
    /// </summary>
    public class WebViewHybridBridge : BridgeController
    {
        /// <summary>
        ///     The delegate for <see cref="JavascriptMessage" /> event
        /// </summary>
        /// <param name="sender">The event raiser</param>
        /// <param name="eventArgs">The event arguments</param>
        public delegate void JavascriptMessageEvent(object sender, JavascriptMessageEventArgs eventArgs);

        /// <summary>
        ///     Creates a new instance of this class for the provided <see cref="WebView" /> with the provided string as the custom
        ///     url protocol
        ///     and passed collection of <see cref="IBridgeHandler" /> instances
        /// </summary>
        /// <param name="webView">The <see cref="WebView" /> to handle requests from</param>
        /// <param name="customProtocol">
        ///     The protocol to use for communication with web browser, or <see langword="null" /> to use
        ///     defualt
        /// </param>
        /// <param name="collection">The collection of <see cref="IBridgeHandler" /> instances to add to this list</param>
        public WebViewHybridBridge(WebView webView, string customProtocol, IEnumerable<IBridgeHandler> collection)
            : base(customProtocol, collection)
        {
            WebView = webView;
            WebView.Settings.JavaScriptEnabled = true;
            WebView.Settings.AllowUniversalAccessFromFileURLs = true;
            WebView.Settings.CacheMode = CacheModes.NoCache;
            WebView.SetWebViewClient(new AndroidWebViewClient(this));
            WebView.SetWebChromeClient(new AndroidWebChromeClient(this));
        }

        /// <summary>
        ///     Creates a new instance of this class for the provided <see cref="WebView" /> with the provided string as the custom
        ///     url protocol
        /// </summary>
        /// <param name="webView">The <see cref="WebView" /> to handle requests from</param>
        /// <param name="customProtocol">
        ///     The protocol to use for communication with web browser, or <see langword="null" /> to use
        ///     defualt
        /// </param>
        public WebViewHybridBridge(WebView webView, string customProtocol) : this(webView, customProtocol, null)
        {
        }

        /// <summary>
        ///     Creates a new instance of this class for the provided <see cref="WebView" /> with passed collection of
        ///     <see cref="IBridgeHandler" /> instances
        /// </summary>
        /// <param name="webView">The <see cref="WebView" /> to handle requests from</param>
        /// <param name="collection">The collection of <see cref="IBridgeHandler" /> instances to add to this list</param>
        public WebViewHybridBridge(WebView webView, IEnumerable<IBridgeHandler> collection)
            : this(webView, null, collection)
        {
        }

        /// <summary>
        ///     Creates a new instance of this class for the provided WebView
        /// </summary>
        /// <param name="webView">The <see cref="WebView" /> to handle requests from</param>
        public WebViewHybridBridge(WebView webView) : this(webView, null, null)
        {
        }

        /// <summary>
        ///     The WebView to handle requests from
        /// </summary>
        public WebView WebView { get; }

        /// <summary>
        ///     The event that gets raised when a new Javascript message is logged
        /// </summary>
        public event JavascriptMessageEvent JavascriptMessage;

        /// <summary>
        ///     Executes a Javascript string and calls the passed callback for result
        /// </summary>
        /// <param name="jsCode">The code to execute</param>
        /// <param name="result">The callback to be called with the result of the execution</param>
        public override void ExecuteJavascript(string jsCode, Action<object> result)
        {
            WebView.Post(
                () => WebView.EvaluateJavascript(jsCode, result == null ? null : new JavascriptCallback(result)));
        }

        /// <summary>
        ///     The class to handle <see cref="WebView" /> chrome specific events
        /// </summary>
        protected class AndroidWebChromeClient : WebChromeClient
        {
            private readonly WebViewHybridBridge _bridge;

            /// <summary>
            ///     Creates a new instance of this class for the passed <see cref="WebViewHybridBridge" /> instance
            /// </summary>
            /// <param name="bridge">The parent <see cref="WebViewHybridBridge" /> instance</param>
            public AndroidWebChromeClient(WebViewHybridBridge bridge)
            {
                _bridge = bridge;
            }

            /// <summary>
            ///     Method to handle new console messages
            /// </summary>
            /// <param name="consoleMessage">The <see cref="ConsoleMessage" /> object containing information about the console message</param>
            /// <returns></returns>
            public override bool OnConsoleMessage(ConsoleMessage consoleMessage)
            {
                _bridge.JavascriptMessage?.Invoke(_bridge,
                    new JavascriptMessageEventArgs(consoleMessage.Message(), consoleMessage.InvokeMessageLevel()));
                return base.OnConsoleMessage(consoleMessage);
            }
        }

        /// <summary>
        ///     Representing a Javascript result callback
        /// </summary>
        protected class JavascriptCallback : Object, IValueCallback
        {
            private readonly Action<object> _callback;

            /// <summary>
            ///     Creates a new instance of this class with the passed callback
            /// </summary>
            /// <param name="callback">The callback to invoke when the value is available</param>
            public JavascriptCallback(Action<object> callback)
            {
                _callback = callback;
            }

            /// <summary>
            ///     The method that gets called when the value of the operation is ready
            /// </summary>
            /// <param name="value">The value of the operation</param>
            public void OnReceiveValue(Object value)
            {
                var result = ((String) value).ToString();
                if (!string.IsNullOrWhiteSpace(result) && result != "undefined")
                {
                    _callback(JsonConvert.DeserializeObject(result));
                    return;
                }
                _callback(null);
            }
        }

        /// <summary>
        ///     The class to handle <see cref="WebView" /> events
        /// </summary>
        protected class AndroidWebViewClient : WebViewClient
        {
            private readonly WebViewHybridBridge _bridge;

            /// <summary>
            ///     Creates a new instance of this class for the passed <see cref="WebViewHybridBridge" /> instance
            /// </summary>
            /// <param name="bridge">The parent <see cref="WebViewHybridBridge" /> instance</param>
            public AndroidWebViewClient(WebViewHybridBridge bridge)
            {
                _bridge = bridge;
            }

            /// <summary>
            ///     The method that gets called when a page started opening after its HTML is loaded
            /// </summary>
            /// <param name="view">The WebView that called this method</param>
            /// <param name="url">The web page url</param>
            /// <param name="favicon">The favicon of the web page</param>
            public override void OnPageStarted(WebView view, string url, Bitmap favicon)
            {
                if (view == _bridge.WebView)
                {
                    _bridge.PushJavascript();
                }
                base.OnPageStarted(view, url, favicon);
            }

            /// <summary>
            ///     The method that gets called when a new resource request created
            /// </summary>
            /// <param name="view">The WebView that called this method</param>
            /// <param name="url">The requested url</param>
            /// <returns>Returns the <see cref="WebResourceResponse" /> for the requested url</returns>
            public override WebResourceResponse ShouldInterceptRequest(WebView view, string url)
            {
                if (view == _bridge.WebView)
                {
                    bool hasResult;
                    object result;
                    if (_bridge.HandleRequestUrl(url, out hasResult, out result))
                    {
                        return new WebResourceResponse("application/json", "UTF-8",
                            new MemoryStream(Encoding.UTF8.GetBytes(hasResult ? JsonConvert.SerializeObject(result) : "")));
                    }
                }
                return base.ShouldInterceptRequest(view, url);
            }
        }
    }
}