using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leore.Main;
using Leore.Objects.Effects;
using Leore.Objects.Effects.Emitters;
using Leore.Objects.Items;
using Leore.Objects.Level;
using Leore.Objects.Level.Blocks;
using Leore.Objects.Obstacles;
using Leore.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Map;
using SPG.Objects;
using SPG.Util;

namespace Leore.Objects.Projectiles
{
    // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

    public class IceProjectile : PlayerProjectile
    {
        private static List<IceProjectile> Instances = new List<IceProjectile>();

        private Player player => GameManager.Current.Player;
        private Orb orb => player.Orb;

        private LightSource light;
        private float maxVel;
        
        private float scale;

        private Vector2 originalPosition;

        private int cooldown;

        private IceEmitter emitter;

        private float lifeTime;
        private float maxLifeTime;

        private float originalAngle;

        private float alpha;

        int dmg;
        int curFrame;
        float angle;
        

        public IceProjectile(float x, float y, SpellLevel level) : base(x, y, level)
        {
            var angle = (float)MathUtil.VectorToAngle(new Vector2(orb.TargetPosition.X - x, orb.TargetPosition.Y - y));

            this.level = level;
            
            AnimationTexture = AssetManager.Projectiles;            
            curFrame = AnimationFrame;

            Element = SpellElement.ICE;
            
            Depth = orb.Depth + .0002f;
            BoundingBox = new RectF(-2, -2, 4, 4);
            DrawOffset = new Vector2(8);

            scale = 1f;

            Scale = new Vector2((int)player.Direction * scale, scale);
            light = new LightSource(this) { Active = true, Scale = new Vector2(scale * .45f) };

            emitter = new IceEmitter(X, Y);
            emitter.Active = true;
            emitter.Depth = Depth - .0001f;

            originalPosition = Position;

            alpha = 1;

            maxVel = 4.5f;            
            maxLifeTime = 999;

            SetAnimation(15, 18, .3f, true);

            switch (level)
            {
                case SpellLevel.ONE:
                    orb.Cooldown = 40;
                    dmg = 3;
                    maxVel = 5;
                    break;                
                case SpellLevel.TWO:
                    SetAnimation(20, 23, .2f, false);
                    angle = angle - 10 + RND.Int(20);
                    orb.Cooldown = 15;
                    dmg = 1;
                    maxVel = 6f;
                    maxLifeTime = .7f * 60;
                    break;
            }

            lifeTime = maxLifeTime;
            originalAngle = angle;

            XVel = (float)MathUtil.LengthDirX(angle) * maxVel;
            YVel = (float)MathUtil.LengthDirY(angle) * maxVel;
        }
        
        public override void Destroy(bool callGC = false)
        {
            Instances.Remove(this);

            FadeOutLight.Create(this, light.Scale);
            light.Parent = null;
            light.Destroy();

            emitter.Active = false;
            
            base.Destroy(callGC);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            emitter.Position = player.Position;
            emitter.SpawnPosition = Position;

            lifeTime = Math.Max(lifeTime - 1, 0);

            cooldown = Math.Max(cooldown - 1, 0);
            if (cooldown > 0)
                Damage = 0;
            else
            {                
                Damage = dmg;
            }
            
            switch (level)
            {
                case SpellLevel.ONE:
                case SpellLevel.TWO:
                case SpellLevel.THREE:
                    var targetAngle = originalAngle - 180;
                    XVel += .25f * (float)(MathUtil.LengthDirX(targetAngle));
                    YVel += .25f * (float)(MathUtil.LengthDirY(targetAngle));

                    if (lifeTime < maxLifeTime - 60 && this.IsOutsideCurrentRoom(2 * Globals.T))
                    {
                        FinishUp();
                    }

                    XVel = MathUtil.AtMost(XVel, maxVel);
                    YVel = MathUtil.AtMost(YVel, maxVel);

                    if (lifeTime < maxLifeTime - 20)
                    {
                        if (MathUtil.Euclidean(Position, originalPosition) < 8)
                        {
                            FinishUp();
                        }
                    }
                    break;
            }
                
            if (lifeTime == 0)
            {
                alpha = Math.Max(alpha - .1f, 0);
                if (alpha == 0)
                {
                    FinishUp();
                }
            }
                
            Move(XVel, YVel);

            if (level == SpellLevel.ONE)// || level == SpellLevel.TWO)
            {
                var ang = MathUtil.RadToDeg(Angle);
                if (curFrame != AnimationFrame)
                {
                    ang = (ang + 20) % 360;
                }
                curFrame = AnimationFrame;
                Angle = (float)MathUtil.DegToRad(ang);
            }
            if (level == SpellLevel.TWO)
            {
                angle = (angle + 10) % 360;
                if (AnimationFrame != MaxFrame)
                {
                    Angle = (float)(MathUtil.VectorToAngle(new Vector2(XVel, YVel), true));
                }
                else
                {
                    Angle = (float)MathUtil.DegToRad(angle);
                }
            }

            Color = new Color(Color, alpha);
        }
        
