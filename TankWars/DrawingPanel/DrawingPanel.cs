///<summary>
/// Author: Ashton Foulger & Austin In - CS3500 Fall 2021
/// Version: 0.1 - 11/23/21
///</summary>

using GameModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TankWars;

namespace DrawingPanel
{
    /// <summary>
    /// Draws all graphical objects with data from the world,
    /// to be used to display the tank wars objects and other clients.
    /// </summary>
    public class DrawingPanel : Panel
    {
        //World and Sprite containers
        public World _world { get; set; }
        private List<Beam> Dead_Beams;
        Dictionary<string, Image> sprites;

        /// <summary>
        /// Default Constructor for Drawing Panel
        /// </summary>
        public DrawingPanel()
        {
            DoubleBuffered = true;
            sprites = new Dictionary<string, Image>();
            Dead_Beams = new List<Beam>();
        }

        /// <summary>
        /// Sets the world that will be used to get meta data for the drawing panel
        /// </summary>
        /// <param name="world"></param>
        public void SetWorld(World world)
        {
            _world = world;
        }

        //Delegate for drawing objects with transforms
        public delegate void ObjectDrawer(object o, PaintEventArgs e);

        /// <summary>
        /// Stores and get sprites from filenames passed into the method.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private Image StoreSprites(string filename)
        {
            //Check if the file is not currently in the sprites dictonary
            if (!sprites.ContainsKey(filename))
            {
                sprites[filename] = Image.FromFile($"..\\..\\..\\Resources\\Images\\{filename}");
            }
            return sprites[filename];
        }

        /// <summary>
        /// Preforms a tranform and rotation of an object to be drawn in the world
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        /// <param name="World_X"></param>
        /// <param name="World_Y"></param>
        /// <param name="angle"></param>
        /// <param name="drawer"></param>
        private void DrawObjectWithTransform(object o, PaintEventArgs e, double World_X, double World_Y, double angle, ObjectDrawer drawer)
        {
            //Get currrent transform
            System.Drawing.Drawing2D.Matrix _matrix = e.Graphics.Transform.Clone();

            //Draw transform
            e.Graphics.TranslateTransform((int)World_X, (int)World_Y);
            e.Graphics.RotateTransform((float)angle);
            drawer(o, e);

            e.Graphics.Transform = _matrix;
        }

        /// <summary>
        /// Drawing delegate for the drawing the tanks in the world,
        /// which is invoked by the DrawObjectsWithTransform Method.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void TankDrawer(object o, PaintEventArgs e)
        {
            //Set object as Tank
            Tank _tank = o as Tank;

            //Get Tank Image
            Image tankBody = StoreSprites($"{_tank.Color}Tank.png");

            //Draw Tank
            e.Graphics.SmoothingMode = SmoothingMode.None;
            e.Graphics.DrawImage(tankBody, -(60 / 2), -(60 / 2), 60, 60);
        }

        /// <summary>
        /// Drawing delegate for drawing the turrets in the world,
        /// which is invoked by the DrawObjectsWithTransform Method.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void TurretDrawer(object o, PaintEventArgs e)
        {
            // Set object as Turret
            Tank _tank = o as Tank;

            // Get Turret Image
            Image turret = StoreSprites($"{_tank.Color}Turret.png");

            // Draw Turret
            e.Graphics.SmoothingMode = SmoothingMode.None;
            e.Graphics.DrawImage(turret, -(50 / 2), -(50 / 2), 50, 50);
        }

        /// <summary>
        /// Drawing delegate for drawing projectiles in the world,
        /// which is invoked by the DrawObjectWithTransform method.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void ProjectileDrawer(object o, PaintEventArgs e)
        {
            //Set object as projectile
            Projectile p = o as Projectile;

            string color;
            //Check if the projectile has an owner if so assign a color to it.
            if (_world.Tanks.ContainsKey(p.owner))
            {
                color = _world.Tanks[p.owner].Color.ToLower();

                //Handle special tank colors and assign projectile color
                if (color == "lightgreen")
                {
                    color = "green";
                }
                else if (color == "orange")
                {
                    color = "brown";
                }
                else if (color == "purple")
                {
                    color = "violet";
                }
                else if (color == "dark")
                {
                    color = "grey";
                }
            }
            else
            {
                //Default color
                color = "white";
            }

            //Draw projectile
            Image projectile = StoreSprites($"shot-{color}.png");
            e.Graphics.SmoothingMode = SmoothingMode.None;
            e.Graphics.DrawImage(projectile, -(30 / 2), -(30 / 2), 30, 30);
        }

