using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leore.Main;
using Leore.Objects.Effects.Emitters;
using Leore.Objects.Enemies;
using Leore.Objects.Items;
using Leore.Objects.Level;
using Leore.Resources;
using Leore.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Objects;
using SPG.Util;

namespace Leore.Objects.Projectiles
{
    public class VoidProjectile : PlayerProjectile
    {
        Orb orb => Parent as Orb;

        private static VoidProjectile Current;

        float alpha = 1;
        float maxScale;
        bool isStatic;
        bool free;

        float angle1, angle2, angle3;

        int hitTimer;
        int delay;
        float succ;

        ObtainParticleEmitter emitter;

        public static VoidProjectile Create(float x, float y, Orb parent)
        {
            if (Current != null)
                return Current;

            Current = new VoidProjectile(x, y, parent);

            new CrimsonBurstEmitter(x, y) { ParticleColors = new List<Color>() { Color.Black } };

            return Current;
        }

        private VoidProjectile(float x, float y, Orb parent) : base(x, y, parent.Level)
        {
            Parent = parent;
            level = parent.Level;

            DrawOffset = new Vector2(32);
            Scale = Vector2.Zero;
            Depth = parent.Depth - .0001f;

            Angle = RND.Int(360);

            emitter = new ObtainParticleEmitter(X, Y, timeout: 2, radius: 38, initialSpeed: -1);
            emitter.Color = Color.Black;
            emitter.Active = true;
            emitter.Parent = this;

            switch (level)
            {
                case SpellLevel.ONE:
                    maxScale = .35f;
                    delay = 15;
                    succ = .2f;
                    break;
                case SpellLevel.TWO:
                    maxScale = .5f;
                    delay = 10;
                    succ = .4f;
                    break;
                case SpellLevel.THREE:
                    maxScale = .75f;
                    delay = 5;
                    succ = .6f;
                    break;
            }            
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            hitTimer = Math.Max(hitTimer - 1, 0);

            if (hitTimer > 0)
                Damage = 0;
            else
                Damage = 1;

            free = Current != this;

            alpha = Math.Min(Math.Max(1 - Scale.X / maxScale, .5f), .8f);
            var radius = Math.Max((32 - 6) * Scale.X, 0);
            var bx = -radius;
            var bw = 2 * radius;
            BoundingBox = new SPG.Util.RectF(bx, bx, bw, bw);

            if (!free)
            {
                if (orb.State == OrbState.ATTACK && GameManager.Current.Player.MP >= GameResources.MPCost[SpellType.VOID][orb.Level])
                    Position = orb.Position;
                else
                {
                    var ang = (float)new Vector2(orb.TargetPosition.X - orb.Parent.X, orb.TargetPosition.Y - orb.Parent.Y).VectorToAngle();            
                    var lx = (float)MathUtil.LengthDirX(ang);
                    var ly = (float)MathUtil.LengthDirY(ang);
                    XVel = 4 * lx;
                    YVel = 4 * ly;

                    orb.Visible = true;

                    new CrimsonBurstEmitter(X, Y) { ParticleColors = new List<Color>() { Color.Black } };

                    Current = null;
                    Parent = null;
                }
            } else
            {
                //emitter.Active = true;

                var t = 1 * Globals.TILE;
                var enemies = this.CollisionRectangles<Enemy>(Left - t, Top - t, Right + t, Bottom + t);
                foreach (var enemy in enemies)
                {
                    var angle = MathUtil.VectorToAngle(enemy.Center - Center);

                    var lx = (float)MathUtil.LengthDirX(angle);
                    var ly = (float)MathUtil.LengthDirY(angle);

                    var sign = -1;
                    
                    var ex = enemy.XVel + lx * sign * succ;
                    var ey = enemy.YVel + ly * sign * succ;

                    enemy.XVel = MathUtil.Limit(ex, 2);
                    enemy.YVel = MathUtil.Limit(ey, 2);
                }

                var coins = this.CollisionRectangles<Coin>(Left - t, Top - t, Right + t, Bottom + t);
                foreach (var coin in coins)
                {
                    coin.SetLoose();
                    var angle = MathUtil.VectorToAngle(coin.Center - Center);

                    var lx = (float)MathUtil.LengthDirX(angle);
                    var ly = (float)MathUtil.LengthDirY(angle);

                    var sign = -1;

                    var ex = coin.XVel + lx * sign * succ;
                    var ey = coin.YVel + ly * sign * succ;

                    coin.XVel = MathUtil.Limit(ex, 2);
                    coin.YVel = MathUtil.Limit(ey, 2);
                }
            }

            if (!isStatic)
            {                
                if (!free)
                {
                    Scale = new Vector2(Math.Min(Scale.X + .04f, maxScale * .5f));
                }
                else
                {
                    XVel = Math.Sign(XVel) * Math.Max(Math.Abs(XVel) - .2f, 0);
                    YVel = Math.Sign(YVel) * Math.Max(Math.Abs(YVel) - .2f, 0);
                    if (XVel == 0 && YVel == 0)
                        isStatic = true;

                    Scale = new Vector2(Math.Min(Scale.X + .04f, maxScale));
                }                
            }
            else
            {
                Scale = new Vector2(Math.Max(Scale.X - .008f, 0));
            }

            if (free)
                Move(XVel, YVel);

            emitter.Position = Position;
            
            //if (Scale.X == maxScale)
            if(isStatic && Scale.X == 0)
            {
                Destroy();
            }
        }

