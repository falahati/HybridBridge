using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using HybridBridge.UWP.Samples.Controllers;

namespace HybridBridge.UWP.Samples
{
    public sealed partial class Sample : Page
    {
        public Sample()
        {
            InitializeComponent();
            webView.ScriptNotify += WebView_ScriptNotify;
        }

        private async void WebView_ScriptNotify(object sender, NotifyEventArgs e)
        {
            await new MessageDialog(e.Value, e.CallingUri + " says:").ShowAsync();
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            GoBack();
        }

        private async void GoBack()
        {
            await
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => { (Window.Current.Content as Frame)?.GoBack(); });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter != null)
            {
                switch ((ESample) e.Parameter)
                {
                    case ESample.MethodSamplesAsync:
                        new WebViewHybridBridge(webView).Add(
                            new ClassBridge<MethodSamplesController>().AddInstance(new MethodSamplesController(),
                                "Controller"));
                        webView.Navigate(new Uri(@"ms-appx-web:///Views/MethodSamplesAsync.html"));
                        break;
                    case ESample.PropertySamplesAsync:
                        new WebViewHybridBridge(webView).Add(
                            new ClassBridge<PropertySamplesControllerAsync>().AddInstance(new PropertySamplesControllerAsync(),
                                "Controller"));
                        webView.Navigate(new Uri(@"ms-appx-web:///Views/PropertySamplesAsync.html"));
                        break;
                    case ESample.EventSamplesAsync:
                        new WebViewHybridBridge(webView).Add(new ClassBridge<EventSamplesController>());
                        webView.Navigate(new Uri(@"ms-appx-web:///Views/EventSamplesAsync.html"));
                        break;
                    case ESample.MessagingSamplesAsync:
                        var bridgeAsync = new WebViewHybridBridge(webView) {new HybridMessagingHandler()};
                        bridgeAsync.JavascriptTimeout = TimeSpan.FromSeconds(30);
                        bridgeAsync.Add(
                            new ClassBridge<MessagingSamplesController>().AddInstance(
                                new MessagingSamplesController(bridgeAsync.Get<HybridMessagingHandler>()), "Controller"));
                        webView.Navigate(new Uri(@"ms-appx-web:///Views/MessagingSamplesAsync.html"));
                        break;
                    case ESample.MethodSamples:
                        new WebViewHybridServer(webView).Add(
                            new ClassBridge<MethodSamplesController>().AddInstance(new MethodSamplesController(),
                                "Controller"));
                        webView.Navigate(new Uri(@"ms-appx-web:///Views/MethodSamples.html"));
                        break;
                    case ESample.PropertySamples:
                        new WebViewHybridServer(webView).Add(
                            new ClassBridge<PropertySamplesController>().AddInstance(new PropertySamplesController(),
                                "Controller"));
                        webView.Navigate(new Uri(@"ms-appx-web:///Views/PropertySamples.html"));
                        break;
                    case ESample.EventSamples:
                        new WebViewHybridServer(webView).Add(new ClassBridge<EventSamplesController>());
                        webView.Navigate(new Uri(@"ms-appx-web:///Views/EventSamples.html"));
                        break;
                    case ESample.MessagingSamples:
                        var bridge = new WebViewHybridServer(webView) { new HybridMessagingHandler() };
                        bridge.JavascriptTimeout = TimeSpan.FromSeconds(30);
                        bridge.Add(
                            new ClassBridge<MessagingSamplesController>().AddInstance(
                                new MessagingSamplesController(bridge.Get<HybridMessagingHandler>()), "Controller"));
                        webView.Navigate(new Uri(@"ms-appx-web:///Views/MessagingSamples.html"));
                        break;
                    default:
                        GoBack();
                        break;
                }
            }
        }
    }
}