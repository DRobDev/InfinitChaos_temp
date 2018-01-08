using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.DebugView;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Shared.Tools;
using xTile;
using xTile.Dimensions;
using xTile.Layers;

namespace Shared.World
{
    /// <summary>
    /// Custom names for enum //TODO: find a better way of doing this.
    /// </summary>
    public class CustomCollisionCategories
    {
        public const Category Everything = Category.All;
        public const Category StaticWorld = Category.Cat10;
        public const Category Testing = Category.Cat11;
        public const Category Players = Category.Cat12;
        public const Category Sensors = Category.Cat13;
        public const Category Nothing = Category.None;
    }




    /// <summary>
    /// World simulated with physics.
    /// </summary>
    public class PhysicsWorld
    {
        // Properties //

        // Variables //

        private Matrix _debugTranFromCamera = Matrix.Identity;
        private Vector3 _debugTranPositionFromCamera = Vector3.Zero;


        // References //

        private readonly Game _game;


        // Members //

        private readonly List<Body> _staticMapColliders = new List<Body>();
        public readonly FarseerPhysics.Dynamics.World World = new FarseerPhysics.Dynamics.World(Vector2.Zero);
#if !PSM
        private DebugViewXNA _debugViewXna;
#endif


        /// <summary>
        /// Constructor
        /// </summary>
        public PhysicsWorld(Game game = null)
        {
            _game = game;
        }


#if !PSM
        /// <summary>
        /// Loads debug view resources.
        /// </summary>
        public void LoadDebugView()
        {
            if (_game == null)
                throw new Exception("Must pass 'Game' into constructor to use debug view.");

            _debugViewXna = new DebugViewXNA(World);
            _debugViewXna.LoadContent(_game.GraphicsDevice, _game.Content, @"Fonts\VerdanaFont");
        }
#endif



        /// <summary>
        /// Called once per frame.
        /// </summary>
        public void UpdateStep(double worldStep)
        {
            // Update world simulation. (cap min update to 60fps)
            World.Step((float)worldStep);
        }


