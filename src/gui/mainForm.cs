using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using LumenWorks.Framework.IO.Csv;


namespace GUI
{
    public partial class mainForm : Form
    {
        #region Initializing Elements & Variables

        //Variables
        private Stopwatch stopWatch = new Stopwatch();
        private AboutBox1 aboutInfo;
        private SerialPort adruinoSerial;
        public SerialSetup adruino_dialog;
        private SettingsSetup settings_dialog;
        private Thread thread1;
        private Chart[] channels;
        private Label[] labels;
        private DataTable foundSessions;
        private bool isCollecting;
        private bool isFrozen = false;
        private bool isFullScreen = false;
        private int sessionsFound = 0;
        private int currentpoint;
        private int progressValue = 0;
        private int INTERVAL;
        private static int delay = GUI.Properties.Settings.Default.delay;
        private int freq = GUI.Properties.Settings.Default.frequency;
        const int NUM_CHANNELS = 4;
        private string sessionPath;

        static BackgroundWorker _bw;
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
            channels = new Chart[NUM_CHANNELS * 2] { channel1, channel2, channel3, channel4, channel1_P, channel2_P, channel3_P, channel4_P };
            labels = new Label[NUM_CHANNELS * 2] { label1, label2, label3, label4, label5, label6, label7, label8 };

            for (int j = 0; j < NUM_CHANNELS * 2; j++)
            {
                channels[j].Visible = false;
                channels[j].Series[0].YAxisType = AxisType.Primary;
                channels[j].Series[1].YAxisType = AxisType.Secondary;
                channels[j].Series[0].IsXValueIndexed = true;
                channels[j].Series[1].IsXValueIndexed = true;
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
            if (File.Exists(sessionPath))
            {
                //File.Delete(sessionPath);
            }

            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            Point pt = progressBar1.PointToScreen(new Point(0, 0));
            progressBar1.Parent = this;
            progressBar1.Location = this.PointToClient(pt);
            progressBar1.BringToFront();
            progressBar1.Visible = false;
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
                delay = GUI.Properties.Settings.Default.delay;
                freq = GUI.Properties.Settings.Default.frequency;
                changeInterval();
                Console.Out.WriteLine(delay + " " + freq);
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
                chart.Series[1].MarkerSize = 5 / (comboBox1.SelectedIndex + 1);

                double newBegin = chart.ChartAreas[0].AxisX.Minimum;
                double newEnd = chart.ChartAreas[0].AxisX.Maximum + INTERVAL;

                if (newEnd > currentpoint)
                {
                    newBegin = currentpoint - INTERVAL;
                    if (newBegin < 0)
                    {
                        newBegin = 0;
                    }
                    newEnd = currentpoint;
                }
                if ((newBegin >= 0) && (newEnd > 0))
                {
                    updateXAxis(newBegin, newEnd, chart);
                    updateYAxis(newBegin, newEnd, chart);
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

                if (newBegin < 0)
                {
                    newBegin = 0;
                    newEnd = INTERVAL;
                    button3.Enabled = false;
                }
                button4.Enabled = true;
                updateXAxis(newBegin, newEnd, chart);
                updateYAxis(newBegin, newEnd, chart);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < NUM_CHANNELS * 2; i++)
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
                updateYAxis(newBegin, newEnd, chart);
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

        private void chageReadFileStatus()
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                sessionPath = openFileDialog1.FileName;

                progressBar1.Visible = true;
                button2.Enabled = true;
                checkBox1.Checked = true;
                checkBox2.Checked = true;
                checkBox3.Checked = true;
                checkBox4.Checked = true;
            }
            else
            {
                return;
            }

            _bw = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            _bw.DoWork += get_fileData;
            _bw.ProgressChanged += bw_ProgressChanged;
            _bw.RunWorkerCompleted += bw_RunWorkerCompleted;

            _bw.RunWorkerAsync("Hello to worker");
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

