using LumenWorks.Framework.IO.Csv;
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Timers;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;



namespace GUI
{
    public partial class mainForm : Form
    {
        #region Initializing Elements & Variables

        //Variables
        private AboutBox1 aboutInfo;
        private SerialPort adruinoSerial;
        public SerialSetup adruino_dialog;
        private SettingsSetup settings_dialog;
        private Thread thread1, thread2;
        private Chart[] channels;
        private Label[] labels;
        private DataTable foundSessions;
        private bool isCollecting;
        private bool isFrozen = false;
        private bool isFullScreen = false;
        private bool justStarted = false;
        private int sessionsFound = 0;
        private int currentpoint, currentpoint_Read;
        private int INTERVAL;
        private int freq = 15;
        const int NUM_CHANNELS = 4;
        private string sessionPath, logPath;

        private double newBegin, newEnd;

        public mainForm()
        {
            InitializeComponent();
            tabControl1.SelectedIndexChanged += new EventHandler(tabControl1_SelectedIndexChanged);
        }

        private void mainForm_Load(object sender, EventArgs e)
        {
            //Properties.Settings.Default.Reset();
            fullScreenToolStripMenuItem_Click(sender, e);

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
            currentpoint_Read = 0;
            channels = new Chart[NUM_CHANNELS * 2] { channel1, channel2, channel3, channel4, channel1_P, channel2_P, channel3_P, channel4_P };
            labels = new Label[NUM_CHANNELS * 2] { label1, label2, label3, label4, label5, label6, label7, label8 };

            for (int j = 0; j < NUM_CHANNELS * 2; j++)
            {
                channels[j].Visible = false;

                channels[j].Series[0].YAxisType = AxisType.Primary;
                channels[j].Series[1].YAxisType = AxisType.Secondary;
                channels[j].Series[0].IsXValueIndexed = true;
                channels[j].Series[1].IsXValueIndexed = true;

                channels[j].Series[2].YAxisType = AxisType.Primary;
                channels[j].Series[3].YAxisType = AxisType.Secondary;
                channels[j].Series[2].IsXValueIndexed = true;
                channels[j].Series[3].IsXValueIndexed = true;

                channels[j].ChartAreas[0].AxisX.Title = "Time (s)";

                if (j > 3)
                {
                    channels[j].ChartAreas[0].AxisY.Title = "%";
                    channels[j].ChartAreas[0].AxisY2.Title = "%";
                }
                else
                {
                    channels[j].ChartAreas[0].AxisY.Title = "mV";
                    channels[j].ChartAreas[0].AxisY2.Title = "mV";
                }

                labels[j].Visible = false;
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
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            folderBrowserDialog1.SelectedPath = GUI.Properties.Settings.Default.workingDirectory;
            ProcessDirectory(GUI.Properties.Settings.Default.workingDirectory);
            sessionPath = GUI.Properties.Settings.Default.workingDirectory.ToString() + "\\session_" + sessionsFound.ToString() + ".drexel";
            logPath = GUI.Properties.Settings.Default.workingDirectory.ToString() + "log.csv";
            if (File.Exists(logPath))
            {
                File.Delete(logPath);
            }
        }
        #endregion

        #region Action Events (Clicks on Buttons and Menu Items)

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

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            settings_dialog = new SettingsSetup();
            settings_dialog.ShowDialog();
            if (settings_dialog.btnPressed())
            {
                changeInterval();   
            }
        }

        private void workingDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult foundDir = folderBrowserDialog1.ShowDialog();
            if (foundDir == DialogResult.OK)
            {
                sessionsFound = 0;
                foundSessions.Rows.Clear();
                GUI.Properties.Settings.Default.workingDirectory = folderBrowserDialog1.SelectedPath;
                Properties.Settings.Default.Save();
                ProcessDirectory(GUI.Properties.Settings.Default.workingDirectory);

                sessionPath = GUI.Properties.Settings.Default.workingDirectory.ToString() + "\\session_" + sessionsFound.ToString() + ".drexel";
                if (File.Exists(sessionPath))
                {
                    //File.Delete(sessionPath);
                }
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
            //serialDialogOpen();
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

        private void uploadDataFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            chageReadFileStatus();
        }

