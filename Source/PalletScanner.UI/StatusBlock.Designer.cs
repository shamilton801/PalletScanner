namespace PalletScanner.UI
{
    partial class StatusBlock
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            MessageLabel = new Label();
            DropDownLabel = new Label();
            TypeLabel = new Label();
            SuspendLayout();
            // 
            // MessageLabel
            // 
            MessageLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            MessageLabel.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            MessageLabel.Location = new Point(80, 0);
            MessageLabel.Name = "MessageLabel";
            MessageLabel.Size = new Size(374, 40);
            MessageLabel.TabIndex = 0;
            MessageLabel.TextAlign = ContentAlignment.MiddleLeft;
            MessageLabel.DoubleClick += DropDownLabel_Click;
            // 
            // DropDownLabel
            // 
            DropDownLabel.Location = new Point(0, 0);
            DropDownLabel.Name = "DropDownLabel";
            DropDownLabel.Size = new Size(40, 40);
            DropDownLabel.TabIndex = 1;
            DropDownLabel.Click += DropDownLabel_Click;
            // 
            // TypeLabel
            // 
            TypeLabel.Location = new Point(40, 0);
            TypeLabel.Name = "TypeLabel";
            TypeLabel.Size = new Size(40, 40);
            TypeLabel.TabIndex = 2;
            TypeLabel.DoubleClick += DropDownLabel_Click;
            // 
            // StatusBlock
            // 
            Controls.Add(TypeLabel);
            Controls.Add(DropDownLabel);
            Controls.Add(MessageLabel);
            Name = "StatusBlock";
            Size = new Size(500, 40);
            DoubleClick += DropDownLabel_Click;
            ResumeLayout(false);
        }

        #endregion

        private Label MessageLabel;
        private Label DropDownLabel;
        private Label TypeLabel;
    }
}
