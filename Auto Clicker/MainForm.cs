using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;


namespace Auto_Clicker
{
    public partial class MainForm : Form
    {
        #region vars
        public List<string> Names { get; set; } //Hold the list of points in the queue
        public List<Point> Points { get; set; } //Hold the list of points in the queue
        public int Iterations { get; set; } //Hold the number of iterations/repeats
        public List<string> ClickType { get; set; } //Is each point right click or left click
        public List<int> Times { get; set; } //Holds sleep times for after each click
                                             //Import unmanaged functions from DLL library
        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SendInput(int nInputs, ref INPUT pInputs, int cbSize);


        /// <summary>
        /// Structure for SendInput function holding relevant mouse coordinates and information
        /// </summary>
        public struct INPUT
        {
            public uint type;
            public MOUSEINPUT mi;

        };

        /// <summary>
        /// Structure for SendInput function holding coordinates of the click and other information
        /// </summary>
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public int dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        };

        //Constants for use in SendInput and mouse_event
        public const int INPUT_MOUSE = 0x0000;
        public const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        public const int MOUSEEVENTF_LEFTUP = 0x0004;
        public const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        public const int MOUSEEVENTF_RIGHTUP = 0x0010;
        public const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        public const int MOUSEEVENTF_MIDDLEUP = 0x0040;


        // Timers
        ListView Timers = new ListView();
        bool ThreadRun = false;

        #endregion

        #region Global Variables and Properties

        private Thread ClickThread; //Thread to take care of clicking the mouse
                                    //so UI is not made unresponsive

        private Point CurrentPosition { get; set; } //The current position of the mouse cursor

        #endregion

        #region Mouse_Event Methods

