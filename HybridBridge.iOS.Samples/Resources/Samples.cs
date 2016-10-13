using UIKit;

namespace HybridBridge.iOS.Samples.Resources
{
    public partial class Samples : UIViewController
    {
        public Samples() : base("Samples", null)
        {
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();

            // Release any cached data, images, etc that aren't in use.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Perform any additional setup after loading the view, typically from a nib.
        }

        private void NavigateToSample(ESample sample)
        {
            (UIApplication.SharedApplication?.KeyWindow?.RootViewController as UINavigationController)?
                .PushViewController(new Sample(sample), true);
        }

        partial void PropertySamples_TouchUpInside(UIButton sender)
        {
            NavigateToSample(ESample.PropertySamples);
        }

        partial void MethodSamples_TouchUpInside(UIButton sender)
        {
            NavigateToSample(ESample.MethodSamples);
        }

        partial void MessagingSamples_TouchUpInside(UIButton sender)
        {
            NavigateToSample(ESample.MessagingSamples);
        }

        partial void EventSamples_TouchUpInside(UIButton sender)
        {
            NavigateToSample(ESample.EventSamples);
        }
    }
}