        private void exportDataToCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string newSessionPath = GUI.Properties.Settings.Default.workingDirectory.ToString() + "\\session_" + sessionsFound.ToString() + ".csv";
            File.Copy(sessionPath, newSessionPath);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            changeInterval();

            for (int i = 0; i < NUM_CHANNELS * 2; i++)
            {
                Chart chart = channels[i];

                chart.Series[0].MarkerSize = 7 / (comboBox1.SelectedIndex + 1);
                chart.Series[2].MarkerSize = 7 / (comboBox1.SelectedIndex + 1);
                chart.Series[1].MarkerSize = 5 / (comboBox1.SelectedIndex + 1);
                chart.Series[3].MarkerSize = 5 / (comboBox1.SelectedIndex + 1);

                newBegin = chart.ChartAreas[0].AxisX.Minimum;
                newEnd = chart.ChartAreas[0].AxisX.Maximum + INTERVAL;

                if (!isFrozen)
                {
                    if (newEnd > currentpoint)
                    {
                        newBegin = currentpoint - INTERVAL;
                        if (newBegin < 0)
                        {
                            newBegin = 0;
                        }
                        newEnd = currentpoint;
                    }
                }
                else
                {
                    if (newEnd > currentpoint_Read)
                    {
                        newBegin = currentpoint_Read - INTERVAL;
                        if (newBegin < 0)
                        {
                            newBegin = 0;
                        }
                        newEnd = currentpoint_Read;
                    }
                }

                if ((newBegin >= 0) && (newEnd > 0))
                {
                    updateXAxis(newBegin, newEnd, chart);
                    updateYAxis(newBegin, newEnd, chart, false);
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
            newBegin = newBegin - INTERVAL;
            newEnd = newBegin + INTERVAL;

            if (newBegin < 0)
            {
                newBegin = 0;
                newEnd = INTERVAL;
                button3.Enabled = false;
            }
            if (currentpoint_Read < newEnd)
            {
                newEnd = currentpoint_Read;
            }

            while (currentpoint_Read > newEnd)
            {
                for (int i = 0; i < NUM_CHANNELS * 2; i++)
                {
                    Chart chart = channels[i];

                    chart.Series["HbO2_Freeze"].Points.RemoveAt(currentpoint_Read - 1);
                    chart.Series["HbR_Freeze"].Points.RemoveAt(currentpoint_Read - 1);

                    button4.Enabled = true;

                    updateXAxis(newBegin, newEnd, chart);
                    updateYAxis(newBegin, newEnd, chart, false);
                }
                currentpoint_Read--;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            newEnd = newEnd + INTERVAL;
            newBegin = newEnd - INTERVAL;

            try
            {
                if (!thread2.IsAlive)
                {
                    thread2 = new Thread(new ThreadStart(get_fileData));
                    thread2.Start();
                }
            }
            catch
            {
                thread2 = new Thread(new ThreadStart(get_fileData));
                thread2.Start();
            }
            button3.Enabled = true;
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
            else if (tabControl1.SelectedTab == tabPage4)
            {
                dataGridView1.DataSource = foundSessions;
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                foundSessions.Rows.Clear();
                sessionsFound = 0;
                ProcessDirectory(GUI.Properties.Settings.Default.workingDirectory);
            }
        }
        #endregion

        #region Helper Functions

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
                    adruinoSerial.Open();

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
                    adruinoSerial.DtrEnable = false;
                    adruinoSerial.Close();
                    
                    break;
            }
        }

        private void chageReadFileStatus()
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                sessionPath = openFileDialog1.FileName;

