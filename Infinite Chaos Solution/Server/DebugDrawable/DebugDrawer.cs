using System;
using System.Collections.Generic;
using FarseerPhysics.DebugView;
using Microsoft.Xna.Framework;
using Shared.PlayerControl;
using Shared.Tools;

namespace Server.DebugDrawable
{
    /// <summary>
    /// List of functions for drawing various debug displays.
    /// </summary>
    public static class DebugDrawer
    {
        static private readonly Vector2 FontLineSpacing = new Vector2(0, 14);
        static private readonly Vector2 TextPanelTopLeftCorner = new Vector2(ServerEngine.ScreenSize.X * .5f + 150, 0);
        private static List<string> _panelText = new List<string>()
                                                     {
                                                         "-------------------------",
                                                         "---     TEXT PANEL    ---",
                                                         "-------------------------",
                                                         "                         "
                                                     };


        /// <summary>
        /// Draws player representation to screen.
        /// </summary>
        public static void DrawDebugPlayer(DebugViewXNA debugView, Player player)
        {
            // draw player circle
            debugView.DrawCircle(player.Position, 20, Color.Blue);

            // Draw facing direction
            var directionVector = new Vector2((float)Math.Cos(player.Rotation), (float)Math.Sin(player.Rotation));
            debugView.DrawArrow(player.Position, player.Position + directionVector * 50, 25, 25, true, Color.Blue);

            // Draw player text
            var fontPos = FontToMapPosition(player.Position) + new Vector2(20, -20);// + (debugView.)
            debugView.DrawString((fontPos - FontLineSpacing * 4), string.Format("Name:{0}", player.Name));
            debugView.DrawString((fontPos - FontLineSpacing * 3), string.Format("Id:{0}", player.UniqueID));
            debugView.DrawString((fontPos - FontLineSpacing * 2), string.Format("Position:{0}", player.Position));
        }





        /// <summary>
        /// Begin drawing panel text left of the map.
        /// </summary>
        public static void BeginPanelText()
        {
            _panelText.RemoveRange(4, _panelText.Count -4);
        }

        /// <summary>
        /// Adds a line of text to panel.
        /// </summary>
        public static void DrawPanelText(string panelText)
        {
            _panelText.Add(panelText);
        }

        /// <summary>
        /// Ends the drawing of text for this frame.
        /// </summary>
        public static void EndPanelText(DebugViewXNA debugView)
        {
            for (var i = 0; i < _panelText.Count; i++)
            {
                debugView.DrawString((TextPanelTopLeftCorner + FontLineSpacing * i), _panelText[i]);
            }
        }






        // Aligns font to debug world.
        internal static Vector2 Fa = (new Vector2(1600, 900) / ServerEngine.MapMultiplyer) * .5f; //Temp:
        private static Vector2 FontToMapPosition(Vector2 pos)
        {
            return (pos / ServerEngine.MapMultiplyer) + Fa;
        }

    }
}
