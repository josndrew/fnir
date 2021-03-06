﻿namespace GUI
{
    partial class SettingsSetup
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
            this.saveButton = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
            this.textBox8 = new System.Windows.Forms.TextBox();
            this.totalDistance = new System.Windows.Forms.NumericUpDown();
            this.textBox9 = new System.Windows.Forms.TextBox();
            this.textBox10 = new System.Windows.Forms.TextBox();
            this.numericUpDown5 = new System.Windows.Forms.NumericUpDown();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.numericUpDown7 = new System.Windows.Forms.NumericUpDown();
            this.textBox12 = new System.Windows.Forms.TextBox();
            this.textBox7 = new System.Windows.Forms.TextBox();
            this.numericUpDown6 = new System.Windows.Forms.NumericUpDown();
            this.textBox6 = new System.Windows.Forms.TextBox();
            this.numericUpDown4 = new System.Windows.Forms.NumericUpDown();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.numericUpDown3 = new System.Windows.Forms.NumericUpDown();
            this.textBox13 = new System.Windows.Forms.TextBox();
            this.numericUpDown9 = new System.Windows.Forms.NumericUpDown();
            this.textBox14 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.totalDistance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown9)).BeginInit();
            this.SuspendLayout();
            // 
            // saveButton
            // 
            this.saveButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.saveButton.Location = new System.Drawing.Point(537, 326);
            this.saveButton.Margin = new System.Windows.Forms.Padding(4);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(167, 44);
            this.saveButton.TabIndex = 0;
            this.saveButton.Text = "SAVE";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // textBox2
            // 
            this.textBox2.BackColor = System.Drawing.SystemColors.Window;
            this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox2.Location = new System.Drawing.Point(9, 72);
            this.textBox2.Margin = new System.Windows.Forms.Padding(4);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(137, 25);
            this.textBox2.TabIndex = 5;
            this.textBox2.Text = "Wavelength 1";
            // 
            // textBox4
            // 
            this.textBox4.BackColor = System.Drawing.SystemColors.Window;
            this.textBox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox4.Location = new System.Drawing.Point(9, 150);
            this.textBox4.Margin = new System.Windows.Forms.Padding(4);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(133, 15);
            this.textBox4.TabIndex = 8;
            this.textBox4.Text = "Hb Extinction Coef:";
            // 
            // numericUpDown2
            // 
            this.numericUpDown2.DecimalPlaces = 3;
            this.numericUpDown2.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.numericUpDown2.Location = new System.Drawing.Point(138, 148);
            this.numericUpDown2.Margin = new System.Windows.Forms.Padding(4);
            this.numericUpDown2.Name = "numericUpDown2";
            this.numericUpDown2.Size = new System.Drawing.Size(80, 22);
            this.numericUpDown2.TabIndex = 9;
            // 
            // textBox8
            // 
            this.textBox8.BackColor = System.Drawing.SystemColors.Window;
            this.textBox8.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox8.Location = new System.Drawing.Point(287, 6);
            this.textBox8.Margin = new System.Windows.Forms.Padding(4);
            this.textBox8.Multiline = true;
            this.textBox8.Name = "textBox8";
            this.textBox8.Size = new System.Drawing.Size(125, 34);
            this.textBox8.TabIndex = 15;
            this.textBox8.Text = "Distance between Source & Detector";
            // 
            // totalDistance
            // 
            this.totalDistance.DecimalPlaces = 2;
            this.totalDistance.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.totalDistance.Location = new System.Drawing.Point(416, 9);
            this.totalDistance.Margin = new System.Windows.Forms.Padding(4);
            this.totalDistance.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.totalDistance.Name = "totalDistance";
            this.totalDistance.Size = new System.Drawing.Size(80, 22);
            this.totalDistance.TabIndex = 16;
            // 
            // textBox9
            // 
            this.textBox9.BackColor = System.Drawing.SystemColors.Window;
            this.textBox9.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox9.Location = new System.Drawing.Point(504, 11);
            this.textBox9.Margin = new System.Windows.Forms.Padding(4);
            this.textBox9.Multiline = true;
            this.textBox9.Name = "textBox9";
            this.textBox9.Size = new System.Drawing.Size(49, 22);
            this.textBox9.TabIndex = 17;
            this.textBox9.Text = "cm";
            // 
            // textBox10
            // 
            this.textBox10.BackColor = System.Drawing.SystemColors.Window;
            this.textBox10.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox10.Location = new System.Drawing.Point(9, 105);
            this.textBox10.Margin = new System.Windows.Forms.Padding(4);
            this.textBox10.Name = "textBox10";
            this.textBox10.Size = new System.Drawing.Size(105, 15);
            this.textBox10.TabIndex = 18;
            this.textBox10.Text = "Normalized DPF:";
            // 
            // numericUpDown5
            // 
            this.numericUpDown5.DecimalPlaces = 3;
            this.numericUpDown5.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.numericUpDown5.Location = new System.Drawing.Point(138, 103);
            this.numericUpDown5.Margin = new System.Windows.Forms.Padding(4);
            this.numericUpDown5.Name = "numericUpDown5";
            this.numericUpDown5.Size = new System.Drawing.Size(80, 22);
            this.numericUpDown5.TabIndex = 19;
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.Window;
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Location = new System.Drawing.Point(281, 152);
            this.textBox1.Margin = new System.Windows.Forms.Padding(4);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(133, 15);
            this.textBox1.TabIndex = 22;
            this.textBox1.Text = "HbR Extinction Coef:";
            // 
            // numericUpDown7
            // 
            this.numericUpDown7.DecimalPlaces = 3;
            this.numericUpDown7.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.numericUpDown7.Location = new System.Drawing.Point(429, 148);
            this.numericUpDown7.Margin = new System.Windows.Forms.Padding(4);
            this.numericUpDown7.Name = "numericUpDown7";
            this.numericUpDown7.Size = new System.Drawing.Size(80, 22);
            this.numericUpDown7.TabIndex = 23;
            // 
            // textBox12
            // 
            this.textBox12.BackColor = System.Drawing.SystemColors.Window;
            this.textBox12.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox12.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox12.Location = new System.Drawing.Point(9, 213);
            this.textBox12.Margin = new System.Windows.Forms.Padding(4);
            this.textBox12.Multiline = true;
            this.textBox12.Name = "textBox12";
            this.textBox12.Size = new System.Drawing.Size(137, 25);
            this.textBox12.TabIndex = 24;
            this.textBox12.Text = "Wavelength 2";
            // 
            // textBox7
            // 
            this.textBox7.BackColor = System.Drawing.SystemColors.Window;
            this.textBox7.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox7.Location = new System.Drawing.Point(9, 292);
            this.textBox7.Margin = new System.Windows.Forms.Padding(4);
            this.textBox7.Name = "textBox7";
            this.textBox7.Size = new System.Drawing.Size(133, 15);
            this.textBox7.TabIndex = 27;
            this.textBox7.Text = "Hb Extinction Coef:";
            // 
            // numericUpDown6
            // 
            this.numericUpDown6.DecimalPlaces = 3;
            this.numericUpDown6.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.numericUpDown6.Location = new System.Drawing.Point(138, 289);
            this.numericUpDown6.Margin = new System.Windows.Forms.Padding(4);
            this.numericUpDown6.Name = "numericUpDown6";
            this.numericUpDown6.Size = new System.Drawing.Size(80, 22);
            this.numericUpDown6.TabIndex = 28;
            // 
            // textBox6
            // 
            this.textBox6.BackColor = System.Drawing.SystemColors.Window;
            this.textBox6.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox6.Location = new System.Drawing.Point(9, 246);
            this.textBox6.Margin = new System.Windows.Forms.Padding(4);
            this.textBox6.Name = "textBox6";
            this.textBox6.Size = new System.Drawing.Size(105, 15);
            this.textBox6.TabIndex = 29;
            this.textBox6.Text = "Normalized DPF:";
            // 
            // numericUpDown4
            // 
            this.numericUpDown4.DecimalPlaces = 3;
            this.numericUpDown4.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.numericUpDown4.Location = new System.Drawing.Point(138, 244);
            this.numericUpDown4.Margin = new System.Windows.Forms.Padding(4);
            this.numericUpDown4.Name = "numericUpDown4";
            this.numericUpDown4.Size = new System.Drawing.Size(80, 22);
            this.numericUpDown4.TabIndex = 30;
            // 
            // textBox5
            // 
            this.textBox5.BackColor = System.Drawing.SystemColors.Window;
            this.textBox5.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox5.Location = new System.Drawing.Point(281, 293);
            this.textBox5.Margin = new System.Windows.Forms.Padding(4);
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(133, 15);
            this.textBox5.TabIndex = 31;
            this.textBox5.Text = "HbR Extinction Coef:";
            // 
            // numericUpDown3
            // 
            this.numericUpDown3.DecimalPlaces = 3;
            this.numericUpDown3.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.numericUpDown3.Location = new System.Drawing.Point(429, 289);
            this.numericUpDown3.Margin = new System.Windows.Forms.Padding(4);
            this.numericUpDown3.Name = "numericUpDown3";
            this.numericUpDown3.Size = new System.Drawing.Size(80, 22);
            this.numericUpDown3.TabIndex = 32;
            // 
            // textBox13
            // 
            this.textBox13.BackColor = System.Drawing.SystemColors.Window;
            this.textBox13.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox13.Location = new System.Drawing.Point(9, 9);
            this.textBox13.Margin = new System.Windows.Forms.Padding(4);
            this.textBox13.Multiline = true;
            this.textBox13.Name = "textBox13";
            this.textBox13.Size = new System.Drawing.Size(105, 20);
            this.textBox13.TabIndex = 33;
            this.textBox13.Text = "Sampling Period:";
            // 
            // numericUpDown9
            // 
            this.numericUpDown9.Location = new System.Drawing.Point(138, 7);
            this.numericUpDown9.Margin = new System.Windows.Forms.Padding(4);
            this.numericUpDown9.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDown9.Name = "numericUpDown9";
            this.numericUpDown9.Size = new System.Drawing.Size(80, 22);
            this.numericUpDown9.TabIndex = 34;
            // 
            // textBox14
            // 
            this.textBox14.BackColor = System.Drawing.SystemColors.Window;
            this.textBox14.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox14.Location = new System.Drawing.Point(226, 7);
            this.textBox14.Margin = new System.Windows.Forms.Padding(4);
            this.textBox14.Multiline = true;
            this.textBox14.Name = "textBox14";
            this.textBox14.Size = new System.Drawing.Size(49, 22);
            this.textBox14.TabIndex = 35;
            this.textBox14.Text = "ms";
            // 
            // SettingsSetup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(522, 319);
            this.Controls.Add(this.textBox14);
            this.Controls.Add(this.numericUpDown9);
            this.Controls.Add(this.textBox13);
            this.Controls.Add(this.numericUpDown3);
            this.Controls.Add(this.textBox5);
            this.Controls.Add(this.numericUpDown4);
            this.Controls.Add(this.textBox6);
            this.Controls.Add(this.numericUpDown6);
            this.Controls.Add(this.textBox7);
            this.Controls.Add(this.textBox12);
            this.Controls.Add(this.numericUpDown7);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.numericUpDown5);
            this.Controls.Add(this.textBox10);
            this.Controls.Add(this.textBox9);
            this.Controls.Add(this.totalDistance);
            this.Controls.Add(this.textBox8);
            this.Controls.Add(this.numericUpDown2);
            this.Controls.Add(this.textBox4);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.saveButton);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "SettingsSetup";
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.SettingsSetup_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.totalDistance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown9)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.NumericUpDown numericUpDown2;
        private System.Windows.Forms.TextBox textBox8;
        private System.Windows.Forms.NumericUpDown totalDistance;
        private System.Windows.Forms.TextBox textBox9;
        private System.Windows.Forms.TextBox textBox10;
        private System.Windows.Forms.NumericUpDown numericUpDown5;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.NumericUpDown numericUpDown7;
        private System.Windows.Forms.TextBox textBox12;
        private System.Windows.Forms.TextBox textBox7;
        private System.Windows.Forms.NumericUpDown numericUpDown6;
        private System.Windows.Forms.TextBox textBox6;
        private System.Windows.Forms.NumericUpDown numericUpDown4;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.NumericUpDown numericUpDown3;
        private System.Windows.Forms.TextBox textBox13;
        private System.Windows.Forms.NumericUpDown numericUpDown9;
        private System.Windows.Forms.TextBox textBox14;
    }
}