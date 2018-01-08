using System;
using Client.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Shared.Networking;
using Shared.PlayerControl;
using Shared.Tools;
using Shared.World;

namespace Client.PlayerControl
{
    public class ControllablePlayer : Player
    {

        // Properties //

        public static Player ControllablePlayerInstance { get { return _playerInstance; } }


        // Variables //

        private static Player _playerInstance;
        private const float MoveSpeed = 60000;
        private const float PlayerRadius = .3f;
        private const float PlayerWeight = 10;
        private Vector2 _desiredMoveDirection;


        // References //

        private readonly ClientNetworkManager _clientNetworkManager;


        // Members //
        private readonly Body _body;
        private readonly Input _input;




        /// <summary>
        /// Constructor.
        /// </summary>
        public ControllablePlayer(Game game, PhysicsWorld physicsWorld, ClientNetworkManager clientNetworkManager)
            : base(game)
        {
            // Assign references //
            _clientNetworkManager = clientNetworkManager;

            // Initialize members //
            _input = new Input();

            if (_playerInstance != null)
                throw new Exception("Only one controllable player is allowed at this point.");

            _playerInstance = this;

            _body = BodyFactory.CreateCircle(physicsWorld.World, PlayerRadius, PlayerWeight, Vector2.Zero, BodyType.Dynamic);
            _body.CollisionCategories = CustomCollisionCategories.Players;
            _body.FixedRotation = true;

        }




        /// <summary>
        /// Moves player based on user input.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void UpdatePlayer(GameTime gameTime)
        {

            // Move on input //

            // Don't move if animating attack
            if (VisualCharacter != null && VisualCharacter.CharacterAnimator.CurrentAnimation != Animator.Animations.Attack)
            {
                // Get direction from input //
                _desiredMoveDirection = Vector2.Zero;

                if (_input.IsKeyDown(Input.eInput.LEFT))
                    _desiredMoveDirection.X -= 1;

                if (_input.IsKeyDown(Input.eInput.RIGHT))
                    _desiredMoveDirection.X += 1;

                if (_input.IsKeyDown(Input.eInput.UP))
                    _desiredMoveDirection.Y -= 1;

                if (_input.IsKeyDown(Input.eInput.DOWN))
                    _desiredMoveDirection.Y += 1;


                // Set rotation to 'direction'.
                if (_desiredMoveDirection != Vector2.Zero)
                    Rotation = _desiredMoveDirection.ToAngle();

                // Move in 'direction'. //
                _body.ApplyForce(_desiredMoveDirection * (MoveSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds));
            }




            // Attack on input //

            if (_input.IsKeyPressed(Input.eInput.PRIMARY))
            {
                // Send attack over network.
                _clientNetworkManager.SendAttackPacket(this);


                // Perform visual attack.
                if (VisualCharacter != null)
                    VisualCharacter.CharacterAnimator.SetAnimation(Animator.Animations.Attack, true);




#if WINDOWS && DEBUG
                // Log attack
                Console.WriteLine("Local Attack.");
#endif
            }





            // Sync player position to pysical body //
            _body.LinearVelocity = Vector2.Zero;
            Position = ConvertUnits.ToDisplayUnits(_body.Position);


            // Updates visual player
            base.UpdatePlayer(gameTime);
        }


    }
}
