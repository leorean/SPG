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
using Leore.Objects.Enemies;
using Leore.Objects.Effects;
using Leore.Objects;
using Microsoft.Xna.Framework.Graphics;
using Leore.Objects.Level;
using Leore.Objects.Effects.Emitters;
using Leore.Objects.Items;
using Leore.Util;
using SPG.Map;
using Leore.Resources;
using Leore.Objects.Level.Blocks;
using Leore.Objects.Effects.Weather;
using Leore.Objects.Level.Obstacles;

namespace Leore.Main
{
    public static class PlayerExtensions
    {
        public static Direction Reverse(this Direction dir)
        {
            return (Direction)(-(int)dir);
        }
    }
    
    [Flags]
    public enum PlayerAbility
    {
        NONE = 0,
        BREATHE_UNDERWATER = 1, // <- ocean
        CLIMB_WALL = 2, // <- ruins of leon (west)
        CLIMB_CEIL = 4, // <- ruins of leon (east)
        LEVITATE = 8,
        PUSH = 16, // <- sanctuary
        ORB = 32, // <- forest temple
        NO_FALL_DAMAGE = 64, // <- lybianna shop (optional)
        DOUBLE_JUMP = 128,

        MAGNETIC = 256 // <- spell exp radius doubled, coins are now magnetic (TODO: implement)
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
            BACKFACING,
            CARRYOBJECT_TAKE, CARRYOBJECT_IDLE, CARRYOBJECT_WALK, CARRYOBJECT_THROW,
            LIMBO
        }

        public PlayerState State { get; set; }

        // stats 
        
        public GameStats Stats { get => GameManager.Current.SaveGame.gameStats; }
        public int HP { get; set; }
        public float MP { get; set; }

        public long KeyObjectID { get; set; } = -1;

        public Direction Direction { get; set; } = Direction.RIGHT;

        private Vector2 safePosition;

        public void UseKeyFromInventory()
        {
            Stats.HeldKeys--;
            new FollowFont(X, Y - 8, "Key used.");
        }
        public void GetKey()
        {
            Stats.HeldKeys++;
            new FollowFont(X, Y - 8, "Got key!");
        }

        // private

        private LightSource light;

        private Direction lastDirection;

        // for up/down regarding orbs
        public Direction LookDirection { get; set; }

        private bool animationComplete;

        private bool onGround;
        public bool OnGround { get => onGround; }
        private bool inWater;
        public bool InWater { get => inWater; }

        private bool inputEnabled = true;
        public void SetControlsEnabled(bool enabled)
        {
            inputEnabled = enabled;
        }

        private bool onWall;
        public bool OnWall { get => onWall; }
        private bool onCeil;
        public bool OnCeil { get => onCeil; }

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

        public int Oxygen { get; set; }
        public int MaxOxygen { get; private set; }

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

        private int lieTimer;
        private int ghostTimer = 60;

        private PlayerGhost ghost;
        private bool jumpControlDisabled;

        private int attackDelay;

        private int jumps;
        private int maxJumps;

        private int limboTimer;

        // other objects

        public Key KeyObject { get; set; }
        public Collider MovingPlatform { get; set; }
        public Orb Orb { get; set; }

        public Teleporter Teleporter { get; set; }

        private Direction flowDirection;

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

            MaxOxygen = 5 * 60;
            Oxygen = MaxOxygen;

            safePosition = Position;

            light = new LightSource(this);
            light.Active = true;
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

