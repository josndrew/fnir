using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.IO.Ports;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms.DataVisualization.Charting;
using System.Drawing;

namespace GUI
{
    public partial class mainForm : Form
    {

/**************************************************************************************************************************
 * 
 * Initializing Elements & Variables
 * 
 **************************************************************************************************************************/

        //Variables
        private AboutBox1 aboutInfo;
        private int currentpoint;
        private SerialPort adruinoSerial;
        public SerialSetup adruino_dialog;
        private bool isCollecting;
        private bool isFrozen = false;
        private bool isFullScreen = false;
        private Thread thread1;
        const int NUM_CHANNELS = 4;
        private Dictionary<int, processedData> collectedPoints_1;
        private Dictionary<int, processedData> collectedPoints_2;
        private Dictionary<int, processedData> collectedPoints_3;
        private Dictionary<int, processedData> collectedPoints_4;
        private Chart [] channels;
        private Dictionary<int, processedData>[] MASTER_DICT;
        private Label[] labels;
        int INTERVAL;
        int[] yLoc = new int[NUM_CHANNELS] { 29, 171, 313, 455};
 
        public mainForm()
        {
            InitializeComponent();
        }

        private void mainForm_Load(object sender, EventArgs e)
        {
            //Properties.Settings.Default.Reset();
            
            fullScreenToolStripMenuItem_Click(sender, e);

            collectedPoints_1 = new Dictionary<int, processedData>();
            collectedPoints_2 = new Dictionary<int, processedData>();
            collectedPoints_3 = new Dictionary<int, processedData>();
            collectedPoints_4 = new Dictionary<int, processedData>();
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
            channels = new Chart [NUM_CHANNELS] {channel1, channel2, channel3, channel4};
            MASTER_DICT = new Dictionary<int, processedData> [NUM_CHANNELS] {collectedPoints_1, collectedPoints_2, collectedPoints_3, collectedPoints_4};
            labels = new Label[NUM_CHANNELS] { label1, label2, label3, label4 };

            for (int j = 0; j < NUM_CHANNELS; j++)
            {
                channels[j].Visible = false;
                labels[j].Visible = false;
            }

            
            setUpInterval();
            changeInterval();
        }

/**************************************************************************************************************************
 * 
 * Action Events (Clicks on Buttons and Menu Items)
 * 
 **************************************************************************************************************************/

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

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            changeInterval();
            
            for (int i = 0; i < NUM_CHANNELS; i++)
            {
                Chart chart = channels[i];
                double newBegin = chart.ChartAreas[0].AxisX.Minimum;
                double newEnd = chart.ChartAreas[0].AxisX.Maximum + INTERVAL;
                if (newEnd > currentpoint)
                {
                    newBegin = currentpoint - INTERVAL;
                    if (newBegin < 1)
                    {
                        newBegin = 1;
                    }
                    newEnd = currentpoint;
                }
                updateXAxis(newBegin, newEnd, chart);
                updateYAxis(newBegin, newEnd, chart, i);
            }
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            changeCollectingStatus();
        }
        
        private void button2_Click(object sender, EventArgs e)
        {
            changeGraphUpdateStatus();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < NUM_CHANNELS; i++)
            {
                Chart chart = channels[i];
                double newBegin = chart.ChartAreas[0].AxisX.Minimum - INTERVAL;
                double newEnd = chart.ChartAreas[0].AxisX.Maximum - INTERVAL;
                if (newBegin < 1)
                {
                    newBegin = 1;
                    newEnd = INTERVAL;
                    button3.Enabled = false;
                }
                button4.Enabled = true;
                updateXAxis(newBegin, newEnd, chart);
                updateYAxis(newBegin, newEnd, chart, i);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < NUM_CHANNELS; i++)
            {
                Chart chart = channels[i];
                double newBegin = chart.ChartAreas[0].AxisX.Minimum + INTERVAL;
                double newEnd = chart.ChartAreas[0].AxisX.Maximum + INTERVAL;
                if (newEnd > currentpoint)
                {
                    newBegin = currentpoint - INTERVAL;
                    newEnd = currentpoint;
                }
                button3.Enabled = true;
                updateXAxis(newBegin, newEnd, chart);
                updateYAxis(newBegin, newEnd, chart, i);
            }
        }
        
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            channel1.Visible = !channel1.Visible;
            label1.Visible = !label1.Visible;
            updateNumDisplays();
        }
        
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            channel2.Visible = !channel2.Visible;
            label2.Visible = !label2.Visible;
            updateNumDisplays();
        }
        
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            channel3.Visible = !channel3.Visible;
            label3.Visible = !label3.Visible;
            updateNumDisplays();
        }
        
        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            channel4.Visible = !channel4.Visible;
            label4.Visible = !label4.Visible;
            updateNumDisplays();
        }
