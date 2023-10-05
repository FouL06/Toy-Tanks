///<summary>
/// Author: Ashton Foulger & Austin In - CS3500 Fall 2021
/// Version: 0.1 - 11/13/21
///</summary>

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using TankWars;

namespace GameModel
{
    /// <summary>
    /// Class to represent a Powerup object in the world.
    /// Allowing the server to interpret its location and setting data.
    /// </summary>
    public class Powerup
    {
        //Powerup ID assigned upon creation
        [JsonProperty(PropertyName ="power")]
        public int ID { get; private set; }

        //Powerup location in the world
        [JsonProperty(PropertyName = "loc")]
        public Vector2D location { get; set; }

        //Powerup has either been picked up or despawned.
        [JsonProperty(PropertyName = "died")]
        public bool died { get; set; }

        Stopwatch respawnTimer = new Stopwatch();

        /// <summary>
        /// Default Constructor for Powerup.
        /// </summary>
        public Powerup()
        {
        }

        /// <summary>
        /// Paramerterized constructor to spawn a powerup at a random point in the world.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="location"></param>
        public Powerup(int ID, Vector2D location)
        {
            this.ID = ID;
            this.location = location;
            this.died = false;
        }

        /// <summary>
        /// Starts powerup respawn timer.
        /// </summary>
        public void PowerupStartTimer()
        {
            respawnTimer.Start();
        }

        /// <summary>
        /// Checks to see if the projectile can respawn.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool Respawnable(int time)
        {
            if(respawnTimer.ElapsedMilliseconds >= time)
            {
                this.died = false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets powerup data to convert to JSON data.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this) + "\n";
        }
    }
}
