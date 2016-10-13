// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace HybridBridge.iOS.Samples.Resources
{
    [Register ("ViewController1")]
    partial class Samples
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton eventSamples { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton messagingSamples { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton methodSamples { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton propertySamples { get; set; }

        [Action ("EventSamples_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void EventSamples_TouchUpInside (UIKit.UIButton sender);

        [Action ("MessagingSamples_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void MessagingSamples_TouchUpInside (UIKit.UIButton sender);

        [Action ("MethodSamples_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void MethodSamples_TouchUpInside (UIKit.UIButton sender);

        [Action ("PropertySamples_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void PropertySamples_TouchUpInside (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (eventSamples != null) {
                eventSamples.Dispose ();
                eventSamples = null;
            }

            if (messagingSamples != null) {
                messagingSamples.Dispose ();
                messagingSamples = null;
            }

            if (methodSamples != null) {
                methodSamples.Dispose ();
                methodSamples = null;
            }

            if (propertySamples != null) {
                propertySamples.Dispose ();
                propertySamples = null;
            }
        }
    }
}