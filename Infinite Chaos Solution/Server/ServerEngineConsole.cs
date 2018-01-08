
#define DRAW_SERVER

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Server.Networking;
using Shared.Networking;
using Shared.PlayerControl;
using Shared.Tools;
using Shared.World;

namespace Server
{
    public class ServerEngineConsole
    {

        // Properties //
        // Variables //
        // References //

        // Members // 

        public ServerNetworkManager ServerNetworkManager;
        public PhysicsWorld PhysicsWorld;

        public ServerEngineConsole()
        {
            // Set console title
            Console.Title = "Server";


            // Initialize network
            ServerNetworkManager = new ServerNetworkManager(this);


            // Subscribe to network events //
            ServerNetworkManager.onPlayerConnected += ServerNetworkManager_OnPlayerConnected;
            ServerNetworkManager.onPlayerDisconnected += ServerNetworkManager_OnPlayerDisconnected;
            ServerNetworkManager.onPlayerMove += ServerNetworkManager_OnPlayerMove;
            ServerNetworkManager.onPlayerAttack += ServerNetworkManager_OnPlayerAttack;

            ServerNetworkManager.Load();
        }


        public void Run()
        {
#if DRAW_SERVER
            var visualEngine = new ServerEngine(this);
            PhysicsWorld = new PhysicsWorld(visualEngine);
            PhysicsWorld.GenerateMapCollidersFromFile(AvailableMapConfigs.BasicMap);
            visualEngine.Run();


#else
            PhysicsWorld = new PhysicsWorld();
            PhysicsWorld.GenerateMapCollidersFromFile(AvailableMapConfigs.BasicMap);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var timeLastFrame = stopwatch.ElapsedMilliseconds;


            while (true)
            {
                // Get delta time
                var timeThisFrame = stopwatch.ElapsedMilliseconds;
                var deltaTime = (timeThisFrame - timeLastFrame) * .001;
                timeLastFrame = timeThisFrame;

                //Update Network Manager
                ServerNetworkManager.Update();

                // Update physics simulation.
                PhysicsWorld.UpdateStep(deltaTime);//TODO: this might not work.
            }
#endif
        }






        #region Network Events -------------------------------------------------------

        /// <summary>
        /// Called when a remote player connects to server.
        /// </summary>
        void ServerNetworkManager_OnPlayerConnected(object sender, NetworkManager.PlayerConnectedEventArgs e)
        {
            // Add a player.
            var player = new Player
                {
                    Name = e.playerPacket.name,
                    UniqueID = e.playerPacket.uid,
                    Position = new Vector2(e.playerPacket.PosX, e.playerPacket.PosY),
                    FacingDirection = e.playerPacket.facingDirection
                };

            Console.WriteLine("New Player Connected:\tName:{0} \tID:{1}", player.Name, player.UniqueID);
        }


        /// <summary>
        /// Called when a remote player disconnects from the server.
        /// </summary>
        void ServerNetworkManager_OnPlayerDisconnected(object sender, NetworkManager.PlayerDisconnectedEventArgs e)
        {
            // Get player from list.
            var player = Player.GetPlayerFromId(e.uniqueID);

            // Remove player from list.
            Player.RemovePlayer(player);

            // Log disconnect.
            Console.WriteLine("Player Disconnected:\tName:{0} \tId:{1}", player.Name, player.UniqueID);
        }


        /// <summary>
        /// Called once for each remote player (per/net-cycle) if moved.
        /// </summary>
        void ServerNetworkManager_OnPlayerMove(object sender, NetworkManager.PlayerMoveEventArgs e)
        {
            // Get player from list.
            var player = Player.GetPlayerFromId(e.uniqueID);

            // Upate player position.
            player.Position = new Vector2(e.posX, e.posY);

            // Update player rotation
            player.FacingDirection = e.facingDirection;

            // Log move
            //Console.WriteLine("Player Moved:\tName:{0} \tFrom:{1} \tTo:{2} \tId:{3}", player.Name, player.OldPosition, player.Position, player.UniqueID);
        }


        /// <summary>
        /// Called once when remote player attacks.
        /// </summary>
        void ServerNetworkManager_OnPlayerAttack(object sender, NetworkManager.PlayerAttackEventArgs e)
        {
            

            Console.WriteLine("Player Attacks: Id:{0}", e.uniqueID);
        }

        #endregion //Network Events -------------------------------------------------------

    }
}
