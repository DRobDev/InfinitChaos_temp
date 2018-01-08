using System;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Shared.PlayerControl;
using Shared.Tools;
using Shared.World;

namespace Client.PlayerControl
{
    public class RemotePlayer : Player
    {

        // Properties //
        public override Vector2 Position
        {
            get { return _smoothedPosition; }
            set { _positionLerper.SetLerpTarget(value); }
        }


        // Variables //

        private const float MovePacketWait = 80;//ms //If no packet is recieved by this time, assumed stationary.

        private Vector2 _smoothedPosition;


        // Members //
        private readonly Vec2Lerper _positionLerper = new Vec2Lerper(lerpDuration: MovePacketWait);

		
		
		
		// Need this for PSM
		public RemotePlayer(Game game): base(game)
		{}
		
		


        /// <summary>
        /// Moves player based on user input.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void UpdatePlayer(GameTime gameTime)
        {
            // Update position-lerper.
            _positionLerper.Update(gameTime);


            // Update position/old-position .
            OldPosition = _smoothedPosition;
            _smoothedPosition = _positionLerper.LerpPosition;


            // Updates visual player
            base.UpdatePlayer(gameTime);
        }


    }
}