        public override void Destroy(bool callGC = false)
        {
            if (Current == this)
                Current = null;

            emitter.Active = false;
            emitter.Parent = null;

            base.Destroy(callGC);
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            //if (isStatic)
            //{
            //    for (var i = 0; i < 3; i++)
            //    {
            //        var angle = RND.Next * 360;

            //        var lx = (float)MathUtil.LengthDirX(angle);
            //        var ly = (float)MathUtil.LengthDirY(angle);

            //        sb.DrawLightning(Position, Position + new Vector2(lx * Scale.X * 48, ly * Scale.X * 48), new Color(Color.Black, alpha), Depth - .0002f);
            //    }
            //}

            angle1 += .1f;
            angle2 += .2f;
            angle3 += .3f;

            //base.Draw(sb, gameTime);
            sb.Draw(AssetManager.VoidCircle[0], Position, null, new Color(Color, alpha * .3f), angle1, DrawOffset, Scale, SpriteEffects.None, Depth + .00001f);
            sb.Draw(AssetManager.VoidCircle[1], Position, null, new Color(Color, alpha * .5f), angle2, DrawOffset, Scale, SpriteEffects.None, Depth + .00002f);
            sb.Draw(AssetManager.VoidCircle[2], Position, null, new Color(Color, alpha * .8f), angle3, DrawOffset, Scale, SpriteEffects.None, Depth + .00003f);
            sb.Draw(AssetManager.VoidCircle[3], Position, null, new Color(Color, alpha * 1f), 0, DrawOffset, Scale, SpriteEffects.None, Depth);
            
        }

        public override void HandleCollision(GameObject obj)
        {
            //throw new NotImplementedException();

            if (hitTimer == 0)
                hitTimer = delay;

            var angle = MathUtil.VectorToAngle(obj.Center - Center);

            var lx = (float)MathUtil.LengthDirX(angle);
            var ly = (float)MathUtil.LengthDirY(angle);

            var sign = isStatic ? -1 : 1;
            
            obj.XVel += lx * sign * Math.Max(Math.Abs(XVel),Math.Abs(YVel));
            obj.YVel += ly * sign * Math.Max(Math.Abs(XVel), Math.Abs(YVel));
        }

        public override void HandleCollisionFromDestroyBlock(DestroyBlock block)
        {
            //base.HandleCollisionFromDestroyBlock(block);
            block.Hit(Damage);
        }
        
    }
}
