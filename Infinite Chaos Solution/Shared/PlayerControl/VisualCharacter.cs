using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Shared.Tools;

namespace Shared.PlayerControl
{
    /// <summary>
    /// Displays the animated sword dude on the screen.
    /// </summary>
    public class VisualCharacter
    {
        // Properties //

        public Animator CharacterAnimator { get; private set; }


        // References //

        private readonly Player _player;



        /// <summary>
        /// Constructor.
        /// </summary>
        public VisualCharacter(Game game, Player player)
        {
            // Assign refrences //
            _player = player;

            // Initialize Members //
            CharacterAnimator = new Animator(game);

            // Load animations //
            CharacterAnimator.AddAnimation(
                @"Content\AnimatedSprites\Player\Dude_Idle_Data.xml",
                @"AnimatedSprites\Player\Dude_Idle_Sprite",
                Animator.Animations.Idle);

            CharacterAnimator.AddAnimation(
                @"Content\AnimatedSprites\Player\Dude_Run_Data.xml",
                @"AnimatedSprites\Player\Dude_Run_Sprite",
                Animator.Animations.Run);

            CharacterAnimator.AddAnimation(
                @"Content\AnimatedSprites\Player\Dude_Attack_Data.xml",
                @"AnimatedSprites\Player\Dude_Attack_Sprite",
                Animator.Animations.Attack);

            CharacterAnimator.SetAnimationSpeed(15);
        }




        /// <summary>
        /// Updates visual character.
        /// </summary>
        public void UpdateVisualCharacter(GameTime gameTime)
        {
            // Set determined Anmation.
            CharacterAnimator.SetAnimation(DeterminAnimation());

            // Update animations.
            CharacterAnimator.Update(gameTime);
        }


        /// <summary>
        /// Draws character to submitted sprite batch.
        /// </summary>
        public void DrawCharacter(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                CharacterAnimator.GetActiveTexture(),
                _player.Position,
                CharacterAnimator.GetActiveRect(),
                Color.White,
                _player.Rotation,
                CharacterAnimator.GetPivotVec(),
                1, SpriteEffects.None, 0);
        }





        // Internal Functions //--------------------------------------//


        // Determins 'Character State' based on position, and animationl.
        private Animator.Animations DeterminAnimation()
        {
            // Less movement than this is considered stationary.
            const float movementTollerance = .0002f;

            // Check if attacking
            if (CharacterAnimator.PlayingPlayOnceAnimation &&
                CharacterAnimator.CurrentAnimation == Animator.Animations.Attack)
                return Animator.Animations.Attack;

            // Then check if moving
            if (Vector2.Distance(_player.OldPosition, _player.Position) > movementTollerance)
                return Animator.Animations.Run;

            // Default to idle.
            return Animator.Animations.Idle;
        }
    }
}
