
//////////////////////////////////////////////////////////////////////
// 
// Player.cs
//
//////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Shared.Tools;


namespace Shared.PlayerControl
{
    /// <summary>
    /// Base player.
    /// </summary>
    public class Player : ICameraFocusable
    {
        public enum FacingDirections
        { E = 0, Se = 45, S = 90, Sw = 135, W = 180, Nw = 225, N = 270, Ne = 315, None }


        // Properties //

        public static IEnumerable<Player> AllPlayers { get { return _allPlayers; } }

        public virtual Vector2 Position
        {
            get { return _position; }
            set { OldPosition = _position; _position = value; }
        }

        public Vector2 OldPosition { get; protected set; }

        public FacingDirections FacingDirection
        {
            get { return _facingDirection; }
            set { _facingDirection = value; _rotation = AngleFromFacingDirection(value); }
        }

        public float Rotation
        {
            get { return _rotation; }
            // Snap rotation
            set { _facingDirection = FacingDirectionFromAngle(value); _rotation = AngleFromFacingDirection(_facingDirection); }
        }

        public bool AttackPerformed { get; protected set; }


        // Variables //

        private static List<Player>_allPlayers = new List<Player>(); 
        public String Name = "Unnamed";
        public Int64 UniqueID = 0;
        private FacingDirections _facingDirection;
        private float _rotation;
        private Vector2 _position;



        // References //

        protected Game Game;


        // Members //

        protected VisualCharacter VisualCharacter;




        /// <summary>
        /// Constructor.
        /// </summary>
        public Player(Game game = null)
        {
            Game = game;
            _allPlayers.Add(this);
        }

        
        /// <summary>
        /// Destructor.
        /// </summary>
        ~Player()
        {
            _allPlayers.Remove(this);
        }



        /// <summary>
        /// Visual character position set.
        /// Visual character Updated.
        /// </summary>
        public virtual void UpdatePlayer(GameTime gametime)
        {
            if (VisualCharacter != null)
                VisualCharacter.UpdateVisualCharacter(gametime);
        }



        /// <summary>
        /// Loads visual player.
        /// </summary>
        public void LoadVisualPlayer()
        {
            if(Game == null)
                throw new Exception("Can't load visual character. 'Game' not passed when intialized.");

            VisualCharacter = new VisualCharacter(Game, this);
        }




        /// <summary>
        /// Draws visual character.
        /// </summary>
        /// <param name="spriteBatch">Active sprite batch</param>
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            VisualCharacter.DrawCharacter(spriteBatch);
        }




        /// <summary>
        /// Clears all players from list.
        /// </summary>
        public static void ClearAllPlayersExcept(Player exceptPlayer)
        {
            _allPlayers.Clear();
            _allPlayers.Add(exceptPlayer);
        }


        /// <summary>
        /// Removes specified player from player list.
        /// </summary>
        /// <param name="player"></param>
        public static void RemovePlayer(Player player)
        {
            _allPlayers.Remove(player);
        }


        /// <summary>
        /// Gets player with submitted id from list.
        /// </summary>
        public static Player GetPlayerFromId(long id)
        {
            foreach (var player in AllPlayers)
            {
                if(player.UniqueID == id)
                    return player;
            }

            throw new Exception("Player Id not contained in player list.");
        }




        /// <summary>
        /// Calculate facing direction based on angle.
        /// </summary>
        /// <param name="angle">Angle in radians</param>
        /// <returns>Angle represented as facing direction</returns>
        private static FacingDirections FacingDirectionFromAngle(float angle)
        {
            // Convert to degrees
            angle = MathHelper.ToDegrees(angle);


            // Round to 45
            angle = (float)Math.Round(angle / 45.0f) * 45.0f;


            // TEMP:fix
            const float rondingTollerance = .00001f;
            if (Math.Abs(angle - -180) < rondingTollerance) angle = 180;
            else if (Math.Abs(angle - -135) < rondingTollerance) angle = 225;
            else if (Math.Abs(angle - -90) < rondingTollerance) angle = 270;
            else if (Math.Abs(angle - -45) < rondingTollerance) angle = 315;


            // Return angle translated to FacingDirection.
            return (FacingDirections)(int)angle;
        }


        /// <summary>
        /// Convert facing direction to angle (radians).
        /// </summary>
        /// <param name="facingDirection">Facing direction to convert</param>
        /// <returns>Angle in radians</returns>
        private static float AngleFromFacingDirection(FacingDirections facingDirection)
        {
            switch (facingDirection)
            {
                case FacingDirections.N:
                    return 4.71238898f;
                case FacingDirections.Ne:
                    return 5.49778714f;
                case FacingDirections.E:
                    return 0.0f;
                case FacingDirections.Se:
                    return 0.785398163f;
                case FacingDirections.S:
                    return 1.57079633f;
                case FacingDirections.Sw:
                    return 2.35619449f;
                case FacingDirections.W:
                    return 3.14159265f;
                case FacingDirections.Nw:
                    return 3.92699082f;
            }
            throw new Exception("Could not calculate direction from facing directin: " + facingDirection.ToString());
        }
    }
}
