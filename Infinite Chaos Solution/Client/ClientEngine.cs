
//////////////////////////////////////////////////////////////////////
// 
// ClientEngine.cs
//
//////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using Client.Networking;
using Client.PlayerControl;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Shared.Networking;
using Shared.PlayerControl;
using Shared.Tools;
using Shared.World;


namespace Client
{

    /// <summary>
    /// This is the Main game loop for clients, enjoy.
    /// </summary>
    public class ClientEngine : Game
    {
        // Properties //

        // Variables //

        internal readonly static Vector2 ScreenSize = new Vector2(1600, 900);
#if PSM
        public static Vector2 PSMScreenSize;
#endif

        // References //

        internal GraphicsDeviceManager GraphicsDeviceManager;


        // Members //

        internal ClientNetworkManager ClientNetworkManager;
        internal ControllablePlayer ControllablePlayer;
        internal SpriteBatch SpriteBatch;
        internal Camera Camera;
        internal TileMap TileMap;
        internal PhysicsWorld PhysicsWorld;



        /// <summary>
        /// Constructor.
        /// Use Initialize() or Load() where possible.
        /// </summary>
        public ClientEngine()
        {

            GraphicsDeviceManager = new GraphicsDeviceManager(this);
#if !PSM
            // Set Windows screen-size.
            GraphicsDeviceManager.PreferredBackBufferWidth = (int)ScreenSize.X;
            GraphicsDeviceManager.PreferredBackBufferHeight = (int)ScreenSize.Y;
#else
            // Get default screen-size for PSM.
            PSMScreenSize.X = GraphicsDeviceManager.PreferredBackBufferWidth;
            PSMScreenSize.Y = GraphicsDeviceManager.PreferredBackBufferHeight;
#endif
        }


        /// <summary>
        /// Use this as constructor.
        /// </summary>
        protected override void Initialize()
        {
#if WINDOWS
            // Set console title.
            Console.Title = "Client";
            IsMouseVisible = true;
#else

#endif

            // Initialize Members //
            ClientNetworkManager = new ClientNetworkManager(this);
            ClientNetworkManager.Connect("Vita", "10.17.23.15" );//TODO: find connections
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            TileMap = new TileMap(this);
            Camera = new Camera(ScreenSize);
#if PSM
            Camera.ConstrainToMap(TileMap, PSMScreenSize);
#else
			Camera.ConstrainToMap(TileMap);
#endif
            PhysicsWorld = new PhysicsWorld(this);
            ControllablePlayer = new ControllablePlayer(this, PhysicsWorld, ClientNetworkManager);
            Camera.Focus = ControllablePlayer;


            // Subscribe to network events //
            ClientNetworkManager.onPlayerConnected += ClientNetworkManager_OnPlayerConnected;
            ClientNetworkManager.onPlayerDisconnected += ClientNetworkManager_OnPlayerDisconnected;
            ClientNetworkManager.onPlayerMove += ClientNetworkManager_OnPlayerMove;
            ClientNetworkManager.onLocalPlayerConnected += ClientNetworkManager_OnLocalPlayerConnected;
            ClientNetworkManager.onPlayerAttack += ClientNetworkManager_OnPlayerAttack;


            // Initialize XNA base engine.
            base.Initialize();
        }


        /// <summary>
        /// Use to load game resources.
        /// </summary>
        protected override void LoadContent()
        {
            //Set root folder for content.
            Content.RootDirectory = "Content";

            // Load members //
            ControllablePlayer.LoadVisualPlayer();
            TileMap.LoadMap(AvailableMapConfigs.BasicMap.MapDirectory);
            PhysicsWorld.GenerateMapCollidersFromFile(AvailableMapConfigs.BasicMap);

#if DEBUG && !PSM
            PhysicsWorld.LoadDebugView();
#endif


            base.LoadContent();
        }




        /// <summary>
        /// Called once per/frame, this is the main body for the game logic.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Update members //

            // Move camera to 'Focus' position.
            Camera.Update(gameTime);

            // Used for animated tiles. //Not needed yet.
            //TileMap.Update(gameTime);

            // Step the physics simulation, collisions etc.
            PhysicsWorld.UpdateStep(gameTime.ToDeltaTime());

            // Read/send network packets, firing events where needed.
            ClientNetworkManager.Update();




            // Perform update for all players, including local player.
            foreach (var player in Player.AllPlayers)
            {
                player.UpdatePlayer(gameTime);
            }




            //Update Xna base game.
            base.Update(gameTime);
        }




        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // Clear device
            GraphicsDevice.Clear(Color.CornflowerBlue);
			
#if PSM
			// Align camera to psm.
			Camera.Position = (Camera.Position + Camera.ScreenCenter) - (PSMScreenSize*.5f);
#endif
			
