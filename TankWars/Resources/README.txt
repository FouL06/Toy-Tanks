Tank-Wars C# Game
	- University of Utah CS 3500 Fall 2021
	- Authors: Ashton Foulger & Austin In

Introduction:
	- Mulitplayer Tank game based on existing arcade games where players fight in a free for all style arena. The players aim is to get the most kills while playing, with special abilities and game modes being added to the game for variance.
	  Players are assigned a random color upon connecting to the server and able to play with up to 8 players of diffrent tank colors until the cycle starts to place people into same color categories. Coming complete with its own server launcher allowing
	  the user to launch and play locally with friends and or host their own server for friends to connect to.

Technology Stack:
	- C#
	- Windows Form Application (GUI)
	- Networking.dll (Netcode)
	- Server Client
	- .NET framework installed on users machine to allow C# code to run.

Requirements:
	- Windows Computer or Windows OS (VM)
	- Linux distribution with proton or wine installed
	- TankWars server connection (local or online)

Design Desisions:
	- We want to implement a mine game mode. It will have a "one-shot" mechanic, so we will replace the beam with the ability to drop mines.
		- When a tank hits a mine (even it's own mine) it will blow up.
		- Initially when the mine is placed, it will have a 2 second delay before any triggers.
		- Because this extra game mode is optional for our assingment, we won't be implementing this.

	- For the death drawer we decided to use an explosion image as it is the easiest to implement and also will fit with the theme of the game. We went for an image that suited the games art style and will spawn on players tank upon death.

	- We used a texturebrush for drawing the walls so that we can have a repeating texture that will fill the space of the walls points that are passed in from the server, this allows us to store the texture in once.
	  This allow us to save time on computation of fitting each image to a wall segment instead we just have the computer render a repeating texture on each wall.

	- Added a button to show the player what controls are bound as standard for the tankwars client, this allows users who have never played the ability to know what commands on their keyboard to use to control the tank.
		- The players can click on this button before playing the game or during.

	- Added the ability for the player to edit the settings file for the server so that certain server settings can be modified to create unique game modes or game abilities if the user so chooses. This is however, limited in scope and only changes certain aspects of the game.
	  The user can also modify the world data in the settings file to add custom walls and world sizes to the game and create new maps following the example code found in the settings.xml file.

	- We only spawn 3 powerups at a time due to balancing reasons and should not increase that but we can however do that in the world file. On player death the players beam ammo will be reset as to create a challenge with getting powerups to get a 1 shot kill on tanks.

	- Implented a random spawner system for players and powerups as to create a dynamic world for the server to keep track of this also creates a challenge for the players as it will balance out the map is spread of comabat.

Design Challenges:
	- Because this is a multiplayer game, we constantly run the risk of race conditions. The placement of locks around critical sections in our code to prevent race conditions was one of our biggest challenge.
	- With such a large scale project brings complexity in data handling and multiplayer net code, requiring us to be vigilant in making sure all data is correct and exact in its execution.
	- MVC (Model, View, Controller) architecture
		- The ability to break the assignment into these three categories will help with the separation of concerns in such a large project. How you might go about deciding what goes where was also a challege we faced.

	- Implenting a settings config file and reader was a design challenge in itself due to the making modular code and a reader for the server to access but also update the Game model is a design challenge. We had to structure our code in
	  a way that allowed for modularity and update the server in realtime with accurate and impactful data in order to keep track of multipule clients.

Features:
	- Changing health bar color for player to know what their tanks health status is.
	- Ability to have a custom username upon entering the server
	- Keep count of player's score next to their name
		- 1 point per kill

General Use:
	- To be used with either a previously setup tank wars server or a locally hosted server on the players machine or Virtual Machine.