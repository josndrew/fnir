using InTheHand.Net.Sockets;
using System;
using System.Data;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

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
        private Chart[] channels;
        private Dictionary<int, processedData>[] MASTER_DICT;
        private Label[] labels;
        private DataTable foundSessions;
        private int sessionsFound = 0;
        private string sessionPath;
        int INTERVAL;

        BluetoothClient cli = new BluetoothClient();
        Stopwatch stopWatch = new Stopwatch();
        int freq = (int)1000 / (200);

        public mainForm()
        {
            InitializeComponent();
            tabControl1.SelectedIndexChanged += new EventHandler(tabControl1_SelectedIndexChanged);
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
            channels = new Chart[NUM_CHANNELS * 2] { channel1, channel2, channel3, channel4, channel1_P, channel2_P, channel3_P, channel4_P };
            MASTER_DICT = new Dictionary<int, processedData>[NUM_CHANNELS] { collectedPoints_1, collectedPoints_2, collectedPoints_3, collectedPoints_4 };
            labels = new Label[NUM_CHANNELS * 2] { label1, label2, label3, label4, label5, label6, label7, label8 };

            for (int j = 0; j < NUM_CHANNELS; j++)
            {
                channels[j].Visible = false;
                channels[j + NUM_CHANNELS].Visible = false;
                labels[j].Visible = false;
                labels[j + NUM_CHANNELS].Visible = false;
            }

            setUpInterval();
            changeInterval();

            
            foundSessions = new DataTable();
            foundSessions.Columns.Add("Session ID:", typeof(int));
            foundSessions.Columns.Add("Session Name:", typeof(string));
            foundSessions.Columns.Add("Session Size (Bytes):", typeof(long));
            foundSessions.Columns.Add("Date of Session:", typeof(DateTime));
            foundSessions.Columns.Add("Session Duration:", typeof(TimeSpan));
            foundSessions.Columns.Add("File Path:", typeof(string));
            
            dataGridView1.DataSource = foundSessions;
            folderBrowserDialog1.SelectedPath = GUI.Properties.Settings.Default.workingDirectory;
            ProcessDirectory(GUI.Properties.Settings.Default.workingDirectory);
            sessionPath = GUI.Properties.Settings.Default.workingDirectory.ToString() + "\\session_" + sessionsFound.ToString() + ".drexel";
            if (File.Exists(sessionPath))
            {
                File.Delete(sessionPath);
            }
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

        private void workingDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sessionsFound = 0;
            folderBrowserDialog1.ShowDialog();
            GUI.Properties.Settings.Default.workingDirectory = folderBrowserDialog1.SelectedPath;
            Properties.Settings.Default.Save();
            ProcessDirectory(GUI.Properties.Settings.Default.workingDirectory);
            
            sessionPath = GUI.Properties.Settings.Default.workingDirectory.ToString() + "\\session_" + sessionsFound.ToString() + ".drexel";
            if (File.Exists(sessionPath))
            {
                File.Delete(sessionPath);
            }
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

            for (int i = 0; i < NUM_CHANNELS * 2; i++)
            {
                Chart chart = channels[i];
                double newBegin = chart.ChartAreas[0].AxisX.Minimum;
                double newEnd = chart.ChartAreas[0].AxisX.Maximum + INTERVAL;
                bool procData = false;
                if (i/4 == 1)
                {
                    procData = true;
                }

                if (newEnd > currentpoint)
                {
                    newBegin = currentpoint - INTERVAL;
                    if (newBegin < 1)
                    {
                        newBegin = 1;
                    }
                    newEnd = currentpoint;
                }
                if ((newBegin > 0) && (newEnd > 0))
                {
                    updateXAxis(newBegin, newEnd, chart);
                    updateYAxis(newBegin, newEnd, chart, i%4, procData);
                }

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
            for (int i = 0; i < NUM_CHANNELS * 2; i++)
            {
                Chart chart = channels[i];
                double newBegin = chart.ChartAreas[0].AxisX.Minimum - INTERVAL;
                double newEnd = chart.ChartAreas[0].AxisX.Maximum - INTERVAL;
                bool procData = false;
                if (i / 4 == 1)
                {
                    procData = true;
                }

                if (newBegin < 1)
                {
                    newBegin = 1;
                    newEnd = INTERVAL;
                    button3.Enabled = false;
                }
                button4.Enabled = true;
                updateXAxis(newBegin, newEnd, chart);
                updateYAxis(newBegin, newEnd, chart, i, procData);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < NUM_CHANNELS * 2; i++)
            {
                Chart chart = channels[i];
                double newBegin = chart.ChartAreas[0].AxisX.Minimum + INTERVAL;
                double newEnd = chart.ChartAreas[0].AxisX.Maximum + INTERVAL;
                bool procData = false;
                if (i / 4 == 1)
                {
                    procData = true;
                }

                if (newEnd > currentpoint)
                {
                    newBegin = currentpoint - INTERVAL;
                    newEnd = currentpoint;
                }
                button3.Enabled = true;
                updateXAxis(newBegin, newEnd, chart);
                updateYAxis(newBegin, newEnd, chart, i, procData);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                channel1.Visible = true;
                channel1_P.Visible = true;
                label1.Visible = true;
                label5.Visible = true;
            }
            else
            {
                channel1.Visible = false;
                channel1_P.Visible = false;
                label1.Visible = false;
                label5.Visible = false;
            }
            updateNumDisplays();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                channel2.Visible = true;
                channel2_P.Visible = true;
                label2.Visible = true;
                label6.Visible = true;
            }
            else
            {
                channel2.Visible = false;
                channel2_P.Visible = false;
                label2.Visible = false;
                label6.Visible = false;
            }
            updateNumDisplays();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                channel3.Visible = true;
                channel3_P.Visible = true;
                label3.Visible = true;
                label7.Visible = true;
            }
            else
            {
                channel3.Visible = false;
                channel3_P.Visible = false;
                label3.Visible = false;
                label7.Visible = false;
            }
            updateNumDisplays();
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked)
            {
                channel4.Visible = true;
                channel4_P.Visible = true;
                label4.Visible = true;
                label8.Visible = true;
            }
            else
            {
                channel4.Visible = false;
                channel4_P.Visible = false;
                label4.Visible = false;
                label8.Visible = false;
            }
            updateNumDisplays();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabPage1)
            {
                updateNumDisplays();
            }
            else if (tabControl1.SelectedTab == tabPage3)
            {
                updateNumDisplays();
            }
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
                    button3.Enabled = false;
                    button4.Enabled = false;
                    try
                    {
                        if (!thread1.IsAlive)
                        {
                            thread1 = new Thread(new ThreadStart(get_bufferData));
                            thread1.Start();
                        }
                    }
                    catch
                    {
                        thread1 = new Thread(new ThreadStart(get_bufferData));
                        thread1.Start();
                    }

                    break;

                case "STOP":
                    isCollecting = false;
                    startToolStripMenuItem.Enabled = true;
                    stopToolStripMenuItem.Enabled = false;
                    button1.Text = "START";
                    button2.Enabled = false;
                    button3.Enabled = true;
                    button4.Enabled = true;
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
            comboBox1.Items.Add("5 sec");
            comboBox1.Items.Add("15 sec");
            comboBox1.Items.Add("30 sec");
            comboBox1.Items.Add("1 min");
            comboBox1.Items.Add("5 min");
            comboBox1.SelectedIndex = 0;
        }

        private void changeInterval()
        {
            if (comboBox1.SelectedItem.Equals("5 sec"))
                INTERVAL = 5 * freq;
            if (comboBox1.SelectedItem.Equals("15 sec"))
                INTERVAL = 15 * freq;
            else if (comboBox1.SelectedItem.Equals("30 sec"))
                INTERVAL = 30 * freq;
            else if (comboBox1.SelectedItem.Equals("1 min"))
                INTERVAL = 60 * freq;
            else if (comboBox1.SelectedItem.Equals("5 min"))
                INTERVAL = 300 * freq;
        }

        private void get_bufferData()
        {
            try
            {
                adruinoSerial.Open();
                stopWatch.Start();
                while (isCollecting)
                {
                    if (adruinoSerial.IsOpen)
                    {
                        TimeSpan ts = stopWatch.Elapsed;

                        if (ts.TotalHours < 5.0)
                        {
                            double timeStamp = Math.Round(ts.TotalSeconds, 1);

                            adruinoSerial.DtrEnable = true;
                            string buffer = adruinoSerial.ReadTo("\n");
                            adruinoSerial.DtrEnable = false;
                            string[] bufferArray = buffer.Split(new string[] { "\x09" }, StringSplitOptions.RemoveEmptyEntries);
                            double[] dataArray = Array.ConvertAll(bufferArray, s => double.Parse(s));


                            displayData(timeStamp, dataArray);

                            
                        }
                        else
                        {
                            Invoke((MethodInvoker)delegate { changeCollectingStatus(); Refresh(); Update(); });
                        }
                    }
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString());
            }
            adruinoSerial.Close();
        }

        private void displayData(double timeStamp, double [] dataArray)
        {
            processedData d1 = new processedData();
            processedData d2 = new processedData();
            processedData d3 = new processedData();
            processedData d4 = new processedData();

            d1.setXCord(timeStamp);
            d1.setYCord1(dataArray[0]);
            d1.setYCord2(dataArray[1]);

            d2.setXCord(timeStamp);
            d2.setYCord1(dataArray[2]);
            d2.setYCord2(dataArray[3]);

            d3.setXCord(timeStamp);
            d3.setYCord1(dataArray[4]);
            d3.setYCord2(dataArray[5]);

            d4.setXCord(timeStamp);
            d4.setYCord1(dataArray[6]);
            d4.setYCord2(dataArray[7]);

            currentpoint++;
            Invoke((MethodInvoker)delegate { Update(); updateChart(d1, channel1, 0, false); Refresh(); Update(); });
            Invoke((MethodInvoker)delegate { Update(); updateChart(d2, channel2, 1, false); Refresh(); Update(); });
            Invoke((MethodInvoker)delegate { Update(); updateChart(d3, channel3, 2, false); Refresh(); Update(); });
            Invoke((MethodInvoker)delegate { Update(); updateChart(d4, channel4, 3, false); Refresh(); Update(); });
            
            Invoke((MethodInvoker)delegate { Update(); updateChart(d1, channel1_P, 0, true); Refresh(); Update(); });
            Invoke((MethodInvoker)delegate { Update(); updateChart(d2, channel2_P, 1, true); Refresh(); Update(); });
            Invoke((MethodInvoker)delegate { Update(); updateChart(d3, channel3_P, 2, true); Refresh(); Update(); });
            Invoke((MethodInvoker)delegate { Update(); updateChart(d4, channel4_P, 3, true); Refresh(); Update(); });


            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@sessionPath, true))
            {
                file.WriteLine(timeStamp + "," + d1.getYCord1() + "," + d1.getYCord2() + "," + d1.getYCord1_P() + "," + d1.getYCord2_P()
                                         + "," + d2.getYCord1() + "," + d2.getYCord2() + "," + d2.getYCord1_P() + "," + d2.getYCord2_P()
                                         + "," + d3.getYCord1() + "," + d3.getYCord2() + "," + d3.getYCord1_P() + "," + d3.getYCord2_P()
                                         + "," + d4.getYCord1() + "," + d4.getYCord2() + "," + d4.getYCord1_P() + "," + d4.getYCord2_P());
            }
        }

        private void updateXAxis(double begin, double end, Chart chart)
        {
            chart.ChartAreas[0].AxisX.Minimum = begin;
            chart.ChartAreas[0].AxisX.Maximum = end;

            chart.ChartAreas[0].AxisX.Minimum = begin;
            chart.ChartAreas[0].AxisX.Maximum = end;
        }

        private void updateYAxis(double begin, double end, Chart chart, int index, bool procData)
        {
            double maxX = -3000;
            double maxA = -3000;

            double minX = 3000;
            double minA = 3000;

            if (currentpoint < end)
            {
                end = currentpoint;
            }

            if (begin < 1)
            {
                begin = 1;
            }

            for (int k = Convert.ToInt32(begin); k < Convert.ToInt32(end); k++)
            {
                Dictionary<int, processedData> collectedPoints = MASTER_DICT[index];
                double xx, aa;
                if (procData)
                {
                    xx = collectedPoints[k].getYCord1_P();
                    aa = collectedPoints[k].getYCord2_P();
                }
                else
                {
                    xx = collectedPoints[k].getYCord1();
                    aa = collectedPoints[k].getYCord2();
                }
                

                if (xx > maxX) { maxX = xx; }

                if (xx < minX) { minX = xx; }

                if (aa > maxA) { maxA = aa; }

                if (aa < minA) { minA = aa; }
            }

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

            chart.ChartAreas[0].AxisY2.MajorGrid.Enabled = false;
            chart.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;

            chart.ChartAreas[0].AxisY2.MajorGrid.Interval = interval2;
            chart.ChartAreas[0].AxisY2.MajorTickMark.Interval = interval2;

            chart.ChartAreas[0].AxisX.MajorGrid.Interval = 2;
            chart.ChartAreas[0].AxisX.MajorTickMark.Interval = 2;

            if (comboBox1.SelectedIndex > 2)
            {
                chart.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
                chart.ChartAreas[0].AxisX.MajorTickMark.Enabled = false;    
            }
            else
            {
                chart.ChartAreas[0].AxisX.MajorGrid.Enabled = true;
                chart.ChartAreas[0].AxisX.MajorTickMark.Enabled = true;                                    
            }

            chart.Update();
        }

        private void updateChart(processedData newData, Chart chart, int index, bool procData)
        {
            chart.Update();

            if (procData)
            {
                chart.Series["HbO2"].Points.AddXY(newData.getXCord(), newData.getYCord1_P());
                chart.Series["HbR"].Points.AddXY(newData.getXCord(), newData.getYCord2_P());
            }
            else
            {
                chart.Series["HbO2"].Points.AddXY(newData.getXCord(), newData.getYCord1());
                chart.Series["HbR"].Points.AddXY(newData.getXCord(), newData.getYCord2());
            }


            chart.Series[0].YAxisType = AxisType.Primary;
            chart.Series[1].YAxisType = AxisType.Secondary;
            chart.Series[0].IsXValueIndexed = true;
            chart.Series[1].IsXValueIndexed = true;

            chart.Series[0].MarkerSize = 7 / (comboBox1.SelectedIndex + 1);
            chart.Series[1].MarkerSize = 5 / (comboBox1.SelectedIndex + 1);


            chart.ChartAreas[0].AxisX.Title = "Time (s)";

            if (procData)
            {
                chart.ChartAreas[0].AxisY.Title = "%";
                chart.ChartAreas[0].AxisY2.Title = "%";
            }
            else
            {
                chart.ChartAreas[0].AxisY.Title = "mV";
                chart.ChartAreas[0].AxisY2.Title = "mV";
            }
            

            Dictionary<int, processedData> collectedPoints = MASTER_DICT[index];
            if (!procData)
            {
                collectedPoints.Add(currentpoint, newData);
            }
            

            if (!isFrozen)
            {
                labels[index].Text = (newData.getYCord1()).ToString();
                if (currentpoint > INTERVAL)
                {
                    updateXAxis(currentpoint - INTERVAL, currentpoint, chart);
                    updateYAxis(currentpoint - INTERVAL, currentpoint, chart, index, procData);
                }
                else
                {
                    updateXAxis(1, INTERVAL, chart);
                    updateYAxis(1, INTERVAL, chart, index, procData);
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
            int[] yLoc = new int[NUM_CHANNELS];
            int[] ySize = new int[NUM_CHANNELS];
            for (int k = 0; k < NUM_CHANNELS; k++)
            {
                if (channels[k].Visible || channels[k + NUM_CHANNELS].Visible)
                {
                    numDisp++;
                }
            }

            if (numDisp == NUM_CHANNELS)
            {
                yLoc = new int[NUM_CHANNELS] { 29, 171, 313, 455 };
                ySize = new int[NUM_CHANNELS] { 139, 139, 139, 139 };
            }
            else if (numDisp == NUM_CHANNELS - 1)
            {
                yLoc = new int[NUM_CHANNELS] { 29, 212, 395, 0 };
                ySize = new int[NUM_CHANNELS] { 180, 180, 180, 0 };
            }
            else if (numDisp == NUM_CHANNELS - 2)
            {
                yLoc = new int[NUM_CHANNELS] { 29, 313, 0, 0 };
                ySize = new int[NUM_CHANNELS] { 275, 275, 0, 0 };
            }
            else if (numDisp == NUM_CHANNELS - 3)
            {
                yLoc = new int[NUM_CHANNELS] { 29, 0, 0, 0 };
                ySize = new int[NUM_CHANNELS] { 275, 0, 0, 0 };
            }


            int j = 0;
            for (int k = 0; k < NUM_CHANNELS; k++)
            {
                if (channels[k].Visible || channels[k + NUM_CHANNELS].Visible)
                {
                    channels[k].Location = new Point(6, yLoc[j]);
                    channels[k].Size = new Size(1184, ySize[j]);
                    channels[k + NUM_CHANNELS].Location = new Point(6, yLoc[j]);
                    channels[k + NUM_CHANNELS].Size = new Size(1184, ySize[j]);

                    labels[k].Location = new Point(1189, yLoc[j]);
                    labels[k].Size = new Size(127, ySize[j]);
                    labels[k + NUM_CHANNELS].Location = new Point(1189, yLoc[j]);
                    labels[k + NUM_CHANNELS].Size = new Size(127, ySize[j]);
                    j++;
                }
            }
        }





        // Process all files in the directory passed in, recurse on any directories 
        // that are found, and process the files they contain.
        private void ProcessDirectory(string targetDirectory)
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory, "*.drexel");
            foreach (string fileName in fileEntries)
                ProcessFile(fileName);

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory);
        }

        // Insert logic for processing found files here.
        private void ProcessFile(string path)
        {
            FileInfo f = new FileInfo(path);
            foundSessions.Rows.Add(sessionsFound + 1, f.Name, f.Length, f.CreationTime, f.LastWriteTime.Subtract(f.CreationTime), f.FullName);
            sessionsFound++;
        }



    }
}
