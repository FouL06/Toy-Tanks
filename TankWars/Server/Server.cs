///<summary>
/// Author: Ashton Foulger & Austin In - CS3500 Fall 2021
/// Version: 0.1 - 12/7/21
///</summary>

using NetworkUtil;
using System;
using System.Text.RegularExpressions;

namespace TankWars
{
    /// <summary>
    /// Creates a server to which clients of the tankwars game can connect to.
    /// </summary>
    public class Server
    {
        static void Main(string[] args)
        {
            Settings settings = new Settings("..\\..\\..\\..\\Resources\\Settings\\settings.xml");
            ServerController serverController = new ServerController(settings);
            serverController.Start();
            Console.WriteLine("Server has started and is accepting new clients...");
            Console.Read();
        }
    }
}
