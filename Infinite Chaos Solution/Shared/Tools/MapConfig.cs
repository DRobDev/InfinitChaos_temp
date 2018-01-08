using System;
using System.Collections.Generic;
using System.Linq;

namespace Shared.Tools
{
    /// <summary>
    /// Required map attributes.
    /// </summary>
    public struct MapConfig
    {
        /// <summary>
        /// Location/Name of xTile Map.
        /// </summary>
        public string MapDirectory;

        /// <summary>
        /// Layer names used for collision.
        /// Found in xTile Map.
        /// </summary>
        public string[] CollisionLayers;

        /// <summary>
        /// Corresponding file with collision vertex information.
        /// Used for generation collider bodies.
        /// </summary>
        public string VertexFile;
    }

    
    /// <summary>
    /// Container for all avialable maps.
    /// </summary>
    public static class AvailableMapConfigs
    {
        /// <summary>
        /// A simple map used for testing shit.
        /// </summary>
        public static readonly MapConfig BasicMap = new MapConfig()
                                                          {
                                                              MapDirectory = @"Map\TestMap",
                                                              CollisionLayers = new[] { "GroundObjects" },
                                                              VertexFile = @"Content\Map\TestMapColliderVerts"
                                                          };
    }
}
