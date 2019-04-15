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
    public static class PlayerExtensions
    {
        public static void Reverse(this Player.Direction dir)
        {
            dir = (Player.Direction)(-(int)dir);
        }
    }

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
            DIE,
            TURN_AROUND
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

        private Direction dir = Direction.RIGHT;
        float xScale = 1f;

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
            
            // state logic

            var maxVel = 1.2f;
            
            switch(State)
            {
                case PlayerState.IDLE:
                    XVel = 0;
                    if (k_rightHolding)
                    {
                        State = PlayerState.WALK;
                        dir = Direction.RIGHT;
                    }
                    if (k_leftHolding)
                    {
                        State = PlayerState.WALK;
                        dir = Direction.LEFT;
                    }
                    break;
                case PlayerState.WALK:
                    if (k_rightHolding)
                    {
                        if (dir == Direction.RIGHT)
                        {                            
                            XVel = Math.Min(XVel + .2f, maxVel);                            
                        }
                        else
                        {
                            //dir.Reverse();
                            State = PlayerState.TURN_AROUND;
                        }
                    }
                    if (k_leftHolding)
                    {
                        if (dir == Direction.LEFT)
                        {
                            XVel = Math.Max(XVel - .2f, -maxVel);
                        }
                        else
                        {
                            //dir.Reverse();
                            State = PlayerState.TURN_AROUND;
                        }
                    }
                    if (!k_leftHolding && !k_rightHolding)
                    {                        
                        XVel = Math.Sign(XVel) * Math.Max(Math.Abs(XVel) - .1f, 0);
                        if (XVel == 0)
                        {                            
                            State = PlayerState.IDLE;
                        }
                    }
                    break;
                case PlayerState.TURN_AROUND:
                    
                    XVel = Math.Sign(XVel) * Math.Max(Math.Abs(XVel) - .07f, 0);
                    
                    if (XVel > 0) dir = Direction.LEFT;
                    if (XVel < 0) dir = Direction.RIGHT;
                    
                    if (XVel == 0)
                        State = PlayerState.IDLE;
                    
                    break;
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

            var cols = 8; // how many columns there are in the sheet
            var row = 0; // which row in the sheet
            var fSpd = 0f; // frame speed
            var fAmount = 4; // how many frames
            var loopAnim = true; // loop animation?

            switch (State)
            {
                case PlayerState.IDLE:                    
                    row = 0;
                    fSpd = 0.03f;
                    fAmount = 4;
                    loopAnim = true;
                    break;
                case PlayerState.WALK:                    
                    row = 1;
                    fAmount = 4;
                    fSpd = 0.1f;
                    loopAnim = true;
                    break;
                case PlayerState.TURN_AROUND:                    
                    row = 7;
                    fAmount = 2;
                    fSpd = 0.1f;
                    loopAnim = false;
                    break;
            }

            SetAnimation(cols * row, cols * row + fAmount - 1, fSpd, loopAnim);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            // scaling effect
            /*xScale = Math.Sign((int)dir);
            float s = 0;

            if (dir == Direction.RIGHT)
                s = Math.Min(Scale.X + .3f, 1);
            else
                s = Math.Max(Scale.X - .3f, -1);
            Scale = new Vector2(s, 1);*/
            xScale = Math.Sign((int)dir);
            Scale = new Vector2(xScale, 1);
        }
    }
}