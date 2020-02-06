using Leore.Main;
using Microsoft.Xna.Framework;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPG.Map;
using Leore.Objects.Effects.Emitters;
using Leore.Objects.Items;
using System.Diagnostics;
using Leore.Objects.Effects;
using Microsoft.Xna.Framework.Graphics;
using SPG.Objects;
using Leore.Objects.Level;
using SPG.Draw;

namespace Leore.Objects.Enemies
{
    public class BossShadowLizard : Boss
    {
        private enum State
        {
            IDLE,
            FOLLOW_PLAYER,
            RETREAT,
            ATTACK,
            CHARGE,
            DIE
        }
        
        private State state;
        private Player player => GameManager.Current.Player;
        private RoomCamera camera => RoomCamera.Current;

        float alpha;

        int attacks;
        int attackTimer;
        int maxFollowTimer = 10 * 60;
        int followTimer;
        int maxRetreatTimer = 2 * 60;
        int retreatTimer;

        public BossShadowLizard(float x, float y, Room room, string setCondition) : base(x, y, room, setCondition)
        {
            HP = 200;
            
            AnimationTexture = AssetManager.BossShadowLizard;
            DrawOffset = new Vector2(48);
            BoundingBox = new SPG.Util.RectF(-16, -16, 32, 32);

            Depth = Globals.LAYER_BG2 + .0001f;
            
            knockback = .03f;

            Direction = Direction.RIGHT;

            followTimer = maxFollowTimer;
            state = State.FOLLOW_PLAYER;

            alpha = .5f;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var hpPercent = HP / MaxHP;

            switch (state)
            {
                case State.IDLE:
                    SetAnimation(0, 3, .1f, true);                    
                    break;
                case State.FOLLOW_PLAYER:
                    {
                        followTimer = Math.Max(followTimer - 1, 0);
                        
                        var maxSpd = .7f;
                        var animSpd = Math.Min(Math.Max(Math.Max(Math.Abs(XVel), Math.Abs(YVel)), .1f), maxSpd) / maxSpd;

                        SetAnimation(4, 7, .15f * animSpd, true);

                        var targetAngle = (float)MathUtil.VectorToAngle(new Vector2(player.X - X, player.Y - Y), false);
                        var lx = .02f * (float)MathUtil.LengthDirX(targetAngle);
                        var ly = .02f * (float)MathUtil.LengthDirY(targetAngle);

                        XVel += lx;
                        YVel += ly;

                        XVel = MathUtil.AtMost(XVel, maxSpd);
                        YVel = MathUtil.AtMost(YVel, maxSpd);
                        
                        Angle = (float)MathUtil.VectorToAngle(new Vector2(XVel, YVel), true);
                        
                        if (followTimer == 0)
                        {
                            followTimer = maxFollowTimer;
                            retreatTimer = maxRetreatTimer;

                            state = State.RETREAT;

                            var angle = (float)MathUtil.VectorToAngle(new Vector2(X - player.X, Y - player.Y - 1 * Globals.T), false);
                            XVel = maxSpd * (float)MathUtil.LengthDirX(angle);
                            YVel = maxSpd * (float)MathUtil.LengthDirY(angle);

                            Angle = (float)MathUtil.DegToRad(angle + 180);
                        }
                    }
                    break;
                case State.RETREAT:
                    {
                        var angle = (float)MathUtil.VectorToAngle(new Vector2(X - player.X, Y - player.Y - 1 * Globals.T), false);
                        XVel = .5f * (float)MathUtil.LengthDirX(angle);
                        YVel = .5f * (float)MathUtil.LengthDirY(angle);

                        Angle = (float)MathUtil.DegToRad(angle + 180);

                        SetAnimation(4, 7, .1f, true);
                        
                        retreatTimer = Math.Max(retreatTimer - 1, 0);

                        if (retreatTimer == 0)
                        {
                            state = State.ATTACK;
                        }
                    }
                    break;
                case State.ATTACK:
                    {
                        XVel = 0;
                        YVel = 0;
                        SetAnimation(0, 3, .1f, true);
                        
                        var targetAngle = (float)MathUtil.VectorToAngle(new Vector2(player.X - X, player.Y - Y), false);
                        Angle = (float)MathUtil.DegToRad(targetAngle - MathUtil.RadToDeg(Angle) / 40);
                        
                        attackTimer = Math.Max(attackTimer - 1, 0);
                        if (attackTimer == 0)
                        {
                            attacks++;
                            attackTimer = 90;

                            if (attacks < 4)
                            {
                                for (var i = -1; i < 2; i++)
                                {

                                    //var angle = (float)MathUtil.VectorToAngle(new Vector2(player.X - X, player.Y - Y), false) + i * 20;
                                    var angle = MathUtil.RadToDeg(Angle) + i * 20;
                                    SpawnProjectile(angle);

                                }
                            }
                            else
                            {
                                SetAnimation(0, 3, .2f, true);

                                if (attacks > 4)
                                {
                                    attacks = 0;
                                    state = State.CHARGE;
                                    attackTimer = 0;
                                }
                            }
                        }                        
                    }
                    break;
                case State.CHARGE:
                    {
                        SetAnimation(4, 7, .3f, true);

                        XVel = 2f * (float)MathUtil.LengthDirX(MathUtil.RadToDeg(Angle));
                        YVel = 2f * (float)MathUtil.LengthDirY(MathUtil.RadToDeg(Angle));

                        if (this.CollisionBoundsFirstOrDefault<Solid>(X + 2 * XVel, Y + 2 * YVel) != null)
                        {
                            XVel = 0;
                            YVel = 0;
                            state = State.IDLE;
                            for (var i = 0; i < 360; i += 30)
                            {
                                SpawnProjectile(i);
                            }

                            camera.Shake(2 * 60, () =>
                            {
                                state = State.FOLLOW_PLAYER;                                
                            });
                        }
                    }
                    break;

            }

            if (this.CollisionBoundsFirstOrDefault<Solid>(X + XVel, Y) == null)
            {
                Move(XVel, 0);
            }
            else
            {
                XVel = 0;
            }

            if (this.CollisionBoundsFirstOrDefault<Solid>(X, Y + YVel) == null)
            {
                Move(0, YVel);
            }
            else
            {
                YVel = 0;
            }
            
            Color = new Color(Color, alpha);
        }

