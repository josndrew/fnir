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
        }

        
        private void saveButton_Click(object sender, EventArgs e)
        {
            int delay = GUI.Properties.Settings.Default.delay;
            connectPressed = true;
            if (comboBox1.SelectedIndex == 0)
            {
                delay = (int)((totalTime.Value / dataPoints.Value) * 1000);
            }
            else if (comboBox1.SelectedIndex == 1)
            {
                delay = (int)((totalTime.Value / dataPoints.Value));
            }
            
            GUI.Properties.Settings.Default.frequency = (int)(1000 / delay); ;
            GUI.Properties.Settings.Default.delay = delay;

            GUI.Properties.Settings.Default.dist = (double) totalDistance.Value;
            GUI.Properties.Settings.Default.DPF_1 = (double) numericUpDown5.Value;
            GUI.Properties.Settings.Default.DPF_2 = (double) numericUpDown6.Value;
            GUI.Properties.Settings.Default.c_1 = (double) numericUpDown2.Value;
            GUI.Properties.Settings.Default.c_2 = (double) numericUpDown3.Value;
            GUI.Properties.Settings.Default.I0_1 = (double) numericUpDown1.Value;
            GUI.Properties.Settings.Default.I0_2 = (double) numericUpDown4.Value;

            Properties.Settings.Default.Save();

            Hide();
        }

        private void SettingsSetup_Load(object sender, EventArgs e)
        {
            dataPoints.Value = GUI.Properties.Settings.Default.frequency;
            dataPoints.Maximum = 10;
            dataPoints.Minimum = 1;

            totalTime.Value = 1;
            totalTime.Maximum = 500;
            totalTime.Minimum = 1;

            totalDistance.Value = (decimal) GUI.Properties.Settings.Default.dist;
            numericUpDown5.Value = (decimal) GUI.Properties.Settings.Default.DPF_1;
            numericUpDown6.Value = (decimal) GUI.Properties.Settings.Default.DPF_2;
            numericUpDown2.Value = (decimal) GUI.Properties.Settings.Default.c_1;
            numericUpDown3.Value = (decimal) GUI.Properties.Settings.Default.c_2;
            numericUpDown1.Value = (decimal) GUI.Properties.Settings.Default.I0_1;
            numericUpDown4.Value = (decimal) GUI.Properties.Settings.Default.I0_2;

            comboBox1.Items.Add("sec");
            comboBox1.Items.Add("msec");
            comboBox1.SelectedIndex = 0;
        }


        public bool btnPressed()
        {
            return connectPressed;
        }



    }
}
