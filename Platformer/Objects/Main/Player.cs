using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SPG.Objects;
using SPG.Util;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using SPG;
using SPG.Draw;
using Platformer.Objects.Enemy;
using Platformer.Objects.Effects;
using Platformer.Objects;
using Platformer.Main;
using Microsoft.Xna.Framework.Graphics;
using Platformer.Objects.Level;
using Platformer.Objects.Effects.Emitters;
using Platformer.Objects.Items;
using Platformer.Util;

namespace Platformer.Objects.Main
{ 
    public static class PlayerExtensions
    {
        public static Direction Reverse(this Direction dir)
        {
            return (Direction)(-(int)dir);
        }
    }
    
    [Serializable]
    public class GameStats
    {
        public int MaxHP { get; set; } = 5;
        public int MaxMP { get; set; } = 100;
        public float MPRegen { get; set; } = 1;
        
        public PlayerAbility Abilities { get; set; } = PlayerAbility.NONE;

        public int Coins { get; set; } = 0;

        // ID, Typename
        public Dictionary<int, string> Items { get; set; } = new Dictionary<int, string>();
    }

    [Flags]
    public enum PlayerAbility
    {
        NONE = 0,
        BREATHE_UNDERWATER = 1,
        CLIMB_WALL = 2,
        CLIMB_CEIL = 4,
        LEVITATE = 8,
        PUSH = 16
        //ideas:
        //WARP
        //STOMP
    }
    
    public class Player : GameObject
    {

        // public
        
        public enum PlayerState
        {
            IDLE, WALK, JUMP_UP, JUMP_DOWN, WALL_IDLE,
            WALL_CLIMB, OBTAIN, TURN_AROUND,
            GET_UP, HIT_AIR, HIT_GROUND, CEIL_IDLE,
            CEIL_CLIMB, SWIM, DEAD, LEVITATE,
            PUSH, LIE, SWIM_DIVE_IN, SWIM_TURN_AROUND,
            DOOR
        }

        public PlayerState State { get; set; }

        // stats 

        //private GameStats stats;
        public GameStats Stats { get => GameManager.Current.SaveGame.gameStats; }
        public int HP { get; set; }
        public float MP { get; set; }
        
        // private

        public Direction Direction { get; set; } = Direction.RIGHT;
        private Direction lastDirection;

        private bool animationComplete;

        private bool onGround;
        public bool OnGround { get => onGround; }

        private float lastGroundY;
        private float lastGroundYbeforeWall;

        private bool hit = false;
        public int InvincibleTimer { get; private set; } = 0;

        private int mpRegenTimeout = 0;
        private int maxMpRegenTimeout = 60;

        Input input = new Input();

        private float gravAir = .1f;
        private float gravWater = .03f;
        
        private float swimAngle;
        private Vector2 swimVector;
        private float targetAngle;
        
        private float levitationSine;
        private PlayerLevitationEmitter levitationEmitter;
        
        private PushBlock pushBlock;

        // diving

        int oxygen;
        int maxOxygen;

        // misc. timers

        private int coinCounter;
        public int CoinCounter {
            get => coinCounter;
            set 
            {
                coinCounter = value;
                coinTimeout = maxCoinTimeout;
            }
        }
        private int coinTimeout;
        private int maxCoinTimeout = 60;
        private Font coinFont;

        private int lieTimer = 0;
        private int ghostTimer = 60;

        private PlayerGhost ghost;

        private MovingPlatform movingPlatform;
        private float movXvel;
        private float movYvel;

        // constructor

