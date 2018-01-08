using System;
using Microsoft.Xna.Framework;

namespace Shared.Tools
{
    public static class Extensions
    {
        /// <summary>
        /// Gets angle.
        /// </summary>
        /// <returns>In radians</returns>
        public static float ToAngle(this Vector2 vec)
        {
            return (float)(Math.Atan2(vec.Y, vec.X));
        }

        /// <summary>
        /// Gets delta time;
        /// </summary>
        public static double ToDeltaTime(this GameTime gameTime)
        {
            return gameTime.ElapsedGameTime.TotalSeconds;
        }
    }
}
