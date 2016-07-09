using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.IO.Ports;
using System.Collections.Generic;
using System.Diagnostics;


namespace GUI
{
   
    public partial class mainForm : Form
    {
        //Variables
        private AboutBox1 aboutInfo;
        private int currentpoint;
        private SerialPort adruinoSerial;
        public SerialSetup adruino_dialog;
        private bool isCollecting;
        private bool isFrozen = false;
        private bool isFullScreen = false;
        private Thread thread1;
        private Dictionary<int,processedData> collectedPoints;
        int INTERVAL;

        public mainForm()
        {
            InitializeComponent();
        }

        //Initializing of Variables
        private void mainForm_Load(object sender, EventArgs e)
        {
            //Properties.Settings.Default.Reset();
            
            fullScreenToolStripMenuItem_Click(sender, e);

            collectedPoints = new Dictionary<int, processedData>();
            adruino_dialog = new SerialSetup();
            adruinoSerial = new SerialPort();
            aboutInfo = new AboutBox1();
            startToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem.Enabled = false;
            button1.Enabled = false;
            isCollecting = false;
            button1.Text = "START";
            button2.Text = "Freeze";
            button2.Enabled = false;
            button3.Text = "<";
            button3.Enabled = false;
            button4.Text = ">";
            button4.Enabled = false;
            currentpoint = 0;

            setUpInterval();
            changeInterval();
        }