                //progressBar1.Visible = true;
                //button2.Enabled = true;
                isFrozen = true;
                button3.Enabled = true;
                button4.Enabled = true;
                checkBox1.Checked = true;
                checkBox2.Checked = true;
                checkBox3.Checked = true;
                checkBox4.Checked = true;
            }
            else
            {
                return;
            }

            newBegin = 0;
            newEnd = INTERVAL;

            try
            {
                if (!thread2.IsAlive)
                {
                    thread2 = new Thread(new ThreadStart(get_fileData));
                    thread2.Start();
                }
            }
            catch
            {
                thread2 = new Thread(new ThreadStart(get_fileData));
                thread2.Start();
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

                    newBegin = 0;
                    newEnd = INTERVAL;

                    try
                    {
                        if (!thread2.IsAlive)
                        {
                            thread2 = new Thread(new ThreadStart(get_fileData));
                            thread2.Start();
                        }
                    }
                    catch
                    {
                        thread2 = new Thread(new ThreadStart(get_fileData));
                        thread2.Start();
                    }

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
            comboBox1.Items.Add("15 min");
            comboBox1.Items.Add("30 min");
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
            else if (comboBox1.SelectedItem.Equals("15 min"))
                INTERVAL = 900 * freq;
            else if (comboBox1.SelectedItem.Equals("30 min"))
                INTERVAL = 1800 * freq;
        }

