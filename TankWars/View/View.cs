///<summary>
/// Author: Ashton Foulger & Austin In - CS3500 Fall 2021
/// Version: 0.1 - 11/15/21
///</summary>

using System;
using System.Drawing;
using System.Windows.Forms;
using GameController;
using GameModel;
using System.Diagnostics;

namespace View
{
    public partial class View : Form
    {
        //Controller to handle server updates
        private Controller controller;

        //World container for players, powerups, and projectiles
        private World world;

        //Form components and variables
        DrawingPanel.DrawingPanel dp;
        Button Connect_To_Server;
        Button Controls_Button;
        Label Server_Label;
        Label Player_Label;
        TextBox Server_IP_Box;
        TextBox Player_Name_Box;
        private const int View_Size = 900;
        private const int Menu_Size = 50;
        private const int FrameTimeMS = 16;

        //Framerate settings
        Stopwatch stopwatch = new Stopwatch();

        public View(Controller _controller) //controller to go inside the parenthesis
        {
            InitializeComponent();
            controller = _controller;

            //Start counting frame updates
            stopwatch.Start();

            //Set Client Size
            ClientSize = new Size(View_Size, View_Size + Menu_Size);

            //Server Lable
            Server_Label = new Label();
            Server_Label.Location = new Point(10, 10);
            Server_Label.Size = new Size(85, 20);
            Server_Label.Text = "Server Address:";
            this.Controls.Add(Server_Label);

            //Server Textbox
            Server_IP_Box = new TextBox();
            Server_IP_Box.Location = new Point(100, 10);
            Server_IP_Box.Size = new Size(100, 20);
            Server_IP_Box.Text = "localhost";
            this.Controls.Add(Server_IP_Box);

            //Player Name Lable
            Player_Label = new Label();
            Player_Label.Location = new Point(210, 10);
            Player_Label.Size = new Size(70, 20);
            Player_Label.Text = "Player Name:";
            this.Controls.Add(Player_Label);

            //Player Name Textbox
            Player_Name_Box = new TextBox();
            Player_Name_Box.Location = new Point(290, 10);
            Player_Name_Box.Size = new Size(100, 20);
            Player_Name_Box.Text = "player";
            this.Controls.Add(Player_Name_Box);

            //Connect Button
            Connect_To_Server = new Button();
            Connect_To_Server.Location = new Point(405, 10);
            Connect_To_Server.Size = new Size(100, 20);
            Connect_To_Server.Text = "Connect";
            Connect_To_Server.Click += ConnectToServerClick;
            this.Controls.Add(Connect_To_Server);

            //Controls Button
            Controls_Button = new Button();
            Controls_Button.Location = new Point(520, 10);
            Controls_Button.Size = new Size(100, 20);
            Controls_Button.Text = "Controls";
            Controls_Button.Click += ControlsClickEvent;
            this.Controls.Add(Controls_Button);

            //Drawing Panel
            dp = new DrawingPanel.DrawingPanel();
            dp.Location = new Point(0, Menu_Size);
            dp.Size = new Size(View_Size, View_Size);
            dp.BackColor = Color.DarkGray;
            this.Controls.Add(dp);

            //Drawing Panel Controls
            dp.KeyUp += new KeyEventHandler(_controller.HandleKeyReleased);
            dp.KeyDown += new KeyEventHandler(_controller.HandleKeyPressed);
            dp.MouseUp += new MouseEventHandler(_controller.HandleMouseClickReleased);
            dp.MouseDown += new MouseEventHandler(_controller.HandleMouseClickPressed);
            dp.MouseMove += new MouseEventHandler(_controller.HandleMouseMovement);
        }

        /// <summary>
        /// Displays all controls for tank wars for the user to see and know how to play the game.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void ControlsClickEvent(object o, EventArgs e)
        {
            MessageBox.Show("Standard Controls: \n" + "Forward = W\n" + "Left = A\n" + "Down = S\n" +
                "Right = D\n" + "Shoot = Left Mouse Button\n" + "Shoot Beam = Right Mouse Button");
        }

        /// <summary>
        /// Event handler for clicking the connect to server button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectToServerClick(object sender, EventArgs e)
        {
            //Disable text fields and ensure user cant connect multiple times
            Connect_To_Server.Enabled = false;
            Player_Name_Box.Enabled = false;
            Server_IP_Box.Enabled = false;

            //Controller Server connection methods
            controller.UpdateArrived += OnReady;
            controller.ConnectToServer(Player_Name_Box.Text, Server_IP_Box.Text, ErrorDialogBox);

            dp.Focus();
            KeyPreview = true;
        }

        /// <summary>
        /// Initializes the connection to the Panel and Controller.
        /// </summary>
        private void OnReady() => this.Invoke((Action)(() =>
        {
            //Get & Set the world
            world = controller.GetWorld();
            dp.SetWorld(world);

            //Update world with server data
            controller.UpdateArrived -= OnReady;
            controller.UpdateArrived += OnFrame;
        }));

        /// <summary>
        /// Invalidates the OnPaint in our view to maintain framerate at 60 FPS.
        /// </summary>
        private void OnFrame()
        {
            //Check if the frame time has become out of sync
            if (stopwatch.ElapsedMilliseconds >= FrameTimeMS)
            {
                stopwatch.Restart();

                //Invalidate the view for OnPaint
                try
                {
                    this.Invoke((Action)(() => this.Invalidate(true)));
                }
                catch (Exception)
                {
                    //Left blank due to error not needing to be thrown
                }
            }
        }

        /// <summary>
        /// Event Handler for errors occured during server connection.
        /// </summary>
        /// <param name="error"></param>
        private void ErrorDialogBox(string error)
        {
            MessageBox.Show("A connection error has occured, ensuring safe disconnect...");
        }
    }
}
