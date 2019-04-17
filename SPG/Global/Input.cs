﻿using Microsoft.Xna.Framework;
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

        public enum Direction
        {
            NONE,
            LEFT,
            RIGHT,
            UP,
            DOWN
        }

        public enum Stick
        {
            LeftStick = 0,
            RightStick = 1
        }

        public bool GamePadEnabled { get { return _gamePadState != null && _gamePadState.IsConnected; } }

        private KeyboardState _keyboardState;
        private KeyboardState _lastKeyboardState;

        private GamePadState _gamePadState;
        private GamePadState _lastGamePadState;

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

        public Input()
        {
            _keyboardState = new KeyboardState();
            _lastKeyboardState = new KeyboardState();

            _gamePadState = new GamePadState();
            _lastGamePadState = new GamePadState();

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
        }

        public Vector2 LeftStick()
        {
            if (!GamePadEnabled) return Vector2.Zero;

            return _gamePadState.ThumbSticks.Left;
        }

        public Vector2 RightStick()
        {
            if (!GamePadEnabled) return Vector2.Zero;

            return _gamePadState.ThumbSticks.Right;
        }
        
        public bool DirectionPressedFromStick(Direction direction, Stick stick, State keyState = State.Pressed)
        {
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
                //return _keyboardState.IsKeyDown(key) && !_lastKeyboardState.IsKeyDown(key);
                case State.Released:
                    switch (direction)
                    {
                        case Direction.LEFT: return leftReleased;
                        case Direction.RIGHT: return rightReleased;
                        case Direction.UP: return upReleased;
                        case Direction.DOWN: return downReleased;
                        default: return false;
                    }
                //return !_keyboardState.IsKeyDown(key) && _lastKeyboardState.IsKeyDown(key);
                case State.Holding:
                    switch (direction)
                    {
                        case Direction.LEFT: return leftHolding;
                        case Direction.RIGHT: return rightHolding;
                        case Direction.UP: return upHolding;
                        case Direction.DOWN: return downHolding;
                        default: return false;
                    }
                //return _keyboardState.IsKeyDown(key) && _lastKeyboardState.IsKeyDown(key);
                default: return false;
            }
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

        public bool IsButtonPressed(Buttons key, State keyState = State.Pressed)
        {
            if (!GamePadEnabled)
                return false;

            switch (keyState)
            {
                case State.Pressed:
                    return _gamePadState.IsButtonDown(key) && !_lastGamePadState.IsButtonDown(key);
                case State.Released:
                    return !_gamePadState.IsButtonDown(key) && _lastGamePadState.IsButtonDown(key);
                case State.Holding:
                    return _gamePadState.IsButtonDown(key) && _lastGamePadState.IsButtonDown(key);
                default: return false;
            }
        }

        public void Update(GameTime gameTime)
        {
            _lastKeyboardState = _keyboardState;
            _keyboardState = Keyboard.GetState();

            _lastGamePadState = _gamePadState;
            _gamePadState = GamePad.GetState(0);

            if (GamePadEnabled)
            {
                _stickVector[0] = _gamePadState.ThumbSticks.Left;
                _stickVector[1] = _gamePadState.ThumbSticks.Right;
                
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