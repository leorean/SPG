using Microsoft.Xna.Framework.Input;
using SPG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leore.Main
{
    public static class InputMapping
    {
        private static InputManager input => MainGame.Current.Input;

        public static bool KeyPressed(Input k, InputManager.State state = InputManager.State.Pressed) => KeyPressed(new[] { k }, state);
        public static bool KeyPressed(Input[] keys, InputManager.State state = InputManager.State.Pressed)
        {
            if (input == null) return false;
            foreach (var k in keys) {

                if (k == null)
                    continue;

                if (input.PreferGamePad && input.GamePadEnabled && k.button != null || k.direction != null)
                {
                    if (k.button == null)
                    {
                        if (input.DirectionPressedFromStick(k.direction.Value.Item1, k.direction.Value.Item2, state))
                            return true;
                    }
                    else
                    {
                        if (input.IsButtonPressed(k.button.Value, state))
                            return true;
                    }
                }

                if (k.key != null && input.IsKeyPressed(k.key.Value, state))
                {
                    return true;
                }

            }
            return false;
        }

        public static float GamePadXFactor => input.LeftStick().X;
        public static float GamePadYFactor => input.LeftStick().Y;

        public static Input Left { get; set; } = new Input { key = Keys.Left, direction = (InputManager.Direction.LEFT, InputManager.Stick.LeftStick) };
        public static Input Right { get; set; } = new Input { key = Keys.Right, direction = (InputManager.Direction.RIGHT, InputManager.Stick.LeftStick) };
        public static Input Up { get; set; } = new Input { key = Keys.Up, direction = (InputManager.Direction.UP, InputManager.Stick.LeftStick) };
        public static Input Down { get; set; } = new Input { key = Keys.Down, direction = (InputManager.Direction.DOWN, InputManager.Stick.LeftStick) };

        public static Input Jump { get; set; } = new Input { key = Keys.A, button = Buttons.A };
        public static Input Attack { get; set; } = new Input { key = Keys.S, button = Buttons.X };

        public static Input L { get; set; } = new Input { key = Keys.Q, button = Buttons.LeftShoulder };
        public static Input R { get; set; } = new Input { key = Keys.E, button = Buttons.RightShoulder };
        //public static Input LT { get; set; } = new Input { key = Keys.D, button = Buttons.LeftTrigger };
        //public static Input RT { get; set; } = new Input { key = Keys.D, button = Buttons.RightTrigger };

        public static Input Roll { get; set; } = new Input { key = Keys.D, button = Buttons.B };

        public static Input Enter { get; set; } = new Input { key = Keys.Enter, button = Buttons.Start };
        public static Input ResetLevel { get; set; } = new Input { key = Keys.R };
        public static Input ResetMenu { get; set; } = new Input { key = Keys.Q };

        public static Input Pause { get; set; } = new Input { key = Keys.P, button = Buttons.Back };
        
        public static Input[] MessageNext { get; } = new[] { Jump, Enter };
        public static Input MessagePrev { get; } = Attack;

        public static Input LeftShift { get; } = new Input { key = Keys.LeftShift };

        //public static Keys Left { get; set; } = Keys.Left;
        //public static Keys Right { get; set; } = Keys.Right;
        //public static Keys Up { get; set; } = Keys.Up;
        //public static Keys Down { get; set; } = Keys.Down;
        //public static Keys Jump { get; set; } = Keys.A;
        //public static Keys Attack { get; set; } = Keys.S;
        //public static Keys L { get; set; } = Keys.Q;
        //public static Keys R { get; set; } = Keys.E;

        //public static Keys Enter { get; set; } = Keys.Enter;

        //public static Keys ResetLevel { get; set; } = Keys.R;
        //public static Keys ResetMenu { get; set; } = Keys.Q;
        //public static Keys Pause { get; set; } = Keys.P;

        //public static Keys[] MessageNext { get; } = new[] { Jump, Enter };
        //public static Keys MessagePrev { get; } = Attack;

        public static bool IsAnyInputPressed() => input != null && input.IsAnyInputPressed();
    }
}
