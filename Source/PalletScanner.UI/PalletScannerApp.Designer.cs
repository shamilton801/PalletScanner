
namespace PalletScanner.UI
{
    partial class PalletScannerApp
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            ValidationStatusPanel = new Panel();
            BarcodesPanel = new Panel();
            tableLayoutPanel1 = new TableLayoutPanel();
            StartButton = new Button();
            StopButton = new Button();
            TimedScanButton = new Button();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // ValidationStatusPanel
            // 
            ValidationStatusPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            ValidationStatusPanel.AutoScroll = true;
            ValidationStatusPanel.Location = new Point(12, 12);
            ValidationStatusPanel.Name = "ValidationStatusPanel";
            ValidationStatusPanel.Size = new Size(604, 591);
            ValidationStatusPanel.TabIndex = 0;
            // 
            // BarcodesPanel
            // 
            BarcodesPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            BarcodesPanel.AutoScroll = true;
            BarcodesPanel.Location = new Point(622, 12);
            BarcodesPanel.Name = "BarcodesPanel";
            BarcodesPanel.Size = new Size(350, 637);
            BarcodesPanel.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            tableLayoutPanel1.ColumnCount = 5;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 3F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333359F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 3F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            tableLayoutPanel1.Controls.Add(StartButton, 0, 0);
            tableLayoutPanel1.Controls.Add(StopButton, 2, 0);
            tableLayoutPanel1.Controls.Add(TimedScanButton, 4, 0);
            tableLayoutPanel1.Location = new Point(12, 609);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(604, 40);
            tableLayoutPanel1.TabIndex = 3;
            // 
            // StartButton
            // 
            StartButton.Dock = DockStyle.Fill;
            StartButton.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            StartButton.Location = new Point(0, 0);
            StartButton.Margin = new Padding(0);
            StartButton.Name = "StartButton";
            StartButton.Size = new Size(199, 40);
            StartButton.TabIndex = 2;
            StartButton.Text = "Start Scanning";
            StartButton.UseVisualStyleBackColor = true;
            StartButton.Click += StartButton_Click;
            // 
            // StopButton
            // 
            StopButton.Dock = DockStyle.Fill;
            StopButton.Enabled = false;
            StopButton.Font = new Font("Segoe UI", 14.25F);
            StopButton.Location = new Point(202, 0);
            StopButton.Margin = new Padding(0);
            StopButton.Name = "StopButton";
            StopButton.Size = new Size(199, 40);
            StopButton.TabIndex = 3;
            StopButton.Text = "Stop Scanning";
            StopButton.UseVisualStyleBackColor = true;
            StopButton.Click += StopButton_Click;
            // 
            // TimedScanButton
            // 
            TimedScanButton.Dock = DockStyle.Fill;
            TimedScanButton.Font = new Font("Segoe UI", 14.25F);
            TimedScanButton.Location = new Point(404, 0);
            TimedScanButton.Margin = new Padding(0);
            TimedScanButton.Name = "TimedScanButton";
            TimedScanButton.Size = new Size(200, 40);
            TimedScanButton.TabIndex = 4;
            TimedScanButton.Text = "Timed Scan";
            TimedScanButton.UseVisualStyleBackColor = true;
            TimedScanButton.Click += TimedScanButton_Click;
            // 
            // PalletScannerApp
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(984, 661);
            Controls.Add(tableLayoutPanel1);
            Controls.Add(BarcodesPanel);
            Controls.Add(ValidationStatusPanel);
            Name = "PalletScannerApp";
            Text = "Pallet Scanner";
            FormClosed += PalletScannerApp_FormClosed;
            tableLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel ValidationStatusPanel;
        private Panel BarcodesPanel;
        private TableLayoutPanel tableLayoutPanel1;
        private Button StartButton;
        private Button StopButton;
        private Button TimedScanButton;
    }
}
