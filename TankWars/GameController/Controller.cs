///<summary>
/// Author: Ashton Foulger & Austin In - CS3500 Fall 2021
/// Version: 0.2 - 11/16/21
///</summary>

using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows.Forms;
using GameModel;
using TankWars;
using NetworkUtil;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq;

namespace GameController
{
    /// <summary>
    /// Controller of the MVC model of programming to control user imput and physics of the game.
    /// </summary>
    public class Controller
    {
        //World and Player variables
        private World world;
        private int PlayerID;
        private string PlayerName;

        //Input command data structures
        private List<string> MovementCommands;
        private List<string> FireCommands;
        private Vector2D AimingDirection = new Vector2D();

        //Server Event Handlers
        public delegate void ServerUpdateHandler();
        public event ServerUpdateHandler UpdateArrived;
        private Action<string> NetworkErrorOccurred;

        /// <summary>
        /// Controller Constructor
        /// </summary>
        public Controller()
        {
            MovementCommands = new List<string>() { "none" };
            FireCommands = new List<string>() { "none" };
        }

        /// <summary>
        /// Getter for the world from MVC model.
        /// </summary>
        /// <returns></returns>
        public World GetWorld()
        {
            return world;
        }

        //-------------------------Client Input-------------------------//

        /// <summary>
        /// Moves the player in the correct direction based upon which key is pressed by the client.
        /// Locking a input thread to process the adding and removing of movement commands.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HandleKeyPressed(object sender, KeyEventArgs e)
        {
            string key = "";

            //Determine which key was pressed
            switch (e.KeyCode)
            {
                case Keys.W:
                    key = "up";
                    break;
                case Keys.A:
                    key = "left";
                    break;
                case Keys.S:
                    key = "down";
                    break;
                case Keys.D:
                    key = "right";
                    break;
                default:
                    break;
            }

            //If key pressed is not empty start the move command at the end of the list and remove
            if (key != "")
            {
                lock (MovementCommands)
                {
                    MovementCommands.Remove(key);
                    MovementCommands.Add(key);
                }
                return;
            }
        }

        /// <summary>
        /// Removes command codes from movment list after check of key being pressed then released.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HandleKeyReleased(object sender, KeyEventArgs e)
        {
            string key = "";

            //Determine which key was released
            switch (e.KeyCode)
            {
                case Keys.W:
                    key = "up";
                    break;
                case Keys.A:
                    key = "left";
                    break;
                case Keys.S:
                    key = "down";
                    break;
                case Keys.D:
                    key = "right";
                    break;
                default:
                    break;
            }

            //If key was released remove the command code
            if (key != "")
            {
                lock (MovementCommands)
                {
                    MovementCommands.Remove(key);
                }
                return;
            }
        }

        /// <summary>
        /// Event handler for when the mouse button is released adding a command to the FireCommands list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HandleMouseClickReleased(object sender, MouseEventArgs e)
        {
            string click = "";

            //Check if mouse click has been released
            if (e.Button == MouseButtons.Left)
            {
                click = "main";
            }
            else if (e.Button == MouseButtons.Right)
            {
                click = "alt";
            }

            //If valid click remove command
            if (click != "")
            {
                FireCommands.Remove(click);
                return;
            }
        }

        /// <summary>
        /// Evetn handler for when the mouse button is clicked assigning the right fire command.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HandleMouseClickPressed(object sender, MouseEventArgs e)
        {
            string click = "";

            //Check if mouse click has been pressed
            if (e.Button == MouseButtons.Left)
            {
                click = "main";
            }
            else if (e.Button == MouseButtons.Right)
            {
                click = "alt";
            }

            //If valid click add to the end of list and remove previous command
            if (click != "")
            {
                FireCommands.Remove(click);
                FireCommands.Add(click);
                return;
            }
        }

        /// <summary>
        /// Event Handler for mouse movement within the drawing panel,
        /// to get accurate turret direction on the players tank. 
        /// Allowing for the player to get accuratly aimed shots of projectiles and beams.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HandleMouseMovement(object sender, MouseEventArgs e)
        {
            this.AimingDirection = new Vector2D(e.X - (900 / 2), e.Y - (900 / 2));
            this.AimingDirection.Normalize();
        }

        //-------------------------Server & Network Connection-------------------------//

        /// <summary>
        /// Gets and error from the state if connection failed or a network error occured.
        /// </summary>
        /// <param name="state"></param>
        private void ConnectionError(SocketState state)
        {
            if (state.TheSocket != null)
            {
                state.TheSocket.Close();
            }

            NetworkErrorOccurred(state.ErrorMessage);
        }

        /// <summary>
        /// Establishes connection of client to the server, to begin connection loop.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="server"></param>
        /// <param name="ErrorOccurred"></param>
        public void ConnectToServer(string ID, string server, Action<string> ErrorOccurred)
        {
            this.PlayerName = ID;
            this.NetworkErrorOccurred = ErrorOccurred;
            Networking.ConnectToServer(OnConnect, server, 11000);
        }

        /// <summary>
        /// Method to be invoked by ConnectToServer to send client data upon connection.
        /// </summary>
        /// <param name="state"></param>
        public void OnConnect(SocketState state)
        {
            // check if error connection occurred
            if (state.ErrorOccured)
            {
                ConnectionError(state);
                return;
            }
            // Start connection handshake process
            state.OnNetworkAction = ProcessConnectionState;
            Networking.Send(state.TheSocket, PlayerName + "\n");
            Networking.GetData(state);
        }

