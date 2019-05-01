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
using Platformer.Objects;
using Platformer.Main;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer.Objects.Main
{ 
    public static class PlayerExtensions
    {
        public static Direction Reverse(this Direction dir)
        {
            return (Direction)(-(int)dir);
        }
    }
    
    public class PlayerStats
    {
        public int MaxHP { get; set; } = 0;
        public int MaxMP { get; set; } = 0;
        public float MPRegen { get; set; } = 0;
    }

    [Flags]
    public enum PlayerAbility
    {
        NONE = 0,
        SWIM = 1,
        CLIMB_WALL = 2,
        CLIMB_CEIL = 4,
        LEVITATE = 8,
        //ideas:
        //WARP
        //STOMP
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
            DEAD,
            LEVITATE,
            PUSH
        }

        public PlayerState State { get; set; }

        // stats 

        private PlayerStats stats;
        public PlayerStats Stats { get => stats;
            set
            {
                stats = value;
                HP = stats.MaxHP;
                MP = stats.MaxMP;
            }
        }
        public int HP { get; set; }
        public float MP { get; set; }
        //public float MPRegen { get; set; }

        public PlayerAbility Abilities;

        // private

        public Direction Direction { get; set; } = Direction.RIGHT;
        private bool animationComplete;

        private bool onGround;
        private float lastGroundY;
        private float lastGroundYbeforeWall;

        private bool hit = false;
        public int InvincibleTimer { get; private set; } = 0;

        private int mpRegenTimeout = 0;
        private int maxMpRegenTimeout = 60;

        Input input = new Input();

        private float gravAir = .1f;
        private float gravWater = .03f;

        private float levitationSine = 0f;

        private PlayerLevitationEmitter levitationEmitter;

        private PushBlock pushBlock;

        // constructor
        
        public Player(float x, float y) : base(x, y)
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

            Stats = new PlayerStats();

            levitationEmitter = new PlayerLevitationEmitter(x, y);
            levitationEmitter.Parent = this;
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

        public void Hit(int hitPoints, float? angle = null)
        {
            HP = Math.Max(HP - hitPoints, 0);

            var dmgFont = new DamageFont(X, Y - Globals.TILE, $"-{hitPoints}");
            dmgFont.Target = this;

            if (State == PlayerState.IDLE || State == PlayerState.WALK || State == PlayerState.GET_UP)
                State = PlayerState.JUMP_UP;

            if (State != PlayerState.HIT_GROUND)
            {
                if (angle == null)
                {
                    XVel = -.7f * Math.Sign((int)Direction);
                    YVel = Math.Min(YVel - .5f, -1.2f);
                }
                else
                {
                    var ldx = MathUtil.LengthDirX((float)angle) * 1.5f;
                    var ldy = MathUtil.LengthDirY((float)angle) * 1.5f;

                    XVel = (float)ldx;
                    YVel = (float)ldy;
                }
            }
            hit = true;
            InvincibleTimer = 60;
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
            if (input.IsKeyPressed(Keys.H, Input.State.Pressed))
            {
                Hit(1);
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

            var tk_leftPressed = k_leftPressed && (k_rightPressed == false);
            var tk_rightPressed = k_rightPressed && (k_leftPressed == false);
            var tk_leftHolding = k_leftHolding && (k_rightHolding == false);
            var tk_rightHolding = k_rightHolding && (k_leftHolding == false);

            k_leftPressed = tk_leftPressed;
            k_rightPressed = tk_rightPressed;
            k_leftHolding = tk_leftHolding;
            k_rightHolding = tk_rightHolding;

            // ++++ getting hit ++++

            InvincibleTimer = Math.Max(InvincibleTimer - 1, 0);

            if (InvincibleTimer == 0)
            {
                var obstacle = ObjectManager.CollisionBounds<Obstacle>(this, X, Y).FirstOrDefault();

                if (obstacle != null)
                {
                    var vec = Position - (obstacle.Position + new Vector2(8, 12));

                    var angle = vec.ToAngle();
                    
                    Hit(obstacle.Damage, angle);
                }                
            }

            if (HP == 0)
            {
                State = PlayerState.DEAD;
            }

            // ++++ collision flags ++++

            var currentRoom = (MainGame.Current.Camera as RoomCamera)?.CurrentRoom;
            
            var onWall = !hit && ObjectManager.CollisionPoint<Solid>(this, X + (.5f * BoundingBox.Width + 1) * Math.Sign((int)Direction), Y + 4)
                            .Where(o => o.Room == currentRoom).Count() > 0;
            var onCeil = !hit && ObjectManager.CollisionPoint<Solid>(this, X, Y - BoundingBox.Height * .5f - 1)
                .Where(o => o.Room == currentRoom && !(o is PushBlock)).Count() > 0;

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
                if ((State == PlayerState.JUMP_UP || State == PlayerState.WALL_CLIMB)// || State == PlayerState.LEVITATE) 
                    && 
                    (k_jumpHolding || k_upHolding)
                    && !k_downHolding)
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

            // ++++ magic regen ++++

            mpRegenTimeout = Math.Max(mpRegenTimeout - 1, 0);

            if (mpRegenTimeout == 0)
                MP = Math.Min(MP + Stats.MPRegen, Stats.MaxMP);

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
                var colSide = this.CollisionBounds<Solid>(X + Math.Sign((int)Direction), Y).FirstOrDefault();

                if (k_rightHolding)
                {
                    if (Direction == Direction.RIGHT)
                    {
                        XVel = Math.Min(XVel + .2f, maxVel);

                        if (colSide != null)
                            State = PlayerState.PUSH;
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

                        if (colSide != null)
                            State = PlayerState.PUSH;
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
            if (State == PlayerState.IDLE 
                || State ==  PlayerState.WALK
                || State == PlayerState.GET_UP 
                || State == PlayerState.TURN_AROUND 
                || State == PlayerState.PUSH)
            {
                if (YVel > 0 && !onGround)
                    State = PlayerState.JUMP_DOWN;

                if (k_jumpPressed)
                {
                    State = PlayerState.JUMP_UP;
                    YVel = -2;
                    k_jumpPressed = false;
                }
            }
            // pushing
            if (State == PlayerState.PUSH)
            {
                if (pushBlock == null)
                {
                    pushBlock = this.CollisionBounds<PushBlock>(X + Math.Sign((int)Direction), Y).FirstOrDefault();

                    if (pushBlock != null && !pushBlock.IsPushing && !pushBlock.IsFalling)
                    {
                        var canBePushed = pushBlock.Push(Direction);
                        if (!canBePushed)
                            pushBlock = null;
                    }
                    
                }

                if(pushBlock != null)
                {
                    XVel = Math.Sign((int)Direction) * pushBlock.PushVel;

                    if (!pushBlock.IsPushing)
                    {
                        pushBlock = null;
                        if (!k_leftHolding && !k_rightHolding)
                            State = PlayerState.IDLE;
                    }
                }
                if (((Direction == Direction.LEFT && !k_leftHolding) 
                    || (Direction == Direction.RIGHT && !k_rightHolding))
                    && pushBlock == null)
                    State = PlayerState.IDLE;
                
                if (hit)
                    State = PlayerState.HIT_GROUND;
            }
            // levitating
            if (State == PlayerState.LEVITATE)
            {
                mpRegenTimeout = Math.Min(mpRegenTimeout + 2, maxMpRegenTimeout);
                MP = Math.Max(MP - 1, 0);

                lastGroundY = Y;

                var acc = 0.04f;
                var leviMaxVel = 1f;

                if (k_leftHolding)
                {
                    XVel = Math.Max(XVel - acc, -leviMaxVel);
                    Direction = Direction.LEFT;
                }
                else if (k_rightHolding)
                {
                    XVel = Math.Min(XVel + acc, leviMaxVel);
                    Direction = Direction.RIGHT;
                }
                else
                {
                    XVel = Math.Sign(XVel) * Math.Max(Math.Abs(XVel) - .02f, 0);
                }

                if (k_upHolding)
                {
                    YVel = Math.Max(YVel - acc - Gravity, -leviMaxVel);
                    levitationSine = (float)Math.PI;
                }
                else if (k_downHolding)
                {
                    YVel = Math.Min(YVel + acc - Gravity, leviMaxVel);
                    levitationSine = (float)Math.PI;
                }
                else
                {
                    levitationSine = (float)((levitationSine + .1f) % (2f * Math.PI));
                    YVel = -Gravity + (float)Math.Sin(levitationSine) * .1f;
                }
                
                if (!k_jumpHolding || MP == 0)
                    State = PlayerState.JUMP_DOWN;

                if (hit)
                    State = PlayerState.HIT_AIR;
                
                levitationEmitter.Active = true;
            } else
            {
                levitationEmitter.Active = false;
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

                if (k_jumpPressed)
                    State = PlayerState.LEVITATE;
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

                if (hit)
                {
                    XVel = -Math.Sign((int)Direction) * .5f;
                    YVel = -1f;
                    State = PlayerState.HIT_AIR;
                }
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
                if (hit)
                {
                    XVel = -Math.Sign((int)Direction) * .5f;
                    State = PlayerState.HIT_AIR;
                }
            }
            if (State == PlayerState.SWIM)
            {
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
            if (State == PlayerState.DEAD)
            {
                XVel = 0;
                YVel = 0;
                Visible = false;
            }

            var saveStatue = this.CollisionBounds<SaveStatue>(X, Y).FirstOrDefault();

            if(saveStatue != null)
            {
                saveStatue.Save();
            }

            // reset hit after state-logic
            hit = false;

            // ++++ collision & movement ++++

            YVel += Gravity;
            YVel = Math.Sign(YVel) * Math.Min(Math.Abs(YVel), 4);

            var colY = this.CollisionBounds<Collider>(X, Y + YVel).Where(o => o is Solid).ToList();
            
            var platform = this.CollisionBounds<Platform>(X, Y + YVel).FirstOrDefault();

            if (platform != null)
            {
                if (Bottom <= platform.Top)
                {
                    if (YVel >= 0)
                    {
                        colY.Clear();
                        colY.Add(platform);
                    }
                }
            }

            if (colY.Count == 0)
            {
                Move(0, YVel);                
            }
            else
            {

                if (YVel >= Gravity)
                {
                    onGround = true;

                    // transition from falling to getting up again
                    if (State == PlayerState.JUMP_UP || State == PlayerState.JUMP_DOWN || State == PlayerState.WALL_CLIMB || State == PlayerState.LEVITATE)
                    {
                        if (lastGroundY < Y - Globals.TILE)
                            State = PlayerState.GET_UP;
                        else
                            State = PlayerState.IDLE;
                    }

                    // trick to "snap" to the bottom:                    
                    var overlap = Bottom - colY.FirstOrDefault().Top;
                    if (Math.Abs(overlap) <= Math.Abs(YVel))
                        Move(0, -overlap - Gravity);

                    // deprecated: solved issue with a loop:

                    /*while (true)
                    {
                        var cy = this.CollisionBounds<Collider>(X, Y + .01f).FirstOrDefault();
                        Move(0, .01f);
                        if (cy != null)
                        {
                            Move(0, -.01f);
                            break;
                        }
                    }*/
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
            levitationEmitter.Position = Position;

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
                case PlayerState.LEVITATE:
                    row = 12;
                    fAmount = 1;
                    fSpd = 0;
                    break;
                case PlayerState.PUSH:
                    row = 13;
                    fAmount = 2;
                    fSpd = .075f;
                    break;
            }

            SetAnimation(cols * row + offset, cols * row + offset + fAmount - 1, fSpd, loopAnim);

            Color = (InvincibleTimer % 4 > 2) ? Color.Transparent : Color.White;
        }
        
        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);
            
            animationComplete = false;
            
            var xScale = Math.Sign((int)Direction);
            Scale = new Vector2(xScale, 1);
        }
    }
}