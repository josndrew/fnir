namespace Fall_Detector
{
    partial class SerialSetup
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
            this.cmbBitRate = new System.Windows.Forms.ComboBox();
            this.cmbPort = new System.Windows.Forms.ComboBox();
            this.cmbStopBit = new System.Windows.Forms.ComboBox();
            this.cmbParity = new System.Windows.Forms.ComboBox();
            this.cmbHandshake = new System.Windows.Forms.ComboBox();
            this.connect_btn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cmbBitRate
            // 
            this.cmbBitRate.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbBitRate.FormattingEnabled = true;
            this.cmbBitRate.Location = new System.Drawing.Point(12, 70);
            this.cmbBitRate.Name = "cmbBitRate";
            this.cmbBitRate.Size = new System.Drawing.Size(177, 23);
            this.cmbBitRate.TabIndex = 0;
            this.cmbBitRate.Text = "Baud Rate:";
            // 
            // cmbPort
            // 
            this.cmbPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbPort.FormattingEnabled = true;
            this.cmbPort.Location = new System.Drawing.Point(12, 12);
            this.cmbPort.Name = "cmbPort";
            this.cmbPort.Size = new System.Drawing.Size(177, 23);
            this.cmbPort.TabIndex = 1;
            this.cmbPort.Text = "COM Ports:";
            // 
            // cmbStopBit
            // 
            this.cmbStopBit.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbStopBit.FormattingEnabled = true;
            this.cmbStopBit.Location = new System.Drawing.Point(218, 12);
            this.cmbStopBit.Name = "cmbStopBit";
            this.cmbStopBit.Size = new System.Drawing.Size(177, 23);
            this.cmbStopBit.TabIndex = 2;
            this.cmbStopBit.Text = "Stop Bit:";
            // 
            // cmbParity
            // 
            this.cmbParity.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbParity.FormattingEnabled = true;
            this.cmbParity.Location = new System.Drawing.Point(218, 70);
            this.cmbParity.Name = "cmbParity";
            this.cmbParity.Size = new System.Drawing.Size(177, 23);
            this.cmbParity.TabIndex = 3;
            this.cmbParity.Text = "Parity:";
            // 
            // cmbHandshake
            // 
            this.cmbHandshake.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbHandshake.FormattingEnabled = true;
            this.cmbHandshake.Location = new System.Drawing.Point(12, 128);
            this.cmbHandshake.Name = "cmbHandshake";
            this.cmbHandshake.Size = new System.Drawing.Size(177, 23);
            this.cmbHandshake.TabIndex = 4;
            this.cmbHandshake.Text = "Handshake:";
            // 
            // connect_btn
            // 
            this.connect_btn.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.connect_btn.Location = new System.Drawing.Point(221, 120);
            this.connect_btn.Name = "connect_btn";
            this.connect_btn.Size = new System.Drawing.Size(174, 34);
            this.connect_btn.TabIndex = 5;
            this.connect_btn.Text = "CONNECT";
            this.connect_btn.UseVisualStyleBackColor = true;
            this.connect_btn.Click += new System.EventHandler(this.connect_btn_Click);
            // 
            // SerialSetup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(401, 163);
            this.Controls.Add(this.connect_btn);
            this.Controls.Add(this.cmbHandshake);
            this.Controls.Add(this.cmbParity);
            this.Controls.Add(this.cmbStopBit);
            this.Controls.Add(this.cmbPort);
            this.Controls.Add(this.cmbBitRate);
            this.Name = "SerialSetup";
            this.Text = "Adruino Setup";
            this.Load += new System.EventHandler(this.SerialSetup_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbBitRate;
        private System.Windows.Forms.ComboBox cmbPort;
        private System.Windows.Forms.ComboBox cmbStopBit;
        private System.Windows.Forms.ComboBox cmbParity;
        private System.Windows.Forms.ComboBox cmbHandshake;
        private System.Windows.Forms.Button connect_btn;
    }
}