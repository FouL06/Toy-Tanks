///<summary>
/// Author: Ashton Foulger & Austin In - CS3500 Fall 2021
/// Version: 0.1 - 11/15/21
///</summary>

using System;
using System.Collections.Generic;
using System.Text;
using TankWars;

namespace GameModel
{
    /// <summary>
    /// Store and parse data into the world with data from a server.
    /// Allows for the creation of the world for the view following the MVC model.
    /// </summary>
    public class World
    {
        //Public dictionaries for storing all object data within the world
        public Dictionary<int, Tank> Tanks;
        public Dictionary<int, Projectile> Projectiles;
        public Dictionary<int, Powerup> Powerups;
        public Dictionary<int, Beam> Beams;
        public Dictionary<int, Wall> Walls;
        public Dictionary<int, ControlCommands> Commands;
        public int TankID;

        public Vector2D tdir;

        //World size setter
        public int Size { get; private set; }
        public int TimePerFrame { get; private set; }
        public int ProjectileFireDelay { get; private set; }
        public int RespawnDelay { get; private set; }
        private Random rng = new Random();

        /// <summary>
        /// World constructure that takes in a world ID and size from the server
        /// </summary>
        /// <param name="ID">World ID from player</param>
        /// <param name="size">World Size</param>
        public World(int ID, int size)
        {
            Tanks = new Dictionary<int, Tank>();
            Projectiles = new Dictionary<int, Projectile>();
            Powerups = new Dictionary<int, Powerup>();
            Beams = new Dictionary<int, Beam>();
            Walls = new Dictionary<int, Wall>();
            Commands = new Dictionary<int, ControlCommands>();

            TankID = ID;
            Size = size;
        }

        /// <summary>
        /// World Constructure that takes in a World size from the server
        /// </summary>
        /// <param name="WorldSize"></param>
        public World(int WorldSize, int TimePerFrame, int ProjectileFireDelay, int RespawnDelay)
        {
            Size = WorldSize;
            this.TimePerFrame = TimePerFrame;
            this.ProjectileFireDelay = ProjectileFireDelay;
            this.RespawnDelay = RespawnDelay;

            Tanks = new Dictionary<int, Tank>();
            Projectiles = new Dictionary<int, Projectile>();
            Powerups = new Dictionary<int, Powerup>();
            Beams = new Dictionary<int, Beam>();
            Walls = new Dictionary<int, Wall>();
            Commands = new Dictionary<int, ControlCommands>();

            //Create Powerups on intial world creation
            for (int i = 0; i < 3; i++)
            {
                Powerups.Add(Powerups.Count, new Powerup(Powerups.Count, GetValidSpawnPoint(20 / 2.0f)));
            }
        }

