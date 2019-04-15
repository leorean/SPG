using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SPG.Map;
using SPG.Objects;
using System;

namespace SPG
{
    public class Input
    {
        public enum State
        {
            Pressed,
            Released,
            Holding
        }

        private KeyboardState _keyboardState;
        private KeyboardState _lastKeyboardState;

        public Input()
        {
            _keyboardState = new KeyboardState();
            _lastKeyboardState = new KeyboardState();
        }

        public bool IsKeyPressed(Keys key, State keyState = State.Pressed)
        {
            switch (keyState)
            {
                case State.Pressed:
                    return _keyboardState.IsKeyDown(key) && !_lastKeyboardState.IsKeyDown(key);
                case State.Released:
                    return !_keyboardState.IsKeyDown(key) && _lastKeyboardState.IsKeyDown(key);
                case State.Holding:
                    return _keyboardState.IsKeyDown(key) && _lastKeyboardState.IsKeyDown(key);
                default: return false;
            }
        }

        public void Update(GameTime gameTime)
        {
            _lastKeyboardState = _keyboardState;
            _keyboardState = Keyboard.GetState();
        }


    }
}