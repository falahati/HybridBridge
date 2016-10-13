namespace HybridBridge.UWP.Samples.Models
{
    internal class SampleModel
    {
        public SampleModel()
        {
        }

        public SampleModel(int intValue, string stringValue)
        {
            IntValue = intValue;
            StringValue = stringValue;
        }

        public int IntValue { get; set; }

        public string StringValue { get; set; }
    }
}