        /// <summary>
        /// Updates the server with client data and current world data for the server to process.
        /// </summary>
        public void Update()
        {
            foreach (KeyValuePair<int, ControlCommands> controls in Commands)
            {
                // update tank's body and turret movements
                Tank t = Tanks[controls.Key];
                switch (controls.Value.moving)
                {
                    case "up":
                        t.velocity = new Vector2D(0, -1);
                        t.orientation = new Vector2D(0, -1);
                        t.aiming = controls.Value.turretDirection;
                        break;
                    case "left":
                        t.velocity = new Vector2D(-1, 0);
                        t.orientation = new Vector2D(-1, 0);
                        t.aiming = controls.Value.turretDirection;
                        break;
                    case "down":
                        t.velocity = new Vector2D(0, 1);
                        t.orientation = new Vector2D(0, 1);
                        t.aiming = controls.Value.turretDirection;
                        break;
                    case "right":
                        t.velocity = new Vector2D(1, 0);
                        t.orientation = new Vector2D(1, 0);
                        t.aiming = controls.Value.turretDirection;
                        break;
                    default:
                        t.velocity = new Vector2D(0, 0);
                        t.aiming = controls.Value.turretDirection;
                        break;
                }

                // update turret movement and fire commands
                switch (controls.Value.fire)
                {
                    case "main":
                        if (t.TryFire())
                        {
                            Projectiles.Add(Projectiles.Count, new Projectile(Projectiles.Count, t.location, t.aiming, t));
                        }
                        break;
                    case "alt":
                        if (t.beamAmmo > 0)
                        {
                            Beams.Add(Beams.Count, new Beam(Beams.Count, t.location, t.aiming, t));
                            t.beamAmmo--;
                        }
                        break;
                    default:
                        break;
                }
                t.velocity *= t.EnginePower;
            }

            //Clear commands before we send update information
            Commands.Clear();

            //Checks for any tanks colliding with walls
            foreach (Tank t in Tanks.Values)
            {
                //Check if tank has disconnected
                if (t.disconnected)
                {
                    continue;
                }

                //Check if tank is dead 
                if (t.IsDead())
                {
                    if (t.Respawnable(TimePerFrame * RespawnDelay))
                    {
                        t.location = GetValidSpawnPoint(60 / 2.0f);
                        t.hitPoints = 3;
                        t.beamAmmo = 0;
                    }
                }

                if (t.velocity.Length() == 0)
                {
                    continue;
                }

                //Detect wall collision
                Vector2D newloc = t.location + t.velocity;
                bool collision = false;
                foreach (Wall wall in Walls.Values)
                {
                    if (wall.TankCollision(newloc))
                    {
                        collision = true;
                        t.velocity = new Vector2D(0, 0);
                        break;
                    }
                }

                if (!collision)
                {
                    t.location = newloc;
                }
            }

            //Check Projectiles Collision
            foreach (Projectile p in Projectiles.Values)
            {
                p.UpdatePosition();

                //skip dead projectiles
                if (p.died)
                {
                    continue;
                }

                //Check if out of bounds
                if (Math.Abs(p.location.GetX()) > Size / 2.0 || Math.Abs(p.location.GetY()) > Size / 2.0)
                {
                    p.died = true;
                    return;
                }

                //Check if projectile collided with any walls
                foreach (Wall wall in Walls.Values)
                {
                    if (wall.ProjectileCollision(p.location))
                    {
                        p.died = true;
                    }
                }

                //Check if the projectile has collided with a tank and or killed a tank
                foreach (Tank tank in Tanks.Values)
                {
                    if (tank.Collides(p.location, 0))
                    {
                        if (tank.ID != p.owner && !(tank.died))
                        {
                            tank.Hit();

                            if (tank.IsDead())
                            {
                                Tanks[p.owner].AddToScore();
                            }
                            p.died = true;
                        }
                    }
                }
            }

            //Check beams Collison
            foreach (Beam b in Beams.Values)
            {
                foreach (Tank tank in Tanks.Values)
                {
                    //Check if beam hits a tank if so kill it
                    if (b.Intersects(tank.location, 60 / 2.0f))
                    {
                        if (tank.ID != b.owner && !(tank.died))
                        {
                            tank.Kill();

                            if (tank.IsDead())
                            {
                                Tanks[b.owner].AddToScore();
                            }
                        }
                    }
                }
            }

            //Check powerup collison
            foreach (Powerup pow in Powerups.Values)
            {
                //Check if powerup is dead or was picked up.
                if (pow.died)
                {
                    if (pow.Respawnable(TimePerFrame * RespawnDelay))
                    {
                        pow.location = GetValidSpawnPoint(20 / 2.0f);
                    }
                }

                //Check if the powerup has collided with any tanks
                foreach (Tank t in Tanks.Values)
                {
                    if (t.Collides(pow.location, 0))
                    {
                        t.CollectPowerup();
                        pow.died = true;
                        pow.PowerupStartTimer();
                    }
                }
            }
        }

        /// <summary>
        /// Gets a valid spawnpoint in the world.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private Vector2D GetValidSpawnPoint(float buffer)
        {
            Vector2D spawn = new Vector2D();
            bool valid = false;
            while (!valid)
            {
                //create a new spawnpoint location
                double x = rng.NextDouble() * Size;
                double y = rng.NextDouble() * Size;
                x -= Size / 2.0;
                y -= Size / 2.0;
                spawn = new Vector2D(x, y);

                valid = true;
                foreach (Wall w in Walls.Values)
                {
                    if (w.Collides(spawn, buffer))
                    {
                        valid = false;
                        break;
                    }
                }
            }
            return spawn;
        }

        /// <summary>
        /// Checks if player tank model exists in the world space.
        /// </summary>
        /// <returns></returns>
        public Tank PlayerExists()
        {
            if (this.Tanks.ContainsKey(this.TankID))
            {
                return this.Tanks[this.TankID];
            }

            return null;
        }
    }
}