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
using Leore.Resources;

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

    public class FireArcProjectile : FireProjectile, IKeepAliveBetweenRooms, IKeepEnabledAcrossRooms
    {
        private Vector2 origin;

        private float angle;
        private float originalAngle;
        
        bool shot;

        private float arcDist;

        private int offAngle;
        private Vector2 centerPos;

        private FireSpell spell;

        private float spd;
        private int cooldown;

        public FireArcProjectile(float x, float y, FireSpell spell) : base(x, y, SpellLevel.TWO)
        {
            this.spell = spell;

            EffectOnDestroy = true;

            Scale = Vector2.One;
            
            origin = Position;
            centerPos = Position;

            Texture = AssetManager.Projectiles[10];
        }

        public override void HandleCollision(GameObject obj)
        {
            //base.HandleCollision(obj);
            cooldown = 5;
        }

        public override void Destroy(bool callGC = false)
        {
            base.Destroy(callGC);
            spell.ArcProjectiles.Remove(this);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            light.Scale = Scale * .6f;

            cooldown = Math.Max(cooldown - 1, 0);
            Damage = cooldown == 0 ? 3 : 0;

            var inWater = GameManager.Current.Map.CollisionTile(X, Y, GameMap.WATER_INDEX) && !GameManager.Current.Map.CollisionTile(X, Y);
            if (inWater)
                Destroy();
            
            if (!shot)
            {
                centerPos = orb.TargetPosition;
                origin = centerPos;
                if (orb.State != OrbState.ATTACK || orb.Level != level)// || player.MP < GameResources.MPCost[SpellType.FIRE][level])
                {
                    originalAngle = (float)MathUtil.VectorToAngle(new Vector2((int)player.Direction, Math.Sign((int)player.LookDirection)));
                    angle = originalAngle;

                    spd = 2;
                    
                    shot = true;
                }

                var off = spell.ArcProjectiles.Count > 0 ? ((float)(spell.ArcProjectiles.IndexOf(this) + 1) / (float)(spell.ArcProjectiles.Count)) * 360 : 0;
                offAngle = spell.ArcAngle + ((int)off);

                arcDist = 6 + spell.Power * 14;
            }
            else
            {
                offAngle = (offAngle + spell.ArcSpeed) % 360;
            }
            
            var xpos = (float)MathUtil.LengthDirX(offAngle) * arcDist;
            var ypos = (float)MathUtil.LengthDirY(offAngle) * arcDist;
            
            XVel = spd * (float)MathUtil.LengthDirX(angle);
            YVel = spd * (float)MathUtil.LengthDirY(angle);

            Angle = (float)MathUtil.DegToRad(offAngle - 45);

            // movement

            Position = centerPos + new Vector2(xpos, ypos);
            centerPos += new Vector2(XVel, YVel);
            
            // destroy

            if (MathUtil.Euclidean(Position, origin) > 5 * Globals.T)
            {
                arcDist = Math.Max(arcDist - 2f, 0);
                if (arcDist == 0)
                    Destroy();
            }
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
        private float maxVel = 0;

        public FlameThrowerProjectile(float x, float y) : base(x, y, SpellLevel.THREE)
        {
            Scale = Vector2.One;
            light.Scale = new Vector2(.2f);

            AnimationTexture = AssetManager.Projectiles;
            SetAnimation(24, 28, .15f, false);

            Damage = 1;
            
            EffectOnDestroy = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            maxVel = Math.Max(Math.Abs(XVel), maxVel);

            XVel *= .96f;
            YVel *= .96f;

            Angle = (float)MathUtil.VectorToAngle(new Vector2(XVel, YVel), true);

            light.Scale = new Vector2(Math.Min(light.Scale.X + .03f, .8f));
            //light.Scale = Scale;

            //if (Math.Abs(XVel) < .1f)
            //    Destroy();

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

            //if (AnimationFrame >= MaxFrame - 2)
            //alpha = Math.Max(alpha - .05f, 0);

            var lifeTime = Math.Abs(XVel - Math.Sign(XVel) * .5f) / maxVel;

            alpha = (float)Math.Sin(lifeTime * Math.PI);

            Color = new Color(Color, alpha);
            
            if (Math.Max(Math.Abs(XVel), Math.Abs(YVel)) < .5f)// || alpha == 0)
                Destroy();
        }        
    }
}
