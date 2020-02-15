using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Leore.Objects;
using Leore.Objects.Effects;
using Leore.Objects.Effects.Emitters;
using Leore.Objects.Projectiles;
using Leore.Resources;
using SPG.Objects;
using SPG.Util;
using System;
using System.Collections.Generic;
using Leore.Objects.Level;
using System.Diagnostics;

namespace Leore.Main
{
    public enum OrbState
    {
        FOLLOW,
        IDLE,
        ATTACK
    }

    /// <summary>
    /// Depending on the element, certain objects behave differently (are immune or triggered...)
    /// </summary>
    public enum SpellElement
    {
        NONE = 0,

        LIGHT,
        DARK,

        FIRE,
        ICE,

        PLANT,
        STONE        
    }

    public enum SpellType
    {
        NONE = 0,                   // = SpellElement.NONE,
        CRIMSON_ARC = 2,            // = SpellElement.NONE,
        SNATCH_KEYS = 4,            // = SpellElement.NONE,

        STAR = 1,                   // = SpellElement.LIGHT,
        VOID = 3,                   // = SpellElement.DARK,

        FIRE = 5,                   // = SpellElement.FIRE,
        ICE = 6,                    // = SpellElement.ICE

        ROCK = 7,                   // = SpellElement.NONE
        PLANT = 8,                  // = SpellElement.NONE
        LIGHTNING = 9               // = SpellElement.NONE
    }

    public enum SpellLevel
    {
        ONE = 1,
        TWO = 2,
        THREE = 3
    }

    public class Orb : GameObject, IKeepEnabledAcrossRooms, IPlayerTransferrable
    {
        public Direction Direction => player.Direction;
        public OrbState State { get; set; }
        public SpellType Type { get; set; } = SpellType.NONE;
        public SpellLevel Level { get; set; } = SpellLevel.ONE;

        private Player player { get => Parent as Player; }
        public Vector2 TargetPosition { get; private set; }
        private int headBackTimer;        
        public int Cooldown { get; set; }

        private float alpha = 1;
        private int alphaTimeout;
        private Direction lastChangeDir;
        
        private double t;
        private Vector2 lastPosition;
        
        private float offY;

        private LightSource light;

        private float lightScaleNormal = .75f;
        private float lightScaleDark = .3f;

        private float targetAngle;

        public Orb(Player player) : base(player.X, player.Y)
        {
            Scale = new Vector2(1);

            BoundingBox = new RectF(-4, -4, 8, 8);
            DrawOffset = new Vector2(8, 8);
            Depth = player.Depth + .0001f;

            Parent = player;
            TargetPosition = player.Position;
            lastPosition = TargetPosition;

            light = new LightSource(this);
            light.Active = true;            
        }