        /// <summary>
        /// Method to be invoked by OnConnect which will provide the client data, 
        /// world data, and wall data from the server. Also taking in current client
        /// data of other users.
        /// </summary>
        /// <param name="state"></param>
        public void ProcessConnectionState(SocketState state)
        {
            // check if connection error occurred
            if (state.ErrorOccured)
            {
                ConnectionError(state);
                return;
            }

            // getting and separating the data string from server
            string data = state.GetData();
            string[] parts = Regex.Split(data, @"(?<=[\n])");

            // check if the world data is null and make sure data has a valid parsing argument 
            if (world is null && parts.Length >= 2 && parts[1].EndsWith("\n"))
            {
                // set the PlayerID
                PlayerID = int.Parse(parts[0]);

                // create a new world
                world = new World(PlayerID, int.Parse(parts[1]));

                // remove world and player data
                state.RemoveData(0, parts[0].Length + parts[1].Length);

                // check to see if update arrived
                if (!(UpdateArrived == null))
                {
                    UpdateArrived?.Invoke();
                }
            }

            // Receive next set of packet data and start packet processing loop
            state.OnNetworkAction = OnReceived;
            Networking.GetData(state);
        }

        /// <summary>
        /// Allows for the client to continually receive data from the server,
        /// so that the client can update with current player data. (tank, projectile, beam location data)
        /// </summary>
        /// <param name="state"></param>
        private void OnReceived(SocketState state)
        {
            // check if connection error occurred
            if (state.ErrorOccured)
            {
                ConnectionError(state);
                return;
            }

            //Process data received
            ProcessPackets(state);
            ReceivePackets(state);
        }

        /// <summary>
        /// Sends any movement commands to the server and updating
        /// the clients view while also getting other player data.
        /// </summary>
        /// <param name="state"></param>
        private void ReceivePackets(SocketState state)
        {
            // check if connection error occurred
            if (state.ErrorOccured)
            {
                ConnectionError(state);
                return;
            }

            //Send any movement commands to the server, and receive any updated players data
            ProcessMovementCommands(state);
            Networking.GetData(state);

            if (!(UpdateArrived is null))
            {
                UpdateArrived.Invoke();
            }
        }

        /// <summary>
        /// Processes packet data sent from the server to update the client 
        /// with current tanks, projectiles, and beam location data from other player.
        /// </summary>
        /// <param name="state"></param>
        private void ProcessPackets(SocketState state)
        {
            // check if connection error occurred
            if (state.ErrorOccured)
            {
                ConnectionError(state);
                return;
            }

            // getting and separating the data string from server
            string data = state.GetData();
            string[] parts = Regex.Split(data, @"(?<=[\n])");

            //Process data from recieved packet
            lock (world)
            {
                foreach (string s in parts)
                {
                    //Ignore empty strings and new line parsers
                    if (s.Length == 0)
                    {
                        continue;
                    }
                    if (s[s.Length - 1] != '\n')
                    {
                        break;
                    }

                    //Parse in object data from Json data
                    JObject obj = JObject.Parse(s);
                    JToken t;

                    //Check if object is of type tank
                    t = obj["tank"];
                    if (!(t is null))
                    {
                        Tank _tank = JsonConvert.DeserializeObject<Tank>(s);
                        world.Tanks[_tank.ID] = _tank;
                    }

                    //Check if object is of type projectile
                    t = obj["proj"];
                    if (!(t is null))
                    {
                        Projectile _projectile = JsonConvert.DeserializeObject<Projectile>(s);
                        world.Projectiles[_projectile.ID] = _projectile;
                    }

                    //Check if object is of type beam
                    t = obj["beam"];
                    if (!(t is null))
                    {
                        Beam _beam = JsonConvert.DeserializeObject<Beam>(s);
                        world.Beams[_beam.ID] = _beam;
                    }

                    //Check if object is of type powerup
                    t = obj["power"];
                    if (!(t is null))
                    {
                        Powerup _powerup = JsonConvert.DeserializeObject<Powerup>(s);
                        world.Powerups[_powerup.ID] = _powerup;
                    }

                    //Check if object is of type wall
                    t = obj["wall"];
                    if (!(t is null))
                    {
                        Wall _wall = JsonConvert.DeserializeObject<Wall>(s);
                        world.Walls[_wall.ID] = _wall;
                    }
                    state.RemoveData(0, s.Length);
                }
            }
        }

        /// <summary>
        /// Process movements made by the client and sends them to the server to update the DrawingPanel
        /// </summary>
        /// <param name="state"></param>
        public void ProcessMovementCommands(SocketState state)
        {
            // check if connection error occurred
            if (state.ErrorOccured)
            {
                ConnectionError(state);
                return;
            }

            ControlCommands commands = new ControlCommands(this.MovementCommands.Last(), this.FireCommands.Last(), this.AimingDirection);

            // make sure beam fire command is only used once, on the frame that it was fired
            FireCommands.Remove("alt");
            Networking.Send(state.TheSocket, commands.ToString());
        }
    }
}