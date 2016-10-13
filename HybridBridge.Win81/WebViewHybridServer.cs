using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using HybridBridge.WebServer;
using Newtonsoft.Json;

namespace HybridBridge.Win81
{
    /// <summary>
    ///     WebViewHybridServer is a class containing a list of registered
    ///     <see cref="IBridgeHandler" /> instances for Windows WebView object managing a local http server for two way
    ///     commication with the WebView object.
    /// </summary>
    public class WebViewHybridServer : HybridBridgeLocalServer
    {
        /// <summary>
        ///     Creates a new instance of this class for the provided <see cref="WebView" /> with passed collection of
        ///     <see cref="IBridgeHandler" /> instances
        /// </summary>
        /// <param name="webView">The <see cref="WebView" /> to handle requests from</param>
        /// <param name="collection">The collection of <see cref="IBridgeHandler" /> instances to add to this list</param>
        public WebViewHybridServer(WebView webView, IEnumerable<IBridgeHandler> collection)
            : base(collection)
        {
            WebView = webView;
            WebView.DOMContentLoaded += WebViewOnDomContentLoaded;
        }


        /// <summary>
        ///     Creates a new instance of this class for the provided WebView
        /// </summary>
        /// <param name="webView">The <see cref="WebView" /> to handle requests from</param>
        public WebViewHybridServer(WebView webView) : this(webView, null)
        {
        }

        /// <summary>
        ///     The WebView to handle requests from
        /// </summary>
        public WebView WebView { get; }

        /// <summary>
        ///     Gets called when the monitoring WebView loads an HTML file and
        /// </summary>
        /// <param name="sender">The WebView calling this method</param>
        /// <param name="args">The event arguments</param>
        /// <exception cref="NotSupportedException">Following protocol is not supported for Hybrid requests</exception>
        protected virtual async void WebViewOnDomContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            if (sender != WebView) return;
            await RunServerAsync();
            PushJavascript();
        }

        /// <summary>
        ///     Executes a Javascript string and calls the passed callback for result
        /// </summary>
        /// <param name="jsCode">The code to execute</param>
        /// <param name="result">The callback to be called with the result of the execution</param>
        public override void ExecuteJavascript(string jsCode, Action<object> result)
        {
            if (result == null)
            {
                // ReSharper disable once UnusedVariable
                var uselessData = CoreApplication.MainView.CoreWindow.Dispatcher
                    .RunAsync(
                        CoreDispatcherPriority.Normal,
                        async () => { await WebView.InvokeScriptAsync(@"eval", new[] {jsCode}); });
            }
            else
            {
                Task.Run(async () =>
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher
                        .RunAsync(
                            CoreDispatcherPriority.Normal,
                            async () =>
                            {
                                var value = await WebView.InvokeScriptAsync(@"eval", new[] {jsCode});
                                if (!string.IsNullOrWhiteSpace(value) && (value != "undefined"))
                                {
                                    result.Invoke(JsonConvert.DeserializeObject(value));
                                    return;
                                }
                                result.Invoke(null);
                            });
                });
            }
        }
    }
}