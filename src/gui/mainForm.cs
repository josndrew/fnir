using LumenWorks.Framework.IO.Csv;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;



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
        private Thread thread1, thread2, thread3;
        private Chart[] channels;
        private Label[] labels;
        private DataTable foundSessions;
        private bool isCollecting;
        private bool isFrozen = false;
        private bool isFullScreen = false;
        private int sessionsFound = 0;
        private int currentpoint, currentProcpoint;
        private int INTERVAL;
        private int freq = 28;
        const int NUM_CHANNELS = 4;
        private string sessionPath, processedPath, logPath;
        

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
            textBox2.KeyDown += new KeyEventHandler(OnKeyDownHandler);
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;

            currentpoint = 0;
            currentProcpoint = 0;
            channels = new Chart[NUM_CHANNELS * 2] { channel1, channel2, channel3, channel4, channel1_P, channel2_P, channel3_P, channel4_P };
            labels = new Label[NUM_CHANNELS * 2] { label1, label2, label3, label4, label5, label6, label7, label8 };

            for (int j = 0; j < NUM_CHANNELS * 2; j++)
            {
                channels[j].Visible = false;

                channels[j].Series[0].IsXValueIndexed = true;
                channels[j].Series[1].IsXValueIndexed = true;
                channels[j].Series[2].IsXValueIndexed = true;
                channels[j].Series[3].IsXValueIndexed = true;

                channels[j].ChartAreas[0].AxisX.Title = "Time (s)";

                if (j > 3)
                {
                    channels[j].ChartAreas[0].AxisY.Title = "%";
                }
                else
                {
                    channels[j].ChartAreas[0].AxisY.Title = "V";
                }

                labels[j].Visible = false;
            }
            label9.Visible = false;

            setUpInterval();
            changeInterval();


            foundSessions = new DataTable();
            foundSessions.Columns.Add("Session ID:", typeof(int));
            foundSessions.Columns.Add("Session Name:", typeof(string));
            foundSessions.Columns.Add("Session Size (Bytes):", typeof(long));
            foundSessions.Columns.Add("Date of Session:", typeof(DateTime));
            foundSessions.Columns.Add("Session Duration:", typeof(TimeSpan));
            foundSessions.Columns.Add("File Path:", typeof(string));
            foundSessions.Columns.Add("File Attribute:", typeof(string));

            

            dataGridView1.DataSource = foundSessions;
            DataGridViewCheckBoxColumn chk = new DataGridViewCheckBoxColumn();
            chk.HeaderText = "Select";
            dataGridView1.Columns.Add(chk);
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            
            folderBrowserDialog1.SelectedPath = GUI.Properties.Settings.Default.workingDirectory;
            ProcessDirectory(GUI.Properties.Settings.Default.workingDirectory);
            sessionPath = GUI.Properties.Settings.Default.workingDirectory.ToString() + "\\session_" + sessionsFound.ToString() + ".drexel";
            processedPath = GUI.Properties.Settings.Default.workingDirectory.ToString() + "\\session_" + sessionsFound.ToString() + "_Processed.drexel";
            logPath = GUI.Properties.Settings.Default.workingDirectory.ToString() + "log.csv";
            if (File.Exists(logPath))
            {
                File.Delete(logPath);
            }

            
            settings_dialog = new SettingsSetup();
            settings_dialog.TopLevel = false;
            settings_dialog.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            tabPage2.Controls.Add(settings_dialog);
            settings_dialog.Location = new Point(400, 150);
            settings_dialog.Visible = true;
            settings_dialog.Show();
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
            settings_dialog.saveButton_Click(sender, e);
            changeCollectingStatus();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            settings_dialog.saveButton_Click(sender, e);
            changeCollectingStatus();
        }

        private void connectWithAdruinoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //serialDialogOpen();
            connectToDevice();
        }

        private void uploadDataFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                textBox1.Text = File.ReadAllText(openFileDialog1.FileName);

                try
                {
                    if (!thread2.IsAlive)
                    {
                        thread2 = new Thread(new ThreadStart(parseData));
                        thread2.Start();
                    }
                }
                catch
                {
                    thread2 = new Thread(new ThreadStart(parseData));
                    thread2.Start();
                }
            }
            
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

                chart.Series[0].MarkerSize = 10 / (comboBox1.SelectedIndex + 1);
                chart.Series[2].MarkerSize = 10 / (comboBox1.SelectedIndex + 1);
                chart.Series[1].MarkerSize = 7 / (comboBox1.SelectedIndex + 1);
                chart.Series[3].MarkerSize = 7 / (comboBox1.SelectedIndex + 1);

                
                double newBegin = chart.ChartAreas[0].AxisX.Minimum;
                double newEnd = chart.ChartAreas[0].AxisX.Maximum + INTERVAL;

                if (!isFrozen)
                {


                    double pointOfValue = 0;
                    if (chart.Series["HbO2"].Points.Count != 0)
                    {
                        pointOfValue = chart.Series["HbO2"].Points[chart.Series["HbO2"].Points.Count - 1].XValue;
                    }

                    if (newEnd > pointOfValue)
                    {
                        newBegin = pointOfValue - INTERVAL;
                        if (newBegin < 0)
                        {
                            newBegin = 0;
                        }
                        newEnd = pointOfValue;
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
            settings_dialog.saveButton_Click(sender, e);
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
                double newBegin = chart.ChartAreas[0].AxisX.Minimum;
                double newEnd = chart.ChartAreas[0].AxisX.Maximum + INTERVAL;

                newBegin = newBegin - INTERVAL;
                newEnd = newBegin + INTERVAL;

                if (newBegin < 0)
                {
                    newBegin = 0;
                    newEnd = INTERVAL;
                    button3.Enabled = false;
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < NUM_CHANNELS * 2; i++)
            {
                Chart chart = channels[i];
                double newBegin = chart.ChartAreas[0].AxisX.Minimum;
                double newEnd = chart.ChartAreas[0].AxisX.Maximum + INTERVAL;

                newEnd = newEnd + INTERVAL;
                newBegin = newEnd - INTERVAL;
                button3.Enabled = true;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            adruinoSerial.Write("m");
            Thread.Sleep(500);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            connectToDevice();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (adruinoSerial.IsOpen)
            {
                adruinoSerial.Write(textBox2.Text);
                textBox2.Clear();
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
                    settings_dialog.Visible = true;
                    settings_dialog.Enabled = false;
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


                    Thread.Sleep(1000);

                    try
                    {
                        if (!thread2.IsAlive)
                        {
                            thread2 = new Thread(new ThreadStart(parseData));
                            thread2.Start();
                        }
                    }
                    catch
                    {
                        thread2 = new Thread(new ThreadStart(parseData));
                        thread2.Start();
                    }

                    Thread.Sleep(2000);

                    try
                    {
                        if (!thread3.IsAlive)
                        {
                            thread3 = new Thread(new ThreadStart(processData));
                            thread3.Start();
                        }
                    }
                    catch
                    {
                        thread3 = new Thread(new ThreadStart(processData));
                        thread3.Start();
                    }

                    break;

                case "STOP":
                    isCollecting = false;
                    startToolStripMenuItem.Enabled = false;
                    stopToolStripMenuItem.Enabled = false;
                    button1.Visible = false;
                    settings_dialog.Visible = true;
                    settings_dialog.Enabled = true;
                    
                    button2.Enabled = false;
                    button3.Enabled = true;
                    button4.Enabled = true;

                    for (int k = 0; k < NUM_CHANNELS; k++)
                    {
                        labels[k].BackColor = Color.Transparent;
                        labels[k + NUM_CHANNELS].BackColor = Color.Transparent;
                    }
                    
                    break;
            }
        }

        private void connectToDevice()
        {
            try
            {
                adruinoSerial.PortName = GUI.Properties.Settings.Default.PortName;
                adruinoSerial.BaudRate = GUI.Properties.Settings.Default.BaudRate;
                adruinoSerial.Parity = GUI.Properties.Settings.Default.Parity;
                adruinoSerial.DataBits = GUI.Properties.Settings.Default.DataBits;
                adruinoSerial.StopBits = GUI.Properties.Settings.Default.StopBits;
                adruinoSerial.Handshake = GUI.Properties.Settings.Default.Handshake;

                adruinoSerial.DataReceived += new SerialDataReceivedEventHandler(comPort_DataReceived);
                adruinoSerial.Open();

                startToolStripMenuItem.Enabled = true;
                stopToolStripMenuItem.Enabled = false;
                button1.Enabled = true;
                button2.Enabled = true;
                button5.Enabled = true;
                button6.Enabled = true;
            }

            catch (System.UnauthorizedAccessException)
            {
                MessageBox.Show("UNAUTHORIZED ACCESS: " + adruinoSerial.PortName + " is probably being used in some other application");
            }

            catch
            {
                serialDialogOpen();
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
        

        private void get_bufferData()
        {
            try
            {
                adruinoSerial.Write("s"); //Reset Micro
                adruinoSerial.DiscardInBuffer();
                adruinoSerial.DiscardInBuffer();
                adruinoSerial.DiscardOutBuffer();
                adruinoSerial.DiscardInBuffer();
                adruinoSerial.DiscardOutBuffer();          

                while (isCollecting)
                {

                    if (adruinoSerial.DtrEnable)
                    {
                        
                        adruinoSerial.Write("r"); //Get Data
                        Thread.Sleep(GUI.Properties.Settings.Default.delay);
                    }
                    else
                    {
                        adruinoSerial.DtrEnable = true;
                    }
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString());
            }
        }

        
        private void parseData()
        {
            int lineCount = 0;

            while (textBox1.Lines.Count() < 10) { }

            while (true)
            {
                string[] bufferArray = (textBox1.Lines[lineCount]).Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                if (bufferArray.Length > 0)
                {
                    if (bufferArray.Length == 6)
                    {
                        double[] fieldArray = Array.ConvertAll(bufferArray, s => double.Parse(s));
                        displayData(fieldArray[0], new double[4] { fieldArray[1], fieldArray[2], fieldArray[3], fieldArray[4] }, fieldArray[5], true);
                    }
                    lineCount++;
                }
                else
                {
                    if ( (lineCount == textBox1.Lines.Count()-1) && (!isCollecting))
                    {
                        return;
                    }
                }
            }
        }

        private void processData()
        {
            int lineCount = 1;

            while (textBox1.Lines.Count() < 80) { }

            while (true)
            {
                try
                {
                    string[] buffer1 = (textBox1.Lines[lineCount]).Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    string[] buffer2 = (textBox1.Lines[lineCount + 1]).Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    string outBuffer = "";
                    if ((buffer1.Length > 0) && (buffer2.Length > 0))
                    {
                        if ((buffer1.Length == 6) && (buffer2.Length == 6))
                        {
                            double[] field1 = Array.ConvertAll(buffer1, s => double.Parse(s));
                            double[] field2 = Array.ConvertAll(buffer2, s => double.Parse(s));

                            outBuffer += field1[0];
                            for (int k = 1; k < 5; k++)
                            {
                                double yp1 = 0, yp2 = 0;
                                calcPerData(field1[k], field2[k], out yp1, out yp2);
                                Invoke((MethodInvoker)delegate { updateChart(field1[0], yp1, yp2, channels[k + 3], k + 3, true, true); });
                                outBuffer = outBuffer + "," + yp1 + "," + yp2;
                            }

                            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@processedPath, true))
                            {
                                file.WriteLine(outBuffer);
                            }
                        }
                        lineCount += 2;
                        if ((lineCount % 5) == 0)
                        {
                            lineCount++;
                        }
                    }
                }
                catch (System.IndexOutOfRangeException)
                {
                    //Thread.Sleep(1000);
                }

                if ((lineCount == textBox1.Lines.Count()) && (!isCollecting))
                {
                    return;
                }
            }
        }

        private void displayData(double timeStamp, double[] dataArray, double ledIndex, bool fromDevice)
        {
            currentpoint++;
            Invoke((MethodInvoker)delegate { updateChart(timeStamp, dataArray[0], dataArray[0], channel1, 0, false, fromDevice); });
            Invoke((MethodInvoker)delegate { updateChart(timeStamp, dataArray[1], dataArray[1], channel2, 1, false, fromDevice); });
            Invoke((MethodInvoker)delegate { updateChart(timeStamp, dataArray[2], dataArray[2], channel3, 2, false, fromDevice); });
            Invoke((MethodInvoker)delegate { updateChart(timeStamp, dataArray[3], dataArray[3], channel4, 3, false, fromDevice); });
            Invoke((MethodInvoker)delegate { updateLED(ledIndex); });
            Invoke((MethodInvoker)delegate { Update(); });
        }

        private void updateChart(double x, double y1, double y2, Chart chart, int index, bool procData, bool fromDevice)
        {
            if (fromDevice)
            {
                chart.Series["HbO2"].Points.AddXY(x, y1);
                chart.Series["HbR"].Points.AddXY(x, y2);

                chart.Series["HbR"].Enabled = false;
        
                if (chart.Series["HbO2"].Points.Count > 4000)
                {
                    chart.Series["HbO2"].Points.RemoveAt(0);
                    chart.Series["HbR"].Points.RemoveAt(0);
                }
                labels[index].Text = ((Math.Abs(y1) + Math.Abs(y2))).ToString("N2");

                //if ((Math.Abs(y1) > 70 ) || (Math.Abs(y2) > 70))
                //{
                //    label9.Visible = true;
                //}
            }


            if (!isFrozen)
            {
                chart.Series["HbO2"].Enabled = true;
                labels[index].Text = (y1).ToString("N2");

                if (index > 3)
                {
                    chart.Series["HbR"].Enabled = true;
                    labels[index].Text = ((Math.Abs(y1) + Math.Abs(y2))).ToString("N2");
                }

                double beg = chart.ChartAreas[0].AxisX.Minimum; 
                double end = chart.ChartAreas[0].AxisX.Maximum + INTERVAL;
                int propValue = 0;

                if (index < 4)
                {
                    propValue = currentpoint;
                    
                    if (end > propValue)
                    {
                        beg = (chart.Series["HbO2"].Points.Count - 1) - (INTERVAL);
                        if (beg < 0)
                        {
                            beg = 0;
                        }
                        end = chart.Series["HbO2"].Points.Count - 1;
                    }
                }
                else
                {
                    propValue = currentProcpoint;

                    if (end > propValue)
                    {
                        beg = (chart.Series["HbO2"].Points.Count-1) - (INTERVAL/2.0);
                        if (beg < 0)
                        {
                            beg = 0;
                        }
                        end = chart.Series["HbO2"].Points.Count-1;
                    }
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
        }

        private void updateLED(double index)
        {
            if(index == 1)
            {
                pictureBox1.Image = GUI.Properties.Resources._1194989231691813435led_circle_red_svg_med;
                pictureBox3.Image = GUI.Properties.Resources._1194989228908147779led_circle_grey_svg_med;
            }
            else if (index == 2)
            {
                pictureBox1.Image = GUI.Properties.Resources._11949892282132520602led_circle_green_svg_med;
                pictureBox3.Image = GUI.Properties.Resources._1194989228908147779led_circle_grey_svg_med;
            }
            else if (index == 3)
            {
                pictureBox1.Image = GUI.Properties.Resources._1194989228908147779led_circle_grey_svg_med;
                pictureBox3.Image = GUI.Properties.Resources._1194989231691813435led_circle_red_svg_med;
            }
            else if (index == 4)
            {
                pictureBox1.Image = GUI.Properties.Resources._1194989228908147779led_circle_grey_svg_med;
                pictureBox3.Image = GUI.Properties.Resources._11949892282132520602led_circle_green_svg_med;
            }
            else if ((index == 0) || (index == 5))
            {
                pictureBox1.Image = GUI.Properties.Resources._1194989228908147779led_circle_grey_svg_med;
                pictureBox3.Image = GUI.Properties.Resources._1194989228908147779led_circle_grey_svg_med;
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
            }
            else
            {
                ser1 = chart.Series["HbO2_Freeze"];
                ser2 = chart.Series["HbR_Freeze"];
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

            double[] limits = { maxX, minX, maxA, minA};

            chart.ChartAreas[0].AxisY.Maximum = Math.Round(limits.Max(), 3, MidpointRounding.AwayFromZero) + .4;
            chart.ChartAreas[0].AxisY.Minimum = Math.Round(limits.Min(), 3, MidpointRounding.AwayFromZero) - .4;

            double interval = Math.Round((chart.ChartAreas[0].AxisY.Maximum - chart.ChartAreas[0].AxisY.Minimum) / 4, 3, MidpointRounding.AwayFromZero);

            chart.ChartAreas[0].AxisY.MajorGrid.Interval = interval;
            chart.ChartAreas[0].AxisY.MajorTickMark.Interval = interval;


            chart.ChartAreas[0].AxisX.MinorGrid.Enabled = false;
            chart.ChartAreas[0].AxisX.MinorTickMark.Enabled = false;

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

            Matrix<double> DPF = DenseMatrix.OfArray(new double[,] {{ GUI.Properties.Settings.Default.DPF_1, GUI.Properties.Settings.Default.DPF_2 },
                                                                    { GUI.Properties.Settings.Default.DPF_1, GUI.Properties.Settings.Default.DPF_2 }});

            Matrix<double> temp = DPF.Multiply(GUI.Properties.Settings.Default.dist);
            Matrix<double> eqn = C.PointwiseMultiply(temp);
            Matrix<double> sol = eqn.Solve(A);

            if ((Double.IsNaN(sol[0,0])) | (Double.IsNaN(sol[1, 0])) | (Double.IsInfinity(sol[0, 0])) | (Double.IsInfinity(sol[1, 0])))
            {
                sol[0, 0] = 0;
                sol[1, 0] = 0;
            }

            yCord1_P = sol[0, 0] * 100;
            yCord2_P = sol[1, 0] * 100;

            
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

        private void calibrateDeviceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            adruinoSerial.Write("b");
            Thread.Sleep(300);
        }

        private void comPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            bool open = adruinoSerial.IsOpen;
            string buffer = adruinoSerial.ReadLine();
            buffer = buffer.TrimEnd('\r');
            buffer = buffer.TrimEnd('\0');
            Invoke((MethodInvoker)delegate { textBox1.AppendText(buffer + "\n"); });
            
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@sessionPath, true))
            {
                file.WriteLine(buffer);
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
            foundSessions.Rows.Add(sessionsFound + 1, f.Name, f.Length, f.CreationTime, f.LastWriteTime.Subtract(f.CreationTime), f.FullName, f.Attributes);
            sessionsFound++;
        }

        #endregion

        private void label9_Click(object sender, EventArgs e)
        {
            label9.Visible = false;
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Return)
            {
                button8_Click(sender, e);
            }
        }

        public static void ShowFormInContainerControl(Control ctl, Form frm)
        {
            frm.TopLevel = false;
            frm.FormBorderStyle = FormBorderStyle.None;
            frm.Dock = DockStyle.Fill;
            frm.Visible = true;
            ctl.Controls.Add(frm);
        }
    }

}