        /// <summary>
        /// Click the left mouse button at the current cursor position using
        /// the imported mouse_event function
        /// </summary>
        private void ClickLeftMouseButtonMouseEvent()
        {
            //Send a left click down followed by a left click up to simulate a 
            //full left click
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        /// <summary>
        /// Click the right mouse button at the current cursor position using
        /// the imported mouse_event function
        /// </summary>
        private void ClickRightMouseButtonMouseEvent()
        {
            //Send a left click down followed by a right click up to simulate a 
            //full right click
            mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
        }

        #endregion

        #region SendInput Methods

        /// <summary>
        /// Click the left mouse button at the current cursor position using
        /// the imported SendInput function
        /// </summary>
        public void ClickLeftMouseButtonSendInput()
        {
            //Initialise INPUT object with corresponding values for a left click
            INPUT input = new INPUT();
            input.type = INPUT_MOUSE;
            input.mi.dx = 0;
            input.mi.dy = 0;
            input.mi.dwFlags = MOUSEEVENTF_LEFTDOWN;
            input.mi.dwExtraInfo = IntPtr.Zero;
            input.mi.mouseData = 0;
            input.mi.time = 0;

            //Send a left click down followed by a left click up to simulate a 
            //full left click
            SendInput(1, ref input, Marshal.SizeOf(input));
            input.mi.dwFlags = MOUSEEVENTF_LEFTUP;
            SendInput(1, ref input, Marshal.SizeOf(input));

        }

        /// <summary>
        /// Click the left mouse button at the current cursor position using
        /// the imported SendInput function
        /// </summary>
        public void ClickRightMouseButtonSendInput()
        {
            //Initialise INPUT object with corresponding values for a right click
            INPUT input = new INPUT();
            input.type = INPUT_MOUSE;
            input.mi.dx = 0;
            input.mi.dy = 0;
            input.mi.dwFlags = MOUSEEVENTF_RIGHTDOWN;
            input.mi.dwExtraInfo = IntPtr.Zero;
            input.mi.mouseData = 0;
            input.mi.time = 0;

            //Send a right click down followed by a right click up to simulate a 
            //full right click
            SendInput(1, ref input, Marshal.SizeOf(input));
            input.mi.dwFlags = MOUSEEVENTF_RIGHTUP;
            SendInput(1, ref input, Marshal.SizeOf(input));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Iterate through all queued clicks, for each deciding which mouse button
        /// to press and how long to sleep afterwards
        /// 
        /// This method is assigned to the ClickThread and is the only place where
        /// the mouse buttons are pressed
        /// </summary>
        public void Run()
        {
            try
            {
                label6.Invoke((EventHandler)delegate {
                    label6.Text = "running...";
                });
                int i = 1;

                while (i <= Iterations)
                {
                    //Iterate through all queued clicks
                    for (int j = 0; j <= Points.Count - 1; j++)
                    {
                        //  label2.Text = Names[j];
                        //   label2.Refresh();

                        label3.Invoke((EventHandler)delegate {
                            label3.Text = Names[j];
                        });
                        label5.Invoke((EventHandler)delegate {
                            label5.Text = j.ToString();
                        });

                        SetCursorPosition(Points[j]); //Set cursor position before clicking
                        if (ClickType[j].Equals("R"))
                        {
                            ClickRightMouseButtonSendInput();
                        }
                        else
                        {
                            ClickLeftMouseButtonSendInput();
                        }
                        Thread.Sleep(Times[j]);
                    }
                    i++;
                }

                label6.Invoke((EventHandler)delegate {
                    label6.Text = "stopped...";
                });

            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Set the current position of the cursor to the coordinates held in point
        /// </summary>
        /// <param name="point">Coordinates to set the cursor to</param>
        private void SetCursorPosition(Point point)
        {
            Cursor.Position = point;
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Construct the form object and initialise all form components
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }

        #endregion

        #region Form Component Events

        /// <summary>
        /// Start the timer to update the cursor position and clear all items in the list view
        /// when the form loads
        /// </summary>
        private void MainForm_Load(object sender, EventArgs e)
        {
            CurrentPositionTimer.Start();
            PositionsListView.Items.Clear();
            PositionsListView.LabelEdit = true;

        }

        /// <summary>
        /// Handle keyboard shortcuts from the user
        /// </summary>
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                CopyToAddButton_Click(null, null);
            }
            else if (e.KeyCode == Keys.F2)
            {
                AddPositionButton_Click(null, null);
            }
            else if (e.KeyCode == Keys.F3)
            {
                StartClickingButton_Click(null, null);
            }
            else if (e.KeyCode == Keys.F4)
            {
                StopClickingButton_Click(null, null);
            }
        }

        /// <summary>
        /// Set the CurrentPosition property to the current position of the mouse cursor
        /// on screen on each interval of the timer
        /// </summary>
        private void CurrentPositionTimer_Tick(object sender, EventArgs e)
        {
            CurrentPosition = Cursor.Position;
            UpdateCurrentPositionTextBoxes();
        }

        /// <summary>
        /// Copy current position of the cursor to alternate textboxes so they are ready to 
        /// be queued by the user
        /// </summary>
        private void CopyToAddButton_Click(object sender, EventArgs e)
        {
            QueuedXPositionTextBox.Text = CurrentPosition.X.ToString();
            QueuedYPositionTextBox.Text = CurrentPosition.Y.ToString();
        }

        /// <summary>
        /// Add the point held in the queued textboxes to the listview so ready to be executed
        /// </summary>
        private void AddPositionButton_Click(object sender, EventArgs e)
        {
            if (CurrentPositionIsValid(QueuedXPositionTextBox.Text, QueuedYPositionTextBox.Text))
            {
                if (IsValidNumericalInput(SleepTimeTextBox.Text))
                {
                    //Add item holding coordinates, right/left click and sleep time to list view
                    //holding all queued clicks
                    ListViewItem item = new ListViewItem(textBox_Name.Text);
                    item.SubItems.Add(QueuedXPositionTextBox.Text);
                    item.SubItems.Add(QueuedYPositionTextBox.Text);
                    string clickType = (RightClickCheckBox.Checked) == true ? "R" : "L";

                    int sleepTime = Convert.ToInt32(SleepTimeTextBox.Text);
                    item.SubItems.Add(clickType);
                    item.SubItems.Add(sleepTime.ToString());
                    PositionsListView.Items.Add(item);
                }
                else
                {
                    MessageBox.Show("Sleep time is not a valid positive integer", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Current Coordinates are not valid", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Assign all points in the queue to the ClickHelper and start the thread
        /// </summary>
        private void StartClickingButton_Click(object sender, EventArgs e)
        {
            if (IsValidNumericalInput(NumRepeatsTextBox.Text))
            {
                int iterations = Convert.ToInt32(NumRepeatsTextBox.Text);
                List<Point> points = new List<Point>();
                List<string> clickType = new List<string>();
                List<string> names = new List<string>();
                List<int> times = new List<int>();

                foreach (ListViewItem item in PositionsListView.Items)
                {
                    //Add data in queued clicks to corresponding List collection

                    int x = Convert.ToInt32(item.SubItems[1].Text); //x coordinate
                    int y = Convert.ToInt32(item.SubItems[2].Text); //y coordinate
                    clickType.Add(item.SubItems[3].Text); //click type
                    times.Add(Convert.ToInt32(item.SubItems[4].Text)); //sleep time
                    names.Add(item.Text); //sleep time

                    points.Add(new Point(x, y));


                }
                try
                {
                    //Create a ClickHelper passing Lists of click information
                    Names = names;
                    Points = points;
                    ClickType = clickType;
                    Iterations = iterations;
                    Times = times;

                    //  ClickThreadHelper helper = new ClickThreadHelper() {Names = names, Points = points, ClickType = clickType, Iterations = iterations, Times = times };
                    //Create the thread passing the Run method
                    ClickThread = new Thread(new ThreadStart(Run));
                    //Start the thread, thus starting the clicks
                    ClickThread.Start();
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Number of repeats is not a valid positive integer", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Abort the clicking thread and so stop all simulated clicks
        /// </summary>
        private void StopClickingButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (ClickThread.IsAlive)
                {
                    ClickThread.Abort(); //Attempt to stop the thread
                    ClickThread.Join(); //Wait for thread to stop
                    MessageBox.Show("Clicking successfully stopped", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (ThreadAbortException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Remove all items from the list view holding queued positions
        /// </summary>
        private void RemoveAllMenuItem_Click(object sender, EventArgs e)
        {
            PositionsListView.Items.Clear();
        }

        /// <summary>
        /// Remove only the selected item from the list view holding all queued positions
        /// </summary>
        private void RemoveSelectedMenuItem_Click(object sender, EventArgs e)
        {
            PositionsListView.SelectedItems[0].Remove();
        }


        #endregion

        #region Helper Methods

        /// <summary>
        /// Update current position textboxes to reflect the current position of the cursor
        /// </summary>
        private void UpdateCurrentPositionTextBoxes()
        {
            CurrentXCoordTextBox.Text = this.CurrentPosition.X.ToString();
            CurrentYCoordTextBox.Text = this.CurrentPosition.Y.ToString();
        }

        /// <summary>
        /// Check whether the input string consists of a valid positive integer
        /// </summary>
        /// <param name="input">The string to check</param>
        /// <returns>True if input is a valid positive integer, otherwise false</returns>
        private bool IsValidNumericalInput(string input)
        {
            int temp = 0;
            return (int.TryParse(input, out temp));
        }

        /// <summary>
        /// Check if the coordinates are valid positive integers and also fit
        /// inside the bounds of the monitor
        /// </summary>
        /// <param name="xCoord">The X coordinate to check</param>
        /// <param name="yCoord">The Y coordinate to check</param>
        /// <returns>True if coordinates are valid, otherwise false</returns>
        private bool CurrentPositionIsValid(string xCoord, string yCoord)
        {
            int x, y, width, height = 0;

            if (int.TryParse(xCoord, out x) && int.TryParse(yCoord, out y))
            {
                width = System.Windows.Forms.SystemInformation.VirtualScreen.Width;
                height = System.Windows.Forms.SystemInformation.VirtualScreen.Height;

                if (x <= width && y <= height)
                // if (x <= width && x >= 0 && y <= height && y >= 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Thread Helper Class
        /*
        class ClickThreadHelper
        {
            #region Fields, DLL Imports and Constants
          
            public List<string> Names { get; set; } //Hold the list of points in the queue
            public List<Point> Points { get; set; } //Hold the list of points in the queue
            public int Iterations { get; set; } //Hold the number of iterations/repeats
            public List<string> ClickType { get; set; } //Is each point right click or left click
            public List<int> Times { get; set; } //Holds sleep times for after each click

            //Import unmanaged functions from DLL library
            [DllImport("user32.dll")]
            public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern int SendInput(int nInputs, ref INPUT pInputs, int cbSize);

            /// <summary>
            /// Structure for SendInput function holding relevant mouse coordinates and information
            /// </summary>
            public struct INPUT
            {
                public uint type;
                public MOUSEINPUT mi;

            };

            /// <summary>
            /// Structure for SendInput function holding coordinates of the click and other information
            /// </summary>
            public struct MOUSEINPUT
            {
                public int dx;
                public int dy;
                public int mouseData;
                public int dwFlags;
                public int time;
                public IntPtr dwExtraInfo;
            };

            //Constants for use in SendInput and mouse_event
            public const int INPUT_MOUSE = 0x0000;
            public const int MOUSEEVENTF_LEFTDOWN = 0x0002;
            public const int MOUSEEVENTF_LEFTUP = 0x0004;
            public const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
            public const int MOUSEEVENTF_RIGHTUP = 0x0010;
            public const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
            public const int MOUSEEVENTF_MIDDLEUP = 0x0040;

            #endregion

            #region Mouse_Event Methods

            /// <summary>
            /// Click the left mouse button at the current cursor position using
            /// the imported mouse_event function
            /// </summary>
            private void ClickLeftMouseButtonMouseEvent()
            {
                //Send a left click down followed by a left click up to simulate a 
                //full left click
                mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            }

            /// <summary>
            /// Click the right mouse button at the current cursor position using
            /// the imported mouse_event function
            /// </summary>
            private void ClickRightMouseButtonMouseEvent()
            {
                //Send a left click down followed by a right click up to simulate a 
                //full right click
                mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
            }

            #endregion

            #region SendInput Methods

            /// <summary>
            /// Click the left mouse button at the current cursor position using
            /// the imported SendInput function
            /// </summary>
            public void ClickLeftMouseButtonSendInput()
            {
                //Initialise INPUT object with corresponding values for a left click
                INPUT input = new INPUT();
                input.type = INPUT_MOUSE;
                input.mi.dx = 0;
                input.mi.dy = 0;
                input.mi.dwFlags = MOUSEEVENTF_LEFTDOWN;
                input.mi.dwExtraInfo = IntPtr.Zero;
                input.mi.mouseData = 0;
                input.mi.time = 0;

                //Send a left click down followed by a left click up to simulate a 
                //full left click
                SendInput(1, ref input, Marshal.SizeOf(input));
                input.mi.dwFlags = MOUSEEVENTF_LEFTUP;
                SendInput(1, ref input, Marshal.SizeOf(input));

            }

            /// <summary>
            /// Click the left mouse button at the current cursor position using
            /// the imported SendInput function
            /// </summary>
            public void ClickRightMouseButtonSendInput()
            {
                //Initialise INPUT object with corresponding values for a right click
                INPUT input = new INPUT();
                input.type = INPUT_MOUSE;
                input.mi.dx = 0;
                input.mi.dy = 0;
                input.mi.dwFlags = MOUSEEVENTF_RIGHTDOWN;
                input.mi.dwExtraInfo = IntPtr.Zero;
                input.mi.mouseData = 0;
                input.mi.time = 0;

                //Send a right click down followed by a right click up to simulate a 
                //full right click
                SendInput(1, ref input, Marshal.SizeOf(input));
                input.mi.dwFlags = MOUSEEVENTF_RIGHTUP;
                SendInput(1, ref input, Marshal.SizeOf(input));
            }

            #endregion

            #region Methods

            /// <summary>
            /// Iterate through all queued clicks, for each deciding which mouse button
            /// to press and how long to sleep afterwards
            /// 
            /// This method is assigned to the ClickThread and is the only place where
            /// the mouse buttons are pressed
            /// </summary>
            public void Run()
            {
                try
                {
                    int i = 1;

                    while (i <= Iterations)
                    {
                        //Iterate through all queued clicks
                        for (int j = 0; j <= Points.Count - 1; j++)
                        {
                       


                            SetCursorPosition(Points[j]); //Set cursor position before clicking
                            if (ClickType[j].Equals("R"))
                            {
                                ClickRightMouseButtonSendInput();
                            }
                            else
                            {
                                ClickLeftMouseButtonSendInput();
                            }
                            Thread.Sleep(Times[j]);
                        }
                        i++;
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            /// <summary>
            /// Set the current position of the cursor to the coordinates held in point
            /// </summary>
            /// <param name="point">Coordinates to set the cursor to</param>
            private void SetCursorPosition(Point point)
            {
                Cursor.Position = point;
            }

            #endregion
        }
        */





        #endregion

        #region Timers

        //Setup Timer
        private bool SetUpTimer(TimeSpan alertTime,string str)
        {
            var runAt = DateTime.Today + alertTime;

         //   var aTimer = new System.Timers.Timer(60 * 60 * 1000); //one hour in milliseconds
    //        aTimer.Elapsed += new ElapsedEventHandler(_ => {});

            if (runAt < DateTime.Now)
            {

                // Startuo( str);
                return false;
            }
            else
            {
                //  MessageBox.Show("2");
               // var autoEvent = new AutoResetEvent(false);
                var dueTime = runAt - DateTime.Now;
                label7.Invoke((EventHandler)delegate { label7.Text = dueTime.ToString(); });
                var timer = new System.Threading.Timer(_ => Startuo(  str), null, dueTime,TimeSpan.Zero);
                return true;
            }
        }

        // Run it with timer
        private void Startuo( string str)
        {
            if (IsValidNumericalInput(NumRepeatsTextBox.Text))
            {
                int iterations = Convert.ToInt32(NumRepeatsTextBox.Text);
                List<Point> points = new List<Point>();
                List<string> clickType = new List<string>();
                List<string> names = new List<string>();
                List<int> times = new List<int>();
             
                //   PositionsListView.Invoke((EventHandler)delegate { test=  PositionsListView; });
                for (int i = 0; i < this.PositionsListView.Items.Count; i++)
                {
                    PositionsListView.Invoke(new MethodInvoker(delegate ()
                    {
                        if (str == PositionsListView.Items[i].Text)
                        {
                            int x = Convert.ToInt32(PositionsListView.Items[i].SubItems[1].Text); //x coordinate
                            int y = Convert.ToInt32(PositionsListView.Items[i].SubItems[2].Text); //y coordinate
                            clickType.Add(PositionsListView.Items[i].SubItems[3].Text); //click type
                            times.Add(Convert.ToInt32(PositionsListView.Items[i].SubItems[4].Text)); //sleep time
                            names.Add(PositionsListView.Items[i].Text); //sleep time

                            points.Add(new Point(x, y));
                        }
                    }));

                }
                try
                {
                    //Create a ClickHelper passing Lists of click information
                    Names = names;
                    Points = points;
                    ClickType = clickType;
                    Iterations = iterations;
                    Times = times;

                    //  ClickThreadHelper helper = new ClickThreadHelper() {Names = names, Points = points, ClickType = clickType, Iterations = iterations, Times = times };
                    //Create the thread passing the Run method
                    ClickThread = new Thread(new ThreadStart(Run));
                    //Start the thread, thus starting the clicks
                    ClickThread.Start();
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Number of repeats is not a valid positive integer", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        
            listBox2.Invoke(new MethodInvoker(delegate ()
            {
                listBox2.Items.Add(str + "runned @ " + DateTime.Now.ToString());
            }));

        }

        // Start Timer
        private void button3_Click_1(object sender, EventArgs e)
        {
            ListViewItem item = new ListViewItem(textBox_search.Text);
            item.SubItems.Add(textBox_hours.Text);
            item.SubItems.Add(textBox_minutes.Text);
            item.SubItems.Add("n");
            Timers.Items.Add(item);
         
            listBox1.BeginUpdate();
            listBox1.Items.Add(textBox_search.Text + " @ " + textBox_hours.Text + ":" + textBox_minutes.Text);
            listBox1.EndUpdate();

            if (ThreadRun != true)
            {
               var timer = new System.Timers.Timer();
                timer.Interval = 1000 * 60;
                timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
                timer.Start();
                ThreadRun = true;
            }
            
        }

        // Run every minute
        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            listBox2.Invoke(new MethodInvoker(delegate ()
            {
                listBox2.Items.Add("Checking @ " + DateTime.Now.ToString());
            }));

            String hourMinute;
            String startTime;
            hourMinute = DateTime.Now.ToString("HH:mm");

            foreach (ListViewItem item in Timers.Items)
            {
                startTime = item.SubItems[1].Text + ":" + item.SubItems[2].Text;
                if (hourMinute == startTime)
                {
                    listBox2.Invoke(new MethodInvoker(delegate ()
                    {
                        listBox2.Items.Add(item.Text + " found @ " + DateTime.Now.ToString());
                    }));
                    Startuo(item.Text);
                }
            }
        }

        #endregion


        //save
        private void button1_Click_1(object sender, EventArgs e)
        {
            using (TextWriter tw = new StreamWriter(new FileStream("test.txt", FileMode.Create), Encoding.UTF8))
            {
                foreach (ListViewItem item in PositionsListView.Items)
                {
                    tw.WriteLine(item.Text + "," + item.SubItems[1].Text + "," + item.SubItems[2].Text + "," + item.SubItems[3].Text + "," + item.SubItems[4].Text);
                }
                MessageBox.Show("Done");
            }
        }

        // Load
        private void button2_Click_1(object sender, EventArgs e)
        {
            Stream myStream = null;
            OpenFileDialog theDialog = new OpenFileDialog();
            theDialog.Title = "Open Text File";
            theDialog.Filter = "TXT files|*.txt";
            theDialog.InitialDirectory = @".";
            if (theDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = theDialog.OpenFile()) != null)
                    {
                        using (var sr = new System.IO.StreamReader(myStream)) // Wrapped it up in a using statement so it is disposed of automagically
                        {
                            string line = string.Empty;
                            while ((line = sr.ReadLine()) != null) // Loop while there is more data to read
                            {
                                string[] lineItems = line.Split(','); // Split only the single line

                                // sr.Dispose();

                                ListViewItem item = new ListViewItem(lineItems[0].ToString());
                                item.SubItems.Add(lineItems[1].ToString());
                                item.SubItems.Add(lineItems[2].ToString());
                                item.SubItems.Add(lineItems[3].ToString());
                                item.SubItems.Add(lineItems[4].ToString());


                                PositionsListView.Items.Add(item);



                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

   
    }
}
