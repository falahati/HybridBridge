using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Newtonsoft.Json;
using ObjCRuntime;
using UIKit;

namespace HybridBridge.iOS
{
    // ReSharper disable once InconsistentNaming
    /// <summary>
    ///     UIWebViewHybridBridge is a class containing a list of registered
    ///     <see cref="IBridgeHandler" /> instances for iOS UIWebView object.
    /// </summary>
    public class UIWebViewHybridBridge : BridgeController, IDisposable
    {
        /// <summary>
        ///     Indicates if the object already disposed
        /// </summary>
        protected bool Disposed;

        /// <summary>
        ///     Creates a new instance of this class for the provided <see cref="UIWebView" /> with the provided string as the
        ///     custom
        ///     url protocol
        ///     and passed collection of <see cref="IBridgeHandler" /> instances
        /// </summary>
        /// <param name="webView">The <see cref="UIWebView" /> to handle requests from</param>
        /// <param name="customProtocol">
        ///     The protocol to use for communication with web browser, or <see langword="null" /> to use
        ///     defualt
        /// </param>
        /// <param name="collection">The collection of <see cref="IBridgeHandler" /> instances to add to this list</param>
        public UIWebViewHybridBridge(UIWebView webView, string customProtocol,
            IEnumerable<IBridgeHandler> collection)
            : base(customProtocol, collection)
        {
            WebView = webView;
            WebView.LoadFinished += WebView_LoadFinished;
            lock (HybridUrlProtocol.Bridges)
            {
                if (!HybridUrlProtocol.Bridges.Contains(this))
                {
                    HybridUrlProtocol.Bridges.Add(this);
                }
            }
            NSUrlProtocol.RegisterClass(new Class(typeof(HybridUrlProtocol)));
        }

        /// <summary>
        ///     Creates a new instance of this class for the provided <see cref="UIWebView" /> with the provided string as the
        ///     custom
        ///     url protocol
        /// </summary>
        /// <param name="webView">The <see cref="UIWebView" /> to handle requests from</param>
        /// <param name="customProtocol">
        ///     The protocol to use for communication with web browser, or <see langword="null" /> to use
        ///     defualt
        /// </param>
        public UIWebViewHybridBridge(UIWebView webView, string customProtocol) : this(webView, customProtocol, null)
        {
        }

        /// <summary>
        ///     Creates a new instance of this class for the provided <see cref="UIWebView" /> with passed collection of
        ///     <see cref="IBridgeHandler" /> instances
        /// </summary>
        /// <param name="webView">The <see cref="UIWebView" /> to handle requests from</param>
        /// <param name="collection">The collection of <see cref="IBridgeHandler" /> instances to add to this list</param>
        public UIWebViewHybridBridge(UIWebView webView, IEnumerable<IBridgeHandler> collection)
            : this(webView, null, collection)
        {
        }

        /// <summary>
        ///     Creates a new instance of this class for the provided WebView
        /// </summary>
        /// <param name="webView">The <see cref="UIWebView" /> to handle requests from</param>
        public UIWebViewHybridBridge(UIWebView webView) : this(webView, null, null)
        {
        }

        /// <summary>
        ///     The UIWebView to handle requests from
        /// </summary>
        public UIWebView WebView { get; }

        /// <summary>
        ///     The method to dispose this class
        /// </summary>
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void WebView_LoadFinished(object sender, EventArgs e)
        {
            if (Equals(sender, WebView))
            {
                PushJavascript();
                FireJavascript("window.getSelection().removeAllRanges();");
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
                WebView.InvokeOnMainThread(() => { WebView.EvaluateJavascript(jsCode); });
            }
            else
            {
                Task.Run(() =>
                {
                    string value = null;
                    WebView.InvokeOnMainThread(() => { value = WebView.EvaluateJavascript(jsCode); });
                    if (!string.IsNullOrWhiteSpace(value) && (value != "undefined"))
                    {
                        result.Invoke(JsonConvert.DeserializeObject(value));
                        return;
                    }
                    result.Invoke(null);
                });
            }
        }


