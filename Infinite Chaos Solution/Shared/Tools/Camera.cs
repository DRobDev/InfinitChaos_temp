using Microsoft.Xna.Framework;
using Shared.World;

namespace Shared.Tools
{
    /// <summary>
    /// Object derived from this can be focused on by the camera.
    /// </summary>
    public interface ICameraFocusable
    {
        Vector2 Position { get; set; }
    }




    /// <summary>
    /// It's a camera.
    /// </summary>
    public class Camera
    {

        // Properties //

        public Vector2 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                // Update transform
                Transform = Matrix.Identity *
                            Matrix.CreateTranslation(-_position.X, -_position.Y, 0) *
                            Matrix.CreateRotationZ(Rotation) *
                            Matrix.CreateTranslation(ScreenCenter.X, ScreenCenter.Y, 0) *
                            Matrix.CreateScale(new Vector3(Scale, Scale, Scale));
            }
        }

        public static Vector2 ScreenCenter { get; private set; }
        public Matrix Transform { get; private set; }


        // Variables //

        public ICameraFocusable Focus;
        public float MoveSpeed = 1000;

        private const float Scale = 1;
        private const float Rotation = 0;

        public readonly Vector2 ViewportSize;
        
        private Vector2 _position;
        private Vector2 _mapLockedPosition;
		
#if PSM
		private Vector2 _psmScreenSize;
#endif


        // References //
        private TileMap _activeMap;




        /// <summary>
        /// Constructor
        /// </summary>
        public Camera(Vector2 viewportSize)
        {
            Transform = Matrix.Identity;
            ViewportSize = viewportSize;
            ScreenCenter = viewportSize * .5f;
        }




        /// <summary>
        /// Updates camera matrix. Call once per frame.
        /// </summary>
        /// <param name="gameTime">Time</param>
        public void Update(GameTime gameTime)
        {
            // If 'Focused' move camera towards focus position.
            if (Focus != null && Focus.Position - Position != Vector2.Zero)
            {
                if (Vector2.Distance(Focus.Position, Position) <= MoveSpeed * gameTime.ElapsedGameTime.TotalSeconds)
                    Position = Focus.Position;
                else
                    Position += Vector2.Normalize(Focus.Position - Position) * MoveSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

#if PSM
			// Keep camera inside map bounds //
            if (_activeMap != null)
            {
				Vector2 psmOffset = (_psmScreenSize - ViewportSize)*.5f;
				
                _mapLockedPosition = _position;
				
                if (Position.X < psmOffset.X)
                    _mapLockedPosition.X =  psmOffset.X;
		   		
         		   else if (Position.X > (_activeMap.MapWidth - ViewportSize.X) - psmOffset.X)
                    _mapLockedPosition.X = _activeMap.MapWidth - ViewportSize.X - psmOffset.X;
		   		
         		   if (Position.Y < psmOffset.Y)
                    _mapLockedPosition.Y = psmOffset.Y;
		   		
         		   else if (Position.Y > _activeMap.MapHeight - ViewportSize.Y - psmOffset.Y)
                    _mapLockedPosition.Y = _activeMap.MapHeight - ViewportSize.Y - psmOffset.Y;
		   		
         		   Position = _mapLockedPosition;
            }
#else
            // Keep camera inside map bounds //
            if (_activeMap != null)
            {
                _mapLockedPosition = _position;
                if (Position.X < 0)
                    _mapLockedPosition.X = 0;

                else if (Position.X > _activeMap.MapWidth - ViewportSize.X)
                    _mapLockedPosition.X = _activeMap.MapWidth - ViewportSize.X;

                if (Position.Y < 0)
                    _mapLockedPosition.Y = 0;

                else if (Position.Y > _activeMap.MapHeight - ViewportSize.Y)
                    _mapLockedPosition.Y = _activeMap.MapHeight - ViewportSize.Y;

                Position = _mapLockedPosition;
            }
#endif
        }




        /// <summary>
        /// Returns world position relative to the center of the camera.
        /// </summary>
        public Vector2 GetRelativePosition(Vector2 position)
        {
            return (position + ScreenCenter) - new Vector2(Position.X, Position.Y);
        }


        /// <summary>
        /// Restricts camera movement to map bounds.
        /// </summary>
        /// <param name="map">Map to be constraied to. 'null' to release constraints</param>
        public void ConstrainToMap(TileMap map
#if PSM
		                           ,Vector2 psmScreenSize
#endif
		                           )
        {
            // Release map constraint if null
            if (map == null) { _activeMap = null; return; }

            // Assign map to constrain to 
            _activeMap = map;
#if PSM
			_psmScreenSize = psmScreenSize;
#endif
        }





        /*
        /// <summary>
        /// Determines whether the submitted position is on camera.
        /// </summary>
        //public bool IsInView(int pointX, int pointY, float tollerance = 0)
        //{
        //    // TODO fix this up if needed
        //    //// If the object is not within the horizontal bounds of the screen

        //    //if ((pointX + texture.Width) < (CharacterPosition.X - Origin.X) || (position.X) > (CharacterPosition.X + Origin.X))
        //    //    return false;

        //    //// If the object is not within the vertical bounds of the screen
        //    //if ((position.Y + texture.Height) < (CharacterPosition.Y - Origin.Y) || (position.Y) > (CharacterPosition.Y + Origin.Y))
        //    //    return false;

        //    // In View
        //    return true;
        //}


        /// <summary>
        /// Changes world position to screen position.
        /// </summary>
        /// <param name="xPos">World 'posX' position</param>
        /// <param name="yPos">World 'posY' position</param>
        //public void WorldToScreenSpace(ref int xPos, ref int yPos)
        //{
        //    xPos = (xPos - this.PositionX) + (int)this.Origin.X;
        //    yPos = (yPos - this.PositionY) + (int)this.Origin.Y;
        //}
         */
    }
}
