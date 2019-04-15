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

        [Flags]
        public enum PlayerState
        {
            IDLE = 1 << 0,
            WALK = 1 << 1,
            JUMP_UP = 1 << 2,
            JUMP_DOWN = 1 << 3,
            WALL_IDLE = 1 << 4,
            WALL_CLIMB = 1 << 5,
            OBTAIN = 1 << 6,
            DIE = 1 << 7,
            TURN_AROUND = 1 << 8,
            GET_UP = 1 << 9
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
        private bool animationComplete;

        private bool onGround;
        private float lastGroundY;

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

            AnimationComplete += Player_AnimationComplete;
        }

        private void Player_AnimationComplete(object sender, EventArgs e)
        {
            animationComplete = true;
        }

        // methods

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // ++++ input ++++

            input.Update(gameTime);

            var k_leftHolding = input.IsKeyPressed(Keys.Left, Input.State.Holding);
            var k_rightHolding = input.IsKeyPressed(Keys.Right, Input.State.Holding);
            var k_leftReleased = input.IsKeyPressed(Keys.Left, Input.State.Released);
            var k_rightReleased = input.IsKeyPressed(Keys.Right, Input.State.Released);
            var k_upPressed = input.IsKeyPressed(Keys.Up, Input.State.Pressed);

            // ++++ collision flags ++++

            var onWall = ObjectManager.CollisionPoint(this, X + XVel + (.5f * BoundingBox.Width + 1) * Math.Sign((int)dir), Y, typeof(Solid)).Count > 0;

            if (onWall)
            {
                // transition from jumping to wall performance
                if (State == PlayerState.JUMP_UP || State == PlayerState.JUMP_DOWN)
                {
                    if ((dir == Direction.LEFT && k_leftHolding)
                        ||
                        (dir == Direction.RIGHT && k_rightHolding)
                    )
                        State = PlayerState.WALL_IDLE;
                }
            }

            // ++++ state logic ++++

            var maxVel = 1.2f;

            if (YVel != 0) onGround = false;

            if (onGround)
                lastGroundY = Y;

            // idle
            if (State == PlayerState.IDLE)
            {
                XVel = Math.Sign(XVel) * Math.Max(Math.Abs(XVel) - .2f, 0f);
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
            }
            // walk
            if (State == PlayerState.WALK)
            {
                if (k_rightHolding)
                {
                    if (dir == Direction.RIGHT)
                    {
                        XVel = Math.Min(XVel + .2f, maxVel);
                    }
                    else
                    {
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
                        State = PlayerState.TURN_AROUND;
                    }
                }
                if (!k_leftHolding && !k_rightHolding)
                {
                    XVel = Math.Sign(XVel) * Math.Max(Math.Abs(XVel) - .2f, 0);
                    if (XVel == 0)
                    {
                        State = PlayerState.IDLE;
                    }
                }
            }
            // walk/idle -> 
            if (State == PlayerState.IDLE || State ==  PlayerState.WALK)
            {
                if (k_upPressed)
                {
                    State = PlayerState.JUMP_UP;
                    YVel = -2;
                }
            }
            // jumping
            if (State == PlayerState.JUMP_UP  || State == PlayerState.JUMP_DOWN)
            {
                if (YVel < 0) State = PlayerState.JUMP_UP;
                if (YVel > 0) State = PlayerState.JUMP_DOWN;

                //if (XVel < -1) dir = Direction.LEFT;
                //if (XVel > 1) dir = Direction.RIGHT;                
                
                if (k_leftHolding)
                {
                    if (XVel < .5)
                        dir = Direction.LEFT;
                    XVel = Math.Max(XVel - .08f, -maxVel);
                }
                if (k_rightHolding)
                {
                    if (XVel > -.5)
                        dir = Direction.RIGHT;
                    XVel = Math.Min(XVel + .08f, maxVel);                    
                }                
            }
            // getting back up
            if (State == PlayerState.GET_UP)
            {
                XVel = Math.Sign(XVel) * Math.Max(Math.Abs(XVel) - .2f, 0f);
                if (animationComplete)
                {
                    State = PlayerState.IDLE;
                }
            }
            // turning around
            if (State == PlayerState.TURN_AROUND)
            {
                XVel = Math.Sign(XVel) * Math.Max(Math.Abs(XVel) - .1f, 0);

                if (XVel > 0) dir = Direction.LEFT;
                if (XVel < 0) dir = Direction.RIGHT;

                if (XVel == 0)
                    State = PlayerState.IDLE;
            }
            // wall performance
            if (State == PlayerState.WALL_IDLE)
            {
                XVel = 0;
                YVel = -Gravity;

                if (dir == Direction.LEFT)
                {
                    if (k_leftReleased)
                        State = PlayerState.JUMP_DOWN;
                }
                if (dir == Direction.RIGHT)
                {
                    if (k_rightReleased)
                        State = PlayerState.JUMP_DOWN;
                }
            }
            
            // ++++ collision & movement ++++

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
                    onGround = true;

                    // transition from falling to getting up again
                    if (State.HasFlag(PlayerState.JUMP_UP & PlayerState.JUMP_DOWN))
                    {
                        if (lastGroundY < Y)
                            State = PlayerState.GET_UP;
                        else
                            State = PlayerState.IDLE;
                    }
                    
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

            // ++++ draw <-> state logic ++++

            var cols = 8; // how many columns there are in the sheet
            var row = 0; // which row in the sheet
            var fSpd = 0f; // frame speed
            var fAmount = 4; // how many frames
            var loopAnim = true; // loop animation?
            var offset = 0; // offset within same row

            switch (State)
            {
                case PlayerState.IDLE:                    
                    row = 0;
                    fSpd = 0.03f;
                    fAmount = 4;
                    break;
                case PlayerState.WALK:                    
                    row = 1;
                    fAmount = 4;
                    fSpd = 0.1f;
                    break;
                case PlayerState.TURN_AROUND:                    
                    row = 7;
                    fAmount = 2;
                    fSpd = 0.15f;
                    loopAnim = false;
                    break;
                case PlayerState.JUMP_UP:
                    row = 2;
                    fAmount = 2;
                    fSpd = 0.2f;
                    break;
                case PlayerState.JUMP_DOWN:
                    row = 2;
                    offset = 2;
                    fAmount = 2;
                    fSpd = 0.2f;
                    break;
                case PlayerState.GET_UP:
                    row = 8;
                    fAmount = 3;
                    fSpd = 0.15f;
                    loopAnim = false;
                    break;
                case PlayerState.WALL_IDLE:
                    row = 3;
                    fAmount = 4;
                    fSpd = 0.1f;                    
                    break;

            }

            SetAnimation(cols * row + offset, cols * row + offset + fAmount - 1, fSpd, loopAnim);
        }
        
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            animationComplete = false;

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