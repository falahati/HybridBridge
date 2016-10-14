using System;
using Android.App;
using Android.OS;
using Android.Webkit;
using HybridBridge.Android.Samples.Controllers;
using HybridBridge.Android.Samples.Views;

namespace HybridBridge.Android.Samples
{
    [Activity(Label = "Sample")]
    public class WebViewActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.WebView);
            var webView = FindViewById<WebView>(Resource.Id.webView);

            webView.LongClickable = false;
            webView.HapticFeedbackEnabled = false;
            webView.LongClick += (sender, args) => { };

            switch ((ESample) Intent.GetIntExtra("Sample", -1))
            {
                case ESample.MethodSamples:
                    new WebViewHybridBridge(webView).Add(
                        new ClassBridge<MethodSamplesController>().AddInstance(new MethodSamplesController(this),
                            "Controller"));
                    webView.LoadDataWithBaseURL("file:///android_asset/", new MethodSamples().GenerateString(),
                        "text/html", "UTF-8", null);
                    break;
                case ESample.PropertySamples:
                    new WebViewHybridBridge(webView).Add(
                        new ClassBridge<PropertySamplesController>().AddInstance(new PropertySamplesController(),
                            "Controller"));
                    webView.LoadDataWithBaseURL("file:///android_asset/", new PropertySamples().GenerateString(),
                        "text/html", "UTF-8", null);
                    break;
                case ESample.EventSamples:
                    new WebViewHybridBridge(webView).Add(new ClassBridge<EventSamplesController>());
                    webView.LoadDataWithBaseURL("file:///android_asset/", new EventSamples().GenerateString(),
                        "text/html", "UTF-8", null);
                    break;
                case ESample.MessagingSamples:
                    var bridge = new WebViewHybridBridge(webView) {new HybridMessagingHandler()};
                    bridge.JavascriptTimeout = TimeSpan.FromSeconds(30);
                    bridge.Add(
                        new ClassBridge<MessagingSamplesController>().AddInstance(
                            new MessagingSamplesController(this, bridge.Get<HybridMessagingHandler>()), "Controller"));
                    webView.LoadDataWithBaseURL("file:///android_asset/", new MessagingSamples().GenerateString(),
                        "text/html", "UTF-8", null);
                    break;
                default:
                    StartActivity(typeof (SamplesActivity));
                    break;
            }
        }
    }
}