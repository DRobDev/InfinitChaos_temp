#region Using Statements
using System;
using System.Collections.Generic;
using Client.PlayerControl;
using FarseerPhysics.DebugView;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Server.DebugDrawable;
using Server.Networking;
using Shared.Networking;
using Shared.PlayerControl;
using Shared.Tools;
using Shared.World;

#endregion

namespace Server
{

    //#define DRAWGAME

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class ServerEngine : Game
    {
        // Properties //
        // Variables //
        public const float MapMultiplyer = 2.28f;//TEMP:
        internal static Vector2 ScreenSize = new Vector2(1600, 900);
        internal Matrix DebugViewProjection;
        internal Matrix DebugViewTranslation;


        // References //

        // Members // 
        internal ServerEngineConsole ServerConsole;
        internal GraphicsDeviceManager GraphicsDeviceManager;
        internal SpriteBatch SpriteBatch;
        internal Camera Camera;
        internal DebugViewXNA DebugTools;


        /// <summary>
        /// Constructor.
        /// Use Initialize() or Load() where possible.
        /// </summary>
        public ServerEngine(ServerEngineConsole serverConsole)
        {
            ServerConsole = serverConsole;
            // Initialize GraphicsDeviceManager
            GraphicsDeviceManager = new GraphicsDeviceManager(this) { PreferredBackBufferWidth = (int)ScreenSize.X, PreferredBackBufferHeight = (int)ScreenSize.Y };
            // Show cursor
            IsMouseVisible = true;
        }


        /// <summary>
        /// Use as initializer
        /// </summary>
        protected override void Initialize()
        {
            // Initialize Members //
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            // Fit map on camera // TEMP:
            Camera = new Camera(ScreenSize * MapMultiplyer);
            Camera.Position += Camera.ScreenCenter;

            // Generate debug view parameters.
            DebugViewProjection = Matrix.CreateOrthographicOffCenter(
                Camera.Position.X, Camera.Position.X + Camera.ViewportSize.X,
                Camera.Position.Y + Camera.ViewportSize.Y, Camera.Position.Y,
                0, 1000);
            DebugViewTranslation = Matrix.CreateTranslation(Camera.Position.X + (ScreenSize.X * .5f), Camera.Position.Y + (ScreenSize.Y * .5f), 0);

            // Initialize XNA base engine.
            base.Initialize();
        }


        /// <summary>
        /// Use to load game resources.
        /// </summary>
        protected override void LoadContent()
        {
            //Content Engine
            Content.RootDirectory = "Content";

            // Load physics debug
            ServerConsole.PhysicsWorld.LoadDebugView();

            // Create debugger
            DebugTools = new DebugViewXNA(new World(Vector2.Zero));
            DebugTools.LoadContent(GraphicsDevice, Content, @"Fonts\DebugFont");

            base.LoadContent();
        }



        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Update Camera
            Camera.Update(gameTime);

            // Update physics simulation.
            ServerConsole.PhysicsWorld.UpdateStep(gameTime.ToDeltaTime());

            // Update network.
            ServerConsole.ServerNetworkManager.Update();

            //Update Base Game
            base.Update(gameTime);
        }




        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Draw background physics map.
            ServerConsole.PhysicsWorld.DrawDebugOverlay(Camera);


            // Draw overlayed map components.
            DebugTools.BeginCustomDraw(DebugViewProjection, DebugViewTranslation);
            {
                foreach (var player in Player.AllPlayers)
                {
                    DebugDrawer.DrawDebugPlayer(DebugTools, player);
                }
            }
            DebugTools.EndCustomDraw();


            // Draw Text to debug pannel.
            DebugDrawer.BeginPanelText();
            {

                DebugDrawer.DrawPanelText("--------- Players ---------");
                foreach (var player in Player.AllPlayers)
                {
                    DebugDrawer.DrawPanelText(" ");
                    DebugDrawer.DrawPanelText(string.Format("Name:{0}", player.Name));
                    DebugDrawer.DrawPanelText(string.Format("Id:{0}", player.UniqueID));
                    DebugDrawer.DrawPanelText(string.Format("Position:{0}", player.Position));
                }
            }
            DebugDrawer.EndPanelText(DebugTools);



            // Outputs map text.
            DebugTools.RenderDebugData(DebugViewProjection, DebugViewTranslation);

            // Draw Xna base game.
            base.Draw(gameTime);
        }

    }
}
