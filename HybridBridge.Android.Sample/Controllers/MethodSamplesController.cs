using Android.App;
using HybridBridge.Android.Samples.Models;

namespace HybridBridge.Android.Samples.Controllers
{
    internal class MethodSamplesController
    {
        private readonly Activity _parentActivity;

        public MethodSamplesController(Activity parentActivity)
        {
            _parentActivity = parentActivity;
        }

        public void AlertMethod()
        {
            _parentActivity.RunOnUiThread(() =>
            {
                new AlertDialog.Builder(_parentActivity)
                    .SetMessage("This is a simple AlertDialog created by the C# side")
                    .Show();
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