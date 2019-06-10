using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leore.Main;
using Leore.Objects.Effects.Emitters;
using Leore.Objects.Level;
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

        //ObtainParticleEmitter emitter;

        public static VoidProjectile Create(float x, float y, Orb parent)
        {
            if (Current != null)
                return Current;

            Current = new VoidProjectile(x, y, parent);
            return Current;
        }

        private VoidProjectile(float x, float y, Orb parent) : base(x, y, parent.Level)
        {
            Parent = parent;

            //Texture = AssetManager.WhiteCircle;
            DrawOffset = new Vector2(32);
            Scale = Vector2.Zero;
            //Color = Color.Black;

            Angle = RND.Int(360);
            
            //emitter = new ObtainParticleEmitter(X, Y, timeout: 10, radius: 9);
            //emitter.Color = Color.Black;
            //emitter.Active = false;
            //emitter.Parent = this;
            
            switch (parent.Level)
            {
                case SpellLevel.ONE:
                    maxScale = .5f;
                    break;
            }            
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!free)
            {
                if (orb.State == OrbState.ATTACK)
                    Position = orb.Position;
                else
                {

                    var ang = (float)new Vector2(orb.X - orb.Parent.X, orb.Y - orb.Parent.Y).VectorToAngle();            
                    var lx = (float)MathUtil.LengthDirX(ang);
                    var ly = (float)MathUtil.LengthDirY(ang);
                    XVel = 4 * lx;
                    YVel = 4 * ly;

                    //XVel = 4 * Math.Sign((int)orb.Direction);


                    free = true;
                    Current = null;
                    Parent = null;
                }
            }

            if (!isStatic) {                
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
                //emitter.Active = false;
            }
            else
            {
                //emitter.Active = true;
                Scale = new Vector2(Math.Max(Scale.X - .004f, 0));
            }

            if (free)
                Move(XVel, YVel);

            //emitter.Position = Position;

            alpha = Math.Min(Math.Max(1 - Scale.X / maxScale, .5f), .8f);
            //Color = new Color(Color, alpha);
            
            var radius = Math.Max((32 - 6) * Scale.X, 0);
            var bx = -radius;
            var bw = 2 * radius;             
            BoundingBox = new SPG.Util.RectF(bx, bx, bw, bw);

            //if (Scale.X == maxScale)
            if(isStatic && Scale.X == 0)
            {
                //emitter.Active = false;
                //emitter.Parent = null;                
                Destroy();
            }
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
            angle2 -= .2f;
            angle3 += .05f;

            //base.Draw(sb, gameTime);
            sb.Draw(AssetManager.VoidCircle, Position, null, new Color(Color, alpha), angle1, DrawOffset, Scale, SpriteEffects.None, Depth);
            sb.Draw(AssetManager.VoidCircle, Position, null, new Color(Color, alpha * .8f), angle2, DrawOffset, Scale * .75f, SpriteEffects.None, Depth + .00001f);
            sb.Draw(AssetManager.VoidCircle, Position, null, new Color(Color, alpha * .5f), angle3, DrawOffset, Scale * .5f, SpriteEffects.None, Depth + .00002f);
        }

        public override void HandleCollision(GameObject obj)
        {
            //throw new NotImplementedException();

            var angle = MathUtil.VectorToAngle(obj.Center - Center);

            var lx = (float)MathUtil.LengthDirX(angle);
            var ly = (float)MathUtil.LengthDirY(angle);

            //obj.Move(lx, ly);
            obj.XVel += lx;
            obj.YVel += ly;
        }

        public override void HandleCollisionFromDestroyBlock(DestroyBlock block)
        {
            //base.HandleCollisionFromDestroyBlock(block);
            block.Hit(Damage);
        }
        
    }
}