        bool InsideRoomArea()
        {
            var clt = 2.5f * Globals.T;
            return X > Room.X + clt && X < Room.X + Room.BoundingBox.Width - clt
                && Y > Room.Y + clt && Y < Room.Y + Room.BoundingBox.Height - clt;
        }

        private void SpawnProjectile(double degAngle)
        {
            var proj = new DefaultEnemyProjectile(X, Y);
            proj.Texture = AssetManager.Projectiles[14];
            var lightSource = new LightSource(proj);
            lightSource.Active = true;
            lightSource.Scale = new Vector2(.3f);

            proj.XVel = 1.5f * (float)MathUtil.LengthDirX(degAngle);
            proj.YVel = 1.5f * (float)MathUtil.LengthDirY(degAngle);
            

        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            if (state != State.DIE)
            {
                base.Draw(sb, gameTime);
            }
            else
            {
            //    deathAlpha = Math.Min(deathAlpha + .01f, 1);

            //    sb.Draw(AssetManager.BossGiantBat[8], Position, null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth);
            //    sb.Draw(AssetManager.BossGiantBat[9], Position, null, new Color(Color, deathAlpha), Angle, DrawOffset, Scale, SpriteEffects.None, Depth + .0001f);
            }            
        }

        public override void Hit(int hitPoints, float degAngle)
        {
            base.Hit(hitPoints, degAngle);            
        }

        public override void Destroy(bool callGC = false)
        {
            base.Destroy();
        }

        public override void OnDeath()
        {
            //base.OnDeath();
            base.OnDeath();
            
        }
    }
}
