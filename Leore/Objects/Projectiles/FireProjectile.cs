using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leore.Main;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Objects;
using SPG.Map;
using Leore.Objects.Effects.Emitters;
using SPG.Util;
using Leore.Objects.Level;
using Leore.Objects.Effects;
using System.Diagnostics;

namespace Leore.Objects.Projectiles
{
    public abstract class FireProjectile : PlayerProjectile
    {
        protected bool dead;

        protected Player player => GameManager.Current.Player;
        protected Orb orb => GameManager.Current.Player.Orb;

        protected LightSource light;

        public FireProjectile(float x, float y, SpellLevel level) : base(x, y, level)
        {
            Depth = player.Depth + .0002f;
            
            Scale = new Vector2(.4f);

            DrawOffset = new Vector2(8);
            BoundingBox = new SPG.Util.RectF(-3, -3, 6, 6);

            Element = SpellElement.FIRE;

            Gravity = 0;

            light = new LightSource(this);
            light.Active = true;
        }

        public override void Destroy(bool callGC = false)
        {

            new SingularEffect(X, Y, 10) { Scale = Scale * .75f };
            base.Destroy(callGC);
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);

        }

        public override void HandleCollision(GameObject obj)
        {
            Destroy();
        }
    }

    // ++++ Level: 1 & 2 ++++

    public class FireProjectile1 : FireProjectile
    {
        private int bounce = 3;
        private List<Vector2> a;
        private int ind;
        private float d;
        
        public FireProjectile1(float x, float y, SpellLevel level) : base(x, y, level)
        {
            Scale = new Vector2(.75f);
            Gravity = .13f;

            Damage = 2;

            a = new List<Vector2>() { Position, Position, Position, Position };            
            d = 0;

            Texture = AssetManager.Projectiles[10];
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Angle = Angle + .3f * Math.Sign((int)XVel != 0 ? XVel : 1);
            light.Scale = Scale * .6f;

            var inWater = GameManager.Current.Map.CollisionTile(X, Y, GameMap.WATER_INDEX);
            if (inWater)
                Destroy();

            var xCol = GameManager.Current.Map.CollisionTile(X + XVel, Y);
            var yCol = GameManager.Current.Map.CollisionTile(X, Y + YVel);

            if (!xCol)
                Move(XVel, 0);
            else
            {
                XVel = -XVel * .5f;
            }

            if (!yCol)
            {
                YVel += Gravity;
                Move(0, YVel);
            }
            else
            {
                XVel *= .95f;
                YVel = -Math.Sign(YVel) * Math.Max(Math.Abs(YVel) * .85f, 1.3f);

                bounce = Math.Max(bounce - 1, 0);
            }

            if ((bounce == 0)
                || Math.Max(Math.Abs(XVel), Math.Abs(YVel)) < .1f)
                Destroy();

            // shadow

            d = Math.Max(d - 1, 0);
            if (d == 0)
            {
                a[ind] = Position;
                ind = (ind + 1) % a.Count;
                d = 2;
            }
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            sb.Draw(Texture, Position, null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth);

            var pos = a.ToList();

            pos.Sort(
                delegate (Vector2 o1, Vector2 o2)
                {
                    if (MathUtil.Euclidean(o1, Position) < MathUtil.Euclidean(o2, Position)) return -1;
                    if (MathUtil.Euclidean(o1, Position) > MathUtil.Euclidean(o2, Position)) return 1;
                    return 0;
                }
            );

            for (var i = 0; i < a.Count; i++)
            {
                var shadow = .75f - i / (float)(a.Count + 1);
                sb.Draw(Texture, pos[i], null, new Color(Color, shadow), Angle - (float)MathUtil.DegToRad(30), DrawOffset, Scale, SpriteEffects.None, Depth - .0001f * (float)i);
            }
            //sb.Draw(Texture, new Vector2(x2, y2), null, new Color(Color, .6f), Angle - (float)MathUtil.DegToRad(30), DrawOffset, Scale, SpriteEffects.None, Depth - .0002f);
        }
    }

    // ++++ Level: 3 ++++

    public class FireProjectile3 : FireProjectile
    {
        public FireProjectile3(float x, float y, SpellLevel level) : base(x, y, level)
        {
            Scale = new Vector2(.4f);
            Texture = AssetManager.Projectiles[12];
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            XVel *= .96f;
            YVel *= .96f;

            Angle = (float)MathUtil.VectorToAngle(new Vector2(XVel, YVel), true);

            Scale = new Vector2(Math.Min(Scale.X + .02f, .8f));

            light.Scale = Scale;

            if (Math.Abs(XVel) < .1f)
                Destroy();

            var inWater = GameManager.Current.Map.CollisionTile(X, Y, GameMap.WATER_INDEX);
            if (inWater)
                Destroy();

            var xCol = GameManager.Current.Map.CollisionTile(X + XVel, Y);
            var yCol = GameManager.Current.Map.CollisionTile(X, Y + YVel);

            if (!xCol)
                Move(XVel, 0);
            else
            {
                XVel = -XVel * .85f;
            }

            if (!yCol)
            {
                Move(0, YVel);
            }
            else
            {
                YVel = -YVel * .85f;
            }

            if (Math.Max(Math.Abs(XVel), Math.Abs(YVel)) < .5f)
                Destroy();
        }        
    }
}
