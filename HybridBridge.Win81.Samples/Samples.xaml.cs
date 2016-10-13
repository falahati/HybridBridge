using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace HybridBridge.Win81.Samples
{
    public sealed partial class Samples : Page
    {
        public Samples()
        {
            InitializeComponent();
        }

        private void methodSamples_Click(object sender, RoutedEventArgs e)
        {

            CreateView(ESample.MethodSamples);
        }

        private void propertySamples_Click(object sender, RoutedEventArgs e)
        {
            CreateView(ESample.PropertySamples);
        }

        private void eventSamples_Click(object sender, RoutedEventArgs e)
        {
            CreateView(ESample.EventSamples);
        }

        private void messagingSamples_Click(object sender, RoutedEventArgs e)
        {
            CreateView(ESample.MessagingSamples);
        }

        private void methodSamplesAsync_Click(object sender, RoutedEventArgs e)
        {
            
            CreateView(ESample.MethodSamplesAsync);
        }

        private void propertySamplesAsync_Click(object sender, RoutedEventArgs e)
        {
            CreateView(ESample.PropertySamplesAsync);
        }

        private void eventSamplesAsync_Click(object sender, RoutedEventArgs e)
        {
            CreateView(ESample.EventSamplesAsync);
        }

        private void messagingSamplesAsync_Click(object sender, RoutedEventArgs e)
        {
            CreateView(ESample.MessagingSamplesAsync);
        }

        private async void CreateView(ESample sample)
        {
            await
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => { (Window.Current.Content as Frame)?.Navigate(typeof (Win81.Samples.Sample), sample); });
        }
    }
}