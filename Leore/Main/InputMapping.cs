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
        private static Input input => MainGame.Current.Input;
        public static bool KeyPressed(Keys k, Input.State state = Input.State.Pressed) => KeyPressed(new[] { k }, state);
        public static bool KeyPressed(Keys[] keys, Input.State state = Input.State.Pressed)
        {
            if (input == null) return false;

            foreach (var k in keys) {
                if (input.IsKeyPressed(k, state))
                    return true;
            }            
            return false;
        }

        public static Keys Left { get; set; } = Keys.Left;
        public static Keys Right { get; set; } = Keys.Right;
        public static Keys Up { get; set; } = Keys.Up;
        public static Keys Down { get; set; } = Keys.Down;
        public static Keys Jump { get; set; } = Keys.A;
        public static Keys Attack { get; set; } = Keys.S;
        public static Keys L { get; set; } = Keys.Q;
        public static Keys R { get; set; } = Keys.E;

        public static Keys Enter { get; set; } = Keys.Enter;

        public static Keys ResetLevel { get; set; } = Keys.R;
        public static Keys ResetMenu { get; set; } = Keys.Q;
        public static Keys Pause { get; set; } = Keys.P;

        public static Keys[] MessageNext { get; } = new[] { Jump, Enter };
        public static Keys MessagePrev { get; } = Attack;


        public static bool IsAnyKeyPressed() => input != null && input.IsAnyKeyPressed();
    }
}
