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
using Leore.Resources;

namespace Leore.Objects.Enemies
{
    public class BossShadowLizard : Boss
    {
        private enum State
        {
            IDLE,
            SNEAK,
            FOLLOW_PLAYER,
            RETREAT,
            ATTACK,
            CHARGE,
            DIE
        }
        
        private State state;
        private Player player => GameManager.Current.Player;
        private RoomCamera camera => RoomCamera.Current;

        private float alpha;
        private float maxAlpha = 1;
        private float minAlpha = 0;//.3f;

        private int attacks;
        private int attackTimer;
        private int maxFollowTimer = 10 * 60;
        private int followTimer;
        private int maxRetreatTimer = 2 * 60;
        private int retreatTimer;
        private int sneakTimer;

        private float eyeFrame;

        private float deathAlpha;

        private int deathTimer = 3 * 60;
        private bool preventDeath;
        private bool sawLight;
        private int footPrint;
        private int sneakTransitionTimer;
        
        private float projectileTimer;

        private float torchTimer;
        
        private int invincibleTimer;
        
        public BossShadowLizard(float x, float y, Room room, string setCondition) : base(x, y, room, setCondition)
        {
            HP = GameResources.BossShadowLizard.HP;
            Damage = GameResources.BossShadowLizard.Damage;
            EXP = GameResources.BossShadowLizard.EXP;

            AnimationTexture = AssetManager.BossShadowLizard;
            DrawOffset = new Vector2(48);
            BoundingBox = new SPG.Util.RectF(-16, -16, 32, 32);

            Depth = Globals.LAYER_BG2 + .0001f;
            
            knockback = .1f;

            Direction = Direction.RIGHT;

            followTimer = maxFollowTimer;
            state = State.SNEAK;

            alpha = 0;

            //ActivateRandomTorch();
        }

        private void ActivateRandomTorch()
        {
            var torches = ObjectManager.FindAll<Torch>();

            int lastIndex = -1;
            int index = -1;
            foreach (var torch in torches)
            {
                if (torch.Active)
                    lastIndex = torches.FindIndex(o => o == torch);
                torch.Active = false;                
            }
            while (index == lastIndex || index == -1)
            {
                index = RND.Int(torches.Count - 1);
            }
            torches[index].Active = true;
            SpellEXP.Spawn(torches[index].Center.X, torches[index].Center.Y, 15);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Damage = (alpha > minAlpha) ? GameResources.BossShadowLizard.Damage : 0;
            IgnoreProjectiles = (Damage == 0);
            var hpPercent = (float)HP / (float)MaxHP;

            invincibleTimer = Math.Max(invincibleTimer - 1, 0);

            torchTimer = Math.Max(torchTimer - 1, 0);
            if (torchTimer == 0)
            {
                ActivateRandomTorch();
                torchTimer = 8 * 60;
            }

            var dst = 2.5f * Globals.T;
            var torch = this.CollisionRectangleFirstOrDefault<Torch>(X - dst, Y - dst, X + dst, Y + dst);
            if (torch != null && torch.Active)
            {
                if (!sawLight)
                {
                    deathAlpha = 1;

                    sneakTransitionTimer = 2 * 60;
                    sawLight = true;
                }
            }

            if (sawLight || state != State.SNEAK)
            {
                alpha = Math.Min(alpha + .02f, maxAlpha);
            }
            else
            {
                alpha = Math.Max(alpha - .05f, minAlpha);
            }

            deathAlpha = (state == State.DIE) ? Math.Min(deathAlpha + .005f, 1) : Math.Max(deathAlpha - .05f, 0);

            if (hpPercent < .25f)
            {
                projectileTimer = Math.Max(projectileTimer - 1, 0);
                if (projectileTimer == 0)
                {
                    projectileTimer = 60 + 240 * hpPercent;
                    SpawnProjectile((float)(MathUtil.VectorToAngle(new Vector2(player.X - X, player.Y - Y))));
                }
            }

            switch (state)
            {
                case State.IDLE:
                    eyeFrame = Math.Max(eyeFrame - .05f, 0);
                    SetAnimation(0, 3, .1f, true);

                    XVel = 0;
                    YVel = 0;
                    
                    break;
                case State.SNEAK:
                    {
                        eyeFrame = Math.Max(eyeFrame - .05f, 0);

                        sneakTimer++;

                        if (sneakTransitionTimer == 0)
                        {
                            if (sneakTimer % 12 == 0)
                            {
                                footPrint = (footPrint + 1) % 6;
                                float ang = 0;
                                //ang = 270 - footPrint * 90;
                                ang = 90 + footPrint * 90;

                                if (footPrint < 4)
                                {
                                    //ang += i * 60;

                                    var dx = (float)(8 * MathUtil.LengthDirX((MathUtil.RadToDeg(Angle) + ang)) % 360);
                                    var dy = (float)(8 * MathUtil.LengthDirY((MathUtil.RadToDeg(Angle) + ang)) % 360);

                                    var foot = new SingularEffect(X + dx, Y + dy, 17);
                                    foot.Angle = Angle;
                                    foot.Scale = new Vector2(.75f);
                                    foot.Color = new Color(Color.White, 1 - alpha);
                                    foot.Depth = Depth - .00001f;
                                }
                            }

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

                        }

                        if (sawLight)
                        {
                            XVel = 0;
                            YVel = 0;

                            SetAnimation(0, 3, .1f, true);
                            eyeFrame = 3;

                            var footPrints = ObjectManager.FindAll<SingularEffect>().Where(o => o.Type == 17);
                            foreach(var footPrint in footPrints)
                            {
                                footPrint.Destroy();
                            }

                            sneakTransitionTimer = Math.Max(sneakTransitionTimer - 1, 0);
                            if (sneakTransitionTimer == 0)
                            {
                                state = State.FOLLOW_PLAYER;
                            }
                        }

                    }
                    break;
                case State.FOLLOW_PLAYER:
                    {
                        var maxSpd = .7f;
                        var animSpd = Math.Min(Math.Max(Math.Max(Math.Abs(XVel), Math.Abs(YVel)), .1f), maxSpd) / maxSpd;

                        SetAnimation(4, 7, .15f * animSpd, true);

                        eyeFrame = 0;
                        
                        followTimer = Math.Max(followTimer - 1, 0);
                        
                        var targetAngle = (float)MathUtil.VectorToAngle(new Vector2(player.X - X, player.Y - Y), false);
                        var lx = .02f * (float)MathUtil.LengthDirX(targetAngle);
                        var ly = .02f * (float)MathUtil.LengthDirY(targetAngle);

                        XVel += lx;
                        YVel += ly;

                        XVel = MathUtil.AtMost(XVel, maxSpd);
                        YVel = MathUtil.AtMost(YVel, maxSpd);
                        
                        Angle = (float)MathUtil.VectorToAngle(new Vector2(XVel, YVel), true);

                        //if (followTimer == 0)
                        if (sawLight || followTimer == 0)
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
                        eyeFrame = Math.Min(eyeFrame + .1f, 3);

                        var angle = (float)MathUtil.VectorToAngle(new Vector2(X - player.X, Y - player.Y - 1 * Globals.T), false);
                        XVel = .5f * (float)MathUtil.LengthDirX(angle);
                        YVel = .5f * (float)MathUtil.LengthDirY(angle);

                        Angle = (float)MathUtil.DegToRad(angle + 180);

                        SetAnimation(4, 7, .1f, true);
                        
                        retreatTimer = Math.Max(retreatTimer - 1, 0);

                        if (retreatTimer == 0)
                        {
                            attackTimer = 2 * 60;
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
                                    var angle = MathUtil.RadToDeg(Angle) + i * 20;
                                    SpawnProjectile(angle);

                                }
                            }
                            else
                            {
                                SetAnimation(0, 3, .2f, true);
                                eyeFrame = 4;

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
                                if (HP > 0)
                                {
                                    BackToSneakState();
                                }
                            });
                        }
                    }
                    break;
                case State.DIE:
                    