/**************************************************************************************************************************
 * 
 * Helper Functions
 * 
 **************************************************************************************************************************/

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

        private void changeGraphUpdateStatus()
        {
            string status = button2.Text;

            switch (status)
            {
                case "Freeze":
                    isFrozen = true;
                    button3.Enabled = true;
                    button4.Enabled = true;
                    button2.Text = "UnFreeze";
                    break;

                case "UnFreeze":
                    isFrozen = false;
                    button3.Enabled = false;
                    button4.Enabled = false;
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
                        
                        processedData d1  = new processedData();
                        processedData d2 = new processedData();
                        processedData d3 = new processedData();
                        processedData d4 = new processedData();
                                                
                        d1.setXCord(dataArray[0]);
                        d1.setAvg(dataArray[1]);

                        d2.setXCord(dataArray[0]*2);
                        d2.setAvg(dataArray[1]*2);

                        d3.setXCord(0);
                        d3.setAvg(0);

                        d3.setXCord(dataArray[0] / 3);
                        d3.setAvg(dataArray[1] / 3);

                        //using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\Andrew Joseph\Desktop\Senior Design\\data.txt", true))
                        //{
                        //    file.WriteLine(dataArray[0]);
                        //}

                        currentpoint++;
                        Invoke((MethodInvoker)delegate { Update(); updateChart(d1, channel1, 0); Refresh(); Update(); });
                        Invoke((MethodInvoker)delegate { Update(); updateChart(d2, channel2, 1); Refresh(); Update(); });
                        Invoke((MethodInvoker)delegate { Update(); updateChart(d3, channel3, 2); Refresh(); Update(); });
                        Invoke((MethodInvoker)delegate { Update(); updateChart(d4, channel4, 3); Refresh(); Update(); });
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

        private void updateXAxis(double begin, double end, Chart chart)
        {
            chart.ChartAreas[0].AxisX.Minimum = begin;
            chart.ChartAreas[0].AxisX.Maximum = end;

            chart.ChartAreas[0].AxisX.Minimum = begin;
            chart.ChartAreas[0].AxisX.Maximum = end;
        }

        private void updateYAxis(double begin, double end, Chart chart, int index)
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
                Dictionary<int, processedData> collectedPoints = MASTER_DICT[index];
                double xx = collectedPoints[k].getXCord();
                double aa = collectedPoints[k].getAvg();

                if (xx > maxX) {maxX = xx;}

                if (xx < minX) {minX = xx;}

                if (aa > maxA) {maxA = aa;}

                if (aa < minA) {minA = aa;}
            }

            chart.ChartAreas[0].AxisX.MajorGrid.Interval = 2;
            chart.ChartAreas[0].AxisX.MajorTickMark.Interval = 2;

            double[] limits = { maxX, minX };

            chart.ChartAreas[0].AxisY.Maximum = Math.Round(limits.Max(), 1, MidpointRounding.AwayFromZero) + 2;
            chart.ChartAreas[0].AxisY.Minimum = Math.Round(limits.Min(), 1, MidpointRounding.AwayFromZero) - .5;

            double interval = Math.Round((chart.ChartAreas[0].AxisY.Maximum - chart.ChartAreas[0].AxisY.Minimum) / 7, 1, MidpointRounding.AwayFromZero);

            chart.ChartAreas[0].AxisY.MajorGrid.Interval = interval;
            chart.ChartAreas[0].AxisY.MajorTickMark.Interval = interval;

            double[] limits2 = { maxA, minA };

            chart.ChartAreas[0].AxisY2.Maximum = Math.Round(limits2.Max(), 1, MidpointRounding.AwayFromZero) + 2;
            chart.ChartAreas[0].AxisY2.Minimum = Math.Round(limits2.Min(), 1, MidpointRounding.AwayFromZero) - .5;

            double interval2 = Math.Round((chart.ChartAreas[0].AxisY2.Maximum - chart.ChartAreas[0].AxisY2.Minimum) / 7, 1, MidpointRounding.AwayFromZero);

            chart.ChartAreas[0].AxisY2.MajorGrid.Interval = interval2;
            chart.ChartAreas[0].AxisY2.MajorTickMark.Interval = interval2;
            chart.Update();
        }

        private void updateChart(processedData newData, Chart chart, int index)
        {
            chart.Update();
            
            chart.Series["Reading"].Points.AddY(newData.getXCord());
            chart.Series["Average"].Points.AddY(newData.getAvg());


            chart.Series[0].YAxisType = AxisType.Primary;
            chart.Series[1].YAxisType = AxisType.Secondary;
            chart.ChartAreas[0].AxisY2.MajorGrid.Enabled = false;
            chart.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;
            

            chart.ChartAreas[0].AxisX.Title = "# of Data Points";
            chart.ChartAreas[0].AxisY.Title = "mV";
            chart.ChartAreas[0].AxisY2.Title = "Avg. mV";

            
            Dictionary<int, processedData> collectedPoints = MASTER_DICT[index];
            collectedPoints.Add(currentpoint, newData);

            if (!isFrozen)
            {
                labels[index].Text = (newData.getXCord()).ToString();
                if (currentpoint > INTERVAL)
                {
                    updateXAxis(currentpoint - INTERVAL, currentpoint, chart);
                    updateYAxis(currentpoint - INTERVAL, currentpoint, chart, index);
                }
                else
                {
                    updateXAxis(1, INTERVAL, chart);
                    updateYAxis(1, INTERVAL, chart, index);
                }
            }

            chart.Update();
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


        private void updateNumDisplays()
        {
            int numDisp = 0;
            for(int k = 0; k < NUM_CHANNELS; k++)
            {
                if (channels[k].Visible)
                {
                    channels[k].Location = new Point(6, yLoc[numDisp]);
                    labels[k].Location = new Point(1189, yLoc[numDisp]);
                    

                    numDisp++;
                }



            }


        }
        





    }
}
