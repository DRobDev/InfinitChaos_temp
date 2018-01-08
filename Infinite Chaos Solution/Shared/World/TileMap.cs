using Microsoft.Xna.Framework;
using Shared.Tools;
using xTile.Dimensions;
using xTile.Display;

namespace Shared.World
{
    public class TileMap
    {

        // Properties //

        public int MapWidth { get { return XMap.DisplayWidth; } }
        public int MapHeight { get { return XMap.DisplayHeight; } }

        
        // Variables //

        protected IDisplayDevice Display;
        protected xTile.Dimensions.Rectangle Viewport;
        

        // References //

        protected Game Game;


        // Members //

        public xTile.Map XMap;




        /// <summary>
        /// Constructor.
        /// </summary>
        public TileMap(Game game)
        {
            Game = game;
        }
        



        /// <summary>
        /// Loads an xTile map into memory.
        /// </summary>
        public void LoadMap(string mapDirectory)
        {
                Display = new XnaDisplayDevice(Game.Content, Game.GraphicsDevice);
                Viewport =
                    new xTile.Dimensions.Rectangle(new Size(Game.GraphicsDevice.PresentationParameters.BackBufferWidth,
                                                            Game.GraphicsDevice.PresentationParameters.BackBufferHeight));
                XMap = Game.Content.Load<xTile.Map>(mapDirectory);
                XMap.LoadTileSheets(Display);
        }


        /// <summary>
        /// Update called once per frame. 
        /// </summary>
        public void Update(GameTime gameTime)
        {
            XMap.Update((long)gameTime.ElapsedGameTime.TotalSeconds);
        }


        /// <summary>
        /// Draws map to screen.
        /// </summary>
        public void Draw(Camera camera)
        {
            Viewport.X = (int)camera.Position.X;
            Viewport.Y = (int)camera.Position.Y;

            XMap.Draw(Display, Viewport);
        }


        /// <summary>
        /// Draw specific layers.
        /// </summary>
        public void DrawLayer(Camera camera, params string[] layerNames)
        {
            Viewport.X = (int)camera.Position.X;
            Viewport.Y = (int)camera.Position.Y;

            Display.BeginScene();
            {
                foreach (var layerName in layerNames)
                {
                    XMap.GetLayer(layerName).Draw(Display, Viewport, Location.Origin, true);
                }
            }
            Display.EndScene();
        }

    }
}
