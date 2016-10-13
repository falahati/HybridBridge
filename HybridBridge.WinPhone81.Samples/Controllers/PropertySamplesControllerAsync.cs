using System;

namespace HybridBridge.WinPhone81.Samples.Controllers
{
    internal class PropertySamplesControllerAsync
    {
        public int IntProperty { get; set; } = 5;

        public static string ReadOnlyProperty
        {
            // ReSharper disable once ConvertPropertyToExpressionBody
            get { return "Custom Property Get Method, current time is: " + DateTime.Now; }
        }
    }
}