        /// <summary>
        /// Drawing delegate for drawing beams in the world,
        /// which is invoked by the DrawObjectWithTransform method.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void BeamDrawer(object o, PaintEventArgs e)
        {
            // set object as beam
            Beam beam = o as Beam;

            // set beam width
            int width = 3;

            // Draw beam line and set color
            using (Pen pen = new Pen(Color.Aqua, width))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawLine(pen, 0, 0, 0, (float)(-Math.Sqrt(2) * _world.Size * 2));
            }
        }

        /// <summary>
        /// Drawing delegate for drawing powerups in the world,
        /// which is invoked by the DrawObjectWithTransform method.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void PowerupDrawer(object o, PaintEventArgs e)
        {
            // set object as powerup
            Powerup powerup = o as Powerup;

            // set powerup width and height
            int width = 8;
            int height = 8;

            // draw powerup and set color
            using (SolidBrush redBrush = new SolidBrush(Color.Red))
            {
                Rectangle roundedSquare = new Rectangle(-(width / 2), -(height / 2), width, height);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(redBrush, roundedSquare);
            }
        }

        /// <summary>
        /// Drawing delegate for drawing wall in the world,
        /// which is invoked by the DrawObjectWithTransform method.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void WallDrawer(object o, PaintEventArgs e)
        {
            TextureBrush wallTexture = new TextureBrush(StoreSprites("WallSprite.png"), WrapMode.Tile);
            Wall wall = o as Wall;

            //Wall variables
            int size = 50;
            int width = (int)Math.Abs(wall.p1.GetX() - wall.p2.GetX()) + size;
            int height = (int)Math.Abs(wall.p1.GetY() - wall.p2.GetY()) + size;
            int x = (int)Math.Min(wall.p1.GetX(), wall.p2.GetX()) - size / 2;
            int y = (int)Math.Min(wall.p1.GetY(), wall.p2.GetY()) - size / 2;

            //Draw Walls
            e.Graphics.SmoothingMode = SmoothingMode.None;
            e.Graphics.FillRectangle(wallTexture, x, y, width, height); ;
        }

