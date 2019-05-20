using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platformer.Objects.Effects.Emitters;
using Platformer.Objects.Projectiles;
using SPG.Objects;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Main.Orbs
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
        BOOMERANG,
        ROCK,
        LIGHTNING,
        FIRE
    }

    public enum SpellLevel
    {
        ONE = 1,
        TWO = 2,
        THREE = 3
    }

    public class Orb : GameObject
    {
        public Direction Direction { get; set; }
        public OrbState State { get; set; }
        public SpellType Type { get; set; } = SpellType.NONE;
        public SpellLevel Level { get; set; } = SpellLevel.ONE;

        private Player player { get => Parent as Player; }
        private Vector2 targetPosition;        
        private int headBackTimer;        
        private int cooldown;

        public Dictionary<SpellType, Dictionary<SpellLevel, int>> MpCost { get; private set; } = new Dictionary<SpellType, Dictionary<SpellLevel, int>>();

        private double t;
        private Vector2 lastPosition;

        public Orb(Player player) : base(player.X, player.Y)
        {
            Scale = new Vector2(.5f);

            BoundingBox = new RectF(-4, -4, 8, 8);
            DrawOffset = new Vector2(16, 16);
            Depth = player.Depth + .0001f;

            Parent = player;
            targetPosition = player.Position;
            lastPosition = targetPosition;

            MpCost.Add(SpellType.NONE, new Dictionary<SpellLevel, int>
            {
                {SpellLevel.ONE, 0 },
                {SpellLevel.TWO, 0 },
                {SpellLevel.THREE, 0 },
            });

            MpCost.Add(SpellType.STAR, new Dictionary<SpellLevel, int>
            {
                {SpellLevel.ONE, 3 },
                {SpellLevel.TWO, 5 },
                {SpellLevel.THREE, 1 },
            });

        }

        public override void BeginUpdate(GameTime gameTime)
        {
            base.BeginUpdate(gameTime);

            targetPosition += new Vector2(player.XVel, player.YVel);

            cooldown = Math.Max(cooldown - 1, 0);

            switch (State)
            {
                case OrbState.FOLLOW:

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

                    targetPosition = player.Position + new Vector2(Math.Sign((int)player.Direction) * 14, 14 * Math.Sign((int)player.LookDirection));

                    MoveTowards(targetPosition, 6);
                    Move(player.XVel, player.YVel);

                    XVel *= .8f;
                    YVel *= .8f;

                    // attack projectiles

                    
                    if (player.MP >= MpCost[Type][Level] && cooldown == 0)
                    {
                        player.MP -= MpCost[Type][Level];

                        switch (Type)
                        {
                            case SpellType.STAR:
                                
                                switch (Level)
                                {
                                    case SpellLevel.ONE:
                                        cooldown = 25;
                                        break;
                                    case SpellLevel.TWO:
                                        cooldown = 15;
                                        break;
                                    case SpellLevel.THREE:
                                        cooldown = 8;
                                        break;
                                }

                                var proj = new StarProjectile(X, Y, Level);

                                // recoil + projectile speed:

                                var degAngle = MathUtil.VectorToAngle(new Vector2(targetPosition.X - player.X, targetPosition.Y - player.Y));

                                var coilX = (float)MathUtil.LengthDirX(degAngle);
                                var coilY = (float)MathUtil.LengthDirY(degAngle);

                                proj.XVel = coilX * 3;
                                proj.YVel = coilY * 3;

                                XVel += -2 * coilX;
                                YVel += -2 * coilY;
                                
                                break;
                            // TODO: other spells!!!
                            default:
                                break;
                        }
                    } else
                    {
                        if (cooldown == 0)
                        {
                            XVel = -2 + (float)RND.Next * 2;
                            YVel = -2 + (float)RND.Next * 2;
                            new OuchEmitter(X, Y);
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

            sb.Draw(AssetManager.Orbs[(int)Type], Position, null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth);
            
            //if (State == OrbState.ATTACK)
            //    sb.Draw(AssetManager.OrbReflection, Position, null, new Color(Color, .75f), Angle, DrawOffset, Scale, SpriteEffects.None, Depth + .0001f);
        }
    }
}
