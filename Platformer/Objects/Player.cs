using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SPG.Objects;
using SPG.Util;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using SPG;
using Platformer.Objects.Enemy;
using Platformer.Objects.Effects;

namespace Platformer
{ 
    public static class PlayerExtensions
    {
        public static Direction Reverse(this Direction dir)
        {
            return (Direction)(-(int)dir);
        }
    }
    
    public enum Direction
    {
        LEFT = -1,
        RIGHT = 1,
        UP = -2,
        DOWN = 2
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
            TURN_AROUND,
            GET_UP,
            HIT_AIR,
            HIT_GROUND,
            CEIL_IDLE,
            CEIL_CLIMB,
            SWIM,
            DEAD             
        }

        public PlayerState State { get; set; }

        public int HP;

        // private

        public Direction Direction { get; set; } = Direction.RIGHT;
        private bool animationComplete;

        private bool onGround;
        private float lastGroundY;
        private float lastGroundYbeforeWall;

        private bool hit = false;
        private int invincible = 0;
        
        Input input = new Input();

        private float gravAir = .1f;
        private float gravWater = .03f;

        // constructor
        
        public Player(float x, float y)
        {
            Name = "Player";
            Position = new Vector2(x, y);
            
            DrawOffset = new Vector2(8, 24);
            BoundingBox = new RectF(-4, -4, 8, 12);
            Depth = Globals.LAYER_FG + 0.0010f;
            State = PlayerState.IDLE;
            Gravity = gravAir;

            AnimationComplete += Player_AnimationComplete;

            lastGroundY = Y;

            // stats:

            HP = 30;
        }

        ~Player()
        {
            AnimationComplete -= Player_AnimationComplete;            
        }

        private void Player_AnimationComplete(object sender, EventArgs e)
        {
            animationComplete = true;
        }

        // methods

