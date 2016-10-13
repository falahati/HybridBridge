using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;

namespace HybridBridge.Android.Samples
{
    [Activity(Label = "Samples", MainLauncher = true)]
    public class SamplesActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Samples);
            FindViewById<Button>(Resource.Id.methodSamples).Click += (sender, args) =>
            {
                var activity = new Intent(this, typeof (WebViewActivity));
                activity.PutExtra("Sample", (int) ESample.MethodSamples);
                StartActivity(activity);
            };
            FindViewById<Button>(Resource.Id.propertySamples).Click += (sender, args) =>
            {
                var activity = new Intent(this, typeof (WebViewActivity));
                activity.PutExtra("Sample", (int) ESample.PropertySamples);
                StartActivity(activity);
            };
            FindViewById<Button>(Resource.Id.eventSamples).Click += (sender, args) =>
            {
                var activity = new Intent(this, typeof (WebViewActivity));
                activity.PutExtra("Sample", (int) ESample.EventSamples);
                StartActivity(activity);
            };
            FindViewById<Button>(Resource.Id.messagingSamples).Click += (sender, args) =>
            {
                var activity = new Intent(this, typeof (WebViewActivity));
                activity.PutExtra("Sample", (int) ESample.MessagingSamples);
                StartActivity(activity);
            };
        }
    }
}