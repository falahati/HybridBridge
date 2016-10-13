using System.Windows;

namespace HybridBridge.WPF.Samples
{
    /// <summary>
    ///     Interaction logic for Samples.xaml
    /// </summary>
    public partial class Samples : Window
    {
        public Samples()
        {
            InitializeComponent();
        }

        private void methodSamples_Click(object sender, RoutedEventArgs e)
        {
            ShowSample(ESample.MethodSamples);
        }

        private void propertySamples_Click(object sender, RoutedEventArgs e)
        {
            ShowSample(ESample.PropertySamples);
        }

        private void eventSamples_Click(object sender, RoutedEventArgs e)
        {
            ShowSample(ESample.EventSamples);
        }

        private void messagingSamples_Click(object sender, RoutedEventArgs e)
        {
            ShowSample(ESample.MessagingSamples);
        }

        private void ShowSample(ESample eSample)
        {
            Hide();
            new Sample(eSample).ShowDialog();
            Show();
            Activate();
        }
    }
}