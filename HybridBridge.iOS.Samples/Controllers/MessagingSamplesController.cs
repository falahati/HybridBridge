using CoreFoundation;
using HybridBridge.iOS.Samples.Models;
using UIKit;

namespace HybridBridge.iOS.Samples.Controllers
{
    internal class MessagingSamplesController
    {
        public MessagingSamplesController(HybridMessagingHandler hybridMessaging)
        {
            hybridMessaging.Subscribe("", () =>
            {
                var xResult = hybridMessaging.Send<int, string>("NewMessageID",
                    "This is a string message argument send from C# side. We expect an int as the result of this event.");
                DispatchQueue.MainQueue.DispatchAsync(() =>
                {
                    using (
                        var alertView = new UIAlertView(@"C# UIAlertView",
                            @"This is a simple AlertDialog created by the C# side for all messages. We sent a message with message identification of: 'NewMessageID' and result of that message was: " +
                            (xResult == default(int) ? "[NODATA]" : xResult.ToString()),
                            null, @"Ok", null))
                    {
                        alertView.Show();
                    }
                });
            });
            hybridMessaging.Subscribe("MyMessageID", (SampleModel model) =>
            {
                DispatchQueue.MainQueue.DispatchAsync(() =>
                {
                    using (
                        var alertView = new UIAlertView(@"C# UIAlertView",
                            @"This is a simple AlertDialog created by the C# side for 'MyMessageID' message. Model.StringValue = " +
                            model.StringValue,
                            null, @"Ok", null))
                    {
                        alertView.Show();
                    }
                });
            });
        }
    }
}