            // Draw map 
            TileMap.Draw(Camera);

            // Draw scene

            SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
                              DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Camera.Transform);
            {
                // Draw Local player.
                ControllablePlayer.Draw(SpriteBatch);

                // Draw Remote player.
                foreach (Player player in Player.AllPlayers)
                {
                    player.Draw(SpriteBatch);
                }
            }
            SpriteBatch.End();

			

            // Draw physics debug.
#if DEBUG && !PSM
            PhysicsWorld.DrawDebugOverlay(Camera);
#endif
						
#if PSM
			// Align camera back to base world.
			Camera.Position = (Camera.Position + (PSMScreenSize*.5f)) - Camera.ScreenCenter;
#endif

            //Draw Xna Base game.
            base.Draw(gameTime);
					
        }




        /// <summary>
        /// Use to free up graphical resources before shutdown.
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
        }





        #region Network Events -------------------------------------------------------

        /// <summary>
        /// Called when local connection gets approved.
        /// </summary>
        private void ClientNetworkManager_OnLocalPlayerConnected(object sender, NetworkManager.PlayerConnectedEventArgs e)
        {
            ControllablePlayer.Name = e.playerPacket.name;
            ControllablePlayer.Position = new Vector2(e.playerPacket.PosX, e.playerPacket.PosY);
            ControllablePlayer.UniqueID = e.playerPacket.uid;
            ControllablePlayer.FacingDirection = e.playerPacket.facingDirection;

#if DEBUG && !PSM
            //Log local connect.
            Console.WriteLine("Local Player connected: Name: {0} ID: {1}", ControllablePlayer.Name, ControllablePlayer.UniqueID);
#endif
        }


        /// <summary>
        /// Called when a remote player connects to server.
        /// </summary>
        private void ClientNetworkManager_OnPlayerConnected(object sender, NetworkManager.PlayerConnectedEventArgs e)
        {
            // Load new player.\
            var remotePlayer = new RemotePlayer(this)
                                   {
                                       Name = e.playerPacket.name,
                                       UniqueID = e.playerPacket.uid,
                                       Position = new Vector2(e.playerPacket.PosX, e.playerPacket.PosY),
                                       FacingDirection = e.playerPacket.facingDirection
                                   };

            // Load visual player
            remotePlayer.LoadVisualPlayer();

#if DEBUG && !PSM
            //Log remote connect.
            Console.WriteLine("Remote Player Connected: Name: {0} ID: {1}", remotePlayer.Name, remotePlayer.UniqueID);
#endif
        }


        /// <summary>
        /// Called when a remote player disconnects from the server.
        /// </summary>
        void ClientNetworkManager_OnPlayerDisconnected(object sender, NetworkManager.PlayerDisconnectedEventArgs e)
        {
            //Obtain Player Object Reference
            var remotePlayer = Player.GetPlayerFromId(e.uniqueID);

            // Remove player
            Player.RemovePlayer(remotePlayer);

#if DEBUG && !PSM
            //Log player disconnect.
            Console.WriteLine("Remote Player Disconnected: Name: {0} ID: {1}", remotePlayer.Name, remotePlayer.UniqueID);
#endif
        }


        /// <summary>
        /// Called once for each remote player (per/net-cycle) if moved.
        /// </summary>
        void ClientNetworkManager_OnPlayerMove(object sender, NetworkManager.PlayerMoveEventArgs e)
        {
            // Get remote player
            var remotePlayer = Player.GetPlayerFromId(e.uniqueID);

            // Set remote player position
            remotePlayer.Position = new Vector2(e.posX, e.posY);

            // Set player rotation
            remotePlayer.FacingDirection = e.facingDirection;

#if DEBUG && !PSM
            //Log player Movement.
            //Console.WriteLine("Remote Player Moved: Name: {0} ID: {1}", remotePlayer.Name, remotePlayer.UniqueID);
#endif
        }


        /// <summary>
        /// Called once when remote player attacks.
        /// </summary>
        void ClientNetworkManager_OnPlayerAttack(object sender, NetworkManager.PlayerAttackEventArgs e)
        {
            // Get remote player.
            var remotePlayer = Player.GetPlayerFromId(e.uniqueID);

            // Update position and rotation.
            remotePlayer.Position = new Vector2(e.posX, e.posY);
            remotePlayer.FacingDirection = e.facingDirection;

#if DEBUG && !PSM
            //Log player disconnect.
            Console.WriteLine("Remote Player Attacked: Name: {0} ID: {1}", remotePlayer.Name, remotePlayer.UniqueID);
#endif
        }

        #endregion Network Events -------------------------------------------------------

    }
}
