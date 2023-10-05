///<summary>
/// Author: Ashton Foulger & Austin In - CS3500 Fall 2021
/// Version: 0.1 - 11/13/21
///</summary>

using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using TankWars;


namespace GameModel
{
    /// <summary>
    /// Class to represent the tank object in the world.
    /// Allowing for the sending and receiving of data and properties.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Tank
    {
        //Player ID number
        [JsonProperty(PropertyName = "tank")]
        public int ID { get; private set; }

        //Players location data in the world
        [JsonProperty(PropertyName = "loc")]
        public Vector2D location { get; set; }

        //Players orientation data in the world
        [JsonProperty(PropertyName = "bdir")]
        public Vector2D orientation { get; set; }

        //Players turret direction data in the world
        [JsonProperty(PropertyName = "tdir")]
        public Vector2D aiming { get; set; }

        //Players name for multiplayer
        [JsonProperty(PropertyName = "name")]
        public string name { get; set; }

        //Players hit point data
        [JsonProperty(PropertyName = "hp")]
        public int hitPoints { get; set; }

        //Players score on the server
        [JsonProperty(PropertyName = "score")]
        public int score { get; set; }

        //Player is alive status data
        [JsonProperty(PropertyName = "died")]
        public bool died { get; set; }

        //Player is connected or not to the server
        [JsonProperty(PropertyName = "dc")]
        public bool disconnected { get; set; }

        //Player has joined the server
        [JsonProperty(PropertyName = "join")]
        public bool joined { get; set; }

        //Player is respawn time data for the server
        public int RespawnTimer { get; set; }

        //Vector to represent tanks speed for moving
        public Vector2D velocity { get; internal set; }

        //String data to represet color of tank for user
        public string Color { get; set; }

        public double EnginePower = 3;
        public const int size = 60;
        public int beamAmmo = 0;

        private Stopwatch watch = new Stopwatch();
        private Stopwatch respawnTimer = new Stopwatch();

        /// <summary>
        /// Default Tank Contstructor
        /// </summary>
        public Tank()
        {
        }

        /// <summary>
        /// Constructor for taking in a client ID from the servers connections.
        /// </summary>
        /// <param name="ID"></param>
        public Tank(int ID, string name, int hp)
        {
            this.ID = ID;
            this.location = new Vector2D(0, 0);
            this.orientation = new Vector2D(0, -1);
            this.aiming = orientation;
            this.name = name;
            this.hitPoints = hp;
            this.score = 0;
            this.died = false;
            this.disconnected = false;
            this.joined = true;
            this.velocity = new Vector2D(0, 0);
            watch.Start();
        }

        /// <summary>
        /// Checks to see if the player is able to fire again, prevents the player from spawning to many projectiles
        /// </summary>
        /// <returns></returns>
        public bool TryFire()
        {
            if (watch.ElapsedMilliseconds > 17 * 80)
            {
                watch.Restart();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Modifies hitpoints on hit from projectile
        /// </summary>
        public void Hit()
        {
            this.hitPoints--;

            if (hitPoints <= 0)
            {
                this.died = true;
                respawnTimer.Start();
            }
        }

        /// <summary>
        /// Sets the tank to killed if hit by a beam.
        /// </summary>
        public void Kill()
        {
            this.hitPoints = 0;
            this.died = true;
            respawnTimer.Start();
        }

        /// <summary>
        /// Adds to players score.
        /// </summary>
        public void AddToScore()
        {
            this.score++;
        }

        /// <summary>
        /// Checks if player is respawnable.
        /// </summary>
        /// <returns></returns>
        public bool Respawnable(int respawnTime)
        {
            if (respawnTimer.ElapsedMilliseconds >= respawnTime)
            {
                respawnTimer.Reset();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the tank is dead.
        /// </summary>
        /// <returns></returns>
        public bool IsDead()
        {
            return (hitPoints <= 0);
        }

        /// <summary>
        /// Adds powerup to beam ammo for the player to fire a beam.
        /// </summary>
        public void CollectPowerup()
        {
            beamAmmo++;
        }

        /// <summary>
        /// Checks to see if the tank has collided with anything
        /// </summary>
        /// <param name="point"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public bool Collides(Vector2D point, float radius)
        {
            float tankRadius = 50 / 2.0f;
            Vector2D distance = point - location;
            return distance.Length() < (tankRadius + radius);
        }

        /// <summary>
        /// Sets the color of the players tank based on the ID assigned to the player.
        /// Applies the color to the tank from the image resources.
        /// </summary>
        /// <param name="context"></param>
        [OnDeserialized]
        private void AssignColorFromID(StreamingContext context)
        {
            string[] colors = { "Blue", "Dark", "Green", "LightGreen", "Orange", "Purple", "Red", "Yellow" };
            Color = colors[ID % colors.Length];
        }

        /// <summary>
        /// Gets tank data to convert to JSON data.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this) + "\n";
        }
    }
}
