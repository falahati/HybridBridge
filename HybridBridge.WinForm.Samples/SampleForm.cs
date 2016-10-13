using System;
using System.IO;
using System.Windows.Forms;
using HybridBridge.Win;
using HybridBridge.WinForm.Samples.Controllers;

namespace HybridBridge.WinForm.Samples
{
    internal partial class SampleForm : Form
    {
        private readonly ESample _eSample;

        public SampleForm()
        {
            InitializeComponent();
        }

        public SampleForm(ESample eSample) : this()
        {
            _eSample = eSample;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Activate();
            switch (_eSample)
            {
                case ESample.MethodSamples:
                    new WebBrowserHybridServer(webBrowser).Add(
                        new ClassBridge<MethodSamplesController>().AddInstance(new MethodSamplesController(),
                            "Controller"));
                    webBrowser.Navigate("file://" + Path.GetFullPath(@"Views/MethodSamples.html"));
                    break;
                case ESample.PropertySamples:
                    new WebBrowserHybridServer(webBrowser).Add(
                        new ClassBridge<PropertySamplesController>().AddInstance(new PropertySamplesController(),
                            "Controller"));
                    webBrowser.Navigate("file://" + Path.GetFullPath(@"Views/PropertySamples.html"));
                    break;
                case ESample.EventSamples:
                    new WebBrowserHybridServer(webBrowser).Add(new ClassBridge<EventSamplesController>());
                    webBrowser.Navigate("file://" + Path.GetFullPath(@"Views/EventSamples.html"));
                    break;
                case ESample.MessagingSamples:
                    var bridge = new WebBrowserHybridServer(webBrowser) {new HybridMessagingHandler()};
                    bridge.JavascriptTimeout = TimeSpan.FromSeconds(30);
                    bridge.Add(
                        new ClassBridge<MessagingSamplesController>().AddInstance(
                            new MessagingSamplesController(bridge.Get<HybridMessagingHandler>()), "Controller"));
                    webBrowser.Navigate("file://" + Path.GetFullPath(@"Views/MessagingSamples.html"));
                    break;
                default:
                    Close();
                    break;
            }
        }
    }
}