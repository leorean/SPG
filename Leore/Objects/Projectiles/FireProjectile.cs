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
        public bool EffectOnDestroy { get; set; }

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
            if (EffectOnDestroy)
                new SingularEffect(X, Y, 10) { Scale = Scale * .75f };

            FadeOutLight.Create(this, light.Scale);
            
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

    // ++++ Level: 1 ++++

    public class FireBallProjectile : FireProjectile
    {
        public int Bounce { get; set; } = 3;
        private List<Vector2> a;
        private int ind;
        private float d;
        
        public FireBallProjectile(float x, float y) : base(x, y, SpellLevel.ONE)
        {
            Scale = new Vector2(.75f);
            Gravity = .13f;

            Damage = 2;

            a = new List<Vector2>() { Position, Position, Position, Position };            
            d = 0;

            Texture = AssetManager.Projectiles[10];

            EffectOnDestroy = true;
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

            if (!xCol) xCol = ObjectManager.CollisionPointFirstOrDefault<Solid>(X + XVel, Y) != null;
            if (!yCol) yCol = ObjectManager.CollisionPointFirstOrDefault<Solid>(X, Y + YVel) != null;

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
                YVel = MathUtil.AtMost(YVel, 2.5f);

                Bounce = Math.Max(Bounce - 1, 0);
            }

            if ((Bounce == 0)
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
                sb.Draw(Texture, pos[i], null, new Color(Color, shadow), Angle - (float)MathUtil.DegToRad((360 / (float)i) * i), DrawOffset, Scale, SpriteEffects.None, Depth - .0001f * (float)i);
            }
        }
    }

    // ++++ Level: 2 ++++

    public class FireArcProjectile : FireProjectile
    {
        private int lifeTime = 40;
        
        private Direction dir;
        private Direction lookDir;

        private float t;
        private float amp;
        private float spd;

        private float maxSpd;

        private float tVel = .15f;

        private float pxVel, pyVel;

        private Vector2 origin;

        public FireArcProjectile(float x, float y, Direction dir, Direction lookDir, float amp, float spd, float t, float tVel) : base(x, y, SpellLevel.TWO)
        {
            Scale = new Vector2(.75f);

            this.maxSpd = spd;
            this.amp = amp;
            this.t = t;
            this.tVel = tVel;
            this.dir = dir;
            this.lookDir = lookDir;

            Damage = 2;

            pxVel = player.XVel;
            pyVel = player.YVel;

            origin = Position;

            Texture = AssetManager.Projectiles[11];
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            light.Scale = Scale * .6f;

            lifeTime = Math.Max(lifeTime - 1, 0);
            
            var col = GameManager.Current.Map.CollisionTile(X + XVel, Y + YVel);
            
            if (!col)
                col = ObjectManager.CollisionPointFirstOrDefault<Solid>(X, Y) != null;

            if (col)
            {
                Destroy();
                return;
            }

            var inWater = GameManager.Current.Map.CollisionTile(X, Y, GameMap.WATER_INDEX);
            if (inWater && !col)
                Destroy();
            
            t = (t + tVel) % (float)(2 * Math.PI);

            spd = Math.Min(spd + .15f, maxSpd);
            if (spd == maxSpd)
            {
                //if (Math.Abs(spd) < .5f)
                //    lifeTime = 0;
                //maxSpd *= .9f;
            }

            var angle = 0;
            switch (lookDir)
            {
                case Direction.NONE:
                    angle = 90;
                    break;
                case Direction.UP:
                    angle = 45;
                    break;
                case Direction.DOWN:
                    angle = - 45;
                    break;
            }

            // sine wave
            
            var xv = (float)MathUtil.LengthDirX(angle) * (float)Math.Sin(t) * amp * Math.Sign((int)dir);
            var yv = (float)MathUtil.LengthDirY(angle) * (float)Math.Sin(t) * amp;

            var lx = Math.Sign((int)dir) * spd;
            var ly = Math.Sign((int)lookDir) * spd;

            XVel = xv + lx;// + .5f * pxVel;
            YVel = yv + ly;// + .5f * pyVel;

            if (amp > 0)
                Angle = (float)MathUtil.VectorToAngle(new Vector2(XVel, YVel), true);
            else
                Angle = (float)MathUtil.VectorToAngle(new Vector2(lx, ly), true);

            // linear movement

            Move(XVel, YVel);
            
            if (lifeTime == 0 || MathUtil.Euclidean(Position, origin) > 5 * Globals.T)
                Destroy();            
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            sb.Draw(Texture, Position, null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth);            
        }
    }

    // ++++ Level: 3 ++++

    public class FlameThrowerProjectile : FireProjectile
    {
        private float alpha = 1;

        public FlameThrowerProjectile(float x, float y) : base(x, y, SpellLevel.THREE)
        {
            Scale = new Vector2(.4f);
            Texture = AssetManager.Projectiles[12];

            Damage = 1;
            
            EffectOnDestroy = false;
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

            if (!xCol) xCol = ObjectManager.CollisionPointFirstOrDefault<Solid>(X + XVel, Y) != null;
            if (!yCol) yCol = ObjectManager.CollisionPointFirstOrDefault<Solid>(X, Y + YVel) != null;

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

            alpha = Math.Max(alpha - .02f, 0);
            Color = new Color(Color, alpha);

            if (Math.Max(Math.Abs(XVel), Math.Abs(YVel)) < .5f || alpha == 0)
                Destroy();
        }        
    }
}
