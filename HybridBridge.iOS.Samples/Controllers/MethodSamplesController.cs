using CoreFoundation;
using HybridBridge.iOS.Samples.Models;
using UIKit;

namespace HybridBridge.iOS.Samples.Controllers
{
    internal class MethodSamplesController
    {
        
        public void AlertMethod()
        {
            DispatchQueue.MainQueue.DispatchAsync(() => {
                using (
                    var alertView = new UIAlertView(@"C# UIAlertView", @"This is a simple AlertDialog created by the C# side",
                        null, @"Ok", null))
                {
                    alertView.Show();
                }
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