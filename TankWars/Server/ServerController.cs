///<summary>
/// Author: Ashton Foulger & Austin In - CS3500 Fall 2021
/// Version: 0.1 - 12/7/21
///</summary>

using GameModel;
using NetworkUtil;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace TankWars
{
    /// <summary>
    /// Controls the Server Handshake and data processing process for the server.
    /// </summary>
    class ServerController
    {
        //Server Variables
        private Settings settings;
        private World _world;
        private string _connectionInformation;

        //Server Clients Datastructures
        private Dictionary<int, SocketState> clients;

        /// <summary>
        /// Default Constructor for Server Controller
        /// </summary>
        /// <param name="settings"></param>
        public ServerController(Settings settings)
        {
            this.settings = settings;
            this._world = new World(settings.UniverseSize, settings.MSPerFrame, settings.FramesPerShot, settings.RespawnRate);
            this.clients = new Dictionary<int, SocketState>();

            //Get wall data and store in world
            foreach (Wall wall in settings.walls)
            {
                _world.Walls[wall.ID] = wall;
            }

            //String builder for sending JSON data
            StringBuilder sb = new StringBuilder();
            sb.Append(_world.Size);
            sb.Append("\n");

            //Send wall data to the client
            foreach (Wall _wall in _world.Walls.Values)
            {
                sb.Append(_wall.ToString());
            }

            _connectionInformation = sb.ToString();
        }

        /// <summary>
        /// Starts a server to begin accepting new clients.
        /// </summary>
        internal void Start()
        {
            Networking.StartServer(ConnectClient, 11000);
            Thread t = new Thread(Update);
            t.Start();
        }

        /// <summary>
        /// Update the world and send updated data to the clients connected to the server.
        /// </summary>
        private void Update()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            while (true)
            {
                while (watch.ElapsedMilliseconds < settings.MSPerFrame) ;

                watch.Restart();
                StringBuilder sb = new StringBuilder();

                //Update world with current tanks, projectiles, beams, and powerups.
                lock (_world)
                {
                    _world.Update();

                    //Update Tanks
                    foreach (Tank tank in _world.Tanks.Values)
                    {
                        if ((tank.died && tank.disconnected) || !tank.disconnected)
                        {
                            sb.Append(tank.ToString());
                            tank.died = false;
                        }
                    }

                    //Update Projectiles
                    foreach (Projectile projectile in _world.Projectiles.Values)
                    {
                        sb.Append(projectile.ToString());
                    }

                    //Update Beams
                    foreach (Beam beam in _world.Beams.Values)
                    {
                        sb.Append(beam.ToString());
                    }

                    //Clear all beams after we have appended them to be sent to all the clients.
                    _world.Beams.Clear();

                    //Update Powerups
                    foreach (Powerup powerup in _world.Powerups.Values)
                    {
                        sb.Append(powerup.ToString());
                    }
                }

                //Current data in that frame
                string frame = sb.ToString();

                //Send data to all clients
                lock (clients)
                {
                    foreach (SocketState state in clients.Values)
                    {
                        Networking.Send(state.TheSocket, frame);
                    }
                }
            }
        }

        /// <summary>
        /// Connects new clients to open server.
        /// </summary>
        /// <param name="state"></param>
        private void ConnectClient(SocketState state)
        {
            //Check if connection was severed
            if (state.ErrorOccured)
            {
                Console.WriteLine("Error occured while trying to connect client: " + state.ID);
                state.TheSocket.Close();
                return;
            }

            //Get player name from client
            state.OnNetworkAction = GetPlayerName;
            Networking.GetData(state);
        }

        /// <summary>
        /// Get players name from client and connect to server.
        /// </summary>
        /// <param name="state"></param>
        private void GetPlayerName(SocketState state)
        {
            //Check if connection was severed
            if (state.ErrorOccured)
            {
                Console.WriteLine("Error occured while trying to get players name from client: " + state.ID);
                state.TheSocket.Close();
                return;
            }

            //Get player name Data
            string name = state.GetData();
            if (!name.EndsWith("\n"))
            {
                state.GetData();
                return;
            }

            //Remove player name data
            state.RemoveData(0, name.Length);
            name = name.Trim();

            //Send client server data
            Networking.Send(state.TheSocket, state.ID + "\n");
            Networking.Send(state.TheSocket, _connectionInformation);

            lock (_world)
            {
                _world.Tanks[(int)state.ID] = new Tank((int)state.ID, name, settings.HitPoints);
            }

            //Add connection to dictionary of clients
            lock (clients)
            {
                clients.Add((int)state.ID, state);
                Console.WriteLine("client " + state.ID + " has connected to the server...");
            }

            state.OnNetworkAction = GetControlCommands;
            Networking.GetData(state);
        }

        /// <summary>
        /// Gets all control commands sent by the clients and appends them to a string to update the world with current movement and fire data.
        /// </summary>
        /// <param name="state"></param>
        private void GetControlCommands(SocketState state)
        {
            if (state.ErrorOccured)
            {
                Console.WriteLine("Error occured while trying to get commands name from client: " + state.ID);
                _world.Tanks[(int)state.ID].disconnected = true;
                _world.Tanks[(int)state.ID].died = true;
                _world.Tanks[(int)state.ID].hitPoints = 0;
                state.TheSocket.Close();
                return;
            }

            string data = state.GetData();
            string[] parts = Regex.Split(data, @"(?<=[\n])");

            foreach (string s in parts)
            {
                if (s.Length == 0)
                {
                    continue;
                }
                if (s[s.Length - 1] != '\n')
                {
                    break;
                }

                ControlCommands control = JsonConvert.DeserializeObject<ControlCommands>(s);
                lock (_world)
                {
                    _world.Commands[(int)state.ID] = control;
                }
                state.RemoveData(0, s.Length);
            }
            Networking.GetData(state);
        }
    }
}