        /// <summary>
        /// Generates collision geometry based on map layer and adds it to the world.
        /// </summary>
        /// <param name="mapConfig">Config file for map used</param>
        public void GenerateMapCollidersFromFile(MapConfig mapConfig)
        {
            var inBodyData = new Dictionary<string, List<Vertices>>();




            // Tranlate vertex file into Dictionary of verticies //
            #region Translate vertex file into 'inBodyData'


            var vertexFilePth = mapConfig.VertexFile;
#if PSM
            vertexFilePth = vertexFilePth.Insert(0, @"Application\");
#endif
            var vertexReader = new StreamReader(vertexFilePth);


            // Read: Number of colliders.
            var colliderCout = Convert.ToInt32(vertexReader.ReadLine());

            for (var i = 0; i < colliderCout; i++)
            {
                // Read: Collider key.
                var colliderKey = vertexReader.ReadLine();

                if (colliderKey == null) throw new Exception("Error when reading collider key.");
                inBodyData[colliderKey] = new List<Vertices>();


                // Read: Number of convex bodies.
                var convexBodyCount = Convert.ToInt32(vertexReader.ReadLine());


                for (var j = 0; j < convexBodyCount; j++)
                {
                    // Add new convex body.
                    inBodyData[colliderKey].Add(new Vertices());

                    // Read: Number of verticies in convex body.
                    var vertexCount = Convert.ToInt32(vertexReader.ReadLine());

                    for (var k = 0; k < vertexCount; k++)
                    {
                        // Read: Vertex data.
                        string test = vertexReader.ReadLine();
                        var x = Convert.ToDouble(test);
                        var y = Convert.ToDouble(vertexReader.ReadLine());

                        // Add vertex data.
                        inBodyData[colliderKey].Last().Add(new Vector2((float)x, (float)y));
                    }
                }
            }

            #endregion Translate vertex file into 'inBodyData'




            var tileTypesUsed = new List<string>();
            var tilePlacement = new Dictionary<Vector2, string>();

            // Translate raw map file into tile placement information //
            #region Translate raw map file into 'tileTypesUsed' and 'tilePlacement'


            var rawMapPath = mapConfig.MapDirectory;
            rawMapPath = rawMapPath.Insert(0, @"Content\");
#if PSM
            rawMapPath = rawMapPath.Insert(0, @"Application\");
#endif
            rawMapPath += ".tide";

            var mapReader = XmlReader.Create(rawMapPath);

            string tileSheetId = "";
            string tileSheetKey = "";
            var tileIndex = new Vector2(-1, -1);
            var tileSize = Size.Zero;
            bool collisionLayer = false;


            // Read in raw map file
            while (mapReader.Read())
            {
                // Initialize collision layer.
                if (mapReader.Name == "Layer")
                {
                    if (mapReader.NodeType != XmlNodeType.EndElement)
                    {
                        if (mapConfig.CollisionLayers.Contains(mapReader.GetAttribute("Id")))
                        {
                            tileIndex = new Vector2(-1, -1);
                            collisionLayer = true;
                        }
                        else
                            collisionLayer = false;
                    }
                }

                // Process collision layer
                if (collisionLayer)
                {
                    // Record tile size.
                    if (mapReader.Name == "Dimensions")
                        tileSize = Size.FromString(mapReader.GetAttribute("TileSize"));

                    // Record tile sheet name.
                    if (mapReader.Name == "TileSheet")
                        tileSheetId = mapReader.GetAttribute("Ref");

                    // Process tile indexes on new row.
                    if (mapReader.Name == "Row")
                    {
                        if (mapReader.NodeType != XmlNodeType.EndElement)
                        {
                            tileIndex.Y += 1;
                            tileIndex.X = -1;
                        }
                    }

                    // Read information for each tiles location.
                    if (mapReader.Name == "Static")
                    {
                        tileIndex.X += 1;

                        // Generate key for tile.
                        tileSheetKey = tileSheetId + mapReader.GetAttribute("Index");

                        // Add key to list if not already contained.
                        if (!tileTypesUsed.Contains(tileSheetKey))
                            tileTypesUsed.Add(tileSheetKey);

                        // Recored tile placement for tile.
                        tilePlacement[new Vector2(tileIndex.X * tileSize.Width, tileIndex.Y * tileSize.Height)] =
                            tileSheetKey;
                    }

                    // Skip unassigned tiles.
                    if (mapReader.Name == "Null")
                        tileIndex.X += Convert.ToInt32(mapReader.GetAttribute("Count"));
                }

            }

            #endregion Translate raw map file into 'tileTypesUsed' and 'tilePlacement'




            // Tranlate tile types into temporary physics bodies //
            #region Populate physics world with map colliders

            var tileColliderTypes = new Dictionary<string, Body>();

            var tempWorld = new FarseerPhysics.Dynamics.World(Vector2.Zero);

            foreach (var tileUsed in tileTypesUsed)
            {
                tileColliderTypes[tileUsed] = BodyFactory.CreateCompoundPolygon(tempWorld, inBodyData[tileUsed], 1, Vector2.Zero);
            }

            // Copy physics bodies into physical world at tile locations //
            foreach (var colliderPair in tilePlacement)
            {
                _staticMapColliders.Add(tileColliderTypes[colliderPair.Value].DeepClone(World));
                _staticMapColliders.Last().Position =
                    ConvertUnits.ToSimUnits(colliderPair.Key - Camera.ScreenCenter);
            }

            #endregion Populate physics world with map colliders
        }




#if !PSM
        /// <summary>
        /// Draws physics debug overlay to screen.
        /// </summary>
        /// <param name="camera"></param>
        public void DrawDebugOverlay(Camera camera)
        {
            if (_debugViewXna == null)
                throw new Exception("Debug view not loaded, call 'LoadDebugView()'.");

            // Position debug view //
            _debugTranPositionFromCamera.X = ConvertUnits.ToSimUnits(-camera.Position.X);
            _debugTranPositionFromCamera.Y = ConvertUnits.ToSimUnits(-camera.Position.Y);

            _debugTranFromCamera.Translation = _debugTranPositionFromCamera;

            // Draw debug.
            _debugViewXna.RenderDebugData(Matrix.CreateOrthographic(ConvertUnits.ToSimUnits(camera.ViewportSize.X), ConvertUnits.ToSimUnits(-camera.ViewportSize.Y), 0, 1000), _debugTranFromCamera);
        }
#endif
    }

}
