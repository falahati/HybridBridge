using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.Web;
using Newtonsoft.Json;

namespace HybridBridge.Win81
{
    /// <summary>
    ///     WebViewHybridBridge is a class containing a list of registered
    ///     <see cref="IBridgeHandler" /> instances for Windows WebView object.
    /// </summary>
    public class WebViewHybridBridge : BridgeController
    {
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
            BridgeResolver = new UriToHybridResolver(this);
            WebView = webView;
            WebView.DOMContentLoaded += WebViewOnDomContentLoaded;
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
        public WebViewHybridBridge(WebView webView, string customProtocol)
            : this(webView, customProtocol, null)
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
        ///     Gets the IUriToStreamResolver responsible for processing hybrid requests
        /// </summary>
        public IUriToStreamResolver BridgeResolver { get; protected set; }

        /// <summary>
        ///     The WebView to handle requests from
        /// </summary>
        public WebView WebView { get; }

        /// <summary>
        ///     Gets or sets the <see cref="IUriToStreamResolver" /> to be used as a custom resolver of requesting Url(s)
        /// </summary>
        public IUriToStreamResolver CustomUriToStreamResolver { get; set; }


        /// <summary>
        ///     Makes sure that the passed custom protocol is acceptable and makes the necessarily changes
        /// </summary>
        protected override string NormalizeCustomProtocol(string customProtocol = null)
        {
            customProtocol = base.NormalizeCustomProtocol(customProtocol);
            return '/' + customProtocol.Substring(0, customProtocol.Length - 1) + '/';
        }

        /// <summary>
        ///     Gets called when the monitoring WebView loads an HTML file and
        /// </summary>
        /// <param name="sender">The WebView calling this method</param>
        /// <param name="args">The event arguments</param>
        /// <exception cref="NotSupportedException">Following protocol is not supported for Hybrid requests</exception>
        protected virtual void WebViewOnDomContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            if (sender != WebView) return;
            if (args.Uri == null)
            {
                throw new NotSupportedException(
                    "NavigateToString is not supported when using WebViewHybridBridge class.");
            }
            if (args.Uri.Scheme == "ms-local-stream")
            {
                PushJavascript();
            }
            else if (args.Uri.Scheme == @"ms-appdata")
            {
                if (CustomUriToStreamResolver == null)
                {
                    CustomUriToStreamResolver = new UriToAppDataResolver();
                }
                sender.NavigateToLocalStreamUri(
                    sender.BuildLocalStreamUri("WebViewHybridBridge", args.Uri.PathAndQuery), BridgeResolver);
            }
            else if (args.Uri.Scheme == @"ms-appx-web")
            {
                if (CustomUriToStreamResolver == null)
                {
                    CustomUriToStreamResolver = new UriToAppxResolver();
                }
                sender.NavigateToLocalStreamUri(
                    sender.BuildLocalStreamUri("WebViewHybridBridge", args.Uri.PathAndQuery), BridgeResolver);
            }
            else
            {
                throw new NotSupportedException(
                    "HTTP navigation is not supported when using WebViewHybridBridge class.");
            }
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

        /// <summary>
        ///     <see cref="IUriToStreamResolver" /> compatible class responsible for handling hybrid requests
        /// </summary>
        protected class UriToHybridResolver : IUriToStreamResolver
        {
            private readonly WebViewHybridBridge _bridge;

            /// <summary>
            ///     Creates a new UriToHybridResolver instance
            /// </summary>
            /// <param name="bridge">The parent <see cref="WebViewHybridBridge" /></param>
            public UriToHybridResolver(WebViewHybridBridge bridge)
            {
                _bridge = bridge;
            }

            /// <summary>
            ///     Handles http requests
            /// </summary>
            /// <param name="uri">The <see cref="Uri" /> of the request</param>
            /// <returns>A IInputStream compatible stream</returns>
            /// <exception cref="Exception"><see cref="Uri" /> is not supported</exception>
            public virtual IAsyncOperation<IInputStream> UriToStreamAsync(Uri uri)
            {
                if (uri == null)
                {
                    throw new ArgumentException(nameof(uri));
                }
                return Task.Run(async () =>
                {
                    bool hasResult;
                    object result;
                    if (_bridge.HandleRequestUrl(uri.PathAndQuery, out hasResult, out result))
                    {
                        var mem = new InMemoryRandomAccessStream();
                        if (hasResult)
                        {
                            await mem.WriteAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result)).AsBuffer());
                            mem.Seek(0);
                        }
                        return mem;
                    }
                    var customResolve = _bridge.CustomUriToStreamResolver?.UriToStreamAsync(uri).AsTask();
                    return customResolve != null ? await customResolve : new InMemoryRandomAccessStream();
                }).AsAsyncOperation();
            }
        }
    }
}