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
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            MainView = new Microsoft.Web.WebView2.WinForms.WebView2();
            tabStatus = new TabPage();
            lblChatStatus = new Label();
            tabPage2 = new TabPage();
            txtLogs = new TextBox();
            lblObs = new Label();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)MainView).BeginInit();
            tabStatus.SuspendLayout();
            tabPage2.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabStatus);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Location = new Point(3, 1);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1260, 864);
            tabControl1.TabIndex = 1;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(MainView);
            tabPage1.Location = new Point(4, 29);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(1252, 831);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Main";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // MainView
            // 
            MainView.AllowExternalDrop = true;
            MainView.CreationProperties = null;
            MainView.DefaultBackgroundColor = Color.White;
            MainView.Location = new Point(6, 6);
            MainView.Name = "MainView";
            MainView.Size = new Size(1240, 819);
            MainView.TabIndex = 1;
            MainView.ZoomFactor = 1D;
            // 
            // tabStatus
            // 
            tabStatus.Controls.Add(lblObs);
            tabStatus.Controls.Add(lblChatStatus);
            tabStatus.Location = new Point(4, 29);
            tabStatus.Name = "tabStatus";
            tabStatus.Padding = new Padding(3);
            tabStatus.Size = new Size(1252, 831);
            tabStatus.TabIndex = 2;
            tabStatus.Text = "Status";
            tabStatus.UseVisualStyleBackColor = true;
            // 
            // lblChatStatus
            // 
            lblChatStatus.AutoSize = true;
            lblChatStatus.Location = new Point(16, 19);
            lblChatStatus.Name = "lblChatStatus";
            lblChatStatus.Size = new Size(42, 20);
            lblChatStatus.TabIndex = 0;
            lblChatStatus.Text = "Chat:";
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(txtLogs);
            tabPage2.Location = new Point(4, 29);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(1252, 831);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Logs";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // txtLogs
            // 
            txtLogs.Location = new Point(5, 3);
            txtLogs.Multiline = true;
            txtLogs.Name = "txtLogs";
            txtLogs.Size = new Size(1241, 823);
            txtLogs.TabIndex = 0;
            // 
            // lblObs
            // 
            lblObs.AutoSize = true;
            lblObs.Location = new Point(16, 48);
            lblObs.Name = "lblObs";
            lblObs.Size = new Size(40, 20);
            lblObs.TabIndex = 1;
            lblObs.Text = "OBS:";
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1266, 868);
            Controls.Add(tabControl1);
            Name = "Main";
            Text = "Stream Artist";
            Load += Main_Load;
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)MainView).EndInit();
            tabStatus.ResumeLayout(false);
            tabStatus.PerformLayout();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl1;
        private TabPage tabPage1;
        private Microsoft.Web.WebView2.WinForms.WebView2 MainView;
        private TabPage tabPage2;
        public TextBox txtLogs;
        private TabPage tabStatus;
        public Label lblChatStatus;
        public Label lblObs;
    }
}