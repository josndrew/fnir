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
        private processedData data;
        public SerialSetup adruino_dialog;
        private bool isCollecting;
        private bool isFullScreen = false;
        private Thread thread1;
        private Dictionary<int,processedData> collectedPoints;

        public mainForm()
        {
            InitializeComponent();
        }

        //Initializing of Variables
        private void mainForm_Load(object sender, EventArgs e)
        {
            //Properties.Settings.Default.Reset();
            //this.TopMost = true;
            //this.WindowState = FormWindowState.Maximized;
            //this.FormBorderStyle = FormBorderStyle.None;

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
            currentpoint = 0;
            data = null;
        }
       
        private void fullScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!isFullScreen)  // FullScreenMode is an enum
            {
                this.WindowState = FormWindowState.Normal;
                //this.FormBorderStyle = FormBorderStyle.None;
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
                    break;
            }
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
                        //if (collectedPoints.ContainsKey(5))
                        //{   
                        //    double [,] prev = new double [5,3];
                            
                        //    for (int k = 0; k < 5; k++)
                        //    {
                        //        if (collectedPoints.TryGetValue(currentpoint - k, out d))
                        //        {
                        //            prev[k,0] = d.getXCord();
                        //        }
                        //    }
                        //}
                        
                        d.setXCord(dataArray[0]);


                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\Andrew Joseph\Desktop\Senior Design\\data.txt", true))
                        {
                            file.WriteLine(dataArray[0]);
                        }

                        Invoke((MethodInvoker)delegate { Update(); updateChart(d); Refresh(); Update(); });
                        data = null;
                    }
                }
                adruinoSerial.Close();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString());
            }     
        }


        private void updateChart(processedData newData)
        {
            rawData.Update();
            
            rawData.Series["Reading"].Points.AddY(newData.getXCord());
           

            analyzedData.Series["Average"].Points.AddY(newData.getAvg());

            rawData.ChartAreas[0].AxisX.Title = "# of Data Points";
            rawData.ChartAreas[0].AxisY.Title = "mV";

            analyzedData.ChartAreas[0].AxisX.Title = "# of Data Points";
            analyzedData.ChartAreas[0].AxisY.Title = "Acceleration-g";

            currentpoint++;

            collectedPoints.Add(currentpoint, newData); 
            

            if (currentpoint > 15)
            {
                rawData.ChartAreas[0].AxisX.Minimum = currentpoint - 15;
                rawData.ChartAreas[0].AxisX.Maximum = currentpoint;

                analyzedData.ChartAreas[0].AxisX.Minimum = currentpoint - 15;
                analyzedData.ChartAreas[0].AxisX.Maximum = currentpoint;

            }
            else
            {
                rawData.ChartAreas[0].AxisX.Minimum = 1;
                rawData.ChartAreas[0].AxisX.Maximum = 15;

                analyzedData.ChartAreas[0].AxisX.Minimum = 1;
                analyzedData.ChartAreas[0].AxisX.Maximum = 15;
            }


            double maxX = -300;
            double maxA = -300;

            double minX = 300;
            double minA = 300;

            for (int k = Convert.ToInt32(rawData.ChartAreas[0].AxisX.Minimum); k < currentpoint; k++)
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

            double[] limits = { maxX, minX};

            rawData.ChartAreas[0].AxisY.Maximum = Math.Round(limits.Max(), 1, MidpointRounding.AwayFromZero) + .2;
            rawData.ChartAreas[0].AxisY.Minimum = Math.Round(limits.Min(), 1, MidpointRounding.AwayFromZero) - .2;

            rawData.ChartAreas[0].AxisX.MajorGrid.Interval = 2;
            rawData.ChartAreas[0].AxisX.MajorTickMark.Interval = 2;

            double interval = Math.Round((rawData.ChartAreas[0].AxisY.Maximum - rawData.ChartAreas[0].AxisY.Minimum) / 7, 1, MidpointRounding.AwayFromZero);

            rawData.ChartAreas[0].AxisY.MajorGrid.Interval = interval;
            rawData.ChartAreas[0].AxisY.MajorTickMark.Interval = interval;

            rawData.Update();


            double[] limits2 = { maxA, minA };

            analyzedData.ChartAreas[0].AxisY.Maximum = Math.Round(limits2.Max(), 1, MidpointRounding.AwayFromZero) + .2;
            analyzedData.ChartAreas[0].AxisY.Minimum = Math.Round(limits2.Min(), 1, MidpointRounding.AwayFromZero) - .2;

            analyzedData.ChartAreas[0].AxisX.MajorGrid.Interval = 2;
            analyzedData.ChartAreas[0].AxisX.MajorTickMark.Interval = 2;

            double interval2 = Math.Round((analyzedData.ChartAreas[0].AxisY.Maximum - analyzedData.ChartAreas[0].AxisY.Minimum) / 7, 1, MidpointRounding.AwayFromZero);

            analyzedData.ChartAreas[0].AxisY.MajorGrid.Interval = interval2;
            analyzedData.ChartAreas[0].AxisY.MajorTickMark.Interval = interval2;

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
                }

                catch
                {
                    serialDialogOpen();
                }
            }
        }




    }
}