        private void FinishUp()
        {
            if (!this.CollisionBounds(orb, X, Y))
                new SingularEffect(X, Y, 10).Scale = new Vector2(.5f);
            Destroy();
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);            
        }

        public override void HandleCollision(GameObject obj)
        {
            if (cooldown > 0)
                return;

            cooldown = 10;
            if (obj is FireBlock)
                FinishUp();
        }
    }

    public class IceSpell : SpellObject
    {
        private static IceSpell instance;

        private static Player player => GameManager.Current.Player;
        private static Orb orb => player.Orb;

        private Vector2 originalPosition;
        private float originalAngle;

        private bool letGo;

        public List<IceArcProjectile> Projectiles { get; private set; } = new List<IceArcProjectile>();

        private int cooldown;

        private IceSpell(float x, float y, SpellLevel level) : base(x, y, level)
        {
            //BoundingBox = new RectF(-4, -4, 8, 8);
            //DrawOffset = new Vector2(8);
            //Texture = AssetManager.Projectiles[11];            
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (level == SpellLevel.ONE || level == SpellLevel.TWO)
                Destroy();

            cooldown = Math.Max(cooldown - 1, 0);
            if (cooldown == 0)
            {
                player.MP = Math.Max(player.MP - GameResources.MPCost[SpellType.ICE][level], 0);

                var proj = new IceArcProjectile(X, Y, this);
                cooldown = 10;
                Projectiles.Add(proj);

                if (Projectiles.Count > 30)
                {
                    FreeProjectile(Projectiles[0]);
                }
            }

            if (orb.State != OrbState.ATTACK || orb.Type != SpellType.ICE || level != orb.Level || player.MP < GameResources.MPCost[SpellType.ICE][level])
            {
                instance = null;
                letGo = true;
            }

            if (!letGo)
            {
                originalPosition = Position;
                originalAngle = (float)MathUtil.VectorToAngle(new Vector2((int)player.Direction, Math.Sign((int)player.LookDirection)));

                Position = orb.Position;
            }

            if (letGo)
                Destroy();
        }

        public static void Create(float x, float y, SpellLevel level)
        {
            if (instance == null)
            {
                instance = new IceSpell(x, y, level);
                return;
            }
            else
            {
                player.MP += GameResources.MPCost[SpellType.ICE][level];
            }

        }

        public override void Destroy(bool callGC = false)
        {
            foreach (var proj in Projectiles.ToList())
            {
                FreeProjectile(proj);
            }

            instance = null;
            base.Destroy(callGC);
        }

        private void FreeProjectile(IceArcProjectile proj)
        {
            //var ta = (float)MathUtil.VectorToAngle(new Vector2(XVel, YVel));
            //proj.TargetAngle = ta;
            if(player.LookDirection == Direction.NONE)
            {
                proj.XVel = 3 * Math.Sign((int)orb.Direction);
                proj.YVel = 0;
            }
            else
            {
                proj.XVel = 0;
                proj.YVel = 3 * Math.Sign((int)player.LookDirection);
            }

            proj.Target = null;
            Projectiles.Remove(proj);
        }
    }

    // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

    public class IceArcProjectile : PlayerProjectile
    {
        private Player player => GameManager.Current.Player;
        public IceSpell Target { get; set; }
        public float TargetAngle { get; set; }
        private IceEmitter emitter;

        private LightSource light;

        private Vector2 origin;

        bool dead;

        public IceArcProjectile(float x, float y, IceSpell spell) : base(x, y, SpellLevel.TWO)
        {
            //Texture = AssetManager.Projectiles[11];
            //AnimationTexture = AssetManager.Projectiles;
            //SetAnimation(29, 30, .2f, true);
            
            Angle = (float)MathUtil.DegToRad(RND.Int(360));

            Texture = AssetManager.Projectiles[11];
            Scale = new Vector2(.5f);

            DrawOffset = new Vector2(8);
            BoundingBox = new RectF(-4, -4, 8, 8);
            Damage = 2;

            Element = SpellElement.ICE;

            Target = spell;
            Position = new Vector2(spell.X - 8 + RND.Int(16), spell.Y - 8 + RND.Int(16));

            emitter = new IceEmitter(X, Y);
            emitter.SpawnTimeout = 10;

            light = new LightSource(this) { Active = true, Scale = new Vector2(.45f) };
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            emitter.Position = Target != null ? Target.Position : Position;
            emitter.SpawnPosition = Position;

            //Scale = (XVel > 0) ? new Vector2(1, 1) : new Vector2(-1, 1);
            //Angle = (float)MathUtil.VectorToAngle(new Vector2(XVel, YVel), true);
            
            Scale = new Vector2(Math.Min(Scale.X + .01f, 1));

            var xCol = GameManager.Current.Map.CollisionTile(X + XVel, Y) || GameManager.Current.Map.CollisionTile(X + XVel, Y, GameMap.WATER_INDEX);
            var yCol = GameManager.Current.Map.CollisionTile(X, Y + YVel) || GameManager.Current.Map.CollisionTile(X, Y + YVel, GameMap.WATER_INDEX);
            
            if (Target != null)
            {
                origin = Position;

                var inBlock = GameManager.Current.Map.CollisionTile(X, Y);                
                if (inBlock)
                    Destroy();
            }

            if (Target != null)
            {
                TargetAngle = (float)MathUtil.VectorToAngle(new Vector2(Target.X - X, Target.Y - Y));
                XVel += .1f * MathUtil.LengthDirX(TargetAngle);
                YVel += .1f * MathUtil.LengthDirY(TargetAngle);

                foreach (var proj in Target.Projectiles)
                {
                    if (this == proj)
                        continue;

                    if (this.CollisionBounds(proj, X, Y))
                    {
                        var ang = MathUtil.VectorToAngle(new Vector2(X - proj.X, Y - proj.Y));

                        XVel += .2f * (float)MathUtil.LengthDirX(ang);
                        YVel += .2f * (float)MathUtil.LengthDirY(ang);
                    }
                }
            }
            
            if (!xCol) { Move(XVel, 0); } else { XVel *= -.25f; }

            if (!yCol) { Move(0, YVel); } else { YVel *= -.25f; }

            if (MathUtil.Euclidean(origin, Position) > 6 * Globals.T) Destroy();
            if (Target == null && (xCol || yCol)) { Destroy(); }

            XVel = MathUtil.AtMost(XVel, 3);
            YVel = MathUtil.AtMost(YVel, 3);            
        }

        public override void Destroy(bool callGC = false)
        {
            if (dead)
                return;

            dead = true;

            emitter.Active = false;

            if (Target != null)
            {
                Target.Projectiles.Remove(this);
            }

            FadeOutLight.Create(this, light.Scale);

            new SingularEffect(X, Y, 10);
            base.Destroy(callGC);            
        }

        public override void HandleCollision(GameObject obj)
        {

            if (obj is Pot)
                return;

            Destroy();
        }
    }
}