        /// <summary>
        /// Drawing delegate for drawing players names in the world,
        /// which is invoked by the DrawObjectWithTransform method.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void NameDrawer(object o, PaintEventArgs e)
        {
            // set object as string
            string name = o as string;

            // draw player's name and set color
            using (SolidBrush whiteBrush = new SolidBrush(Color.White))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawString(name, new Font("Calibri", 12.0f), whiteBrush, 0, 0);
            }
        }

        /// <summary>
        /// Drawing delegate for drawing health bars in the world,
        /// which is invoked by the DrawObjectWithTransform method.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void HealthDrawer(object o, PaintEventArgs e)
        {
            //Set object to type tank
            Tank tank = o as Tank;

            //Health bar variables
            Color healthColor;
            float healthBarMaxWidth = 50;
            float healthBarMaXHeight = 5;
            float hitPointPercentage = tank.hitPoints / (float)3;
            float healthBarWidth = healthBarMaxWidth * hitPointPercentage;

            //Set bar color based on tank health
            switch (tank.hitPoints)
            {
                case 3:
                    healthColor = Color.Green;
                    break;
                case 2:
                    healthColor = Color.Yellow;
                    break;
                case 1:
                    healthColor = Color.Red;
                    break;
                default:
                    healthColor = Color.Transparent;
                    break;
            }

            //Draw health bar
            using (SolidBrush bar = new SolidBrush(healthColor))
            {
                e.Graphics.SmoothingMode = SmoothingMode.None;
                e.Graphics.FillRectangle(bar, 0, 0, healthBarWidth, healthBarMaXHeight);
            }
        }

        private void OnDeathDrawer(object o, PaintEventArgs e)
        {
            //Load explosion image
            Image explosion = StoreSprites("explosion.png");

            //Draw Image
            e.Graphics.SmoothingMode = SmoothingMode.None;
            e.Graphics.DrawImage(explosion, -(70 / 2), -(70 / 2), 70, 70);

        }

        /// <summary>
        /// Invoked by DrawingPanel to paint all graphics needed by the client to display TankWars.
        /// Using painting delegates to paint all players', projectiles', walls' and beams in the world space.
        /// Along with displaying helpful information to the player such as health and names.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            // if world is null, don't paint anything
            if (_world is null)
            {
                return;
            }

            lock (_world)
            {
                // check if player exists in world space
                if (!(_world.PlayerExists() is null))
                {
                    // get player tank object
                    Tank tankPlayer = _world.PlayerExists();
                    double x = tankPlayer.location.GetX();
                    double y = tankPlayer.location.GetY();
                    e.Graphics.TranslateTransform((float)(-x) + (Size.Width / 2), (float)(-y) + (Size.Width / 2));
                }

                // Center and draw the background
                Image backgroundImage = StoreSprites("Background.png");
                e.Graphics.DrawImage(backgroundImage, (-_world.Size / 2), (-_world.Size / 2), _world.Size, _world.Size);

                // Draw and place walls
                foreach (Wall _wall in _world.Walls.Values)
                {
                    DrawObjectWithTransform(_wall, e, 0, 0, 0, WallDrawer);
                }

                // Draw the tanks
                foreach (Tank tank in _world.Tanks.Values)
                {
                    // check if player is not dead or has disconnected
                    if (tank.hitPoints > 0 && !(tank.disconnected))
                    {
                        // draw tank body
                        DrawObjectWithTransform(tank, e, tank.location.GetX(), tank.location.GetY(), tank.orientation.ToAngle(), TankDrawer);
                        // draw tank turret
                        DrawObjectWithTransform(tank, e, tank.location.GetX(), tank.location.GetY(), tank.aiming.ToAngle(), TurretDrawer);
                        // draw health bar
                        DrawObjectWithTransform(tank, e, tank.location.GetX() - 25, tank.location.GetY() - 40, 0, HealthDrawer);
                        // draw player's name
                        DrawObjectWithTransform(tank.name + ": " + tank.score, e, tank.location.GetX() - 33, tank.location.GetY() + 35, 0, NameDrawer);
                    }

                    //Draw death animation
                    if (tank.hitPoints == 0)
                    {
                        DrawObjectWithTransform(tank, e, tank.location.GetX(), tank.location.GetY(), 0, OnDeathDrawer);
                    }
                }

                // Draw the projectiles
                foreach (Projectile projectile in _world.Projectiles.Values)
                {
                    // check if projectiles are alive
                    if (!(projectile.died))
                    {
                        // draw projectile
                        DrawObjectWithTransform(projectile, e, projectile.location.GetX(), projectile.location.GetY(), projectile.direction.ToAngle(), ProjectileDrawer);
                    }
                }

                // Draw the powerups
                foreach (Powerup powerup in _world.Powerups.Values)
                {
                    // if the powerup is alive
                    if (!(powerup.died))
                    {
                        // draw the powerup
                        DrawObjectWithTransform(powerup, e, powerup.location.GetX(), powerup.location.GetY(), 0, PowerupDrawer);
                    }
                }

                // Draw the Beams and despawns
                foreach (Beam beam in _world.Beams.Values)
                {
                    // if the beam has despawned
                    if (beam.Despawn())
                    {
                        Dead_Beams.Add(beam);
                    }
                    // if the beam hasn't despawned
                    else
                    {
                        // draw the beam
                        DrawObjectWithTransform(beam, e, beam.origin.GetX(), beam.origin.GetY(), beam.direction.ToAngle(), BeamDrawer);
                    }
                }
            }
            base.OnPaint(e);
        }
    }
}
