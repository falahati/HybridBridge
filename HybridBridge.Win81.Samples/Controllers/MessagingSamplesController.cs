using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Popups;
using HybridBridge.Win81.Samples.Models;

namespace HybridBridge.Win81.Samples.Controllers
{
    internal class MessagingSamplesController
    {
        public MessagingSamplesController(HybridMessagingHandler hybridMessaging)
        {
            hybridMessaging.Subscribe("", () =>
            {
                var xResult = hybridMessaging.Send<int, string>("NewMessageID",
                    "This is a string message argument send from C# side. We expect an int as the result of this event.");
                // ReSharper disable once UnusedVariable
                var uselessVariable = CoreApplication.MainView.CoreWindow.Dispatcher
                    .RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        await new MessageDialog(
                            "This is a simple AlertDialog created by the C# side for all messages. We sent a message with message identification of: 'NewMessageID' and result of that message was: " +
                            (xResult == default(int) ? "[NODATA]" : xResult.ToString())).ShowAsync().AsTask();
                    });
            });
            hybridMessaging.Subscribe("MyMessageID", (SampleModel model) =>
            {
                // ReSharper disable once UnusedVariable
                var uselessVariable = CoreApplication.MainView.CoreWindow.Dispatcher
                    .RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        await
                            new MessageDialog(
                                "This is a simple AlertDialog created by the C# side for 'MyMessageID' message. Model.StringValue = " +
                                model.StringValue).ShowAsync().AsTask();
                    });
            });
        }
    }
}