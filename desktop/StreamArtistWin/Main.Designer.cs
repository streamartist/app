using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using SizeF = System.Drawing.SizeF;

namespace StreamArtist
{
    partial class Main
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
            MainView = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)MainView).BeginInit();
            SuspendLayout();
            // 
            // MainView
            // 
            MainView.AllowExternalDrop = true;
            MainView.CreationProperties = null;
            MainView.DefaultBackgroundColor = Color.White;
            MainView.Location = new Point(2, 1);
            MainView.Name = "MainView";
            MainView.Size = new Size(1262, 862);
            MainView.TabIndex = 0;
            MainView.ZoomFactor = 1D;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1266, 868);
            Controls.Add(MainView);
            Name = "Main";
            Text = "Stream Artist";
            Load += Main_Load;
            ((System.ComponentModel.ISupportInitialize)MainView).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 MainView;
    }
}