        public Player(float x, float y) : base(x, y)
        {
            Name = "Player";
            Position = new Vector2(x, y);
            
            DrawOffset = new Vector2(8, 24);
            BoundingBox = new RectF(-4, -4, 8, 12);
            Depth = Globals.LAYER_PLAYER;
            State = PlayerState.IDLE;
            Gravity = gravAir;

            AnimationComplete += Player_AnimationComplete;

            lastGroundY = Y;

            State = PlayerState.LIE;
            lieTimer = 30;

            coinFont = AssetManager.DamageFont.Copy();
            coinFont.Halign = Font.HorizontalAlignment.Center;
            coinFont.Valign = Font.VerticalAlignment.Top;
            coinFont.Color = new Color(153, 229, 80);

            levitationEmitter = new PlayerLevitationEmitter(x, y, this);

            HP = Stats.MaxHP;
            MP = Stats.MaxMP;

            maxOxygen = 5 * 60;
            oxygen = maxOxygen;

            Debug.WriteLine("Created Player!");
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
            //movingPlatform = null;

            HP = Math.Max(HP - hitPoints, 0);

            var ouch = new OuchEmitter(X, Y);

            var dmgFont = new FollowFont(X, Y - Globals.TILE, $"-{hitPoints}");
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

        // ++++++++++++++++++++++++++
        // UPDATE
        // ++++++++++++++++++++++++++

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // ++++ input ++++

            if (ObjectManager.Exists<MessageBox>() || GameManager.Current.Transition != null)
            {
                input.Enabled = false;
            } else
            {
                input.Enabled = true;
            }

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
                Hit(1);

            if (input.IsKeyPressed(Keys.D9, Input.State.Pressed))
            {
                //stats.Abilities = PlayerAbility.NONE;

                // add: flags |= flag
                // remove: flags &= ~flag
                // toggle: flags ^= flag

                Stats.Abilities |= PlayerAbility.PUSH;
                Stats.Abilities |= PlayerAbility.LEVITATE;
                Stats.Abilities |= PlayerAbility.CLIMB_CEIL;
                Stats.Abilities |= PlayerAbility.CLIMB_WALL;                
            }

            if (input.IsKeyPressed(Keys.O, Input.State.Pressed))
            {
                //var ouch = new OuchEmitter(X, Y);
                //var message = new MessageBox("Hello World!\nHello World!\nHello World!|What is going on here?!\nI have no idea...|Wow.", "Title");
                //var message = new MessageBox("Hello 'World' what 'is' going 'on'!\nHello World!\nHello World!|What is going on here?!\nI have no idea...|Wow.", "Title");
                var flash = new FlashEmitter(X, Y);
            }

            var gamePadLeftXFactor = 1f;
            var gamePadLeftYFactor = 1f;

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

                gamePadLeftXFactor = Math.Abs(input.LeftStick().X);
                gamePadLeftYFactor = Math.Abs(input.LeftStick().Y);
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
            
            if (InvincibleTimer == 0 && HP > 0)
            {
                var obstacle = ObjectManager.CollisionBounds<Obstacle>(this, X, Y).FirstOrDefault();

                if (obstacle != null)
                {
                    var vec = Position - (obstacle.Center + new Vector2(0, 0));
                    var angle = vec.ToAngle();                    
                    Hit(obstacle.Damage, angle);
                }                
            }

            if (HP == 0)
            {
                if (State != PlayerState.HIT_AIR && State != PlayerState.DEAD)
                    State = PlayerState.HIT_AIR;
                InvincibleTimer = 0;
            }

            // ++++ collision flags ++++

            var currentRoom = RoomCamera.Current.CurrentRoom;
            
            var onWall = !hit && ObjectManager.CollisionPoint<Solid>(this, X + (.5f * BoundingBox.Width + 1) * Math.Sign((int)Direction), Y + 4)
                            .Where(o => o.Room == currentRoom).Count() > 0;
            var onCeil = !hit && ObjectManager.CollisionPoint<Solid>(this, X, Y - BoundingBox.Height * .5f - 1)
                .Where(o => o.Room == currentRoom && !(o is PushBlock)).Count() > 0;

            int tx = MathUtil.Div(X, Globals.TILE);
            int ty = MathUtil.Div(Y + 4, Globals.TILE);

            var inWater = (GameManager.Current.Map.LayerData[2].Get(tx, ty) != null);

            if (!Stats.Abilities.HasFlag(PlayerAbility.CLIMB_WALL))
                onWall = false;
            if (!Stats.Abilities.HasFlag(PlayerAbility.CLIMB_CEIL))
                onCeil = false;

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
                    (k_jumpHolding || k_upHolding)
                    && !k_downHolding)
                {
                    State = PlayerState.CEIL_IDLE;
                }
            }
            if (inWater)
            {
                if (!Stats.Abilities.HasFlag(PlayerAbility.BREATHE_UNDERWATER))
                {
                    oxygen = Math.Max(oxygen - 1, 0);

                    if (oxygen == 0)
                    {
                        if (InvincibleTimer == 0 && HP > 0)
                        {
                            Hit(1);
                            
                            // dampen the hit from oxygen deplation:

                            XVel *= .5f;
                            YVel *= .5f;
                        }
                    }
                }

                Gravity = gravWater;

                if (HP > 0)
                {
                    if (State != PlayerState.SWIM && State != PlayerState.SWIM_DIVE_IN && State != PlayerState.SWIM_TURN_AROUND)
                    {
                        if (State != PlayerState.HIT_AIR && State != PlayerState.HIT_GROUND)
                        {
                            State = PlayerState.SWIM_DIVE_IN;
                            YVel = 2;
                            
                            var splash = new WaterSplashEmitter(X, Y, XVel);
                        }

                        if (State == PlayerState.HIT_GROUND)
                            Gravity = -gravWater;
                    }
                } else // 0 hp in water -> dead
                {
                    YVel = Math.Min(YVel, 1.5f);
                    State = PlayerState.DEAD;
                }
            } else
            {
                oxygen = Math.Min(oxygen + 5, maxOxygen);

                Gravity = gravAir;

                if (State == PlayerState.SWIM || State == PlayerState.SWIM_DIVE_IN || State == PlayerState.SWIM_TURN_AROUND)
                {
                    YVel = -1.3f;
                    State = PlayerState.JUMP_UP;
                    
                    var splash = new WaterSplashEmitter(X, Y, XVel);
                }
            }
            
