using Microsoft.Xna.Framework.Input;

namespace Shared.Tools
{
    public class Input
    {
        public enum eInput { UP, LEFT, DOWN, RIGHT, PRIMARY, SECONDARY };






        private bool[] keys = new bool[6];

        public Input()
        {


            for (int i = 0; i < 6; i++)
            {
                keys[i] = false;
            }
        }

        public bool IsKeyPressed(eInput key)
        {
            if (!keys[(int)key])
            {
                UpdateKey(key);
                if (keys[(int)key])
                    return true;
            }

            UpdateKey(key);
            return false;
        }

        public bool IsKeyReleased(eInput key)
        {

            if (keys[(int)key])
            {
                UpdateKey(key);
                if (!keys[(int)key])
                    return true;
            }

            UpdateKey(key);
            return false;
        }

        public bool IsKeyDown(eInput key)
        {
            UpdateKey(key);
            return keys[(int)key];
        }


        private void UpdateKey(eInput key)
        {
            if (key == eInput.UP)
            {
#if PSM
                if (GamePad.GetState(0).DPad.Up == ButtonState.Pressed)
#else
                if (Keyboard.GetState().IsKeyDown(Keys.W))
#endif
                    keys[(int)key] = true;
                else
                    keys[(int)key] = false;

                return;
            }

            if (key == eInput.LEFT)
            {
#if PSM
                if (GamePad.GetState(0).DPad.Left == ButtonState.Pressed)
#else
                if (Keyboard.GetState().IsKeyDown(Keys.A))
#endif
                    keys[(int)key] = true;
                else
                    keys[(int)key] = false;

                return;
            }


            if (key == eInput.DOWN)
            {
#if PSM
                if (GamePad.GetState(0).DPad.Down == ButtonState.Pressed)
#else
                if (Keyboard.GetState().IsKeyDown(Keys.S))
#endif
                    keys[(int)key] = true;
                else
                    keys[(int)key] = false;

                return;
            }


            if (key == eInput.RIGHT)
            {
#if PSM
                if (GamePad.GetState(0).DPad.Right == ButtonState.Pressed)
#else
                if (Keyboard.GetState().IsKeyDown(Keys.D))
#endif
                    keys[(int)key] = true;
                else
                    keys[(int)key] = false;

                return;
            }


            if (key == eInput.PRIMARY)
            {
#if PSM
                if (GamePad.GetState(0).Buttons.A == ButtonState.Pressed)
#else
                if (Keyboard.GetState().IsKeyDown(Keys.Space))
#endif
                    keys[(int)key] = true;
                else
                    keys[(int)key] = false;

                return;
            }

            if (key == eInput.SECONDARY)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.X))
                    keys[(int)key] = true;
                else
                    keys[(int)key] = false;

                return;
            }

        }
    }
}
