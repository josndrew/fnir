using System;
using System.Windows.Forms;
using System.IO.Ports;

namespace GUI
{
    public partial class SerialSetup : Form
    {
        private int[] bitRates;
        private string[] ports;
        private SerialPort serial;
        private bool connectPressed = false;

        public SerialSetup()
        {
            InitializeComponent();
        }

        private void SerialSetup_Load(object sender, EventArgs e)
        {
            serial = new SerialPort();
            ports = SerialPort.GetPortNames();
            connectPressed = false;

            if (ports.Length == 0)
            {
                MessageBox.Show("No COM Ports have been detected. Please make sure your Audrino is properly connected", "ERROR 02x01");
                Close();
            }

            else
            {
                bitRates = new int[10];
                bitRates[0] = 300;
                bitRates[1] = 600;
                bitRates[2] = 1200;
                bitRates[3] = 2400;
                bitRates[4] = 9600;
                bitRates[5] = 14400;
                bitRates[6] = 19200;
                bitRates[7] = 38400;
                bitRates[8] = 57600;
                bitRates[9] = 115200;

                cmbPort.Items.AddRange(ports);

                for (int k = 0; k < bitRates.Length; k++)
                {
                    cmbBitRate.Items.Add(bitRates[k]);
                }

                foreach (string bit in Enum.GetNames(typeof(StopBits)))
                {
                    cmbStopBit.Items.Add(bit);
                }

                foreach (string parity in Enum.GetNames(typeof(Parity)))
                {
                    cmbParity.Items.Add(parity);
                }

                foreach (string shake in Enum.GetNames(typeof(Handshake)))
                {
                    cmbHandshake.Items.Add(shake);
                }

            }

        }

        private void connect_btn_Click(object sender, EventArgs e)
        {
            connectPressed = true;
            try
            {
                serial.PortName = cmbPort.SelectedItem.ToString();
                serial.BaudRate = bitRates[cmbBitRate.SelectedIndex];
                serial.Parity = (Parity)cmbParity.SelectedIndex;
                serial.DataBits = 8;
                serial.StopBits = (StopBits)cmbStopBit.SelectedIndex;
                serial.Handshake = (Handshake)cmbHandshake.SelectedIndex;


                //serial.ReadTimeout = 5000;
                //serial.WriteTimeout = 5000;

                Hide();

            }
            catch
            {
                MessageBox.Show("The specified Serial Configurations could not be set. Please Review the settings and Try Again.", "ERROR 02x02");
            }
        }

        public bool btnPressed()
        {
            return connectPressed;
        }

        public SerialPort getSerialConfig()
        {
            return serial;
        }


    }
}
