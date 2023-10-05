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
    /// Class to represent a projectile object in the world.
    /// Allowing for the sending and receiving of data and properties.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Projectile
    {
        //Projectile ID created upon creation
        [JsonProperty(PropertyName = "proj")]
        public int ID { get; private set; }

        //Projectile Location data for server
        [JsonProperty(PropertyName = "loc")]
        public Vector2D location { get; set; }

        //Projectile direction data for server
        [JsonProperty(PropertyName = "dir")]
        public Vector2D direction { get; set; }

        //Projectile collided bool
        [JsonProperty(PropertyName = "died")]
        public bool died { get; set; }

        //Projectile is owner which is the player who shot
        [JsonProperty(PropertyName = "owner")]
        public int owner { get; private set; }

        public int velocity = 25;

        /// <summary>
        /// Default constructor for Projectile
        /// </summary>
        public Projectile()
        {
        }

        /// <summary>
        /// Parmterized contstuctor for spawning a projectile based on the users location and ID
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="location"></param>
        /// <param name="direction"></param>
        /// <param name="owner"></param>
        public Projectile(int ID, Vector2D location, Vector2D direction, Tank owner)
        {
            this.ID = ID;
            this.location = location;
            this.direction = direction;
            this.owner = owner.ID;
            this.died = false;
        }

        /// <summary>
        /// Updates the projectiles location within the world.
        /// </summary>
        public void UpdatePosition()
        {
            location += direction * velocity;
        }

        /// <summary>
        /// Gets projectile data to convert to JSON data.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this) + "\n";
        }
    }
}