        public override void BeginUpdate(GameTime gameTime)
        {
            base.BeginUpdate(gameTime);

            light.Scale = new Vector2(Type == SpellType.VOID ?  lightScaleDark : lightScaleNormal);
            TargetPosition += new Vector2(player.XVel, player.YVel);
                        
            Cooldown = Math.Max(Cooldown - 1, 0);

            if (alphaTimeout > 0)
            {
                alphaTimeout = Math.Max(alphaTimeout - 1, 0);
                State = OrbState.IDLE;

                if (alphaTimeout == 0)
                {
                    Position -= new Vector2(7 * Math.Sign((int)lastChangeDir), 0);
                }
            } else
                alpha = Math.Min(alpha + .1f, 1);

            Scale = new Vector2(1, 1);

            if (Type == SpellType.SNATCH_KEYS)
            {
                Scale = new Vector2((int)player.Direction, 1);
            }

            // enable for bending orb
            //if (Type == SpellType.SNATCH_KEYS)
            //{
            //    if (player.Direction == Direction.LEFT)
            //        targetAngle = (float)MathUtil.VectorToAngle(new Vector2(1, -Math.Sign((int)player.LookDirection)), true);
            //    if (player.Direction == Direction.RIGHT)
            //        targetAngle = (float)MathUtil.VectorToAngle(new Vector2(1, Math.Sign((int)player.LookDirection)), true);
            //    Scale = new Vector2((int)player.Direction, 1);
            //} else
            //{
            //    targetAngle = 0;
            //}

            targetAngle = 0;

            Angle += (targetAngle - Angle) / 6f;

            switch (State)
            {
                case OrbState.FOLLOW:
                    headBackTimer = 60;
                    TargetPosition = player.Position + new Vector2(-Math.Sign((int)player.Direction) * 10, -6);

                    t = (t + .05f) % (2 * Math.PI);

                    XVel = (float)Math.Sin(t) * .01f * Math.Sign((int)player.Direction);
                    YVel = (float)Math.Cos(t) * .1f;

                    MoveTowards(TargetPosition, 12);
                    Move(XVel, YVel);
                    if (player.MovingPlatform != null)
                        Move(player.MovingPlatform.XVel, player.MovingPlatform.YVel);
                    
                    break;
                case OrbState.IDLE:

                    MoveTowards(TargetPosition, 6);

                    headBackTimer = Math.Max(headBackTimer - 1, 0);
                    if (headBackTimer == 0)
                    {
                        State = OrbState.FOLLOW;
                    }

                    break;
                case OrbState.ATTACK:
                    headBackTimer = 5;

                    // positioning
                    switch (Type)
                    {
                        case SpellType.CRIMSON_ARC:

                            var arcDst = 10;

                            offY += .25f * (int)player.LookDirection;
                            offY = Math.Sign(offY) * Math.Min(Math.Abs(offY), arcDst);

                            TargetPosition = player.Position + new Vector2(Math.Sign((int)player.Direction) * (arcDst - .5f * Math.Abs(offY) / arcDst), offY);
                            break;
                        case SpellType.SNATCH_KEYS:                            
                            if (!ObjectManager.Exists<KeySnatchProjectile>() && Cooldown == 0) {
                                new CrimsonBurstEmitter(X, Y)
                                {
                                    ParticleColors = new List<Color> { Color.White },
                                    SpawnRate = 5
                                };
                                if (player.LookDirection != Direction.NONE)
                                    new SingularEffect(X, Y, 10);
                            }
                            TargetPosition = player.Position + new Vector2(Math.Sign((int)player.Direction) * 8, 2 * Math.Sign((int)player.LookDirection));
                            //Position = TargetPosition;
                            break;
                        case SpellType.FIRE:
                            TargetPosition = player.Position + new Vector2(Math.Sign((int)player.Direction) * 0, 0);
                            break;
                        case SpellType.ICE:
                            TargetPosition = player.Position + new Vector2(Math.Sign((int)player.Direction) * 8, 4 * Math.Sign((int)player.LookDirection));
                            break;
                        default:
                            TargetPosition = player.Position + new Vector2(Math.Sign((int)player.Direction) * 14, 14 * Math.Sign((int)player.LookDirection));
                            break;
                    }

                    MoveTowards(TargetPosition, 6);
                    Move(player.XVel, player.YVel);
                    if (player.MovingPlatform != null)
                        Move(player.MovingPlatform.XVel, player.MovingPlatform.YVel);


                    XVel *= .8f;
                    YVel *= .8f;

                    // attack projectiles                    
                    if (player.MP >= GameResources.MPCost[Type][Level])
                    {
                        if (Cooldown == 0)
                        {
                            player.MP -= GameResources.MPCost[Type][Level];

                            switch (Type)
                            {
                                case SpellType.STAR: // ++++ STAR ++++
                                    { // <- because reusing names
                                        switch (Level)
                                        {
                                            case SpellLevel.ONE:
                                                Cooldown = 20;
                                                break;
                                            case SpellLevel.TWO:
                                                Cooldown = 15;
                                                break;
                                            case SpellLevel.THREE:
                                                Cooldown = 8;
                                                break;
                                        }
                                        
                                        var proj = new StarProjectile(TargetPosition.X, Y, Level);
                                        var starDegAngle = MathUtil.VectorToAngle(new Vector2(TargetPosition.X - player.X, 0));

                                        var starCoilX = (float)MathUtil.LengthDirX(starDegAngle);
                                        var starCoilY = (float)MathUtil.LengthDirY(starDegAngle);

                                        proj.XVel = starCoilX * 3;
                                        proj.YVel = starCoilY * 3;

                                        XVel += -2 * starCoilX;
                                        YVel += -2 * starCoilY;
                                    }
                                    break;
                                case SpellType.CRIMSON_ARC: // ++++ CRIMSON ++++

                                    Cooldown = 60;

                                    ObjectManager.Enable<CrimsonSpell>();
                                    
                                    if (!ObjectManager.Exists<CrimsonSpell>())
                                    {
                                        new CrimsonBurstEmitter(X, Y);
                                        new CrimsonSpell(this);
                                    } else
                                    {
                                        player.MP += GameResources.MPCost[Type][Level];
                                    }
                                    
                                    break;

                                case SpellType.VOID: // ++++ VOID ++++
                                    Visible = false;
                                    VoidProjectile.Create(X, Y, this);
                                    break;

                                case SpellType.SNATCH_KEYS: // ++++ Snatch Keys ++++

                                    //if (MathUtil.Euclidean(Position, TargetPosition) < 2)
                                    {
                                        KeySnatchProjectile.Create(TargetPosition.X, TargetPosition.Y);
                                        Cooldown = 30;
                                    }
                                    break;

                                case SpellType.FIRE: // ++++ Fire ++++

                                    Cooldown = 30;
                                    player.MP += GameResources.MPCost[Type][Level];

                                    FireSpell.Create(TargetPosition.X, TargetPosition.Y, Level);

                                    break;
                                case SpellType.ICE: // ++++ Boomerang ++++                                    

                                    switch (Level)
                                    {
                                        case SpellLevel.ONE:
                                        case SpellLevel.TWO:                                            
                                            new IceProjectile(player.X, player.Y, Level);
                                            break;
                                        case SpellLevel.THREE:
                                            IceSpell.Create(TargetPosition.X, TargetPosition.Y, Level);
                                            break;
                                    }
                                                                        
                                    break;
                                // TODO: other spells!!!
                                default:
                                    break;
                            }
                        }
                    }
                    else
                    {
                        var shake = true;

                        if (ObjectManager.Exists<CrimsonSpell>() || ObjectManager.Exists<FireSpell>())
                            shake = false;

                        if (Type == SpellType.SNATCH_KEYS)
                        {
                            Cooldown++;
                        }
                        
                        if (shake)
                        {
                            if (Cooldown == 0)
                            {
                                XVel = -2 + (float)RND.Next * 2;
                                YVel = -2 + (float)RND.Next * 2;
                                new StarEmitter(X, Y);
                                Cooldown = 25;
                            }
                        }
                        
                    }

                    Move(XVel, YVel);
                    break;
            }


            lastPosition = Position;
        }
        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            //base.Draw(sb, gameTime);
            sb.Draw(AssetManager.Orbs[(int)Type], Position, null, new Color(Color, alpha), Angle, DrawOffset, Scale, SpriteEffects.None, Depth);            
        }

        public void ChangeType(Direction direction)
        {
            if (alphaTimeout == 0)
            {
                var eff = new SingularEffect(X, Y, 4);                
            }
            lastChangeDir = direction;            
            State = OrbState.IDLE;

            alphaTimeout = 20;
            alpha = 0;
        }        
    }
}
