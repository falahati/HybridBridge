using System;
using Foundation;
using HybridBridge.iOS.Samples.Controllers;
using UIKit;

namespace HybridBridge.iOS.Samples.Resources
{
    public partial class Sample : UIViewController
    {
        private readonly ESample _sample;

        public Sample(ESample sample) : base("Sample", null)
        {
            _sample = sample;


        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();

            // Release any cached data, images, etc that aren't in use.
        }

        private void NavigateToHtml(string htmlName)
        {
            webView.LoadRequest(
                NSUrlRequest.FromUrl(
                    NSUrl.CreateFileUrl(NSBundle.MainBundle.PathForResource(htmlName, "html", "Views"), null)));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            webView.ScalesPageToFit = false;

            if (webView.ScrollView != null)
            {
                webView.ScrollView.DelaysContentTouches = false;
                webView.ScrollView.Bounces = false;
                webView.ScrollView.BouncesZoom = false;
                webView.ScrollView.AlwaysBounceHorizontal = false;
                webView.ScrollView.AlwaysBounceVertical = false;
                webView.ScrollView.MaximumZoomScale = 1;
                webView.ScrollView.MinimumZoomScale = 1;
            }
            webView.LoadFinished += (sender, args) =>
            {
                webView.EvaluateJavascript(@"
var cssStyle = document.createElement(""style"");
cssStyle.type = ""text/css"";
cssStyle.innerHTML = ""*:not(input[type=text]):not(textarea) {-webkit-touch-callout: none; -webkit-user-select: none;}"";
document.body.appendChild(cssStyle);
                ");
            };

            switch (_sample)
            {
                case ESample.MethodSamples:
                    new UIWebViewHybridBridge(webView).Add(
                        new ClassBridge<MethodSamplesController>().AddInstance(new MethodSamplesController(),
                            "Controller"));
                    NavigateToHtml(@"MethodSamples");
                    break;
                case ESample.PropertySamples:
                    new UIWebViewHybridBridge(webView).Add(
                        new ClassBridge<PropertySamplesController>().AddInstance(new PropertySamplesController(),
                            "Controller"));
                    NavigateToHtml(@"PropertySamples");
                    break;
                case ESample.EventSamples:
                    new UIWebViewHybridBridge(webView).Add(new ClassBridge<EventSamplesController>());
                    NavigateToHtml(@"EventSamples");
                    break;
                case ESample.MessagingSamples:
                    var bridge = new UIWebViewHybridBridge(webView) { new HybridMessagingHandler() };
                    bridge.JavascriptTimeout = TimeSpan.FromSeconds(30);
                    bridge.Add(
                        new ClassBridge<MessagingSamplesController>().AddInstance(
                            new MessagingSamplesController(bridge.Get<HybridMessagingHandler>()), "Controller"));
                    NavigateToHtml(@"MessagingSamples");
                    break;
                default:
                    (UIApplication.SharedApplication?.KeyWindow?.RootViewController as UINavigationController)?
                        .PopViewController(true);
                    break;
            }
        }
    }
}