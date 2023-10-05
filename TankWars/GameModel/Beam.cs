///<summary>
/// Author: Ashton Foulger & Austin In - CS3500 Fall 2021
/// Version: 0.1 - 12/7/21
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
    /// Represents the Beam object in the world,
    /// with data and location being sent to the server
    /// </summary>
    public class Beam
    {
        //Beam ID created upon creation
        [JsonProperty(PropertyName = "beam")]
        public int ID { get; private set; }
        
        //Beam is origin point for being created by player
        [JsonProperty(PropertyName = "org")]
        public Vector2D origin { get; private set; }

        //Beam is direction data for the server
        [JsonProperty(PropertyName = "dir")]
        public Vector2D direction { get; private set; }

        //Beam is owner determied by player ID
        [JsonProperty(PropertyName = "owner")]
        public int owner { get; private set; }

        //Beam despawn variables
        private const int lifetimeMS = 400;
        private Stopwatch timeAlive = new Stopwatch();

        /// <summary>
        /// Default constructor for the Beam object
        /// </summary>
        public Beam()
        {
        }

        /// <summary>
        /// Paramaterized constructor for Beam object for passing in data, location, and owner.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="origin"></param>
        /// <param name="direction"></param>
        /// <param name="owner"></param>
        public Beam(int ID, Vector2D origin, Vector2D direction, Tank owner)
        {
            this.ID = ID;
            this.origin = origin;
            this.direction = direction;
            this.owner = owner.ID;
            timeAlive.Start();
        }

        /// <summary>
        /// Tracks the amount of time a beam has been alive and if it has exceeded the time it will despawn.
        /// </summary>
        /// <returns></returns>
        public bool Despawn()
        {
            if(timeAlive.ElapsedMilliseconds > lifetimeMS)
            {
                timeAlive.Restart();
                return true;
            }
            return false;
        }


        /// <summary>
        /// Determines if a ray interescts a circle. 
        /// From https://utah.instructure.com/courses/717551/pages/beam-intersections?module_item_id=15352879
        /// as part of kopta's beam intersection example.
        /// </summary>
        /// <param name="rayOrig">The origin of the ray</param>
        /// <param name="rayDir">The direction of the ray</param>
        /// <param name="center">The center of the circle</param>
        /// <param name="r">The radius of the circle</param>
        /// <returns></returns>
        public bool Intersects( Vector2D center, double r)
        {
            // ray-circle intersection test
            // P: hit point
            // ray: P = O + tV
            // circle: (P-C)dot(P-C)-r^2 = 0
            // substituting to solve for t gives a quadratic equation:
            // a = VdotV
            // b = 2(O-C)dotV
            // c = (O-C)dot(O-C)-r^2
            // if the discriminant is negative, miss (no solution for P)
            // otherwise, if both roots are positive, hit

            double a = direction.Dot(direction);
            double b = ((origin - center) * 2.0).Dot(direction);
            double c = (origin - center).Dot(origin - center) - r * r;

            // discriminant
            double disc = b * b - 4.0 * a * c;

            if (disc < 0.0)
                return false;

            // find the signs of the roots
            // technically we should also divide by 2a
            // but all we care about is the sign, not the magnitude
            double root1 = -b + Math.Sqrt(disc);
            double root2 = -b - Math.Sqrt(disc);

            return (root1 > 0.0 && root2 > 0.0);
        }

        /// <summary>
        /// Gets beam data to convert to JSON data.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this) + "\n";
        }
    }
}
