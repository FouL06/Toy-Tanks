///<summary>
/// Author: Ashton Foulger & Austin In - CS3500 Fall 2021
/// Version: 0.1 - 11/13/21
///</summary>

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TankWars;

namespace GameModel
{
    /// <summary>
    /// ControlCommands object for sending moving, fire, and turret direction data to the server
    /// </summary>
    public class ControlCommands
    {
        //Bool for determining if the player is moving
        [JsonProperty(PropertyName = "moving")]
        public string moving { get; private set; }

        //Bool for determining if the player has fired
        [JsonProperty(PropertyName = "fire")]
        public string fire { get; private set; }

        //Players turret direction / aiming in the world
        [JsonProperty(PropertyName = "tdir")]
        public Vector2D turretDirection { get; private set; }

        /// <summary>
        /// Default constructor for ControlCommands
        /// </summary>
        public ControlCommands()
        {
        }

        /// <summary>
        /// Parameterized constructor for ControlCommands for passing in direction, fire and turret direction
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="fired"></param>
        /// <param name="turretDirection"></param>
        public ControlCommands(string direction, string fired, Vector2D turretDirection)
        {
            this.moving = direction;
            this.fire = fired;
            this.turretDirection = turretDirection;
        }

        /// <summary>
        /// Overriding the ToString() method allowing to serialize the commands,
        /// to be sent as a JSON element for the server to interpret.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this) + "\n";
        }
    }
}
