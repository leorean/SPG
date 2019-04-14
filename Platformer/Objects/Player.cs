using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SPG.Objects;
using SPG.Util;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using SPG;

namespace Platformer
{
    public class Player : GameObject
    {

        // public

        public enum PlayerState
        {
            IDLE,
            WALK,
            JUMP_UP,
            JUMP_DOWN,
            WALL_IDLE,
            WALL_CLIMB,
            OBTAIN,
            DIE
        }

        public PlayerState State { get; set; }

        // flags are: shooting, hurt & invincible, 
        /*
         * shooting: angle + projectile
         * 
         * shooting can be done during: idle, walk, jump, climb
         * 
         */

        public enum Direction
        {
            LEFT = -1,
            RIGHT = 1,
            UP = -2,
            DOWN = 2
        }

        // private

        private Direction _dir = Direction.RIGHT;
        float _xScale = 1f;

        Input input = new Input();

        // constructor
        
        public Player(int x, int y)
        {
            Name = "Player";
            Position = new Vector2(x, y);
            
            DrawOffset = new Vector2(8, 24);
            BoundingBox = new RectF(-4, -4, 8, 12);
            Depth = Globals.LAYER_FG + 0.0010f;
            State = PlayerState.IDLE;
            Gravity = .1f;
        }

        // methods

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Input

            input.Update(gameTime);

            var k_leftHolding = input.IsKeyPressed(Keys.Left, Input.State.Holding);
            var k_rightHolding = input.IsKeyPressed(Keys.Right, Input.State.Holding);
            var k_upPressed = input.IsKeyPressed(Keys.Up, Input.State.Pressed);
            
            if (k_upPressed)
            {
                YVel = -2f;
                State = PlayerState.JUMP_UP;
            }
            if (k_leftHolding)
            {
                _dir = Direction.LEFT;
                XVel = -1f;
                State = PlayerState.WALK;
            }
            if (k_rightHolding)
            {
                _dir = Direction.RIGHT;
                XVel = 1f;
                State = PlayerState.WALK;
            }
            
            // state logic

            switch(State)
            {

            }

            // collision & movement

            YVel += Gravity;

            var colY = ObjectManager.Find(this, X, Y + YVel, typeof(Solid));
            if (colY.Count == 0)
            {
                Move(0, YVel);
            }
            else
            {
                if (YVel > Gravity)
                {
                    State = PlayerState.IDLE;

                    var b = colY.Min(x => x.Y);
                    var bottom = colY.Where(o => o.Y == b).First();

                    var newY = bottom.Y + BoundingBox.Y + BoundingBox.Height - bottom.BoundingBox.Height - Gravity;

                    Position = new Vector2(X, newY);
                } else
                {
                    
                }
                YVel = 0;
            }
            

            var colX = ObjectManager.Find(this, X + XVel, Y, typeof(Solid));

            if (colX.Count == 0)
            {
                Move(XVel, 0);
            } else
            {
                XVel = 0;
            }

            // draw <-> state logic

            switch (State)
            {
                case PlayerState.IDLE:
                    SetAnimation(0, 3, 0.03, true);
                    break;
                case PlayerState.WALK:
                    SetAnimation(8, 11, 0.1, true);
                    break;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            // scaling effect
            _xScale = Math.Sign((int)_dir);
            float s = 0;

            if (_dir == Direction.RIGHT)
                s = Math.Min(Scale.X + .3f, 1);
            else
                s = Math.Max(Scale.X - .3f, -1);
            Scale = new Vector2(s, 1);
        }
    }
}