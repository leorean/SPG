using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SPG.Map;
using SPG.Objects;
using System;

namespace SPG
{
    public struct Input
    {
        public (InputManager.Direction, InputManager.Stick)? direction;        
        public Keys? key;
        public Buttons? button;
    }

    public class InputManager
    {
        public enum State
        {
            Pressed, Released, Holding
        }

        public enum Direction
        {
            NONE, LEFT, RIGHT, UP, DOWN
        }

        public enum Stick
        {
            LeftStick = 0,
            RightStick = 1
        }

        public bool GamePadEnabled { get { return _gamePadState != null && _gamePadState.Value.IsConnected; } }

        private KeyboardState _keyboardState;
        private KeyboardState _lastKeyboardState;

        private GamePadState? _gamePadState;
        private GamePadState? _lastGamePadState;

        private Vector2[] _stickVector;
        private Vector2[] _lastStickVector;

        private bool[] left;
        private bool[] right;
        private bool[] up;
        private bool[] down;

        private bool[] lastLeft;
        private bool[] lastRight;
        private bool[] lastUp;
        private bool[] lastDown;

        public bool Enabled { get; set; }

        public bool PreferGamePad { get; set; }

        private int gpIndex;

        public InputManager()
        {
            _keyboardState = new KeyboardState();
            _lastKeyboardState = new KeyboardState();
            
            _stickVector = new[] { Vector2.Zero, Vector2.Zero };
            _lastStickVector = new[] { Vector2.Zero, Vector2.Zero };

            left = new[] { false, false };
            right = new[] { false, false };
            up = new[] { false, false };
            down = new[] { false, false };

            lastLeft = new[] { false, false };
            lastRight = new[] { false, false };
            lastUp = new[] { false, false };
            lastDown = new[] { false, false };

            Enabled = true;            
        }

        public Vector2 LeftStick()
        {
            if (!Enabled) return Vector2.Zero;
            if (!GamePadEnabled || IsAnyKeyPressed()) return Vector2.One;

            return _gamePadState.Value.ThumbSticks.Left;
        }

        public Vector2 RightStick()
        {
            if (!Enabled) return Vector2.Zero;
            if (!GamePadEnabled || IsAnyKeyPressed()) return Vector2.One;

            return _gamePadState.Value.ThumbSticks.Right;
        }
        
        public bool DirectionPressedFromStick(Direction direction, Stick stick, State keyState = State.Pressed)
        {
            if (!Enabled) return false;

            var i = (int)stick;
            
            var leftPressed = left[i] && !lastLeft[i];
            var rightPressed = right[i] && !lastRight[i];
            var upPressed = up[i] && !lastUp[i];
            var downPressed = down[i] && !lastDown[i];

            var leftHolding = left[i] && lastLeft[i];
            var rightHolding = right[i] && lastRight[i];
            var upHolding = up[i] && lastUp[i];
            var downHolding = down[i] && lastDown[i];

            var leftReleased = !left[i] && lastLeft[i];
            var rightReleased = !right[i] && lastRight[i];
            var upReleased = !up[i] && lastUp[i];
            var downReleased = !down[i] && lastDown[i];

            switch (keyState)
            {
                case State.Pressed:                    
                    switch(direction)
                    {
                        case Direction.LEFT: return leftPressed;
                        case Direction.RIGHT: return rightPressed;
                        case Direction.UP: return upPressed;
                        case Direction.DOWN: return downPressed;
                        default: return false;
                    }
                case State.Released:
                    switch (direction)
                    {
                        case Direction.LEFT: return leftReleased;
                        case Direction.RIGHT: return rightReleased;
                        case Direction.UP: return upReleased;
                        case Direction.DOWN: return downReleased;
                        default: return false;
                    }
                case State.Holding:
                    switch (direction)
                    {
                        case Direction.LEFT: return leftHolding;
                        case Direction.RIGHT: return rightHolding;
                        case Direction.UP: return upHolding;
                        case Direction.DOWN: return downHolding;
                        default: return false;
                    }
                default: return false;
            }
        }
        
        public bool IsAnyInputPressed()
        {
            return IsAnyButtonPressed() || IsAnyButtonPressed();
        }

        public bool IsAnyKeyPressed()
        {
            if (!Enabled) return false;

            var defaultState = new KeyboardState();
            return _keyboardState != defaultState;
        }

        public bool IsKeyPressed(Keys key, State keyState = State.Holding)
        {
            if (!Enabled) return false;
            
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

        public bool IsAnyButtonPressed()
        {
            if (!Enabled) return false;

            if (!GamePadEnabled)
                return false;

            var defaultState = new GamePadState();
            return _gamePadState != defaultState;
        }

        public bool IsButtonPressed(Buttons key, State keyState = State.Pressed)
        {
            if (!Enabled) return false;

            if (!GamePadEnabled)
                return false;

            switch (keyState)
            {
                case State.Pressed:
                    return _gamePadState.Value.IsButtonDown(key) && !_lastGamePadState.Value.IsButtonDown(key);
                case State.Released:
                    return !_gamePadState.Value.IsButtonDown(key) && _lastGamePadState.Value.IsButtonDown(key);
                case State.Holding:
                    return _gamePadState.Value.IsButtonDown(key) && _lastGamePadState.Value.IsButtonDown(key);
                default: return false;
            }
        }

        public void Update(GameTime gameTime)
        {
            _lastKeyboardState = _keyboardState;
            _keyboardState = Keyboard.GetState();
            
            gpIndex = 0;

            for(var i = 0; i < GamePad.MaximumGamePadCount; i++)
            {
                if (GamePad.GetState(i).IsConnected)
                {
                    gpIndex = i;
                    break;
                }
            }

            _lastGamePadState = _gamePadState;
            _gamePadState = GamePad.GetState(gpIndex);

            // disconnect if gamepad is lost
            
            if (!GamePad.GetState(gpIndex).IsConnected)
            {
                _gamePadState = null;
                _lastGamePadState = null;
            }
            else
            {
                // reconnect if no gamepad was there, but now it is
                if (_gamePadState == null)
                {
                    _gamePadState = new GamePadState();
                    _lastGamePadState = new GamePadState();
                }
            }
            
            if (GamePadEnabled)
            {
                _stickVector[0] = _gamePadState.Value.ThumbSticks.Left;
                _stickVector[1] = _gamePadState.Value.ThumbSticks.Right;
                
                double thresh = 0.2f;
                for (int i = 0; i < 2; i++)
                {
                    var dir = _stickVector;
                    var lastDir = _lastStickVector;

                    left[i] = dir[i].X < -thresh;
                    right[i] = dir[i].X > thresh;
                    up[i] = -dir[i].Y < -thresh;
                    down[i] = -dir[i].Y > thresh;

                    lastLeft[i] = lastDir[i].X < -thresh;
                    lastRight[i] = lastDir[i].X > thresh;
                    lastUp[i] = -lastDir[i].Y < -thresh;
                    lastDown[i] = -lastDir[i].Y > thresh;
                }

                _lastStickVector[0] = _stickVector[0];
                _lastStickVector[1] = _stickVector[1];
            }
        }
    }
}