        private void get_fileData()
        {
            try
            {
                StreamReader sr;

                using (FileStream file = new FileStream(sessionPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (sr = new StreamReader(file))
                    {
                        using (CachedCsvReader csv = new CachedCsvReader(sr, false))
                        {
                            int fieldCount = csv.FieldCount;
                            while (csv.ReadNextRecord())
                            {
                                double[] fieldArray = new double[18];
                                Parallel.For(0, fieldCount, i =>
                                {
                                    fieldArray[i] = Convert.ToDouble(csv[i]);
                                });

                                if (fieldArray[0] >= newBegin / freq)
                                {
                                    double[] dataArray = new double[16] {fieldArray[1] , fieldArray[2] , fieldArray[3] , fieldArray[4], 
                                                                 fieldArray[5] , fieldArray[6] , fieldArray[7], fieldArray[8],
                                                                 fieldArray[9] , fieldArray[10] , fieldArray[11] , fieldArray[12],
                                                                 fieldArray[13], fieldArray[14], fieldArray[15], fieldArray[16] 
                                                                };

                                    int progress = (int)(((double)sr.BaseStream.Position / (double)sr.BaseStream.Length) * 100);
                                    displayData(fieldArray[0], dataArray, fieldArray[17], false);

                                }
                                if (fieldArray[0] > newEnd / freq)
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString());
            }
            newEnd = currentpoint_Read;
            newBegin = currentpoint_Read - INTERVAL;
        }

        private void get_bufferData()
        {
            try
            {
                adruinoSerial.DiscardInBuffer();
                adruinoSerial.Write("s"); //Reset Micro
                justStarted = true;
                adruinoSerial.DiscardInBuffer();
                adruinoSerial.DiscardInBuffer();
                adruinoSerial.DtrEnable = true;
                string buffer = null;

                while (isCollecting)
                {
                    if (adruinoSerial.IsOpen)
                    {
                        adruinoSerial.Write("r"); //Get Data
                        buffer = adruinoSerial.ReadTo("\n");

                        try
                        {
                            string[] bufferArray = buffer.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                            double[] fieldArray = Array.ConvertAll(bufferArray, s => double.Parse(s));
                            
                            if ((justStarted) && (fieldArray[0] > 0))
                            {
                                continue;
                            }
                            justStarted = false;


                            double[] dataArray = new double[16] {fieldArray[1] , fieldArray[2] , fieldArray[3] , fieldArray[4], 
                                                                 fieldArray[5] , fieldArray[6] , fieldArray[7], fieldArray[8],
                                                                 0 , 0 , 0 , 0, 0, 0, 0, 0};
                            displayData(fieldArray[0], dataArray, fieldArray[9], true);
                        }
                        catch (Exception err)
                        {
                            //MessageBox.Show(err.ToString());
                        }
                    }
                }
                adruinoSerial.DtrEnable = false;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString());
            }
        }

        
        private void displayData(double timeStamp, double[] dataArray, double ledIndex, bool fromDevice)
        {
            if (fromDevice)
            {
                Parallel.Invoke(() =>
                { Invoke((MethodInvoker)delegate { calcPerData(dataArray[0], dataArray[1], out dataArray[8], out dataArray[9]); }); }, () =>
                { Invoke((MethodInvoker)delegate { calcPerData(dataArray[2], dataArray[3], out dataArray[10], out dataArray[11]); }); }, () =>
                { Invoke((MethodInvoker)delegate { calcPerData(dataArray[4], dataArray[5], out dataArray[12], out dataArray[13]); }); }, () =>
                { Invoke((MethodInvoker)delegate { calcPerData(dataArray[6], dataArray[7], out dataArray[14], out dataArray[15]); }); }
                );

                currentpoint++;
            }
            else
            {
                currentpoint_Read++;
            }

            Parallel.Invoke(() =>
                { Invoke((MethodInvoker)delegate { updateChart(timeStamp, dataArray[0], dataArray[1], channel1, 0, false, fromDevice); }); }, () =>
                { Invoke((MethodInvoker)delegate { updateChart(timeStamp, dataArray[2], dataArray[3], channel2, 1, false, fromDevice); }); }, () =>
                { Invoke((MethodInvoker)delegate { updateChart(timeStamp, dataArray[4], dataArray[5], channel3, 2, false, fromDevice); }); }, () =>
                { Invoke((MethodInvoker)delegate { updateChart(timeStamp, dataArray[6], dataArray[7], channel4, 3, false, fromDevice); }); }, () =>

                { Invoke((MethodInvoker)delegate { updateChart(timeStamp, dataArray[8], dataArray[9], channel1_P, 0, true, fromDevice); }); }, () =>
                { Invoke((MethodInvoker)delegate { updateChart(timeStamp, dataArray[10], dataArray[11], channel2_P, 1, true, fromDevice); }); }, () =>
                { Invoke((MethodInvoker)delegate { updateChart(timeStamp, dataArray[12], dataArray[13], channel3_P, 2, true, fromDevice); }); }, () =>
                { Invoke((MethodInvoker)delegate { updateChart(timeStamp, dataArray[14], dataArray[15], channel4_P, 3, true, fromDevice); }); }, () =>
                { Invoke((MethodInvoker)delegate { updateLED(ledIndex); }); }, () =>
                { BeginInvoke((MethodInvoker)delegate { Update(); Refresh(); }); }
            );

            if (fromDevice)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@sessionPath, true))
                {
                    file.WriteLine(timeStamp + "," + dataArray[0] + "," + dataArray[1] + "," + dataArray[2] + "," + dataArray[3]
                                             + "," + dataArray[4] + "," + dataArray[5] + "," + dataArray[6] + "," + dataArray[7]
                                             + "," + dataArray[8] + "," + dataArray[9] + "," + dataArray[10] + "," + dataArray[11]
                                             + "," + dataArray[12] + "," + dataArray[13] + "," + dataArray[14] + "," + dataArray[15]
                                             + "," + ledIndex);
                }
            }
        }

