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
    /// Represents the wall objects in the world,
    /// with location data known from the server.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Wall
    {
        //Wall ID data from the server
        [JsonProperty(PropertyName = "wall")]
        public int ID { get; private set; }

        //Wall point 1 for determining location of wall
        [JsonProperty(PropertyName = "p1")]
        public Vector2D p1 { get; private set; }

        //Wall point 2 for determining location of wall
        [JsonProperty(PropertyName = "p2")]
        public Vector2D p2 { get; private set; }

        double top, bottom, left, right;

        public int wallsize = 50;

        private static int nextID = 0;

        /// <summary>
        /// Default constructor for the wall object.
        /// </summary>
        public Wall()
        {
        }

        /// <summary>
        /// Contstructor for creating walls from XML file.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public Wall(Vector2D p1, Vector2D p2)
        {
            this.ID = nextID++;
            this.p1 = p1;
            this.p2 = p2;

            double expansion = wallsize / 2 + Tank.size / 2;
            left = Math.Min(p1.GetX(), p2.GetX()) - expansion;
            right = Math.Max(p1.GetX(), p2.GetX()) + expansion;
            top = Math.Min(p1.GetY(), p2.GetY()) - expansion;
            bottom = Math.Max(p1.GetY(), p2.GetY()) + expansion;
        }

        /// <summary>
        /// Paramertized contstructor for passing in location data to the wall object.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public Wall(int ID, Vector2D p1, Vector2D p2)
        {
            this.ID = ID;
            this.p1 = p1;
            this.p2 = p2;
        }

        /// <summary>
        /// Detects tank collison with walls.
        /// </summary>
        /// <param name="_tankLocation"></param>
        /// <returns></returns>
        public bool TankCollision(Vector2D _tankLocation)
        {
            return left < _tankLocation.GetX()
                && _tankLocation.GetX() < right
                && top < _tankLocation.GetY()
                && _tankLocation.GetY() < bottom;
        }

        /// <summary>
        /// Detects projectile collison with walls.
        /// </summary>
        /// <param name="_projectileLocation"></param>
        /// <returns></returns>
        public bool ProjectileCollision(Vector2D _projectileLocation)
        {
            return left < _projectileLocation.GetX()
                && _projectileLocation.GetX() < right
                && top < _projectileLocation.GetY()
                && _projectileLocation.GetY() < bottom;
        }

        /// <summary>
        /// Checks to see if a spawnpoint will collide with a wall
        /// </summary>
        /// <param name="spawnPoint"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public bool Collides(Vector2D spawnPoint, float buffer)
        {
            float wallSpawnDistance = 50 / 2.0f;
            float minX, maxX, minY, maxY;
            //Check HorizontalWall
            if (HorizontalWall())
            {
                minY = (float)p1.GetY() - wallSpawnDistance - buffer;
                maxY = (float)p1.GetY() + wallSpawnDistance + buffer;

                if (p1.GetX() < p2.GetX())
                {
                    minX = (float)p1.GetX() - wallSpawnDistance - buffer;
                    maxX = (float)p2.GetX() + wallSpawnDistance + buffer;
                }
                else
                {
                    minX = (float)p2.GetX() - wallSpawnDistance - buffer;
                    maxX = (float)p1.GetX() + wallSpawnDistance + buffer;
                }
            }
            //Check vertical all
            else
            {
                minX = (float)p1.GetX() - wallSpawnDistance - buffer;
                maxX = (float)p1.GetX() + wallSpawnDistance + buffer;
                if (p1.GetY() < p2.GetY())
                {
                    minY = (float)p1.GetY() - wallSpawnDistance - buffer;
                    maxY = (float)p2.GetY() + wallSpawnDistance + buffer;
                }
                else
                {
                    minY = (float)p2.GetY() - wallSpawnDistance - buffer;
                    maxY = (float)p1.GetY() + wallSpawnDistance + buffer;
                }
            }
            return spawnPoint.GetX() > minX && spawnPoint.GetX() < maxX &&
                spawnPoint.GetY() > minY && spawnPoint.GetY() < maxY;
        }

        /// <summary>
        /// Returns true when a wall is horizontal facing
        /// </summary>
        /// <returns></returns>
        private bool HorizontalWall()
        {
            return (p1.GetY() == p2.GetY());
        }

        /// <summary>
        /// Gets wall data to convert to JSON data.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this) + "\n";
        }
    }
}
