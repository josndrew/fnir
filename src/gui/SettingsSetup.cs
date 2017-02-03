using System;
using System.Windows.Forms;

namespace GUI
{
    public partial class SettingsSetup : Form
    {
        private bool connectPressed = false;
        
        public SettingsSetup()
        {
            InitializeComponent();
            saveButton.Visible = false;
        }

        
        public void saveButton_Click(object sender, EventArgs e)
        {
            connectPressed = true;

            GUI.Properties.Settings.Default.delay = (int) numericUpDown9.Value;
            GUI.Properties.Settings.Default.dist = (double) totalDistance.Value;
            GUI.Properties.Settings.Default.DPF_1 = (double) numericUpDown5.Value;
            GUI.Properties.Settings.Default.DPF_2 = (double) numericUpDown4.Value;

            GUI.Properties.Settings.Default.c_1 = (double) numericUpDown2.Value;
            GUI.Properties.Settings.Default.c_2 = (double) numericUpDown7.Value;
            GUI.Properties.Settings.Default.c_3 = (double) numericUpDown6.Value;
            GUI.Properties.Settings.Default.c_4 = (double) numericUpDown3.Value;

            GUI.Properties.Settings.Default.I0_1 = (double) numericUpDown1.Value;
            GUI.Properties.Settings.Default.I0_2 = (double) numericUpDown8.Value;

            
            Properties.Settings.Default.Save();

            Hide();
        }

        private void SettingsSetup_Load(object sender, EventArgs e)
        {
            
            totalDistance.Value = (decimal) GUI.Properties.Settings.Default.dist;
            numericUpDown5.Value = (decimal) GUI.Properties.Settings.Default.DPF_1;
            numericUpDown4.Value = (decimal) GUI.Properties.Settings.Default.DPF_2;

            numericUpDown2.Value = (decimal) GUI.Properties.Settings.Default.c_1;
            numericUpDown7.Value = (decimal) GUI.Properties.Settings.Default.c_2;
            numericUpDown6.Value = (decimal) GUI.Properties.Settings.Default.c_3;
            numericUpDown3.Value = (decimal) GUI.Properties.Settings.Default.c_4;
            numericUpDown9.Value = (decimal)GUI.Properties.Settings.Default.delay;

            numericUpDown1.Value = (decimal) GUI.Properties.Settings.Default.I0_1;
            numericUpDown8.Value = (decimal) GUI.Properties.Settings.Default.I0_2; 
        }


        public bool btnPressed()
        {
            return connectPressed;
        }



    }
}