        private void updateLED (double index)
        {
            for (int k = 0; k < NUM_CHANNELS; k++)
            {
                labels[k].BackColor = Color.Transparent;
                labels[k + NUM_CHANNELS].BackColor = Color.Transparent;
            }

            Color ledColor;
            if ((index % 2) == 0)
            {
                ledColor = Color.Red;
            }
            else
            {
                ledColor = Color.Green;
            }

            if ((index == 0) || (index == 1))
            {
                label1.BackColor = ledColor;
                label5.BackColor = ledColor;
            }
            else if ((index == 2) || (index == 3))
            {
                label2.BackColor = ledColor;
                label6.BackColor = ledColor;
            }
            
            else if ((index == 4) || (index == 5))
            {
                label3.BackColor = ledColor;
                label7.BackColor = ledColor;
            }
            else if ((index == 6) || (index == 7))
            {
                label4.BackColor = ledColor;
                label8.BackColor = ledColor;
            }
        }

        private void updateChart(double x, double y1, double y2, Chart chart, int index, bool procData, bool fromDevice)
        {
            double beg = chart.ChartAreas[0].AxisX.Minimum;
            double end = chart.ChartAreas[0].AxisX.Maximum + INTERVAL;

            if (fromDevice)
            {
                chart.Series["HbO2"].Points.AddXY(x, y1);
                chart.Series["HbR"].Points.AddXY(x, y2);

                if (chart.Series["HbO2"].Points.Count > 3650)
                {
                    chart.Series["HbO2"].Points.RemoveAt(0);
                    chart.Series["HbR"].Points.RemoveAt(0);
                    end = chart.ChartAreas[0].AxisX.Maximum;
                }
            }
            else
            {
                chart.Series["HbO2_Freeze"].Points.AddXY(x, y1);
                chart.Series["HbR_Freeze"].Points.AddXY(x, y2);
            }

            if (!isFrozen)
            {
                chart.Series["HbO2"].Enabled = true;
                chart.Series["HbR"].Enabled = true;
                chart.Series["HbO2_Freeze"].Enabled = false;
                chart.Series["HbO2_Freeze"].Points.Clear();
                chart.Series["HbR_Freeze"].Enabled = false;
                chart.Series["HbR_Freeze"].Points.Clear();
                currentpoint_Read = 0;


                if (end > currentpoint)
                {
                    beg = currentpoint - INTERVAL;
                    if (beg < 0)
                    {
                        beg = 0;
                    }
                    end = currentpoint;
                }

                if ((beg >= 0) && (end > 0))
                {
                    updateXAxis(beg, end, chart);
                    if (chart.Visible)
                    {
                        updateYAxis(beg, end, chart, fromDevice);
                        chart.Update();
                    }
                }

            }
            else
            {
                chart.Series["HbO2"].Enabled = false;
                chart.Series["HbR"].Enabled = false;
                chart.Series["HbO2_Freeze"].Enabled = true;
                chart.Series["HbR_Freeze"].Enabled = true;

                if (end > currentpoint_Read)
                {
                    beg = currentpoint_Read - INTERVAL;
                    if (beg < 0)
                    {
                        beg = 0;
                    }
                    end = currentpoint_Read;
                }

                if ((beg >= 0) && (end > 0))
                {
                    updateXAxis(beg, end, chart);
                    if (chart.Visible)
                    {
                        updateYAxis(beg, end, chart, false);
                        chart.Update();
                    }
                }

            }
        }

        private void updateXAxis(double begin, double end, Chart chart)
        {
            chart.ChartAreas[0].AxisX.Minimum = begin;
            chart.ChartAreas[0].AxisX.Maximum = end;
        }

