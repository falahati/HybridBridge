using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using HybridBridge.WebServer;
using Newtonsoft.Json;

namespace HybridBridge.Win
{
    /// <summary>
    ///     WebBrowserHybridServer is a class containing a list of registered
    ///     <see cref="IBridgeHandler" /> instances for WinForm WebBrowser control.
    /// </summary>
    public class WebBrowserHybridServer : HybridBridgeLocalServer
    {
        /// <summary>
        ///     Creates a new instance of this class for the provided <see cref="WebBrowser" /> and with the passed collection of
        ///     <see cref="IBridgeHandler" />s
        /// </summary>
        /// <param name="webView">The <see cref="WebBrowser" /> to handle requests from</param>
        /// <param name="collection">A collection of <see cref="IBridgeHandler" /> objects</param>
        public WebBrowserHybridServer(WebBrowser webView, IEnumerable<IBridgeHandler> collection)
            : base(collection)
        {
            WebView = webView;
            WebView.Navigated += WebViewOnNavigated;
        }

        /// <summary>
        ///     Creates a new instance of this class for the provided <see cref="WebBrowser" />
        /// </summary>
        /// <param name="webView">The <see cref="WebBrowser" /> to handle requests from</param>
        public WebBrowserHybridServer(WebBrowser webView) : this(webView, null)
        {
        }

        /// <summary>
        ///     The WebBrowser to handle requests from
        /// </summary>
        public WebBrowser WebView { get; }

        private async void WebViewOnNavigated(object sender, WebBrowserNavigatedEventArgs webBrowserNavigatedEventArgs)
        {
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
                InvokeOnUiThread(() => { WebView.Document?.InvokeScript(@"eval", new object[] {jsCode}); });
            else
                Task.Factory.StartNew(() =>
                {
                    string value = null;
                    InvokeOnUiThread(
                        () => { value = WebView.Document?.InvokeScript(@"eval", new object[] {jsCode}) as string; });
                    if (!string.IsNullOrWhiteSpace(value) && (value != "undefined"))
                    {
                        result.Invoke(JsonConvert.DeserializeObject(value));
                        return;
                    }
                    result.Invoke(null);
                });
        }

        /// <summary>
        ///     Invokes a piece of code on the WebView thread
        /// </summary>
        /// <param name="action">The code to execute</param>
        protected virtual void InvokeOnUiThread(Action action)
        {
            if (WebView.InvokeRequired)
                WebView.Invoke(action);
            else
                action();
        }
    }
}