        private void get_fileData(object sender, DoWorkEventArgs e)
        {
            TimeSpan ts = stopWatch.Elapsed;
            double totalTime = Math.Round(ts.TotalSeconds, 1);

            try
            {
                StreamReader sr;
                stopWatch.Start();

                using (sr = new StreamReader(sessionPath))
                {
                    using (CachedCsvReader csv = new CachedCsvReader(sr, false))
                    {
                        int fieldCount = csv.FieldCount;
                        while (csv.ReadNextRecord())
                        {
                            if (_bw.CancellationPending)
                            {
                                e.Cancel = true; return;
                            }

                            double[] fieldArray = new double[17];
                            Parallel.For(0, fieldCount, i =>
                            {
                                fieldArray[i] = Convert.ToDouble(csv[i]);
                            });

                            double[] dataArray = new double[16] { fieldArray[1], fieldArray[2], fieldArray[5], fieldArray[6], 
                                                                         fieldArray[9], fieldArray[10], fieldArray[13], fieldArray[14],
                                                                         fieldArray[3], fieldArray[4], fieldArray[7], fieldArray[8],
                                                                         fieldArray[11], fieldArray[12], fieldArray[15], fieldArray[16]};

                            int progress = (int)(((double)sr.BaseStream.Position / (double)sr.BaseStream.Length) * 100);
                            _bw.ReportProgress(progress);
                            displayData(fieldArray[0], dataArray, false);

                            ts = stopWatch.Elapsed;
                            totalTime = Math.Round(ts.TotalSeconds, 1);
                            Console.WriteLine("Current Time: " +  totalTime + "Proccessed Time: " + fieldArray[0] + "  " + progress);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString());
            }

            ts = stopWatch.Elapsed;
            totalTime = Math.Round(ts.TotalSeconds, 1);
            Console.WriteLine("TOTAL TIME: " + totalTime);
            e.Result = 123;
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                Console.WriteLine("You canceled!");
            else if (e.Error != null)
                Console.WriteLine("Worker exception: " + e.Error.ToString());
            else
                Console.WriteLine("Complete: " + e.Result);      // from DoWork
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage != progressValue)
            {
                progressValue = e.ProgressPercentage;
                BeginInvoke((MethodInvoker)delegate { progressBar1.Value = progressValue; });
                Console.WriteLine("Reached " + e.ProgressPercentage + "%");
            }
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

                        if (ts.TotalHours < 24.0)
                        {
                            double timeStamp = Math.Round(ts.TotalSeconds, 1);

                            adruinoSerial.DtrEnable = true;
                            string buffer = adruinoSerial.ReadTo("\n");
                            adruinoSerial.DtrEnable = false;
                            string[] bufferArray = buffer.Split(new string[] { "\x09" }, StringSplitOptions.RemoveEmptyEntries);
                            double[] dataArray = Array.ConvertAll(bufferArray, s => double.Parse(s));

                            Array.Resize<double>(ref dataArray, 16);
                            for (int i = 8; i < 16; i++)
                            {
                                dataArray[i] = 0;
                            }

                            Console.WriteLine(timeStamp);
                            displayData(timeStamp, dataArray, true);
                        }
                        else
                        {
                            Invoke((MethodInvoker)delegate { changeCollectingStatus(); Refresh(); Update(); });
                        }
                    }
                    Thread.Sleep(delay);
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString());
            }
            adruinoSerial.Close();
        }

        private void displayData(double timeStamp, double[] dataArray, bool fromDevice)
        {

            if (fromDevice)
            {
                calcPerData(dataArray[0], dataArray[1], out dataArray[8], out dataArray[9]);
                calcPerData(dataArray[2], dataArray[3], out dataArray[10], out dataArray[11]);
                calcPerData(dataArray[4], dataArray[5], out dataArray[12], out dataArray[13]);
                calcPerData(dataArray[6], dataArray[7], out dataArray[14], out dataArray[15]);
            }

            currentpoint++;

            Parallel.Invoke(() =>
                { Invoke((MethodInvoker)delegate { updateChart(timeStamp, dataArray[0], dataArray[1], channel1, 0, false, fromDevice); }); }, () =>
                { Invoke((MethodInvoker)delegate { updateChart(timeStamp, dataArray[2], dataArray[3], channel2, 1, false, fromDevice); }); }, () =>
                { Invoke((MethodInvoker)delegate { updateChart(timeStamp, dataArray[4], dataArray[5], channel3, 2, false, fromDevice); }); }, () =>
                { Invoke((MethodInvoker)delegate { updateChart(timeStamp, dataArray[6], dataArray[7], channel4, 3, false, fromDevice); }); }, () =>
                  
                { Invoke((MethodInvoker)delegate { updateChart(timeStamp, dataArray[8], dataArray[9], channel1_P, 0, true, fromDevice); }); }, () =>
                { Invoke((MethodInvoker)delegate { updateChart(timeStamp, dataArray[10], dataArray[11], channel2_P, 1, true, fromDevice); }); }, () =>
                { Invoke((MethodInvoker)delegate { updateChart(timeStamp, dataArray[12], dataArray[13], channel3_P, 2, true, fromDevice); }); }, () =>
                { Invoke((MethodInvoker)delegate { updateChart(timeStamp, dataArray[14], dataArray[15], channel4_P, 3, true, fromDevice); }); }, () =>
                { BeginInvoke((MethodInvoker)delegate { Update(); Refresh(); }); }
            );

            if (fromDevice)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@sessionPath, true))
                {
                    file.WriteLine(timeStamp + "," + dataArray[0] + "," + dataArray[1] + "," + dataArray[8] + "," + dataArray[9]
                                             + "," + dataArray[2] + "," + dataArray[3] + "," + dataArray[10] + "," + dataArray[11]
                                             + "," + dataArray[4] + "," + dataArray[5] + "," + dataArray[12] + "," + dataArray[13]
                                             + "," + dataArray[6] + "," + dataArray[7] + "," + dataArray[14] + "," + dataArray[15]);
                }
            }
        }

        private void updateXAxis(double begin, double end, Chart chart)
        {
            chart.ChartAreas[0].AxisX.Minimum = begin;
            chart.ChartAreas[0].AxisX.Maximum = end;
        }

        private void updateYAxis(double begin, double end, Chart chart)
        {
            double maxX = -3000;
            double maxA = -3000;

            double minX = 3000;
            double minA = 3000;

            if (currentpoint < end)
            {
                end = currentpoint;
            }

            if (end > chart.Series["HbO2"].Points.Count)
            {
                end = chart.Series["HbO2"].Points.Count;
            }

            if (begin < 1)
            {
                begin = 1;
            }

            Parallel.For(Convert.ToInt32(begin), Convert.ToInt32(end), k =>
            {
                double xx, aa;

                xx = chart.Series["HbO2"].Points[k].YValues[0];
                aa = chart.Series["HbR"].Points[k].YValues[0];

                if (xx > maxX) { maxX = xx; }

                if (xx < minX) { minX = xx; }

                if (aa > maxA) { maxA = aa; }

                if (aa < minA) { minA = aa; }
            });

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

        private void updateChart(double x, double y1, double y2, Chart chart, int index, bool procData, bool fromDevice)
        {
            chart.Series["HbO2"].Points.AddXY(x, y1);
            chart.Series["HbR"].Points.AddXY(x, y2);

            if (!isFrozen)
            {
                double newBegin = chart.ChartAreas[0].AxisX.Minimum;
                double newEnd = chart.ChartAreas[0].AxisX.Maximum + INTERVAL;

                if (newEnd > currentpoint)
                {
                    newBegin = currentpoint - INTERVAL;
                    if (newBegin < 0)
                    {
                        newBegin = 0;
                    }
                    newEnd = currentpoint;
                }
                if ((newBegin >= 0) && (newEnd > 0))
                {
                    updateXAxis(newBegin, newEnd, chart);
                    if (chart.Visible)
                    {
                        updateYAxis(newBegin, newEnd, chart);
                        chart.Update();
                    }
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

        private void calcPerData(double yCord1, double yCord2, out double yCord1_P, out double yCord2_P)
        {
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
