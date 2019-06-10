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

namespace Leore.Main
{
    public enum OrbState
    {
        FOLLOW,
        IDLE,
        ATTACK
    }

    public enum SpellType
    {
        NONE = 0,
        STAR,
        CRIMSON_ARC,
        VOID
        //BOOMERANG,
        //ROCK,
        //LIGHTNING,
        //FIRE
    }

    public enum SpellLevel
    {
        ONE = 1,
        TWO = 2,
        THREE = 3
    }

    public class Orb : GameObject
    {
        public Direction Direction => player.Direction;
        public OrbState State { get; set; }
        public SpellType Type { get; set; } = SpellType.NONE;
        public SpellLevel Level { get; set; } = SpellLevel.ONE;

        private Player player { get => Parent as Player; }
        private Vector2 targetPosition;        
        private int headBackTimer;        
        private int cooldown;

        private float alpha = 1;
        private int alphaTimeout;
        private Direction lastChangeDir;
        
        private double t;
        private Vector2 lastPosition;
        
        private float offY;

        public Orb(Player player) : base(player.X, player.Y)
        {
            Scale = new Vector2(1);

            BoundingBox = new RectF(-4, -4, 8, 8);
            DrawOffset = new Vector2(8, 8);
            Depth = player.Depth + .0001f;

            Parent = player;
            targetPosition = player.Position;
            lastPosition = targetPosition;            
        }

        public override void BeginUpdate(GameTime gameTime)
        {
            base.BeginUpdate(gameTime);

            targetPosition += new Vector2(player.XVel, player.YVel);

            cooldown = Math.Max(cooldown - 1, 0);

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
            
            switch (State)
            {
                case OrbState.FOLLOW:
                    headBackTimer = 60;
                    targetPosition = player.Position + new Vector2(-Math.Sign((int)player.Direction) * 10, -6);

                    t = (t + .05f) % (2 * Math.PI);

                    XVel = (float)Math.Sin(t) * .01f * Math.Sign((int)player.Direction);
                    YVel = (float)Math.Cos(t) * .1f;

                    MoveTowards(targetPosition, 12);
                    Move(XVel, YVel);

                    break;
                case OrbState.IDLE:

                    MoveTowards(targetPosition, 6);

                    headBackTimer = Math.Max(headBackTimer - 1, 0);
                    if (headBackTimer == 0)
                    {
                        State = OrbState.FOLLOW;
                    }

                    break;
                case OrbState.ATTACK:
                    headBackTimer = 20;

                    // positioning
                    switch (Type)
                    {
                        case SpellType.CRIMSON_ARC:

                            var arcDst = 10;

                            offY += .25f * (int)player.LookDirection;
                            offY = Math.Sign(offY) * Math.Min(Math.Abs(offY), arcDst);

                            targetPosition = player.Position + new Vector2(Math.Sign((int)player.Direction) * (arcDst - .5f * Math.Abs(offY) / arcDst), offY);
                            break;
                        default:
                            targetPosition = player.Position + new Vector2(Math.Sign((int)player.Direction) * 14, 14 * Math.Sign((int)player.LookDirection));
                            break;
                    }

                    MoveTowards(targetPosition, 6);
                    Move(player.XVel, player.YVel);

                    XVel *= .8f;
                    YVel *= .8f;

                    // attack projectiles

                    //spellEmitter.Active = true;

                    if (player.MP >= GameResources.MPCost[Type][Level])
                    {
                        if (cooldown == 0)
                        {
                            player.MP -= GameResources.MPCost[Type][Level];

                            switch (Type)
                            {
                                case SpellType.STAR: // ++++ STAR ++++
                                    { // <- because reusing names
                                        switch (Level)
                                        {
                                            case SpellLevel.ONE:
                                                cooldown = 20;
                                                break;
                                            case SpellLevel.TWO:
                                                cooldown = 15;
                                                break;
                                            case SpellLevel.THREE:
                                                cooldown = 8;
                                                break;
                                        }

                                        var proj = new StarProjectile(X, Y, Level);
                                        var starDegAngle = MathUtil.VectorToAngle(new Vector2(targetPosition.X - player.X, 0));

                                        var starCoilX = (float)MathUtil.LengthDirX(starDegAngle);
                                        var starCoilY = (float)MathUtil.LengthDirY(starDegAngle);

                                        proj.XVel = starCoilX * 3;
                                        proj.YVel = starCoilY * 3;

                                        XVel += -2 * starCoilX;
                                        YVel += -2 * starCoilY;
                                    }
                                    break;
                                case SpellType.CRIMSON_ARC: // ++++ CRIMSON ++++

                                    //cooldown = 60;

                                    ObjectManager.Enable<CrimsonBow>();

                                    if (!ObjectManager.Exists<CrimsonBow>())
                                    {
                                        new CrimsonBurstEmitter(X, Y);
                                        new CrimsonBow(this);
                                    }
                                    
                                    break;

                                case SpellType.VOID:                                    
                                    {
                                        //if (!ObjectManager.Exists<VoidProjectile>())
                                        {
                                            cooldown = 60;

                                            VoidProjectile.Create(X, Y, this);

                                            //new VoidProjectile(X, Y, this);
                                        }
                                    }

                                    break;

                                // TODO: other spells!!!
                                default:
                                    break;
                            }
                        }
                    } else
                    {
                        if (cooldown == 0)
                        {
                            XVel = -2 + (float)RND.Next * 2;
                            YVel = -2 + (float)RND.Next * 2;
                            new StarEmitter(X, Y);
                            cooldown = 25;
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
