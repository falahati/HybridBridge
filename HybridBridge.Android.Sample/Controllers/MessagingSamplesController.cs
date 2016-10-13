using Android.App;
using HybridBridge.Android.Samples.Models;

namespace HybridBridge.Android.Samples.Controllers
{
    internal class MessagingSamplesController
    {
        public MessagingSamplesController(Activity parentActivity, HybridMessagingHandler hybridMessaging)
        {
            hybridMessaging.Subscribe("", () =>
            {
                var xResult = hybridMessaging.Send<int, string>("NewMessageID",
                    "This is a string message argument send from C# side. We expect an int as the result of this event.");
                parentActivity.RunOnUiThread(() =>
                {
                    new AlertDialog.Builder(parentActivity)
                        .SetMessage(
                            "This is a simple AlertDialog created by the C# side for all messages. We sent a message with message identification of: 'NewMessageID' and result of that message was: " +
                            (xResult == default(int) ? "[NODATA]" : xResult.ToString()))
                        .Show();
                });
            });
            hybridMessaging.Subscribe("MyMessageID", (SampleModel model) =>
            {
                parentActivity.RunOnUiThread(() =>
                {
                    new AlertDialog.Builder(parentActivity)
                        .SetMessage(
                            "This is a simple AlertDialog created by the C# side for 'MyMessageID' message. Model.StringValue = " +
                            model.StringValue)
                        .Show();
                });
            });
        }
    }
}