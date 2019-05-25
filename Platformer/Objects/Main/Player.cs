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
using Platformer.Objects.Enemies;
using Platformer.Objects.Effects;
using Platformer.Objects;
using Platformer.Main;
using Microsoft.Xna.Framework.Graphics;
using Platformer.Objects.Level;
using Platformer.Objects.Effects.Emitters;
using Platformer.Objects.Items;
using Platformer.Util;
using SPG.Save;
using Platformer.Objects.Main.Orbs;
using SPG.Map;

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
        public int MaxMP { get; set; } = 30;
        public float MPRegen { get; set; } = .1f;

        public PlayerAbility Abilities { get; set; } = PlayerAbility.NONE;

        public float Coins { get; set; } = 0;

        public Dictionary<SpellType, SpellLevel> Spells { get; set; } = new Dictionary<SpellType, SpellLevel> { { SpellType.NONE, SpellLevel.ONE } };
        public Dictionary<SpellType, int> SpellEXP { get; set; } = new Dictionary<SpellType, int> { { SpellType.NONE, 0 } };
        public int SpellIndex;
        
        // ID, Typename
        public Dictionary<int, string> Items { get; set; } = new Dictionary<int, string>();
        
        public List<int> KeysAndKeyblocks { get; set; } = new List<int>();
    }

    [Flags]
    public enum PlayerAbility
    {
        NONE = 0,
        BREATHE_UNDERWATER = 1,
        CLIMB_WALL = 2,
        CLIMB_CEIL = 4,
        LEVITATE = 8,
        PUSH = 16,
        ORB = 32
        //ideas:
        //PUSH_BIG <- inspired by zelda
        //WARP
        //STOMP
    }
    
    public class Player : GameObject, IMovable
    {

        // public
        
        public enum PlayerState
        {
            IDLE, WALK, JUMP_UP, JUMP_DOWN, WALL_IDLE,
            WALL_CLIMB, OBTAIN, TURN_AROUND,
            GET_UP, HIT_AIR, HIT_GROUND, CEIL_IDLE,
            CEIL_CLIMB, SWIM, DEAD, LEVITATE,
            PUSH, LIE, SWIM_DIVE_IN, SWIM_TURN_AROUND,
            DOOR,
            CARRYOBJECT_TAKE, CARRYOBJECT_IDLE, CARRYOBJECT_WALK, CARRYOBJECT_THROW
        }

        public PlayerState State { get; set; }

        // stats 

        //private GameStats stats;
        public GameStats Stats { get => GameManager.Current.SaveGame.gameStats; }
        public int HP { get; set; }
        public float MP { get; set; }

        public int KeyObjectID { get; set; } = -1;

        // private

        public Direction Direction { get; set; } = Direction.RIGHT;
        private Direction lastDirection;

        // for up/down regarding orbs
        public Direction LookDirection { get; set; }

        private bool animationComplete;

        private bool onGround;
        public bool OnGround { get => onGround; }

        private float lastGroundY;
        private float lastGroundYbeforeWall;

        private bool hit = false;
        public int InvincibleTimer { get; private set; } = 0;

        private int mpRegenTimeout = 0;
        private int maxMpRegenTimeout = 60;

        // input vars

        Input input = new Input();

        bool k_leftPressed, k_leftHolding, k_leftReleased;
        bool k_rightPressed, k_rightHolding, k_rightReleased;
        bool k_upPressed, k_upHolding;
        bool k_downPressed, k_downHolding;
        bool k_jumpPressed, k_jumpHolding;
        bool k_attackPressed, k_attackHolding, k_attackReleased;

        bool k_LPressed, k_RPressed;

        float gamePadLeftXFactor;
        float gamePadLeftYFactor;

        // swim vars

        private float gravAir = .1f;
        private float gravWater = .03f;
        
        private float swimAngle;
        private Vector2 swimVector;
        private float targetAngle;
        
        // levitation vars

        private float levitationSine;
        private PlayerLevitationEmitter levitationEmitter;
        
        private PushBlock pushBlock;

        // diving

        int oxygen;
        int maxOxygen;

        // misc. timers

        private float coinCounter;
        public float CoinCounter {
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
        private bool jumpControlDisabled;

        // other objects

        public Key KeyObject { get; set; }
        public Collider MovingPlatform { get; set; }
        public Orb Orb { get; set; }
        
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
            MovingPlatform = null;
            
            var hpPrev = HP;
            
            HP = Math.Max(HP - hitPoints, 0);

            // spell EXP penalty
            if (Orb != null)
            {
                var currentSpellType = Stats.Spells.ElementAt(Stats.SpellIndex).Key;
                var currentSpellLevel = Stats.Spells.ElementAt(Stats.SpellIndex).Value;

                var maxSpellExpForLevel = Orb.MaxEXP[currentSpellType][currentSpellLevel];

                int expHit = 0;

                switch (currentSpellLevel)
                {
                    case SpellLevel.ONE:
                        expHit = (int)Math.Ceiling(Orb.MaxEXP[currentSpellType][currentSpellLevel] * .15f);
                        break;
                    case SpellLevel.TWO:
                        expHit = (int)Math.Ceiling(Orb.MaxEXP[currentSpellType][currentSpellLevel] * .25f);
                        break;
                    case SpellLevel.THREE:
                        expHit = (int)Math.Ceiling(Orb.MaxEXP[currentSpellType][currentSpellLevel] * .35f);
                        break;
                    default:
                        break;
                }

                // don't deduct exp from "none" spell
                if (currentSpellType == SpellType.NONE)
                    expHit = 0;

                var expAfterHit = Math.Max(Stats.SpellEXP[currentSpellType] - expHit, 0);
                Stats.SpellEXP[currentSpellType] = expAfterHit;
                
                if (expAfterHit == 0)
                {
                    switch (currentSpellLevel)
                    {
                        case SpellLevel.ONE:
                            break;
                        case SpellLevel.TWO:
                            CreateSpellDownEffect();
                            Stats.Spells[currentSpellType] = SpellLevel.ONE;
                            Stats.SpellEXP[currentSpellType] = Orb.MaxEXP[currentSpellType][SpellLevel.ONE];
                            break;
                        case SpellLevel.THREE:
                            CreateSpellDownEffect();
                            Stats.Spells[currentSpellType] = SpellLevel.TWO;
                            Stats.SpellEXP[currentSpellType] = Orb.MaxEXP[currentSpellType][SpellLevel.TWO];
                            break;
                        default:
                            break;
                    }
                }                
            }

            // death penalty
            if (hpPrev > 0 && HP == 0)
            {
                var temp = (float)Math.Floor(Stats.Coins * .5f);
                var amountToDrop = Stats.Coins - temp;
                
                var stats = GameManager.Current.SaveGame.gameStats;
                stats.Coins = temp;

                GameManager.Current.CoinsAfterDeath = temp;                
                Coin.Spawn(X, Y, RoomCamera.Current.CurrentRoom, amountToDrop);
            }

            var ouch = new StarEmitter(X, Y);

            //var dmgFont = new FollowFont(X, Y - Globals.TILE, $"-{hitPoints}");
            //dmgFont.Color = Color.Red;
            //dmgFont.Target = this;

            new FallingFont(X, Y, $"-{hitPoints}", new Color(170, 0, 231), new Color(255, 0, 0));

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
        // BEGIN UPDATE
        // ++++++++++++++++++++++++++

        public override void BeginUpdate(GameTime gameTime)
        {
            base.BeginUpdate(gameTime);

            // ++++ input ++++

            if (ObjectManager.Exists<MessageBox>() 
                || GameManager.Current.Transition != null)
            {
                input.Enabled = false;
            }
            else
            {
                input.Enabled = true;
            }

            input.Update(gameTime);

            k_leftPressed = input.IsKeyPressed(Keys.Left, Input.State.Pressed);
            k_leftHolding = input.IsKeyPressed(Keys.Left, Input.State.Holding);
            k_leftReleased = input.IsKeyPressed(Keys.Left, Input.State.Released);

            k_rightPressed = input.IsKeyPressed(Keys.Right, Input.State.Pressed);
            k_rightHolding = input.IsKeyPressed(Keys.Right, Input.State.Holding);
            k_rightReleased = input.IsKeyPressed(Keys.Right, Input.State.Released);

            k_upPressed = input.IsKeyPressed(Keys.Up, Input.State.Pressed);
            k_upHolding = input.IsKeyPressed(Keys.Up, Input.State.Holding);

            k_downPressed = input.IsKeyPressed(Keys.Down, Input.State.Pressed);
            k_downHolding = input.IsKeyPressed(Keys.Down, Input.State.Holding);

            k_jumpPressed = input.IsKeyPressed(Keys.A, Input.State.Pressed);
            k_jumpHolding = input.IsKeyPressed(Keys.A, Input.State.Holding);

            k_attackPressed = input.IsKeyPressed(Keys.S, Input.State.Pressed);
            k_attackHolding = input.IsKeyPressed(Keys.S, Input.State.Holding);
            k_attackReleased = input.IsKeyPressed(Keys.S, Input.State.Released);

            k_LPressed = input.IsKeyPressed(Keys.Q, Input.State.Pressed);
            k_RPressed = input.IsKeyPressed(Keys.E, Input.State.Pressed);

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

                Stats.Abilities |= PlayerAbility.ORB;
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

                Coin.Spawn(X, Y, RoomCamera.Current.CurrentRoom, 2000);
                Debug.WriteLine($"{ObjectManager.Count<Coin>()} coins exist. (Blocks: {ObjectManager.Count<Solid>()}, active: {ObjectManager.ActiveObjects.Count()}, overall: {ObjectManager.Count<GameObject>()})");

                Stats.KeysAndKeyblocks.Clear();
                Stats.Items.Clear();
                //var flash = new FlashEmitter(X, Y);
            }

            gamePadLeftXFactor = 1f;
            gamePadLeftYFactor = 1f;

            // gamepad overrides keyboard input if possible
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

                k_attackPressed= input.IsButtonPressed(Buttons.X, Input.State.Pressed);
                k_attackHolding = input.IsButtonPressed(Buttons.X, Input.State.Holding);
                k_attackReleased = input.IsButtonPressed(Buttons.X, Input.State.Released);

                k_LPressed = input.IsButtonPressed(Buttons.LeftShoulder, Input.State.Released);
                k_RPressed = input.IsButtonPressed(Buttons.RightShoulder, Input.State.Released);

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
        }

        // ++++++++++++++++++++++++++
        // UPDATE
        // ++++++++++++++++++++++++++

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            // ++++ look direction ++++

            if (k_upHolding) LookDirection = Direction.UP;
            else if (k_downHolding) LookDirection = Direction.DOWN;
            else LookDirection = Direction.NONE;

            // ++++ getting hit ++++

            InvincibleTimer = Math.Max(InvincibleTimer - 1, 0);
            
            if (InvincibleTimer == 0 && HP > 0)
            {
                var obstacle = ObjectManager.CollisionBoundsFirstOrDefault<Obstacle>(this, X, Y);

                if (obstacle != null)
                {
                    var vec = Position - (obstacle.Center + new Vector2(0, 0));
                    var angle = vec.VectorToAngle();                    
                    Hit(obstacle.Damage, (float)angle);
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
            
            var onWall = !hit && ObjectManager.CollisionPoints<Solid>(this, X + (.5f * BoundingBox.Width + 1) * Math.Sign((int)Direction), Y + 4)
                            .Where(o => o.Room == currentRoom).Count() > 0;
            var onCeil = !hit && ObjectManager.CollisionPoints<Solid>(this, X, Y - BoundingBox.Height * .5f - 1)
                .Where(o => o.Room == currentRoom && !(o is PushBlock)).Count() > 0;

            var inWater = GameManager.Current.Map.CollisionTile(X, Y + 4, GameMap.WATER_INDEX);

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
                    if (!k_attackHolding && ((Direction == Direction.LEFT && k_leftHolding)
                        ||
                        (Direction == Direction.RIGHT && k_rightHolding))
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
                    && !k_downHolding && !k_attackHolding)
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

            // ++++ attacking ++++

            if (Stats.Abilities.HasFlag(PlayerAbility.ORB))
            {
                if (Orb == null)
                {
                    new SaveBurstEmitter(X, Y);
                    Orb = new Orb(this);                    
                }

                if (k_attackHolding && !inWater && HP > 0
                && !jumpControlDisabled
                && (State == PlayerState.IDLE
                || State == PlayerState.WALK
                || State == PlayerState.TURN_AROUND
                || State == PlayerState.JUMP_UP
                || State == PlayerState.JUMP_DOWN
                || State == PlayerState.GET_UP))
                {
                    Orb.State = OrbState.ATTACK;
                    if (Stats.Spells.ElementAt(Stats.SpellIndex).Key != SpellType.NONE)
                        mpRegenTimeout = Math.Min(mpRegenTimeout + 2, maxMpRegenTimeout);                    
                }
                else
                {
                    if (Orb.State == OrbState.ATTACK)
                        Orb.State = OrbState.IDLE;                    
                }

                if (k_RPressed)
                {
                    Orb.ChangeType(Direction.RIGHT);
                    Stats.SpellIndex++;
                }
                if (k_LPressed)
                {
                    Orb.ChangeType(Direction.LEFT);
                    Stats.SpellIndex--;
                }
                
                if (Stats.SpellIndex > Stats.Spells.Count - 1)
                    Stats.SpellIndex = 0;
                if (Stats.SpellIndex < 0)
                    Stats.SpellIndex = Stats.Spells.Count - 1;

                Orb.Type = Stats.Spells.ElementAt(Stats.SpellIndex).Key;
                Orb.Level = Stats.Spells.ElementAt(Stats.SpellIndex).Value;
            }

            // ++++++++++++++++++++++++++++++
            // INTERACTION WITH OTHER OBJECTS
            // ++++++++++++++++++++++++++++++

            if (HP > 0)
            {
                // ++++ save statues ++++

                var saveStatue = this.CollisionBoundsFirstOrDefault<SaveStatue>(X, Y);

                if (saveStatue != null)
                {
                    saveStatue.Save();
                }

                // ++++ spell EXP ++++

                var spellExp = this.CollisionBounds<SpellEXP>(X, Y);
                if (Orb != null)
                {
                    foreach (var s in spellExp)
                    {
                        if (s.Taken)
                            continue;

                        var currentSpellType = Stats.Spells.ElementAt(Stats.SpellIndex).Key;
                        var currentSpellLevel = Stats.Spells.ElementAt(Stats.SpellIndex).Value;

                        var maxSpellExpForLevel = Orb.MaxEXP[currentSpellType][currentSpellLevel];

                        // only add EXP to spells other than "none"
                        if (currentSpellType != SpellType.NONE)
                        {

                            Stats.SpellEXP[currentSpellType] = Math.Min(Stats.SpellEXP[currentSpellType] + (int)s.Exp, maxSpellExpForLevel);

                            MP = Math.Min(MP + (int)s.Exp, Stats.MaxMP);
                            
                            if (Stats.SpellEXP[currentSpellType] == maxSpellExpForLevel)
                            {
                                switch (Stats.Spells[currentSpellType])
                                {
                                    case SpellLevel.ONE:
                                        Stats.Spells[currentSpellType] = SpellLevel.TWO;
                                        Stats.SpellEXP[currentSpellType] = 0;
                                        CreateSpellUpEffect();
                                        break;
                                    case SpellLevel.TWO:
                                        Stats.Spells[currentSpellType] = SpellLevel.THREE;
                                        Stats.SpellEXP[currentSpellType] = 0;
                                        CreateSpellUpEffect();
                                        break;
                                }
                            }
                        }                        
                        s.Taken = true;
                    }
                }

                // ++++ pickup items, chests, etc. ++++

                var items = this.CollisionBounds<Item>(X, Y);
                foreach (var item in items)
                {
                    /*if (item is Chest && !item.Taken)
                    {
                        var toolTip = new ToolTip(item, this, new Vector2(0, 8), 0);
                        if (k_upPressed)
                        {                            
                            ObjectManager.DestroyAll<ToolTip>();
                            item.Take(this);
                        }
                    }
                    else
                        item.Take(this);
                    */
                    item.Take(this);
                }

                // ++++ doors ++++

                var door = this.CollisionBoundsFirstOrDefault<Door>(X, Y);

                if (door != null)
                {
                    if (k_upPressed && onGround)
                    {
                        XVel = 0;
                        YVel = -Gravity;

                        var pos = Position + new Vector2(door.Tx * Globals.TILE, door.Ty * Globals.TILE);
                        RoomCamera.Current.ChangeRoomsFromPosition(pos);

                    }
                }

                // ++++ keys ++++
                
                var key = this.CollisionBoundsFirstOrDefault<Key>(X, Y + Globals.TILE);
                if (key != null)
                {
                    var toolTip = new ToolTip(key, this, new Vector2(0, 8), 1);

                    if (State == PlayerState.IDLE && k_downPressed)
                    {
                        KeyObject = key;
                        key.Take(this);
                        if (MovingPlatform == KeyObject)
                            MovingPlatform = null;
                        State = PlayerState.CARRYOBJECT_TAKE;

                        ObjectManager.DestroyAll<ToolTip>();
                    }
                }

                // ++++ npcs ++++

                var t = Globals.TILE;
                var npc = this.CollisionBoundsFirstOrDefault<NPC>(X, Y);
                if (npc != null)
                {
                    if (!ObjectManager.Exists<MessageBox>())
                        npc.ShowToolTip(this);

                    if (k_upPressed && !k_attackHolding)
                    {
                        npc.Interact(this);
                    }
                }

            }

            var waterFalls = ObjectManager.FindAll<WaterFallEmitter>();

            foreach (var waterFall in waterFalls)
            {
                if (waterFall.X < Right && waterFall.X + waterFall.BoundingBox.Width > Left && waterFall.Y < Top)
                {
                    foreach (var part in waterFall.Particles)
                    {
                        //if (!(part is WaterFallParticle))
                        //    continue;

                        if (part.Position.X > Left - 2&& part.Position.Y + part.YVel > Top && part.Position.X < Right + 2 && part.Position.Y + part.YVel < Bottom)
                        {
                            if (part is WaterFallParticle)
                                (part as WaterFallParticle).Collision = true;
                            if (part is WaterSplashParticle && part.YVel > 0)
                            {
                                part.XVel *= .5f;
                                part.YVel = -1f;

                                part.Alpha = Math.Max(part.Alpha - .3f, 0);
                                if (part.Alpha == 0)
                                    part.LifeTime = 0;                                
                            }
                        }
                    }                    
                }
            }

            // +++++++++++++++++++++
            // ++++ state logic ++++
            // +++++++++++++++++++++

            var maxVel = 1.2f;

            if (YVel != 0) onGround = false;

            if (onGround)
            {
                lastGroundY = Y;                
            }

            // idle
            if (State == PlayerState.IDLE || State == PlayerState.CARRYOBJECT_IDLE)
            {
                XVel = Math.Sign(XVel) * Math.Max(Math.Abs(XVel) - .2f, 0f);
                if (k_rightHolding)
                {
                    if (State == PlayerState.IDLE)
                        State = PlayerState.WALK;
                    if (State == PlayerState.CARRYOBJECT_IDLE)
                        State = PlayerState.CARRYOBJECT_WALK;

                    Direction = Direction.RIGHT;
                }
                if (k_leftHolding)
                {
                    if (State == PlayerState.IDLE)
                        State = PlayerState.WALK;
                    if (State == PlayerState.CARRYOBJECT_IDLE)
                        State = PlayerState.CARRYOBJECT_WALK;

                    Direction = Direction.LEFT;
                }
            }
            // walk
            if (State == PlayerState.WALK)
            {
                var colSide = this.CollisionBoundsFirstOrDefault<Solid>(X + Math.Sign((int)Direction), Y);

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
                    if (MovingPlatform != null)
                    {
                        YVel = Math.Min(-2, MovingPlatform.YVel - .1f); ;
                    }
                    else
                        YVel = -2;

                    MovingPlatform = null;
                    State = PlayerState.JUMP_UP;                    
                    k_jumpPressed = false;
                }
            }
            // pushing
            if (State == PlayerState.PUSH)
            {
                if (pushBlock == null)
                {
                    pushBlock = this.CollisionBoundsFirstOrDefault<PushBlock>(X + Math.Sign((int)Direction), Y);

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
                
                if (this.CollisionBoundsFirstOrDefault<PushBlock>(X + Math.Sign((int)Direction), Y) == null)
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
                    var mush = this.CollisionBoundsFirstOrDefault<Mushroom>(X, Y);
                    if (mush != null && !mush.Bouncing)
                    {
                        lastGroundY = Y;
                        mush.Bounce();
                        YVel = -3.2f;
                    }
                }

                if (this.CollisionBoundsFirstOrDefault<JumpControlDisabler>(X, Y) != null)
                    jumpControlDisabled = true;

                if (!jumpControlDisabled)
                {

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
                } else
                {
                    XVel *= .94f;
                }
                if (Stats.Abilities.HasFlag(PlayerAbility.LEVITATE))
                {
                    if (MP > 0) {
                        if (k_jumpPressed)
                            State = PlayerState.LEVITATE;
                    }
                }                
            } else
            {
                jumpControlDisabled = false;
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
                
                swimAngle = (float)new Vector2(Math.Sign((int)Direction) * Math.Max(Math.Abs(sx), 1), sy).VectorToAngle() + 90;

                if (State == PlayerState.SWIM_DIVE_IN)
                {
                    //swimAngle = 180;
                    swimAngle = (float)new Vector2(XVel, YVel).VectorToAngle() + 90;
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
            // taking stuff
            if (State == PlayerState.CARRYOBJECT_TAKE)
            {
                XVel = 0;
                YVel = -Gravity;
                
                if (animationComplete)
                {
                    State = PlayerState.CARRYOBJECT_IDLE;
                }
            }
            if (State == PlayerState.CARRYOBJECT_WALK)
            {
                
                if (k_leftHolding)
                {
                    Direction = Direction.LEFT;
                    XVel = Math.Max(XVel - .2f, -1 * gamePadLeftXFactor);
                }
                if (k_rightHolding)
                {
                    Direction = Direction.RIGHT;
                    XVel = Math.Min(XVel + .2f, 1 * gamePadLeftXFactor);
                }
                if (!k_leftHolding && !k_rightHolding)
                {
                    XVel = Math.Sign(XVel) * Math.Max(Math.Abs(XVel) - .2f, 0);
                    if (XVel == 0)
                    {
                        State = PlayerState.CARRYOBJECT_IDLE;
                    }
                }
            }
            // carrying keys/objects
            if (State == PlayerState.CARRYOBJECT_THROW)
            {
                XVel = 0;
                //YVel = -Gravity;
                if (animationComplete)
                    State = PlayerState.IDLE;
            }
            if (State == PlayerState.CARRYOBJECT_TAKE || State == PlayerState.CARRYOBJECT_IDLE || State == PlayerState.CARRYOBJECT_WALK)
            {
                if (KeyObject == null)
                    State = PlayerState.IDLE;

                if (KeyObject != null)
                {
                    if (hit || inWater)
                        KeyObject.Throw();
                    else if (State != PlayerState.CARRYOBJECT_TAKE)
                    {
                        if (k_attackPressed)
                        {
                            KeyObject.Throw();
                            State = PlayerState.CARRYOBJECT_THROW;
                        }
                    }
                    //if (onGround && k_jumpPressed)
                    //    YVel = -1;
                }
            } else
            {
                if (KeyObject != null)
                    KeyObject.Throw();
            }
            
            // +++++++++++++++++++++
            // AFTER STATE LOGIC
            // +++++++++++++++++++++
            
            // reset hit after state-logic
            hit = false;

            // ++++ collision & movement ++++

            bool moveWithPlatforms = (State != PlayerState.SWIM
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
                          && State != PlayerState.HIT_AIR);
            
            var g = this.MoveAdvanced(moveWithPlatforms);
            
            if (g)
            {
                // TODO: FIX GAP TO GET TO GROUND!

                //var groundBlock = this.CollisionBoundsFirstOrDefault<Solid>(X, Y + 3);
                //if (groundBlock != null)
                //{
                //    if (YVel > 0)
                //        Move(0, Math.Abs(groundBlock.Top - Bottom - Gravity));
                //}

                onGround = true;

                // transition from falling to getting up again
                if (State == PlayerState.JUMP_UP
                    || State == PlayerState.JUMP_DOWN
                    || State == PlayerState.WALL_CLIMB
                    || State == PlayerState.LEVITATE
                    || State == PlayerState.CARRYOBJECT_IDLE
                    || State == PlayerState.CARRYOBJECT_WALK)
                {
                    if (lastGroundY < Y - 9 * Globals.TILE)
                    {
                        var eff = new SingularEffect(X, Y + 8);
                        Hit(3);
                        State = PlayerState.LIE;
                        lieTimer = 120;
                    }
                    else if (KeyObject == null)
                    {
                        if (lastGroundY < Y - Globals.TILE)
                            State = PlayerState.GET_UP;
                        else
                            State = PlayerState.IDLE;
                    }
                }
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
                    offset = 4 * Convert.ToInt32(Orb?.State == OrbState.ATTACK);
                    fSpd = 0.03f;                                        
                    break;
                case PlayerState.WALK:
                    row = 1;
                    offset = 4 * Convert.ToInt32(Orb?.State == OrbState.ATTACK);
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
                    fAmount = 1;
                    offset = 4 * Convert.ToInt32(Orb?.State == OrbState.ATTACK);
                    fSpd = 0;
                    break;
                case PlayerState.JUMP_DOWN:
                    row = 2;
                    if (jumpControlDisabled)
                        offset = 2;
                    else
                        offset = 1 + 4 * Convert.ToInt32(Orb?.State == OrbState.ATTACK);
                    fAmount = 1;
                    fSpd = 0;
                    break;
                case PlayerState.GET_UP:
                    row = 8;
                    fAmount = 3;
                    fSpd = 0.15f;
                    loopAnim = false;
                    break;
                case PlayerState.WALL_IDLE:
                    row = 3;
                    fSpd = 0.1f;
                    break;
                case PlayerState.WALL_CLIMB:
                    row = 4;
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
                    fSpd = 0.05f;
                    break;
                case PlayerState.CEIL_CLIMB:
                    row = 11;
                    fSpd = 0.15f;
                    break;
                case PlayerState.SWIM_DIVE_IN:
                case PlayerState.SWIM:
                    row = 5;
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
                case PlayerState.CARRYOBJECT_TAKE:
                    row = 18;
                    fSpd = 0.2f;
                    loopAnim = false;
                    break;
                case PlayerState.CARRYOBJECT_IDLE:
                    row = 19;
                    fSpd = 0.03f;
                    break;
                case PlayerState.CARRYOBJECT_WALK:
                    row = 20;
                    fSpd = 0.1f;
                    break;
                case PlayerState.CARRYOBJECT_THROW:
                    row = 21;
                    fAmount = 2;
                    fSpd = 0.1f;
                    loopAnim = false;
                    break;
            }

            SetAnimation(cols * row + offset, cols * row + offset + fAmount - 1, fSpd, loopAnim);
            Color = (InvincibleTimer % 4 > 2) ? Color.Transparent : Color.White;
            
            var xScale = Math.Sign((int)Direction);
            Scale = new Vector2(xScale, 1);

            animationComplete = false;
        }

        private void CreateSpellUpEffect()
        {
            //FallingFont spellFont = new FallingFont(X, Y - 8, "Spell Up!", Potion.MpColors[0], Color.White);
            //spellFont.XVel = 0;
            //spellFont.Gravity = .025f;
            //spellFont.Scale = new Vector2(1);
            //spellFont.YVel = -1;
        }

        private void CreateSpellDownEffect()
        {
            //FallingFont spellFont = new FallingFont(X, Y - 8, "Spell Down!", new Color(218, 218, 218), new Color(250, 92, 117));
            //spellFont.XVel = 0;
            //spellFont.Gravity = .025f;
            //spellFont.Scale = new Vector2(1);
            //spellFont.YVel = -1;
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
        }
    }
}