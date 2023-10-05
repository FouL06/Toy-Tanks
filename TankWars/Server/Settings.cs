///<summary>
/// Author: Ashton Foulger & Austin In - CS3500 Fall 2021
/// Version: 0.1 - 12/7/21
///</summary>

using GameModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace TankWars
{
    /// <summary>
    /// Reads and XML file with world data and world game settings data that can be manipulated on the clients.
    /// </summary>
    public class Settings
    {
        //World Settings Variables
        public int UniverseSize { get; private set; }
        public int MSPerFrame { get; private set; }
        public int FramesPerShot { get; private set; }

        public int RespawnRate { get; private set; }
        public int HitPoints { get; } = 3;

        public HashSet<Wall> walls { get; } = new HashSet<Wall>();

        /// <summary>
        /// Default Constructor for Settings
        /// </summary>
        /// <param name="filepath"></param>
        public Settings(string filepath)
        {
            XmlReaderSettings _settings = new XmlReaderSettings();
            _settings.IgnoreComments = true;
            _settings.IgnoreWhitespace = true;

            //Read Settings XML File
            using (XmlReader reader = XmlReader.Create(filepath, _settings))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        switch (reader.Name)
                        {
                            case "UniverseSize":
                                reader.ReadStartElement();
                                UniverseSize = reader.ReadContentAsInt();
                                break;
                            case "MSPerFrame":
                                reader.ReadStartElement();
                                MSPerFrame = reader.ReadContentAsInt();
                                break;
                            case "FramesPerShot":
                                reader.ReadStartElement();
                                FramesPerShot = reader.ReadContentAsInt();
                                break;
                            case "Wall":
                                WallUpdater(reader);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reads wall data from settings file.
        /// </summary>
        /// <param name="reader"></param>
        private void WallUpdater(XmlReader reader)
        {
            //Read Wall Tag
            reader.ReadStartElement();
            reader.MoveToContent();

            //Create wall points
            Vector2D p1 = new Vector2D();
            Vector2D p2 = new Vector2D();

            //Get Wall data
            for (int i = 0; i < 2; i++)
            {
                string elementName = reader.Name;
                double x, y;

                //Check for p1 or p2
                reader.ReadStartElement();
                reader.MoveToContent();

                //Get X value
                reader.ReadStartElement();
                x = reader.ReadContentAsDouble();
                reader.ReadEndElement();

                //Get Y value
                reader.ReadStartElement();
                y = reader.ReadContentAsDouble();
                reader.ReadEndElement();

                //Close wall data read
                reader.ReadEndElement();

                //Update vectors
                switch (elementName)
                {
                    case "p1":
                        p1 = new Vector2D(x, y);
                        break;
                    case "p2":
                        p2 = new Vector2D(x, y);
                        break;
                    default:
                        throw new Exception("Error reading XML file.");
                }
            }

            //Add walls to the list to populate the world.
            walls.Add(new Wall(p1, p2));
        }
    }
}