            // ++++ magic regen ++++

            mpRegenTimeout = Math.Max(mpRegenTimeout - 1, 0);

            if (mpRegenTimeout == 0)
                MP = Math.Min(MP + Stats.MPRegen, Stats.MaxMP);

            
            if (HP > 0)
            {
                // ++++ pickup items ++++

                var item = this.CollisionBounds<Item>(X, Y).FirstOrDefault();
                if (item != null)
                {
                    item.Take(this);
                }

                // ++++ doors ++++

                var door = this.CollisionBounds<Door>(X, Y).FirstOrDefault();

                if (door != null)
                {
                    if (k_upPressed)
                    {
                        XVel = 0;
                        YVel = -Gravity;

                        var pos = Position + new Vector2(door.Tx * Globals.TILE, door.Ty * Globals.TILE);
                        RoomCamera.Current.ChangeRoomsFromPosition(pos);

                    }
                }

                // ++++ npcs ++++

                var npc = this.CollisionBounds<NPC>(X, Y).FirstOrDefault();

                if (npc != null)
                {
                    if (k_upPressed)
                    {
                        npc.Interact(this);
                    }
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
                var colSide = this.CollisionBounds<Solid>(X + Math.Sign((int)Direction), Y).FirstOrDefault();

                if (k_rightHolding)
                {
                    if (Direction == Direction.RIGHT)
                    {
                        XVel = Math.Min(XVel + .2f, maxVel * gamePadLeftXFactor);

                        if (colSide != null && Stats.Abilities.HasFlag(PlayerAbility.PUSH))
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
                        XVel = Math.Max(XVel - .2f, -maxVel * gamePadLeftXFactor);

                        if (colSide != null && Stats.Abilities.HasFlag(PlayerAbility.PUSH))
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
                    movingPlatform = null;
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
                
                if (this.CollisionBounds<PushBlock>(X + Math.Sign((int)Direction), Y).FirstOrDefault() == null)
                {
                    State = PlayerState.IDLE;
                    pushBlock = null;
                }

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
                    // prevent down-drifting even though the player already presses up
                    if (YVel > 0) YVel = 0;

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
                    if (Math.Abs(YVel) < 1)
                    {
                        levitationSine = (float)((levitationSine + .1f) % (2f * Math.PI));
                        YVel = -Gravity + (float)Math.Sin(levitationSine) * .1f;
                    }
                    else
                    {
                        YVel -= Gravity;
                        YVel *= .8f;
                    }
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
                
                // mushrooms
                if (YVel > 0)
                {
                    var mush = this.CollisionBounds<Mushroom>(X, Y).FirstOrDefault();
                    if (mush != null && !mush.Bouncing)
                    {
                        lastGroundY = Y;
                        mush.Bounce();
                        YVel = -3.2f;
                    }
                }

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
                if (Stats.Abilities.HasFlag(PlayerAbility.LEVITATE))
                {
                    if (MP > 0) {
                        if (k_jumpPressed)
                            State = PlayerState.LEVITATE;
                    }
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

                if (HP == 0 && onGround)
                    State = PlayerState.DEAD;

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
            // diving-in
            if (State == PlayerState.SWIM_DIVE_IN)
            {
                Gravity = 0;

                XVel *= .9f;
                YVel *= .85f;

                if (Math.Abs(YVel) < .21f)
                {                    
                    State = PlayerState.SWIM;
                }
            }
            // swimming
            if (State == PlayerState.SWIM || State == PlayerState.SWIM_DIVE_IN || State == PlayerState.SWIM_TURN_AROUND)
            {
                var tSwimVecX = 5;
                var maxSwimVecX = 10;

                var v = -Math.Sign(swimVector.X);

                if (k_leftHolding) v = -1;
                if (k_rightHolding) v = 1;

                var sx = swimVector.X + .6f * Math.Sign(v);
                var sy = (YVel - Gravity) * maxSwimVecX;

                sx = sx.Clamp(-maxSwimVecX, maxSwimVecX);
                sy = sy.Clamp(-maxSwimVecX, maxSwimVecX);

                swimVector = new Vector2(sx, sy);
                
                if (State == PlayerState.SWIM_TURN_AROUND)
                {
                    YVel -= Gravity;

                    targetAngle = -Math.Sign((int)Direction) * 90;

                    if (animationComplete)
                    {
                        targetAngle = -targetAngle;                        
                        State = PlayerState.SWIM;
                    }
                }
                // swimming
                if (State == PlayerState.SWIM || State == PlayerState.SWIM_DIVE_IN)
                {
                    lastGroundY = Y;

                    var waterAccX = 0.03f;
                    var waterAccY = 0.03f;
                    var waterVelMax = 1f;

                    if (State == PlayerState.SWIM_DIVE_IN)
                    {
                        if (Direction == Direction.LEFT)
                            XVel = Math.Min(XVel, 0);
                        if (Direction == Direction.RIGHT)
                            XVel = Math.Max(XVel, 0);
                    }

                    if (k_leftHolding)
                    {
                        XVel = Math.Max(XVel - waterAccX, -waterVelMax);
                        if (sx < tSwimVecX)
                        {
                            Direction = Direction.LEFT;
                            if (lastDirection == Direction.RIGHT && State != PlayerState.SWIM_DIVE_IN)
                            {
                                ResetAnimation();
                                State = PlayerState.SWIM_TURN_AROUND;
                            }
                        }
                    }
                    else if (k_rightHolding)
                    {
                        XVel = Math.Min(XVel + waterAccX, waterVelMax);
                        if (sx > -tSwimVecX)
                        {
                            Direction = Direction.RIGHT;
                            if (lastDirection == Direction.LEFT && State != PlayerState.SWIM_DIVE_IN)
                            {
                                ResetAnimation();
                                State = PlayerState.SWIM_TURN_AROUND;
                            }
                        }
                    }
                    else
                    {
                        XVel = Math.Sign(XVel) * Math.Max(Math.Abs(XVel) - .02f, 0);
                    }

                    if (k_upHolding)
                    {
                        YVel = Math.Max(YVel - waterAccY - Gravity, -waterVelMax);
                    }
                    else if (k_downHolding)
                    {
                        YVel = Math.Min(YVel + waterAccY - Gravity, waterVelMax);
                    }
                    else
                    {
                        YVel = Math.Sign(YVel) * Math.Max(Math.Abs(YVel) - .02f, 0);
                        if (YVel > Gravity)
                            YVel -= Gravity;
                    }
                }
                
                swimAngle = new Vector2(Math.Sign((int)Direction) * Math.Max(Math.Abs(sx), 1), sy).ToAngle() + 90;

                if (State == PlayerState.SWIM_DIVE_IN)
                {
                    //swimAngle = 180;
                    swimAngle = new Vector2(XVel, YVel).ToAngle() + 90;
                    targetAngle = swimAngle;
                }

                if (Math.Abs(swimAngle - targetAngle) > 180)
                    targetAngle -= Math.Sign(targetAngle - swimAngle) * 360;

                targetAngle += (swimAngle - targetAngle) / 29f;

                Angle = (float)((targetAngle / 360) * (2 * Math.PI));
            }
            
            if (State != PlayerState.SWIM && State != PlayerState.SWIM_TURN_AROUND && State != PlayerState.SWIM_DIVE_IN)
                Angle = 0;
            
            // lieing around
            if (State == PlayerState.LIE)
            {
                XVel = 0;
                YVel = 0;

                lieTimer = Math.Max(lieTimer - 1, 0);

                if (lieTimer == 0)
                    State = PlayerState.GET_UP;
            }
            // death
            if (State == PlayerState.DEAD)
            {
                ghostTimer = Math.Max(ghostTimer - 1, 0);
                if (inWater)
                {
                    YVel = Math.Max(YVel - .15f, -.2f);
                    XVel *= .95f;
                }

                if (ghost == null && ghostTimer == 0)
                {
                    ghost = new PlayerGhost(X, Y, Direction);
                    ghost.Parent = this;
                }

                if (onGround)
                    XVel *= .8f;                
            }
            // obtaining items:
            if (State == PlayerState.OBTAIN)
            {
                XVel *= .9f;
            }
            // entering doors
            if (State == PlayerState.DOOR)
            {
                // nothing to do here for now
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

            // moving platform pre-calculations

            var platform = this.CollisionBounds<Platform>(X, Y + YVel).FirstOrDefault();

            // get off platform when not in X-range
            if (movingPlatform != null)
            {
                if (Left > movingPlatform.Right || Right < movingPlatform.Left)
                    movingPlatform = null;
            }

            if (movingPlatform == null)
            {
                movXvel = 0f;
                movYvel = 0f;
            }
            else
            {
                movXvel = movingPlatform.XVel;
                movYvel = movingPlatform.YVel;
            }

            var colY = this.CollisionBounds<Collider>(X, Y + movYvel + YVel).Where(o => o is Solid).ToList();
            
            if (platform != null && movingPlatform == null)
            {
                if (Bottom <= platform.Top - platform.YVel)
                {
                    if (YVel >= 0)
                    {
                        colY.Clear();
                        colY.Add(platform);

                        if (State != PlayerState.SWIM
                            && State != PlayerState.SWIM_DIVE_IN
                            && State != PlayerState.SWIM_TURN_AROUND
                            && State != PlayerState.PUSH
                            && State != PlayerState.OBTAIN
                            && State != PlayerState.LEVITATE
                            && State != PlayerState.WALL_CLIMB
                            && State != PlayerState.WALL_IDLE
                            && State != PlayerState.CEIL_CLIMB
                            && State != PlayerState.CEIL_IDLE
                            && State != PlayerState.DOOR
                            && State != PlayerState.HIT_AIR)
                        {
                            if (movingPlatform == null && platform is MovingPlatform)
                                movingPlatform = platform as MovingPlatform;
                        }
                    }
                }
            }
            
            // get off platform when touching y blocks
            if (movingPlatform != null)
            {
                if (colY.Where(o => o is Solid && !(o is MovingPlatform)).Count() == 0)
                    colY.Add(movingPlatform);
                else
                {
                    if(movingPlatform.YVel >= 0)
                        movingPlatform = null;
                    else
                    {
                        colY = this.CollisionBounds<Collider>(X, Y + movYvel + YVel - 1).Where(o => o is Solid).ToList();
                        if (colY.Count > 0)
                            movingPlatform = null;
                    }
                }

                // this is dangerous!
                if (movingPlatform != null && movingPlatform.YVel > 0)
                {
                    Position = new Vector2(X, movingPlatform.Y - 8);
                    //Move(0, movYvel);
                }
                if (!this.CollisionBounds(movingPlatform, X, Y + movYvel + YVel))
                    movingPlatform = null;
            }

            if (movingPlatform != null)
            {
                if (Bottom > movingPlatform.Y)
                    Move(0, -Math.Abs(movingPlatform.YVel));
            }

            if (colY.Count == 0)
            {
                Move(0, YVel + movYvel);                
            }
            else
            {
                if (YVel >= Gravity)
                {
                    onGround = true;

                    // transition from falling to getting up again
                    if (State == PlayerState.JUMP_UP || State == PlayerState.JUMP_DOWN || State == PlayerState.WALL_CLIMB || State == PlayerState.LEVITATE)
                    {
                        if (lastGroundY < Y - 9 * Globals.TILE)
                        {
                            var eff = new SingularEffect(X, Y + 8);
                            Hit(3);
                            State = PlayerState.LIE;
                            lieTimer = 120;
                        }
                        else if (lastGroundY < Y - Globals.TILE)
                            State = PlayerState.GET_UP;
                        else
                            State = PlayerState.IDLE;
                    }

                    // trick to "snap" to the bottom:                    
                    var overlap = Bottom - colY.FirstOrDefault().Top;
                    if (Math.Abs(overlap) <= Math.Abs(YVel) + 1)
                        Move(0, -overlap - Gravity);
                    
                    // deprecated: solved issue with a loop:
                    /*
                    while (true)
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
            
            var colX = ObjectManager.CollisionBounds<Solid>(this, X + XVel + movXvel, Y);
            
            if (colX.Count == 0)
            {
                Move(XVel + movXvel, 0);
            } else
            {
                XVel = 0;
            }

            // ++++ limit positin within room bounds ++++

            var boundX = Position.X;
            var boundY = Position.Y;

            if (boundX < 4) { XVel = 0; }
            if (boundX > GameManager.Current.Map.Width * Globals.TILE - 4) { XVel = 0; }
            if (boundY < 4) { YVel = 0; }
            if (boundY > GameManager.Current.Map.Height * Globals.TILE - 4) { YVel = 0; }

            boundX = boundX.Clamp(4, GameManager.Current.Map.Width * Globals.TILE - 4);
            boundY = boundY.Clamp(4, GameManager.Current.Map.Height * Globals.TILE - 4);

            Position = new Vector2(boundX, boundY);
            levitationEmitter.Position = Position;

            // ++++ previous vars ++++

            lastDirection = Direction;

            // ++++ coin counter ++++

            coinTimeout = Math.Max(coinTimeout - 1, 0);
            if (coinTimeout == 0)
                CoinCounter = 0;

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
                case PlayerState.SWIM_DIVE_IN:
                case PlayerState.SWIM:
                    row = 5;
                    fAmount = 4;
                    fSpd = 0.1f;
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
                case PlayerState.DEAD:
                    row = 14;
                    fAmount = 1;
                    offset = 1;
                    fSpd = 0;
                    break;
                case PlayerState.LIE:
                    row = 14;
                    fAmount = 1;
                    fSpd = 0;
                    break;
                case PlayerState.SWIM_TURN_AROUND:
                    row = 15;
                    fAmount = 4;
                    fSpd = .25f;
                    loopAnim = false;
                    break;
                case PlayerState.OBTAIN:
                    row = 16;
                    fAmount = 1;
                    fSpd = 0;                    
                    break;
                case PlayerState.DOOR:
                    row = 17;
                    fSpd = 0.05f;
                    fAmount = 4;
                    break;
            }

            SetAnimation(cols * row + offset, cols * row + offset + fAmount - 1, fSpd, loopAnim);
            Color = (InvincibleTimer % 4 > 2) ? Color.Transparent : Color.White;
            
            var xScale = Math.Sign((int)Direction);
            Scale = new Vector2(xScale, 1);

            animationComplete = false;
        }
        
        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);
            
            if (CoinCounter > 0)
            {
                float coinAlpha = coinTimeout / (.5f * maxCoinTimeout);
                coinFont.Color = new Color(coinFont.Color, coinAlpha);
                coinFont.Draw(sb, X, Y - Globals.TILE, $"+{CoinCounter}");
            }
            
            if (oxygen < maxOxygen && HP > 0)
            {
                float rel = 1 - (float)oxygen / (float)maxOxygen;
                var r = 3 + (int)(76 * rel);
                var g = 243 - (int)(240 * rel);
                var b = 243;

                var fg = new Color(r, g, b);
                var bg = new Color(20, 113, 126);

                sb.DrawBar(Position + new Vector2(0, 12), (int) (1.5 * Globals.TILE), oxygen / (float)maxOxygen, fg, bg, height:2, border:false);                
            }

            //sb.DrawPixel(X, Y + swimVector.Y, Color.AliceBlue);
            //sb.DrawPixel(X + swimVector.X, Y + swimVector.Y, Color.Red);
            //sb.DrawPixel(X, Y, Color.Blue);
        }
    }
}