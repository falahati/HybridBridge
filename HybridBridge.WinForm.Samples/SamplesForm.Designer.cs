namespace HybridBridge.WinForm.Samples
{
    partial class SamplesForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.methodSamples = new System.Windows.Forms.Button();
            this.propertySamples = new System.Windows.Forms.Button();
            this.eventSamples = new System.Windows.Forms.Button();
            this.messagingSamples = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // methodSamples
            // 
            this.methodSamples.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.methodSamples.Location = new System.Drawing.Point(12, 12);
            this.methodSamples.Name = "methodSamples";
            this.methodSamples.Size = new System.Drawing.Size(360, 23);
            this.methodSamples.TabIndex = 0;
            this.methodSamples.Text = "Class Method Samples";
            this.methodSamples.UseVisualStyleBackColor = true;
            this.methodSamples.Click += new System.EventHandler(this.SampleButton_Click);
            // 
            // propertySamples
            // 
            this.propertySamples.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertySamples.Location = new System.Drawing.Point(12, 41);
            this.propertySamples.Name = "propertySamples";
            this.propertySamples.Size = new System.Drawing.Size(360, 23);
            this.propertySamples.TabIndex = 1;
            this.propertySamples.Text = "Class Property Samples";
            this.propertySamples.UseVisualStyleBackColor = true;
            this.propertySamples.Click += new System.EventHandler(this.SampleButton_Click);
            // 
            // eventSamples
            // 
            this.eventSamples.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.eventSamples.Location = new System.Drawing.Point(12, 70);
            this.eventSamples.Name = "eventSamples";
            this.eventSamples.Size = new System.Drawing.Size(360, 23);
            this.eventSamples.TabIndex = 2;
            this.eventSamples.Text = "Event Samples";
            this.eventSamples.UseVisualStyleBackColor = true;
            this.eventSamples.Click += new System.EventHandler(this.SampleButton_Click);
            // 
            // messagingSamples
            // 
            this.messagingSamples.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.messagingSamples.Location = new System.Drawing.Point(12, 99);
            this.messagingSamples.Name = "messagingSamples";
            this.messagingSamples.Size = new System.Drawing.Size(360, 23);
            this.messagingSamples.TabIndex = 3;
            this.messagingSamples.Text = "Messaging Samples";
            this.messagingSamples.UseVisualStyleBackColor = true;
            this.messagingSamples.Click += new System.EventHandler(this.SampleButton_Click);
            // 
            // SamplesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 138);
            this.Controls.Add(this.messagingSamples);
            this.Controls.Add(this.eventSamples);
            this.Controls.Add(this.propertySamples);
            this.Controls.Add(this.methodSamples);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SamplesForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Samples";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button methodSamples;
        private System.Windows.Forms.Button propertySamples;
        private System.Windows.Forms.Button eventSamples;
        private System.Windows.Forms.Button messagingSamples;
    }
}