                    eyeFrame = 5;
                    alpha = maxAlpha;
                    SetAnimation(13, 13, 0, false);

                    preventDeath = false;
                    
                    XVel = 0;
                    YVel = 0;
                    Damage = 0;
                    deathTimer = Math.Max(deathTimer - 1, 0);
                    if (deathTimer % 5 == 0)
                        new SingularEffect(Left + RND.Int((int)BoundingBox.Width), Top + RND.Int((int)BoundingBox.Height), 8);

                    if (deathTimer == 0)
                    {
                        Coin.Spawn(X, Y, Room, 2000);
                        new FlashEmitter(X, Y, longFlash: true);
                        base.OnDeath();
                        base.Destroy();
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
        }

        private void BackToSneakState()
        {
            sawLight = false;
            sneakTransitionTimer = 0;
            sneakTimer = 0;
            state = State.SNEAK;
        }

        private void SpawnProjectile(double degAngle)
        {
            if (state == State.DIE)
                return;

            var dst = 6;
            var dx = dst * (float)MathUtil.LengthDirX(degAngle);
            var dy = dst * (float)MathUtil.LengthDirY(degAngle);

            var proj = new DefaultEnemyProjectile(X + dx, Y + dy);
            proj.Texture = AssetManager.Projectiles[14];
            var lightSource = new LightSource(proj);
            lightSource.Active = true;
            lightSource.Scale = new Vector2(.3f);

            proj.XVel = 1.5f * (float)MathUtil.LengthDirX(degAngle);
            proj.YVel = 1.5f * (float)MathUtil.LengthDirY(degAngle);
            

        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            //base.Draw(sb, gameTime);
            sb.Draw(AssetManager.BossShadowLizard[AnimationFrame], Position, null, new Color(Color, alpha), Angle, DrawOffset, Scale, SpriteEffects.None, Depth);
            
            if ((int)eyeFrame > 0)
            {
                sb.Draw(AssetManager.BossShadowLizard[7 + (int)eyeFrame], Position, null, new Color(Color, alpha), Angle, DrawOffset, Scale, SpriteEffects.None, Depth + .0002f);
            }

            if (deathAlpha > 0)
            {
                sb.Draw(AssetManager.BossShadowLizard[13], Position, null, new Color(Color, deathAlpha), Angle, DrawOffset, Scale, SpriteEffects.None, Depth + .0003f);
            }
        }

        public override void Hit(int hitPoints, float degAngle)
        {
            if (invincibleTimer != 0)
                return;

            invincibleTimer = 2;
            
            base.Hit(hitPoints, degAngle);            
        }

        public override void Destroy(bool callGC = false)
        {
            if (preventDeath)
                return;

            base.Destroy();
        }

        public override void OnDeath()
        {
            preventDeath = true;
            if (state == State.DIE)
                return;
            state = State.DIE;
            
            base.OnDeath();
            
        }
    }
}
