using System.Windows.Forms;
using HybridBridge.WinForm.Samples.Models;

namespace HybridBridge.WinForm.Samples.Controllers
{
    internal class MethodSamplesController
    {
        
        public void AlertMethod()
        {
            MessageBox.Show(@"This is a simple AlertDialog created by the C# side");
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