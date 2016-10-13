using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Popups;
using HybridBridge.UWP.Samples.Models;

namespace HybridBridge.UWP.Samples.Controllers
{
    internal class MethodSamplesController
    {
        public void AlertMethod()
        {
            // ReSharper disable once UnusedVariable
            var uselessVariable = CoreApplication.MainView.CoreWindow.Dispatcher
                .RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    await
                        new MessageDialog("This is a simple AlertDialog created by the C# side")
                            .ShowAsync().AsTask();
                });
        }

        public SampleModel ComplexObjectMethod(int sampleInt)
        {
            return new SampleModel
            {
                IntValue = sampleInt,
                StringValue = "This object is created in C# with the value provided by the Javascript side"
            };
        }

        public static int StaticAddMethod(int a, int b)
        {
            return a + b;
        }
    }
}