        /// <summary>
        ///     The method to dispose this object
        /// </summary>
        ~UIWebViewHybridBridge()
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

            if (disposing)
            {
                lock (HybridUrlProtocol.Bridges)
                {
                    if (HybridUrlProtocol.Bridges.Contains(this))
                    {
                        HybridUrlProtocol.Bridges.Remove(this);
                    }
                }
            }

            Disposed = true;
        }

        /// <summary>
        ///     Custom implementation of the <see cref="NSUrlProtocol" /> class for capturing Application-Wide web requests
        /// </summary>
        protected class HybridUrlProtocol : NSUrlProtocol
        {
            /// <summary>
            ///     Creates a new instance of this object
            /// </summary>
            /// <param name="request">The <see cref="NSUrlRequest" /> to initialize this instance</param>
            /// <param name="cachedResponse">
            ///     The <see cref="NSCachedUrlResponse" /> object representing the cached response for this
            ///     request
            /// </param>
            /// <param name="client">The <see cref="INSUrlProtocolClient" /> client</param>
            [Export(@"initWithRequest:cachedResponse:client:")]
            public HybridUrlProtocol(NSUrlRequest request, NSCachedUrlResponse cachedResponse,
                INSUrlProtocolClient client)
                : base(request, cachedResponse, client)
            {
            }

            /// <summary>
            ///     A list of all active <see cref="UIWebViewHybridBridge" /> instances on this application
            /// </summary>
            public static List<UIWebViewHybridBridge> Bridges { get; } = new List<UIWebViewHybridBridge>();

            /// <summary>
            ///     Indicates if the passed request can be handled by this class
            /// </summary>
            /// <param name="request">The request to analyze</param>
            /// <returns><see langword="true" /> if the passed request can be handled, otherwise <see langword="false" /></returns>
            [Export(@"canInitWithRequest:")]
            public new static bool CanInitWithRequest(NSUrlRequest request)
            {
                lock (Bridges)
                {
                    return
                        Bridges.Any(
                            bridge =>
                                !string.IsNullOrWhiteSpace(bridge.Protocol) &&
                                request.Url.ToString().StartsWith(bridge.Protocol));
                }
            }

            /// <summary>
            ///     Returns the manipulated version of the initial request
            /// </summary>
            /// <param name="forRequest">The request to process</param>
            /// <returns>The new request</returns>
            [Export(@"canonicalRequestForRequest:")]
            public new static NSUrlRequest GetCanonicalRequest(NSUrlRequest forRequest)
            {
                return forRequest;
            }

            /// <summary>
            ///     Starts processing of the request
            /// </summary>
            [Export(@"startLoading")]
            public override void StartLoading()
            {
                var hasResult = false;
                object result = null;
                lock (Bridges)
                {
                    if (Bridges.FirstOrDefault(
                                bridge =>
                                    !string.IsNullOrWhiteSpace(bridge.Protocol) &&
                                    Request.Url.ToString().StartsWith(bridge.Protocol))?
                            .HandleRequestUrl(Request.Url.ToString(), out hasResult, out result) != true)
                    {
                        return;
                    }
                }
                var response = new NSHttpUrlResponse(Request.Url, 200, "1.1",
                    NSDictionary.FromObjectsAndKeys(
                        new object[] {"Access-Control-Allow-Origin", "Access-Control-Allow-Headers"},
                        new object[] {"*", "Content-Type"}));
                Client.ReceivedResponse(this, response, NSUrlCacheStoragePolicy.NotAllowed);
                Client.DataLoaded(this, NSData.FromString(hasResult ? JsonConvert.SerializeObject(result) : ""));
                Client.FinishedLoading(this);
            }

            /// <summary>
            ///     Handling the after match of the request
            /// </summary>
            [Export(@"stopLoading")]
            public override void StopLoading()
            {
                // ignore
            }
        }
    }
}