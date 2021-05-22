
namespace SaveConcert
{
    partial class SetListView
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
            this.panelForData = new System.Windows.Forms.Panel();
            this.labelArtist = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // panelForData
            // 
            this.panelForData.AutoScroll = true;
            this.panelForData.Location = new System.Drawing.Point(12, 41);
            this.panelForData.Name = "panelForData";
            this.panelForData.Size = new System.Drawing.Size(776, 397);
            this.panelForData.TabIndex = 0;
            // 
            // labelArtist
            // 
            this.labelArtist.AutoSize = true;
            this.labelArtist.Location = new System.Drawing.Point(12, 9);
            this.labelArtist.Name = "labelArtist";
            this.labelArtist.Size = new System.Drawing.Size(42, 13);
            this.labelArtist.TabIndex = 1;
            this.labelArtist.Text = "Artista: ";
            // 
            // SetListView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.labelArtist);
            this.Controls.Add(this.panelForData);
            this.Name = "SetListView";
            this.Text = "SetListView";
            this.Load += new System.EventHandler(this.SetListView_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelForData;
        private System.Windows.Forms.Label labelArtist;
    }
}