        public void Hit(int hitPoints, float? degAngle = null)
        {
            MovingPlatform = null;

            if (hitPoints == 0)
                return;

            var hpPrev = HP;
            
            HP = Math.Max(HP - hitPoints, 0);

            // spell EXP penalty
            if (Orb != null)
            {
                var currentSpellType = Stats.Spells.ElementAt(Stats.SpellIndex).Key;
                var currentSpellLevel = Stats.Spells.ElementAt(Stats.SpellIndex).Value;

                var maxSpellExpForLevel = GameResources.MaxEXP[currentSpellType][currentSpellLevel];

                int expHit = 20 + (int)(Math.Ceiling(GameResources.MaxEXP[currentSpellType][currentSpellLevel] * .1f));

                /*switch (currentSpellLevel)
                {
                    case SpellLevel.ONE:
                        expHit = (int)Math.Ceiling(GameResources.MaxEXP[currentSpellType][currentSpellLevel] * .15f);
                        break;
                    case SpellLevel.TWO:
                        expHit = (int)Math.Ceiling(GameResources.MaxEXP[currentSpellType][currentSpellLevel] * .25f);
                        break;
                    case SpellLevel.THREE:
                        expHit = (int)Math.Ceiling(GameResources.MaxEXP[currentSpellType][currentSpellLevel] * .35f);
                        break;
                    default:
                        break;
                }*/

                // don't deduct exp from "none" spell or special spells
                if (GameResources.MaxEXP[currentSpellType][currentSpellLevel] == 0)
                    expHit = 0;
                
                var expAfterHit = Stats.SpellEXP[currentSpellType] - expHit;
                var expAfterHitRemainder = 0;
                if(expAfterHit < 0)
                {
                    expAfterHitRemainder = Math.Abs(expAfterHit);
                    expAfterHit = 0;
                }

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
                            Stats.SpellEXP[currentSpellType] = Math.Max(GameResources.MaxEXP[currentSpellType][SpellLevel.ONE] - expAfterHitRemainder, (int)(.5f * (float)GameResources.MaxEXP[currentSpellType][SpellLevel.ONE]));
                            break;
                        case SpellLevel.THREE:
                            CreateSpellDownEffect();
                            Stats.Spells[currentSpellType] = SpellLevel.TWO;
                            Stats.SpellEXP[currentSpellType] = Math.Max(GameResources.MaxEXP[currentSpellType][SpellLevel.TWO] - expAfterHitRemainder, (int)(.5f * (float)GameResources.MaxEXP[currentSpellType][SpellLevel.TWO]));
                            break;
                        default:
                            break;
                    }
                }                
            }

            // death penalty
            if (hpPrev > 0 && HP == 0)
            {
                var temp = Math.Max((float)Math.Floor(Stats.Coins * .5f), Stats.Coins - 1000);
                var amountToDrop = Stats.Coins - temp;
                
                var stats = GameManager.Current.SaveGame.gameStats;
                stats.Coins = temp;

                GameManager.Current.CoinsAfterDeath = temp;                
                Coin.Spawn(X, Y, RoomCamera.Current.CurrentRoom, amountToDrop);
            }

            var ouch = new StarEmitter(X, Y);
            
            new FallingFont(X, Y, $"-{hitPoints}", new Color(170, 0, 231), new Color(255, 0, 0));

            if (State == PlayerState.IDLE || State == PlayerState.WALK || State == PlayerState.GET_UP)
                State = PlayerState.JUMP_UP;

            if (State != PlayerState.HIT_GROUND)
            {
                if (degAngle == null)
                {
                    XVel = -.7f * Math.Sign((int)Direction);
                    YVel = Math.Min(YVel - .5f, -1.2f);
                }
                else
                {
                    var ldx = MathUtil.LengthDirX((float)degAngle) * 1.5f;
                    var ldy = MathUtil.LengthDirY((float)degAngle) * 1.5f;

                    XVel = (float)ldx;
                    YVel = (float)ldy;
                }
            }
            hit = true;
            InvincibleTimer = 60;
        }

        public void HurtAndSpawnBack()
        {
            if (State == PlayerState.LIMBO)
                return;
            
            Hit(1, 270);
            
            new SingularEffect(X, Y, 9);
            
            var dummy = new Dummy(safePosition.X, safePosition.Y);
            var burst = new KeyBurstEmitter(X, Y, dummy);
            burst.Colors = GameResources.HpColors;
            dummy.Parent = burst;
            
            State = PlayerState.LIMBO;
            limboTimer = 1 * 60;            
        }

        // ++++++++++++++++++++++++++
        // BEGIN UPDATE
        // ++++++++++++++++++++++++++

        public override void BeginUpdate(GameTime gameTime)
        {
            base.BeginUpdate(gameTime);

            // ++++ input ++++

            if (ObjectManager.Exists<MessageBox>() 
                || GameManager.Current.Transition != null
                || Teleporter != null)
            {
                input.Enabled = false;
            }
            else
            {
                input.Enabled = inputEnabled;
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

            // ++++ debug ++++

            if (input.IsKeyPressed(Keys.LeftShift, Input.State.Holding) || input.IsKeyPressed(Keys.RightShift, Input.State.Holding))
            {
                if (k_leftPressed)
                    Position = new Vector2(Position.X - 16 * Globals.T, Position.Y);
                if (k_rightPressed)
                    Position = new Vector2(Position.X + 16 * Globals.T, Position.Y);
                if (k_upPressed)
                    Position = new Vector2(Position.X, Position.Y - 9 * Globals.T);
                if (k_downPressed)
                    Position = new Vector2(Position.X, Position.Y + 9 * Globals.T);
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

            var obstacle = ObjectManager.CollisionBoundsFirstOrDefault<Obstacle>(this, X, Y);

            if (obstacle is Lava)
            {
                if (HP > 0)
                    HurtAndSpawnBack();
                else
                {
                    XVel *= .75f; YVel = Math.Min(-Gravity, YVel - .2f);
                    State = PlayerState.DEAD;                    
                }                
            }

            if (InvincibleTimer == 0 && HP > 0 
                && State != PlayerState.OBTAIN
                && State != PlayerState.BACKFACING)
            {                
                if (obstacle != null)
                {
                    var vec = Position - (obstacle.Center + new Vector2(0, 0));
                    var angle = vec.VectorToAngle();
                    Hit(obstacle.Damage, (float)angle);

                    if (obstacle is LaserObstacle)
                        InvincibleTimer = 0;
                    
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

            if (currentRoom != null)                
            {
                onWall = !hit &&
                    (GameManager.Current.Map.CollisionTile(X + (.5f * BoundingBox.Width + 1) * Math.Sign((int)Direction), Y + 4)
                    && Left > currentRoom.X + 2 && Right < currentRoom.X + currentRoom.BoundingBox.Width - 2);

                // TODO: fix ceil flag for room transitions

                onCeil = !hit && ObjectManager.CollisionPoints<Solid>(this, X, Y - BoundingBox.Height * .5f - 1)
                    .Where(o => o.Room == currentRoom && !(o is PushBlock)).Count() > 0;
            }
            inWater = GameManager.Current.Map.CollisionTile(X, Y + 4, GameMap.WATER_INDEX);

            if (!Stats.Abilities.HasFlag(PlayerAbility.CLIMB_WALL))
                onWall = false;
            if (!Stats.Abilities.HasFlag(PlayerAbility.CLIMB_CEIL))
                onCeil = false;

            if (onWall)
            {
                // transition from jumping to wall performance
                if (
                    (State == PlayerState.JUMP_UP && lastGroundY > Y + Globals.T)
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
                if (!Stats.Abilities.HasFlag(PlayerAbility.BREATHE_UNDERWATER) && !ObjectManager.Exists<MessageBox>())
                {
                    Oxygen = Math.Max(Oxygen - 1, 0);

                    if (Oxygen == 0)
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
                    if (State != PlayerState.SWIM && State != PlayerState.SWIM_DIVE_IN && State != PlayerState.SWIM_TURN_AROUND && State != PlayerState.BACKFACING)
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
                Oxygen = Math.Min(Oxygen + 10, MaxOxygen);

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

                attackDelay = Math.Max(attackDelay - 1, 0);

                if (k_attackHolding && !inWater && HP > 0
                && attackDelay == 0
                && !jumpControlDisabled
                && (State == PlayerState.IDLE
                || State == PlayerState.WALK
                || State == PlayerState.TURN_AROUND
                || State == PlayerState.JUMP_UP
                || State == PlayerState.JUMP_DOWN
                || State == PlayerState.GET_UP
                || State == PlayerState.LEVITATE))
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

                if (k_attackReleased)
                    attackDelay = 20;

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

            var currentFlowDirection = Direction.NONE;

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

                // ++++ pushblock evasion/respawn ++++
                
                var pushBlock = this.CollisionBoundsFirstOrDefault<PushBlock>(X, Y);
                if (pushBlock != null)
                {
                    if (pushBlock.IsFalling)
                        HurtAndSpawnBack();
                }

                if (KeyObject == null)
                {
                    // ++++ key blocks (when possessing keys in inventory) ++++

                    if (Stats.HeldKeys > 0)
                    {
                        var keyblock = this.CollisionBoundsFirstOrDefault<KeyBlock>(X + (int)Direction * 4, Y + YVel + .1f);
                        if (keyblock != null)
                        {
                            if (!keyblock.Unlocked)
                            {
                                keyblock.Unlock(X, Y);
                                UseKeyFromInventory();
                            }
                        }
                    }

                    // ++++ key doors (when possessing keys in inventory) ++++

                    if (Stats.HeldKeys > 0 && KeyObject == null)
                    {
                        var keyDoor = this.CollisionBoundsFirstOrDefault<DoorDisabler>(X + (int)Direction * 4, Y);
                        if (keyDoor != null)
                        {
                            if (keyDoor.Type == DoorDisabler.TriggerType.Key && !keyDoor.Open && !keyDoor.Unlocked)
                            {
                                var toolTip = new ToolTip(keyDoor, this, new Vector2(0, 8), 0);
                                if (k_upPressed)
                                {
                                    keyDoor.Unlock(X, Y, true);
                                    ObjectManager.DestroyAll<ToolTip>();
                                }
                            }
                            else
                            {
                                ObjectManager.DestroyAll<ToolTip>();
                            }
                        }
                    }
                }

                // ++++ spell EXP ++++

                var spellExp = this.CollisionBounds<SpellEXP>(X, Y);
                if (Orb != null)
                {
                    foreach (var s in spellExp)
                    {
                        if (s.Taken || !s.CanTake)
                            continue;

                        var currentSpellType = Stats.Spells.ElementAt(Stats.SpellIndex).Key;
                        var currentSpellLevel = Stats.Spells.ElementAt(Stats.SpellIndex).Value;

                        var maxSpellExpForLevel = GameResources.MaxEXP[currentSpellType][currentSpellLevel];

                        // only add EXP to spells other than "none" or special spells
                        if (maxSpellExpForLevel > 0)
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
                    item.Take(this);

                // ++++ teleporters ++++

                var tel = this.CollisionBoundsFirstOrDefault<Teleporter>(X, Y);

                if (tel != null && Teleporter == null)
                {
                    var toolTip = new ToolTip(tel, this, new Vector2(0, 8), 0);

                    if (!Stats.Teleporters.ContainsKey(tel.ID))
                        Stats.Teleporters.Add(tel.ID, tel.Position.ToPoint());

                    if (k_upPressed && !k_attackHolding && onGround)
                    {
                        XVel = 0;
                        YVel = 0;

                        Darkness.Current.Disable();

                        Teleporter = tel;
                        Teleporter.Active = true;
                        Teleporter.OnFinishedAnimation = () => {

                            var index = Stats.Teleporters.ToList().IndexOf(Stats.Teleporters.Where(o => o.Key == Teleporter.ID).FirstOrDefault());

                            if (index == Stats.Teleporters.Count - 1)
                                index = 0;
                            else
                                index++;

                            var newPosition = Stats.Teleporters.ElementAt(index).Value.ToVector2();

                            RoomCamera.Current.ChangeRoomsToPosition(newPosition, 1);
                            Teleporter.OnFinishedAnimation = null;

                        };
                        ObjectManager.DestroyAll<ToolTip>();
                    }
                }

                if (Teleporter != null)
                {
                    //if (Orb != null) Orb.Visible = false;
                    Gravity = 0;
                    State = PlayerState.JUMP_UP;
                    MoveTowards(Teleporter, 60);                    

                }
                
                // ++++ keys ++++
                
                var key = this.CollisionBoundsFirstOrDefault<Key>(X, Y + 1);
                if (key != null)
                {
                    var toolTip = new ToolTip(key, this, new Vector2(0, 8), 1);

                    if ((State == PlayerState.IDLE || State == PlayerState.WALK) && k_downPressed)
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

                var t = Globals.T;
                var npc = this.CollisionBoundsFirstOrDefault<NPC>(X, Y);
                if (npc != null && npc.Active)
                {
                    if (!ObjectManager.Exists<MessageBox>())
                        npc.ShowToolTip(this);

                    if (k_upPressed && !k_attackHolding)
                    {
                        npc.Interact(this);
                    }
                }

                // ++++ doors ++++

                var door = this.CollisionBoundsFirstOrDefault<Door>(X, Y);

                if ((npc == null || !npc.Active) && door != null && door.Open)
                {
                    if (k_upPressed && !k_attackHolding && (onGround || inWater))
                    {
                        XVel = 0;
                        YVel = -Gravity;

                        var pos = new Vector2(door.Center.X + door.Tx * Globals.T, door.Center.Y + door.Ty * Globals.T);
                        State = PlayerState.BACKFACING;
                        RoomCamera.Current.ChangeRoomsToPosition(pos, 0);

                    }
                }

                // ++++ shop items ++++

                var shopItem = this.CollisionBoundsFirstOrDefault<ShopItem>(X, Y);
                if (shopItem != null && !shopItem.Sold && shopItem.CanBeBought)
                {
                    if (!ObjectManager.Exists<MessageBox>())
                    {
                        var toolTip = new ToolTip(shopItem, this, new Vector2(0, 8), 0);
                    }
                    if (k_upPressed && !k_attackHolding)
                    {
                        shopItem.Buy();
                    }
                }

                // ++++ flow ++++
                var flow = this.CollisionBounds<Flow>(X, Y);
                foreach(var f in flow)
                {
                    if (!f.Active)
                        continue;
                    
                    flowDirection = f.Direction;
                    currentFlowDirection = f.Direction;

                    var flowPower = .3f;
                    switch (f.Direction)
                    {
                        case Direction.LEFT:
                            XVel = MathUtil.AtMost(XVel - flowPower, 2);
                            break;
                        case Direction.RIGHT:
                            XVel = MathUtil.AtMost(XVel + flowPower, 2);
                            break;
                        case Direction.UP:
                            if (onGround)
                                YVel -= 1;
                            YVel = MathUtil.AtMost(YVel - flowPower, 2);
                            break;
                        case Direction.DOWN:
                            YVel = MathUtil.AtMost(YVel + flowPower, 2);
                            break;                        
                    }
                }
            }

            var waterFalls = ObjectManager.FindAll<WaterFallEmitter>();

            foreach (var waterFall in waterFalls)
            {
                //bool isLava = (waterFall.Parent != null && (waterFall.Parent as WaterFall).IsLava);
                //bool partCol = false;

                if (waterFall.X < Right && waterFall.X + waterFall.BoundingBox.Width > Left && waterFall.Y < Top)
                {
                    foreach (var part in waterFall.Particles)
                    {
                        if (part.Position.X > Left - 2&& part.Position.Y + part.YVel > Top && part.Position.X < Right + 2 && part.Position.Y + part.YVel < Bottom)
                        {
                            //if (isLava) partCol = true;

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

                //if (partCol)
                //    HurtAndSpawnBack();
            }

            // +++++++++++++++++++++
            // ++++ state logic ++++
            // +++++++++++++++++++++

            var defaultMaxVel = 1.2f;

            // reset maxVel through this
            if (Direction != flowDirection)
                flowDirection = Direction.NONE;

            var maxVel = (Direction == flowDirection) ? 2 : defaultMaxVel;

            if (YVel != 0) onGround = false;

            if (onGround)
            {
                lastGroundY = Y;

                if (flowDirection != currentFlowDirection)
                    flowDirection = Direction.NONE;

            }

            // limbo

            if(State == PlayerState.LIMBO)
            {
                XVel = 0;
                YVel = -Gravity;

                Visible = false;
                if (Orb != null) Orb.Visible = false;

                limboTimer = Math.Max(limboTimer - 1, 0);

                if (!ObjectManager.Exists<KeyBurstEmitter>())
                    limboTimer = 0;

                if (limboTimer == 0)
                {
                    if (this.CollisionBoundsFirstOrDefault<PushBlock>(safePosition.X, safePosition.Y + 1) != null)
                    {
                        safePosition = GameManager.Current.SaveGame.playerPosition;
                        RoomCamera.Current.ChangeRoomsToPosition(safePosition, 0);
                    }
                    else
                    {
                        Position = safePosition;
                    }

                    Visible = true;
                    if (Orb != null) Orb.Visible = true;
                    
                    State = PlayerState.LIE;
                    lieTimer = 60;

                    //if (HP == 0)
                    //    State = PlayerState.DEAD;
                }

                return;
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

            if (Stats.Abilities.HasFlag(PlayerAbility.DOUBLE_JUMP))
                maxJumps = 2;
            else
                maxJumps = 1;

            // walk/idle -> jump
            if (State == PlayerState.IDLE 
                || State ==  PlayerState.WALK
                || State == PlayerState.GET_UP 
                || State == PlayerState.TURN_AROUND 
                || State == PlayerState.PUSH
                || State == PlayerState.JUMP_DOWN
                || State == PlayerState.JUMP_UP)
            {
                if (YVel > 0 && !onGround)
                    State = PlayerState.JUMP_DOWN;

                if (jumps < maxJumps)
                {
                    if (k_jumpPressed)
                    {
                        if (jumps > 0)
                            new SingularEffect(X, Y, 11);
                        jumps++;

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
            }
            // pushing
            if (State == PlayerState.PUSH)
            {
                if (pushBlock == null)
                {
                    pushBlock = this.CollisionBoundsFirstOrDefault<PushBlock>(X + Math.Sign((int)Direction), Y);

                    if (pushBlock != null && this.CollisionBounds(pushBlock, X - Math.Sign((int)Direction), Y))
                        pushBlock = null;

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
                MP = Math.Max(MP - .2f, 0);

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
                        if (k_jumpPressed)// && jumps == maxJumps)
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
                
                if (!onWall)
                    State = PlayerState.JUMP_DOWN;

                var jumpOff = false;

                if (Direction == Direction.LEFT)
                {
                    if (k_jumpPressed || k_rightHolding)
                    {
                        if (k_rightHolding)
                            Direction = Direction.RIGHT;
                        XVel = 1;
                        YVel = wallJumpVel;
                        State = PlayerState.JUMP_UP;
                        jumpOff = true;

                    }
                }
                else if (Direction == Direction.RIGHT)
                {
                    
                    if (k_jumpPressed || k_leftHolding)
                    {
                        if (k_leftHolding)
                            Direction = Direction.LEFT;
                        XVel = -1;
                        YVel = wallJumpVel;
                        State = PlayerState.JUMP_UP;
                        jumpOff = true;
                    }
                }

                if (jumpOff)
                {
                    // switch back the ground Y
                    lastGroundY = Math.Min(lastGroundY, lastGroundYbeforeWall);
                    jumps++;
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
                jumps = maxJumps;

                //if (YVel > 0)
                //{
                //    if (k_jumpPressed)
                //    {
                //        YVel = -1;
                //        State = PlayerState.JUMP_UP;
                //    }
                //}

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
                if (k_leftHolding || k_rightHolding)
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
                YVel = -Gravity;

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
            if (State == PlayerState.BACKFACING)
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
                          && State != PlayerState.BACKFACING
                          && State != PlayerState.HIT_AIR);

            var yVelBeforeCollision = YVel;

            var g = this.MoveAdvanced(moveWithPlatforms);
            
            if (inWater || onWall || onCeil)                
                jumps = Math.Min(jumps, 1);
                        
            if (g)
            {
                // TODO: FIX GAP TO GET TO GROUND!

                //var groundBlock = this.CollisionBoundsFirstOrDefault<Solid>(X, Y + 3);
                //if (groundBlock != null)
                //{
                //    if (YVel > 0)
                //        Move(0, Math.Abs(groundBlock.Top - Bottom - Gravity));
                //}

                jumps = 0;
                if (!onGround && yVelBeforeCollision > Gravity && !inWater && State != PlayerState.BACKFACING)
                {
                    new SingularEffect(X + XVel, Y, 12);
                }

                onGround = true;

                // transition from falling to getting up again
                if (State == PlayerState.JUMP_UP
                    || State == PlayerState.JUMP_DOWN
                    || State == PlayerState.WALL_CLIMB
                    || State == PlayerState.LEVITATE
                    || State == PlayerState.CARRYOBJECT_IDLE
                    || State == PlayerState.CARRYOBJECT_WALK)
                {
                    if (lastGroundY < Y - 9 * Globals.T && !Stats.Abilities.HasFlag(PlayerAbility.NO_FALL_DAMAGE))
                    {
                        var eff = new SingularEffect(X, Y + 8);
                        Hit(3);
                        State = PlayerState.LIE;
                        lieTimer = 120;
                    }
                    else if (KeyObject == null)
                    {
                        if (lastGroundY < Y - Globals.T)
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
            if (boundX > GameManager.Current.Map.Width * Globals.T - 4) { XVel = 0; }
            if (boundY < 4) { YVel = 0; }
            if (boundY > GameManager.Current.Map.Height * Globals.T - 4) { YVel = 0; }

            boundX = boundX.Clamp(4, GameManager.Current.Map.Width * Globals.T - 4);
            boundY = boundY.Clamp(4, GameManager.Current.Map.Height * Globals.T - 4);

            Position = new Vector2(boundX, boundY);
            levitationEmitter.Position = Position;

            // ++++ safe position ++++

            var roomY = RoomCamera.Current.CurrentRoom != null ? RoomCamera.Current.CurrentRoom.Y : Top;

            if (!hit && onGround && !inWater && this.CollisionRectangleFirstOrDefault<Solid>(Left - Globals.T, Top - Globals.T, Right + Globals.T, Bottom) == null
                && this.CollisionRectangleFirstOrDefault<PushBlock>(Left, roomY, Right, Bottom) == null)
            {
                var tmp = new Vector2(MathUtil.Div(X, Globals.T) * Globals.T + 8, MathUtil.Div(Y, Globals.T) * Globals.T + 8);

                var groundCollider = this.CollisionPointFirstOrDefault<Collider>(tmp.X, tmp.Y + 8);

                if (groundCollider != null 
                    && !(groundCollider is FallingPlatform || groundCollider is IMovable || groundCollider is PushBlock || groundCollider is MovingPlatform))
                    safePosition = new Vector2(MathUtil.Div(X, Globals.T) * Globals.T + 8, MathUtil.Div(Y, Globals.T) * Globals.T + 8);
            }
            
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
                case PlayerState.BACKFACING:
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
            new FollowFont(X, Y - 12, $"Level Up!");
        }

        private void CreateSpellDownEffect()
        {
            new FollowFont(X, Y - 12, $"Level Down!");
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);
            
            if (CoinCounter > 0)
            {
                float coinAlpha = coinTimeout / (.5f * maxCoinTimeout);
                coinFont.Color = new Color(coinFont.Color, coinAlpha);
                coinFont.Draw(sb, X, Y - Globals.T, $"+{CoinCounter}", depth: Globals.LAYER_UI - .0001f);
            }
            
            if (Oxygen < MaxOxygen && HP > 0)
            {
                float rel = 1 - (float)Oxygen / (float)MaxOxygen;
                var r = 3 + (int)(76 * rel);
                var g = 243 - (int)(240 * rel);
                var b = 243;

                var fg = new Color(r, g, b);
                var bg = new Color(20, 113, 126);

                sb.DrawBar(Position + new Vector2(0, 12), (int)(1.5 * Globals.T), Oxygen / (float)MaxOxygen, fg, bg, height: 2, border: false, depth: Globals.LAYER_UI - .001f);
            }

            // draws safe-rect for debug
            //sb.DrawRectangle(new RectF(safePosition.X - 8, safePosition.Y - 8, 16, 16), Color.Red, false, 1);
        }
    }
}