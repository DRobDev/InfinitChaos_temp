using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Shared.Tools
{

    /// <summary>
    /// Used for lerping Vector2's
    /// </summary>
    public class Vec2Lerper : LerperBase
    {
        // Properties //

        public bool Lerping { get; private set; }
        public Vector2 LerpPosition { get; private set;}

        // Variables //

        private float _lerpDuration;
        private float _normalizedLerpTimer = 0;
        private Vector2 _lerpTarget;
        private Vector2 _lerpStartPosition;



        /// <summary>
        /// Create lerper.
        /// </summary>
        /// <param name="lerpDuration">In milliseconds.</param>
        public Vec2Lerper(float lerpDuration)
        {
            _lerpDuration = lerpDuration;
        }

        /// <summary>
        /// Update lerp.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            if (Lerping)
            {
                _normalizedLerpTimer += gameTime.ElapsedGameTime.Milliseconds/_lerpDuration;

                if (_normalizedLerpTimer >= 1)
                {
                    _normalizedLerpTimer = 1;
                    Lerping = false;
                }

                LerpPosition = Vector2.Lerp(_lerpStartPosition, _lerpTarget, _normalizedLerpTimer);
            }
        }



        /// <summary>
        /// Lerps from current position to new target.
        /// Resets lerp.
        /// </summary>
        public void SetLerpTarget(Vector2 target)
        {
            _lerpStartPosition = LerpPosition;
            _lerpTarget = target;

            _normalizedLerpTimer = 0;

            Lerping = true;
        }


        /// <summary>
        /// Lerps from current position to new target.
        /// Resets lerp.
        /// </summary>
        public void SetLerpTarget(Vector2 target, float duration)
        {
            _lerpDuration = duration;

            SetLerpTarget(target);
        }



        /// <summary>
        /// Lerps form start to target.
        /// Resets lerp.
        /// </summary>
        public void SetLerp(Vector2 start, Vector2 target, float duration)
        {
            _lerpDuration = duration;
            _lerpStartPosition = LerpPosition;
            _lerpTarget = target;

            _normalizedLerpTimer = 0;
        }

    }






    /// <summary>
    /// Lerper base.
    /// </summary>
    public abstract class LerperBase
    {
        private int _lerpDuration = 80;//ms
        private float _lerpTimer;

        public void UpdateLerp(GameTime gameTime)
        {

        }
    }
}