        private void updateYAxis(double begin, double end, Chart chart, bool fromDevice)
        {
            double maxX = -3000;
            double maxA = -3000;

            double minX = 3000;
            double minA = 3000;

            Series ser1, ser2;
            if (fromDevice)
            {
                ser1 = chart.Series["HbO2"];
                ser2 = chart.Series["HbR"];
                if (currentpoint < end)
                {
                    end = currentpoint;
                }
            }
            else
            {
                ser1 = chart.Series["HbO2_Freeze"];
                ser2 = chart.Series["HbR_Freeze"];
                if (currentpoint_Read < end)
                {
                    end = currentpoint_Read;
                }
            }

            if (end > ser1.Points.Count)
            {
                end = ser1.Points.Count;
            }

            if (begin < 1)
            {
                begin = 1;
            }

            Parallel.For(Convert.ToInt32(begin), Convert.ToInt32(end), k =>
            {
                double xx, aa;

                xx = ser1.Points[k].YValues[0];
                aa = ser2.Points[k].YValues[0];

                if (xx > maxX) { maxX = xx; }

                if (xx < minX) { minX = xx; }

                if (aa > maxA) { maxA = aa; }

                if (aa < minA) { minA = aa; }
            });

            double[] limits = { maxX, minX };

            chart.ChartAreas[0].AxisY.Maximum = Math.Round(limits.Max(), 1, MidpointRounding.AwayFromZero) + .1;
            chart.ChartAreas[0].AxisY.Minimum = Math.Round(limits.Min(), 1, MidpointRounding.AwayFromZero) - .1;

            double interval = Math.Round((chart.ChartAreas[0].AxisY.Maximum - chart.ChartAreas[0].AxisY.Minimum) / 1, 1, MidpointRounding.AwayFromZero);

            chart.ChartAreas[0].AxisY.MajorGrid.Interval = interval;
            chart.ChartAreas[0].AxisY.MajorTickMark.Interval = interval;

            double[] limits2 = { maxA, minA };

            chart.ChartAreas[0].AxisY2.Maximum = Math.Round(limits2.Max(), 1, MidpointRounding.AwayFromZero) + .1;
            chart.ChartAreas[0].AxisY2.Minimum = Math.Round(limits2.Min(), 1, MidpointRounding.AwayFromZero) - .1;

            double interval2 = Math.Round((chart.ChartAreas[0].AxisY2.Maximum - chart.ChartAreas[0].AxisY2.Minimum) / 1, 1, MidpointRounding.AwayFromZero);

            chart.ChartAreas[0].AxisY2.MajorGrid.Enabled = false;
            chart.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;

            chart.ChartAreas[0].AxisY2.MajorGrid.Interval = interval2;
            chart.ChartAreas[0].AxisY2.MajorTickMark.Interval = interval2;

            chart.ChartAreas[0].AxisX.MajorGrid.Interval = 2;
            chart.ChartAreas[0].AxisX.MajorTickMark.Interval = 2;

            if (comboBox1.SelectedIndex > 1)
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

        private void calcPerData(double yCord1, double yCord2, out double yCord1_P, out double yCord2_P)
        {
            Matrix<double> C = DenseMatrix.OfArray(new double[,] {{ GUI.Properties.Settings.Default.c_1, GUI.Properties.Settings.Default.c_2 },
                                                                  { GUI.Properties.Settings.Default.c_3, GUI.Properties.Settings.Default.c_4 }});

            Matrix<double> A = DenseMatrix.OfArray(new double[,] {{ Math.Log10(GUI.Properties.Settings.Default.I0_1/yCord1) },
                                                                  { Math.Log10(GUI.Properties.Settings.Default.I0_2/yCord2) }});

            Matrix<double> temp = (GUI.Properties.Settings.Default.dist * GUI.Properties.Settings.Default.DPF_1) * C;
            Matrix<double> sol = temp.Solve(A);

            Console.WriteLine(sol);

            yCord1_P = (yCord1 + yCord2) / 2 * .4;
            yCord2_P = (yCord1 + yCord2) / 3 * 1.6;


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

        private void serialDialogOpen()
        {
            adruino_dialog = new SerialSetup();
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

        private void ProcessFile(string path)
        {
            FileInfo f = new FileInfo(path);
            foundSessions.Rows.Add(sessionsFound + 1, f.Name, f.Length, f.CreationTime, f.LastWriteTime.Subtract(f.CreationTime), f.FullName);
            sessionsFound++;
        }

        #endregion

    }

}