        private void fullScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!isFullScreen)  // FullScreenMode is an enum
            {
                this.WindowState = FormWindowState.Normal;
                this.WindowState = FormWindowState.Maximized;
                isFullScreen = true;
            }
            else
            {
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
                this.WindowState = FormWindowState.Normal;
                isFullScreen = false;
            }
        }


        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(Environment.ExitCode);
            Close();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            aboutInfo.ShowDialog();
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changeCollectingStatus();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changeCollectingStatus();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            changeCollectingStatus();
        }

        private void changeCollectingStatus()
        {
            string status = button1.Text;

            switch (status)
            {
                case "START":
                    isCollecting = true;
                    startToolStripMenuItem.Enabled = false;
                    stopToolStripMenuItem.Enabled = true;
                    button1.Text = "STOP";
                    button2.Enabled = true;
                    try 
                    {
                        if (!thread1.IsAlive)
                        {
                            thread1 = new Thread(new ThreadStart(get_processData));
                            thread1.Start();
                        }
                    }
                    catch
                    {
                        thread1 = new Thread(new ThreadStart(get_processData));
                        thread1.Start();
                    }
                    
                    break;

                case "STOP":
                    isCollecting = false;
                    startToolStripMenuItem.Enabled = true;
                    stopToolStripMenuItem.Enabled = false;
                    button1.Text = "START";
                    button2.Enabled = false;
                    break;
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            changeGraphUpdateStatus();
        }


        private void changeGraphUpdateStatus()
        {
            string status = button2.Text;

            switch (status)
            {
                case "Freeze":
                    isFrozen = true;
                    button3.Enabled = true;
                    button4.Enabled = true;
                    label1.Visible = false;
                    label2.Visible = false;
                    button2.Text = "UnFreeze";
                    break;

                case "UnFreeze":
                    isFrozen = false;
                    button3.Enabled = false;
                    button4.Enabled = false;
                    label1.Visible = true;
                    label2.Visible = true;
                    button2.Text = "Freeze";
                    break;
            }
        }

        private void setUpInterval()
        {
            comboBox1.Items.Add("15 sec");
            comboBox1.Items.Add("30 sec");
            comboBox1.Items.Add("1 min");
            comboBox1.Items.Add("5 min");
            comboBox1.SelectedIndex = 0;
        }

        private void changeInterval()
        {
            if (comboBox1.SelectedItem.Equals("15 sec"))
                INTERVAL = 15;
            else if (comboBox1.SelectedItem.Equals("30 sec"))
                INTERVAL = 30;
            else if (comboBox1.SelectedItem.Equals("1 min"))
                INTERVAL = 60;
            else if (comboBox1.SelectedItem.Equals("5 min"))
                INTERVAL = 300;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            changeInterval();
        }

        
        private void button3_Click(object sender, EventArgs e)
        { 
            double newBegin = rawData.ChartAreas[0].AxisX.Minimum - INTERVAL;
            double newEnd = rawData.ChartAreas[0].AxisX.Maximum -INTERVAL;
            if (newBegin < 1)
            {
                newBegin = 1;
                newEnd = INTERVAL;
                button3.Enabled = false;
            }
            button4.Enabled = true;
            updateXAxis(newBegin, newEnd);
            updateYAxis(newBegin, newEnd);
        }
        
        private void button4_Click(object sender, EventArgs e)
        {
            double newBegin = rawData.ChartAreas[0].AxisX.Minimum + INTERVAL;
            double newEnd = rawData.ChartAreas[0].AxisX.Maximum + INTERVAL;
            if (newEnd > currentpoint)
            {
                newBegin = currentpoint - INTERVAL;
                newEnd = currentpoint;
            }
            button3.Enabled = true;
            updateXAxis(newBegin, newEnd);
            updateYAxis(newBegin, newEnd);
        }

        private void get_processData()
        {
            try
            {
                adruinoSerial.Open();
                while (isCollecting)
                {
                    if (adruinoSerial.IsOpen)
                    {
                        string buffer = adruinoSerial.ReadTo("\n");
                        Console.WriteLine(buffer);
                        string[] bufferArray = buffer.Split(new string[] { "\x09" }, StringSplitOptions.RemoveEmptyEntries);
                        double[] dataArray = Array.ConvertAll(bufferArray, s => double.Parse(s));
                        
                        processedData d  = new processedData();
                                                
                        d.setXCord(dataArray[0]);
                        d.setAvg(dataArray[1]);

                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\Andrew Joseph\Desktop\Senior Design\\data.txt", true))
                        {
                            file.WriteLine(dataArray[0]);
                        }

                        Invoke((MethodInvoker)delegate { Update(); updateChart(d); Refresh(); Update(); });
                        //data = null;
                    }
                }
                adruinoSerial.Close();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString());
            }     
        }

        private void updateXAxis(double begin, double end)
        {
            rawData.ChartAreas[0].AxisX.Minimum = begin;
            rawData.ChartAreas[0].AxisX.Maximum = end;

            analyzedData.ChartAreas[0].AxisX.Minimum = begin;
            analyzedData.ChartAreas[0].AxisX.Maximum = end;
        }

        private void updateYAxis(double begin, double end)
        {
            double maxX = -3000;
            double maxA = -3000;

            double minX = 3000;
            double minA = 3000;

            if (currentpoint < end)
            {
                end = currentpoint;
            }
        
            for (int k = Convert.ToInt32(begin); k < Convert.ToInt32(end); k++)
            {

                double xx = collectedPoints[k].getXCord();
                double aa = collectedPoints[k].getAvg();

                if (xx > maxX)
                {
                    maxX = xx;
                }

                if (xx < minX)
                {
                    minX = xx;
                }

                if (aa > maxA)
                {
                    maxA = aa;
                }

                if (aa < minA)
                {
                    minA = aa;
                }
            }

            double[] limits = { maxX, minX };

            rawData.ChartAreas[0].AxisY.Maximum = Math.Round(limits.Max(), 1, MidpointRounding.AwayFromZero) + 2;
            rawData.ChartAreas[0].AxisY.Minimum = Math.Round(limits.Min(), 1, MidpointRounding.AwayFromZero) - .5;

            rawData.ChartAreas[0].AxisX.MajorGrid.Interval = 2;
            rawData.ChartAreas[0].AxisX.MajorTickMark.Interval = 2;

            double interval = Math.Round((rawData.ChartAreas[0].AxisY.Maximum - rawData.ChartAreas[0].AxisY.Minimum) / 7, 1, MidpointRounding.AwayFromZero);

            rawData.ChartAreas[0].AxisY.MajorGrid.Interval = interval;
            rawData.ChartAreas[0].AxisY.MajorTickMark.Interval = interval;

            rawData.Update();


            double[] limits2 = { maxA, minA };

            analyzedData.ChartAreas[0].AxisY.Maximum = Math.Round(limits2.Max(), 1, MidpointRounding.AwayFromZero) + 2;
            analyzedData.ChartAreas[0].AxisY.Minimum = Math.Round(limits2.Min(), 1, MidpointRounding.AwayFromZero) - .5;

            analyzedData.ChartAreas[0].AxisX.MajorGrid.Interval = 2;
            analyzedData.ChartAreas[0].AxisX.MajorTickMark.Interval = 2;

            double interval2 = Math.Round((analyzedData.ChartAreas[0].AxisY.Maximum - analyzedData.ChartAreas[0].AxisY.Minimum) / 7, 1, MidpointRounding.AwayFromZero);

            analyzedData.ChartAreas[0].AxisY.MajorGrid.Interval = interval2;
            analyzedData.ChartAreas[0].AxisY.MajorTickMark.Interval = interval2;
        }

        private void updateChart(processedData newData)
        {
            rawData.Update();
            
            rawData.Series["Reading"].Points.AddY(newData.getXCord());
           

            analyzedData.Series["Average"].Points.AddY(newData.getAvg());

            rawData.ChartAreas[0].AxisX.Title = "# of Data Points";
            rawData.ChartAreas[0].AxisY.Title = "mV";

            analyzedData.ChartAreas[0].AxisX.Title = "# of Data Points";
            analyzedData.ChartAreas[0].AxisY.Title = "mV";

            currentpoint++;

            collectedPoints.Add(currentpoint, newData);

            if (!isFrozen)
            {
                label1.Text = (newData.getXCord()).ToString();
                label2.Text = (newData.getAvg()).ToString();
                if (currentpoint > INTERVAL)
                {
                    updateXAxis(currentpoint - INTERVAL, currentpoint);
                    updateYAxis(currentpoint - INTERVAL, currentpoint);
                }
                else
                {
                    updateXAxis(1, INTERVAL);
                    updateYAxis(1, INTERVAL);
                }
            }

            analyzedData.Update();
        }

        private void serialDialogOpen()
        {
            adruino_dialog.ShowDialog();

            if (adruino_dialog.btnPressed())
            {
                try
                {
                    adruinoSerial = adruino_dialog.getSerialConfig();
                    startToolStripMenuItem.Enabled = true;
                    stopToolStripMenuItem.Enabled = false;
                    button1.Enabled = true;
                    button2.Enabled = true;

                    GUI.Properties.Settings.Default.PortName = adruinoSerial.PortName;
                    GUI.Properties.Settings.Default.BaudRate = adruinoSerial.BaudRate;
                    GUI.Properties.Settings.Default.Parity = adruinoSerial.Parity;
                    GUI.Properties.Settings.Default.DataBits = adruinoSerial.DataBits;
                    GUI.Properties.Settings.Default.StopBits = adruinoSerial.StopBits;
                    GUI.Properties.Settings.Default.Handshake = adruinoSerial.Handshake;
                    Properties.Settings.Default.Save();
                }

                catch (Exception err)
                {
                    MessageBox.Show(err.ToString());
                }
            }
        }

        private void connectWithAdruinoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string tPort = GUI.Properties.Settings.Default.PortName;

            if (tPort.Equals("#NOT_SET"))
            {
                serialDialogOpen();
            }

            else
            {
                try
                {
                    adruinoSerial.PortName = GUI.Properties.Settings.Default.PortName;
                    adruinoSerial.BaudRate = GUI.Properties.Settings.Default.BaudRate;
                    adruinoSerial.Parity = GUI.Properties.Settings.Default.Parity;
                    adruinoSerial.DataBits = GUI.Properties.Settings.Default.DataBits;
                    adruinoSerial.StopBits = GUI.Properties.Settings.Default.StopBits;
                    adruinoSerial.Handshake = GUI.Properties.Settings.Default.Handshake;
                    
                    adruinoSerial.Open();
                    adruinoSerial.Close();

                    startToolStripMenuItem.Enabled = true;
                    stopToolStripMenuItem.Enabled = false;
                    button1.Enabled = true;
                    button2.Enabled = true;
                }

                catch
                {
                    serialDialogOpen();
                }
            }
        }




        

        




    }
}
