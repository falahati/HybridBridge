using System;
using System.Windows.Forms;

namespace HybridBridge.WinForm.Samples
{
    internal partial class SamplesForm : Form
    {
        public SamplesForm()
        {
            InitializeComponent();
        }

        private void SampleButton_Click(object sender, EventArgs e)
        {
            if (sender == methodSamples)
                ShowSample(ESample.MethodSamples);
            else if (sender == propertySamples)
                ShowSample(ESample.PropertySamples);
            else if (sender == eventSamples)
                ShowSample(ESample.EventSamples);
            else if (sender == messagingSamples)
                ShowSample(ESample.MessagingSamples);
        }

        private void ShowSample(ESample eSample)
        {
            Hide();
            new SampleForm(eSample).ShowDialog();
            Show();
            Activate();
        }
    }
}