        public void Hit(int hitPoints)
        {
            hit = true;
            HP = Math.Max(HP - hitPoints, 0);

            var dmgFont = new DamageFont(X, Y, $"-{hitPoints}");
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // ++++ input ++++

            input.Update(gameTime);
            
            var k_leftPressed = input.IsKeyPressed(Keys.Left, Input.State.Pressed);
            var k_leftHolding = input.IsKeyPressed(Keys.Left, Input.State.Holding);
            var k_leftReleased = input.IsKeyPressed(Keys.Left, Input.State.Released);

            var k_rightPressed = input.IsKeyPressed(Keys.Right, Input.State.Pressed);
            var k_rightHolding = input.IsKeyPressed(Keys.Right, Input.State.Holding);
            var k_rightReleased = input.IsKeyPressed(Keys.Right, Input.State.Released);

            var k_upPressed = input.IsKeyPressed(Keys.Up, Input.State.Pressed);
            var k_upHolding = input.IsKeyPressed(Keys.Up, Input.State.Holding);

            var k_downPressed = input.IsKeyPressed(Keys.Down, Input.State.Pressed);
            var k_downHolding = input.IsKeyPressed(Keys.Down, Input.State.Holding);

            var k_jumpPressed = input.IsKeyPressed(Keys.A, Input.State.Pressed);
            var k_jumpHolding = input.IsKeyPressed(Keys.A, Input.State.Holding);

            // debug keys

            if (input.IsKeyPressed(Keys.LeftShift, Input.State.Holding) || input.IsKeyPressed(Keys.RightShift, Input.State.Holding))
            {
                if (k_leftPressed)
                    Position = new Vector2(Position.X - 16 * Globals.TILE, Position.Y);
                if (k_rightPressed)
                    Position = new Vector2(Position.X + 16 * Globals.TILE, Position.Y);
                if (k_upPressed)
                    Position = new Vector2(Position.X, Position.Y - 9 * Globals.TILE);
                if (k_downPressed)
                    Position = new Vector2(Position.X, Position.Y + 9 * Globals.TILE);

            }

            // gamepad overrides keyboard input if pussible
            if (input.GamePadEnabled)
            {

                k_leftPressed = input.DirectionPressedFromStick(Input.Direction.LEFT, Input.Stick.LeftStick, Input.State.Pressed);
                k_leftHolding = input.DirectionPressedFromStick(Input.Direction.LEFT, Input.Stick.LeftStick, Input.State.Holding);
                k_leftReleased = input.DirectionPressedFromStick(Input.Direction.LEFT, Input.Stick.LeftStick, Input.State.Released);

                k_rightPressed = input.DirectionPressedFromStick(Input.Direction.RIGHT, Input.Stick.LeftStick, Input.State.Pressed);
                k_rightHolding = input.DirectionPressedFromStick(Input.Direction.RIGHT, Input.Stick.LeftStick, Input.State.Holding);
                k_rightReleased = input.DirectionPressedFromStick(Input.Direction.RIGHT, Input.Stick.LeftStick, Input.State.Released);

                k_upHolding = input.DirectionPressedFromStick(Input.Direction.UP, Input.Stick.LeftStick, Input.State.Holding);
                k_upPressed = input.DirectionPressedFromStick(Input.Direction.UP, Input.Stick.LeftStick, Input.State.Pressed);
                
                k_downHolding = input.DirectionPressedFromStick(Input.Direction.DOWN, Input.Stick.LeftStick, Input.State.Holding);
                k_downPressed = input.DirectionPressedFromStick(Input.Direction.DOWN, Input.Stick.LeftStick, Input.State.Pressed);

                k_jumpPressed = input.IsButtonPressed(Buttons.A, Input.State.Pressed);
                k_jumpHolding = input.IsButtonPressed(Buttons.A, Input.State.Holding);                
            }
            
            if (input.IsKeyPressed(Keys.H, Input.State.Pressed))
            {
                Hit(1);
                
            }

            // ++++ getting hit ++++

            invincible = Math.Max(invincible - 1, 0);

            if (invincible == 0)
            {
                var obstacle = ObjectManager.CollisionBounds<SpikeBottom>(this, X, Y).FirstOrDefault();

                if (obstacle != null)
                {
                    Hit(obstacle.Damage);
                }                
            }
            
            // impulse
            if (hit)
            {
                State = PlayerState.HIT_AIR;
                XVel = -.7f * Math.Sign((int)Direction);
                YVel = -1.5f;
                hit = false;
                invincible = 60;
            }

            if (HP == 0)
            {
                State = PlayerState.DEAD;
            }
            
            // ++++ collision flags ++++

            var onWall = ObjectManager.CollisionPoint<Solid>(this, X + (.5f * BoundingBox.Width + 1) * Math.Sign((int)Direction), Y + 4).Count > 0;
            var onCeil = ObjectManager.CollisionPoint<Solid>(this, X, Y - BoundingBox.Height * .5f - 1).Count > 0;

            int tx = MathUtil.Div(X, Globals.TILE);
            int ty = MathUtil.Div(Y + 4, Globals.TILE);

            var inWater = (GameManager.Game.Map.LayerData[2].Get(tx, ty) != null);

            if (onWall)
            {
                // transition from jumping to wall performance
                if (
                    (State == PlayerState.JUMP_UP && lastGroundY > Y + Globals.TILE)
                    ||
                    State == PlayerState.JUMP_DOWN)
                {
                    if ((Direction == Direction.LEFT && k_leftHolding)
                        ||
                        (Direction == Direction.RIGHT && k_rightHolding)
                    )
                    {
                        lastGroundYbeforeWall = lastGroundY;
                        lastGroundY = Y;
                        State = PlayerState.WALL_IDLE;
                    }
                }
            }
            if (onCeil)
            {
                if ((State == PlayerState.JUMP_UP || State == PlayerState.WALL_CLIMB) 
                    && 
                    (k_jumpHolding || k_upHolding))
                {
                    State = PlayerState.CEIL_IDLE;
                }
            }
            if (inWater)
            {
                Gravity = gravWater;

                if (State != PlayerState.SWIM)
                {                    
                    if (State != PlayerState.HIT_AIR && State != PlayerState.HIT_GROUND)
                    {
                        XVel *= .3f;
                        YVel *= .3f;
                        State = PlayerState.SWIM;

                        //TODO: splash effect
                    }

                    if (State == PlayerState.HIT_GROUND)
                        Gravity = -gravWater;
                }                
            } else
            {
                Gravity = gravAir;

                if (State == PlayerState.SWIM)
                {
                    YVel = -1.3f;
                    State = PlayerState.JUMP_UP;

                    //TODO: splash effect
                }
            }

            // ++++ state logic ++++

            var maxVel = 1.2f;

            if (YVel != 0) onGround = false;

            if (onGround)
            {
                lastGroundY = Y;                
            }

            // idle
            if (State == PlayerState.IDLE)
            {
                XVel = Math.Sign(XVel) * Math.Max(Math.Abs(XVel) - .2f, 0f);
                if (k_rightHolding)
                {
                    State = PlayerState.WALK;
                    Direction = Direction.RIGHT;
                }
                if (k_leftHolding)
                {
                    State = PlayerState.WALK;
                    Direction = Direction.LEFT;
                }
            }
            // walk
            if (State == PlayerState.WALK)
            {
                if (k_rightHolding)
                {
                    if (Direction == Direction.RIGHT)
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
                    if (Direction == Direction.LEFT)
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
            // walk/idle -> jump
            if (State == PlayerState.IDLE || State ==  PlayerState.WALK || State == PlayerState.GET_UP)
            {
                if (YVel > 0 && !onGround)
                    State = PlayerState.JUMP_DOWN;

                if (k_jumpPressed)
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
                
                if (k_leftHolding)
                {
                    if (XVel < .5)
                        Direction = Direction.LEFT;
                    XVel = Math.Max(XVel - .08f, -maxVel);
                }
                if (k_rightHolding)
                {
                    if (XVel > -.5)
                        Direction = Direction.RIGHT;
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

                if (XVel > 0) Direction = Direction.LEFT;
                if (XVel < 0) Direction = Direction.RIGHT;

                if (XVel == 0)
                    State = PlayerState.IDLE;
            }
            // wall
            if (State == PlayerState.WALL_IDLE || State == PlayerState.WALL_CLIMB)
            {
                XVel = 0;
                YVel = -Gravity;

                var wallJumpVel = -2.2f;

                if (Direction == Direction.LEFT)
                {
                    var jumpOff = false;

                    /*if (k_jumpPressed) // jump off
                    {
                        XVel = .5f;
                        YVel = -1;
                        State = PlayerState.JUMP_UP;

                        jumpOff = true;
                    }*/

                    if (k_jumpPressed || k_rightHolding)
                    {
                        if (k_rightHolding)
                            Direction = Direction.RIGHT;
                        XVel = 1;
                        YVel = wallJumpVel;
                        State = PlayerState.JUMP_UP;
                        jumpOff = true;

                    }
                    if (jumpOff)
                    {
                        // switch back the ground Y
                        lastGroundY = Math.Min(lastGroundY, lastGroundYbeforeWall);
                    }
                }
                else if (Direction == Direction.RIGHT)
                {
                    var jumpOff = false;

                    /*if (k_jumpPressed) // jump off
                    {
                        XVel = -.5f;
                        YVel = -1;
                        State = PlayerState.JUMP_UP;

                        jumpOff = true;
                    }*/

                    if (k_jumpPressed || k_leftHolding)
                    {
                        if (k_leftHolding)
                            Direction = Direction.LEFT;
                        XVel = -1;
                        YVel = wallJumpVel;
                        State = PlayerState.JUMP_UP;
                        jumpOff = true;
                    }

                    if (jumpOff)
                    {
                        // switch back the ground Y
                        lastGroundY = Math.Min(lastGroundY, lastGroundYbeforeWall);
                    }
                }
                
                if (k_upHolding || k_downHolding)
                    State = PlayerState.WALL_CLIMB;
            }
            // wall climb
            if (State == PlayerState.WALL_CLIMB)
            {
                if (k_upHolding)
                {
                    YVel = -1;
                }
                else if (k_downHolding)
                {
                    YVel = 1;
                }
                else
                    State = PlayerState.WALL_IDLE;

                if (!onWall)
                {
                    if (k_upHolding)
                    {
                        YVel = -1.5f;
                    }
                    State = PlayerState.JUMP_DOWN;
                }
            }
            // hit -> ground / stand up
            if (State == PlayerState.HIT_AIR)
            {

                if (YVel > 0)
                {
                    if (k_jumpPressed)
                    {
                        YVel = -1;
                        State = PlayerState.JUMP_UP;
                    }
                }

                if (onGround)
                {
                    YVel = -1f;
                    State = PlayerState.HIT_GROUND;
                }
            }
            if (State == PlayerState.HIT_GROUND)
            {
                if (inWater)
                    YVel = 0;

                XVel = Math.Sign(XVel) * Math.Min(Math.Abs(XVel) - .01f, 0);
                if (YVel == 0)
                {
                    if (k_leftHolding || k_rightHolding || k_jumpHolding)
                    {                        
                        State = PlayerState.GET_UP;                        
                    }
                }
            }
            // ceiling climbing
            if (State == PlayerState.CEIL_IDLE)
            {
                YVel = -Gravity;
                XVel = 0;
                if (k_leftPressed || k_rightPressed)
                    State = PlayerState.CEIL_CLIMB;
            }
            if (State == PlayerState.CEIL_CLIMB)
            {
                YVel = -Gravity;
                if (k_leftPressed || k_leftHolding)
                {
                    XVel = -.5f;
                    Direction = Direction.LEFT;
                }
                else if (k_rightPressed || k_rightHolding)
                {
                    XVel = .5f;
                    Direction = Direction.RIGHT;
                }
                else
                {
                    State = PlayerState.CEIL_IDLE;
                }
            }
            if (State == PlayerState.CEIL_IDLE || State == PlayerState.CEIL_CLIMB)
            {
                if (k_downPressed || k_jumpPressed || !onCeil)
                {
                    XVel = 0;
                    YVel = 0;
                    State = PlayerState.JUMP_DOWN;
                }
            }
            if (State == PlayerState.SWIM)
            {

                //Gravity = 0.01f;

                var waterAcc = 0.03f;
                var waterVelMax = 1f;

                if (k_leftHolding)
                {
                    XVel = Math.Max(XVel - waterAcc, -waterVelMax);
                    Direction = Direction.LEFT;
                }
                else if (k_rightHolding)
                {
                    XVel = Math.Min(XVel + waterAcc, waterVelMax);
                    Direction = Direction.RIGHT;
                }
                else
                {
                    XVel = Math.Sign(XVel) * Math.Max(Math.Abs(XVel) - .02f, 0);
                }

                if (k_upHolding || k_jumpHolding)
                {
                    YVel = Math.Max(YVel - waterAcc - Gravity, -waterVelMax);
                }
                else if (k_downHolding)
                {
                    YVel = Math.Min(YVel + waterAcc, waterVelMax);
                }
                else
                {
                    YVel = Math.Sign(YVel) * Math.Max(Math.Abs(YVel) - .02f, 0);
                }
            }
            if (State == PlayerState.HIT_AIR || State == PlayerState.HIT_GROUND)
            {
                // "stops" the invincibility timer
                invincible++;
            }
            if (State == PlayerState.DEAD)
            {
                XVel = 0;
                YVel = 0;
                Visible = false;
            }

            // ++++ collision & movement ++++

            YVel += Gravity;

            var colY = ObjectManager.CollisionBounds<Solid>(this, X, Y + YVel);
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
                    if (State == PlayerState.JUMP_UP || State == PlayerState.JUMP_DOWN || State == PlayerState.WALL_CLIMB)
                    {
                        if (lastGroundY < Y - Globals.TILE)
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
            
            var colX = ObjectManager.CollisionBounds<Solid>(this, X + XVel, Y);
            
            if (colX.Count == 0)
            {
                Move(XVel, 0);
            } else
            {
                XVel = 0;
            }

            // ++++ limit positin within room bounds ++++

            var boundX = Position.X;
            var boundY = Position.Y;

            if (boundX < 4) { XVel = 0; }
            if (boundX > GameManager.Game.Map.Width * Globals.TILE - 4) { XVel = 0; }
            if (boundY < 4) { YVel = 0; }
            if (boundY > GameManager.Game.Map.Height * Globals.TILE - 4) { YVel = 0; }

            boundX = boundX.Clamp(4, GameManager.Game.Map.Width * Globals.TILE - 4);
            boundY = boundY.Clamp(4, GameManager.Game.Map.Height * Globals.TILE - 4);

            Position = new Vector2(boundX, boundY);


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
                case PlayerState.WALL_CLIMB:
                    row = 4;
                    fAmount = 4;
                    fSpd = 0.15f;
                    break;
                case PlayerState.HIT_AIR:
                    row = 9;
                    fAmount = 1;
                    fSpd = 0;
                    break;
                case PlayerState.HIT_GROUND:
                    row = 9;
                    offset = 1;
                    fAmount = 1;
                    fSpd = 0;
                    break;
                case PlayerState.CEIL_IDLE:
                    row = 10;
                    fAmount = 4;
                    fSpd = 0.05f;
                    break;
                case PlayerState.CEIL_CLIMB:
                    row = 11;
                    fAmount = 4;
                    fSpd = 0.15f;
                    break;
                case PlayerState.SWIM:
                    row = 5;
                    fAmount = 4;
                    fSpd = 0.05f;
                    break;
            }

            SetAnimation(cols * row + offset, cols * row + offset + fAmount - 1, fSpd, loopAnim);

            Color = (invincible > 0) ? new Color(255, 255, 255, 128) : Color.White;
        }
        
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            
            animationComplete = false;
            
            var xScale = Math.Sign((int)Direction);
            Scale = new Vector2(xScale, 1);
        }
    }
}