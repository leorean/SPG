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
using SPG.Objects;
using SPG.Util;

namespace Leore.Objects.Projectiles
{
    public class IceProjectile : PlayerProjectile
    {
        private static List<IceProjectile> Instances = new List<IceProjectile>();

        private Player player => GameManager.Current.Player;
        private Orb orb;

        private LightSource light;
        private Key key;

        private bool headBack;

        private float spd;
        private float acc;
        private float maxVel;
        private float maxDist;

        private float scale;

        private Vector2 originalPosition;

        private int cooldown;

        private IceEmitter emitter;

        private float lifeTime;        
        private int level;
        private float originalAngle;
        private float dec;

        int curFrame;

        public static void Create(float x, float y, Orb orb)
        {
            if (Instances.Count > 0)
            {
                Instances[0].player.MP += GameResources.MPCost[Instances[0].orb.Type][Instances[0].orb.Level];
                return;
            }

            //orb.Visible = false;

            var angle = (float)MathUtil.VectorToAngle(new Vector2(orb.TargetPosition.X - x, orb.TargetPosition.Y - y));

            switch (orb.Level)
            {
                case SpellLevel.ONE:
                    new IceProjectile(x, y, orb, angle, 1);
                    break;
                case SpellLevel.TWO:
                    new IceProjectile(x, y, orb, angle, 2);
                    break;
                case SpellLevel.THREE:
                    new IceProjectile(x, y, orb, angle, 3);
                    break;
            }
        }

        private IceProjectile(float x, float y, Orb orb, float angle, int level) : base(x, y, orb.Level)
        {
            Instances.Add(this);

            this.level = level;
            this.orb = orb;

            AnimationTexture = AssetManager.Projectiles;            
            SetAnimation(15, 18, .3f, true);
            curFrame = AnimationFrame;

            Element = SpellElement.ICE;
            
            Depth = orb.Depth + .0002f;
            BoundingBox = new RectF(-4, -4, 8, 8);
            DrawOffset = new Vector2(8);

            scale = 1f;

            Scale = new Vector2((int)player.Direction * scale, scale);
            light = new LightSource(this) { Active = true, Scale = new Vector2(scale * .45f) };

            emitter = new IceEmitter(X, Y);
            emitter.Active = true;
            emitter.Depth = Depth - .0001f;

            originalPosition = Position;
            
            maxVel = 4.5f;
            
            XVel = (float)MathUtil.LengthDirX(angle) * maxVel;
            YVel = (float)MathUtil.LengthDirY(angle) * maxVel;

            originalAngle = angle;

            switch (level)
            {
                case 1:
                    lifeTime = .3f * 60;
                    dec = .95f;
                    break;
                case 2:
                    lifeTime = .3f * 60;
                    dec = .9f;
                    break;
                case 3:
                    lifeTime = .3f * 60;
                    dec = .9f;
                    break;
            }
        }
        
        public override void Destroy(bool callGC = false)
        {
            Instances.Remove(this);

            FadeOutLight.Create(this, light.Scale);
            light.Parent = null;
            light.Destroy();

            emitter.Active = false;
            
            base.Destroy(callGC);

            if (level > 1)
            {
                new IceProjectile(X, Y, orb, originalAngle - 10, level - 1);
                new IceProjectile(X, Y, orb, originalAngle + 10, level - 1);
            }
            if (level > 2)
            {
                new IceProjectile(X, Y, orb, originalAngle, level - 1);                
            }
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
                Damage = 1;
            }

            if (this.CollisionBoundsFirstOrDefault<Solid>(X + XVel, Y))
                XVel *= -1;
            
            //XVel *= .95f;
            //YVel *= .95f;
            XVel *= dec;            
            YVel *= dec;

            Move(XVel, YVel);

            var ang = MathUtil.RadToDeg(Angle);
            if (curFrame != AnimationFrame)
            {
                ang = (ang + 20) % 360;
            }
            curFrame = AnimationFrame;

            Angle = (float)MathUtil.DegToRad(ang);

            if (lifeTime == 0) {
                FinishUp();
            }

        }
        
        private void FinishUp()
        {
            orb.Cooldown = 10;
            Destroy();
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);

            //sb.Draw(AssetManager.Projectiles[17], orb.Position, null, Color, 0, new Vector2(8), Vector2.One, SpriteEffects.None, Depth);            
        }

        public override void HandleCollision(GameObject obj)
        {
            if (cooldown == 0)
                cooldown = 4;
            return;            
        }
    }

    //public class IceProjectile : PlayerProjectile
    //{
    //    private static List<IceProjectile> Instances = new List<IceProjectile>();

    //    private Player player => GameManager.Current.Player;
    //    private Orb orb;

    //    private LightSource light;
    //    private Key key;

    //    private bool headBack;

    //    private float spd;
    //    private float acc;
    //    private float maxVel;
    //    private float maxDist;

    //    private float scale;

    //    private Vector2 originalPosition;

    //    private int cooldown;

    //    private IceEmitter emitter;

    //    private int lifeTime;
    //    private int maxLifeTime;

    //    private IceProjectile(float x, float y, Orb orb, float angle) : base(x, y, orb.Level)
    //    {
    //        Instances.Add(this);

    //        this.orb = orb;

    //        AnimationTexture = AssetManager.Projectiles;

    //        Element = SpellElement.ICE;

    //        Depth = orb.Depth + .0002f;
    //        BoundingBox = new RectF(-4, -4, 8, 8);
    //        DrawOffset = new Vector2(8);

    //        Scale = new Vector2((int)player.Direction * scale, scale);
    //        light = new LightSource(this) { Active = true, Scale = new Vector2(scale * .45f) };

    //        emitter = new IceEmitter(X, Y);
    //        emitter.Active = true;
    //        emitter.Depth = Depth - .0001f;

    //        originalPosition = Position;

    //        scale = 1f;

    //        spd = 0;
    //        maxDist = 48;
    //        acc = .01f;

    //        maxLifeTime = 5 * 60;
    //        lifeTime = maxLifeTime;

    //        switch (orb.Level)
    //        {
    //            case SpellLevel.ONE:
    //                maxDist = 48;
    //                maxVel = 3.5f;
    //                break;
    //            case SpellLevel.TWO:
    //                maxVel = 3.5f;
    //                maxDist = 64;
    //                break;
    //            case SpellLevel.THREE:
    //                maxVel = 3.5f;
    //                maxDist = 64;
    //                break;
    //        }

    //        XVel = (float)MathUtil.LengthDirX(angle) * maxVel;
    //        YVel = (float)MathUtil.LengthDirY(angle) * maxVel;
    //    }

    //    public static void Create(float x, float y, Orb orb)
    //    {
    //        if (Instances.Count > 0)
    //        {
    //            Instances[0].player.MP += GameResources.MPCost[Instances[0].orb.Type][Instances[0].orb.Level];
    //            return;
    //        }

    //        orb.Visible = false;

    //        var angle = (float)MathUtil.VectorToAngle(new Vector2(orb.TargetPosition.X - x, orb.TargetPosition.Y - y));

    //        switch (orb.Level)
    //        {
    //            case SpellLevel.ONE:
    //                new IceProjectile(x, y, orb, angle);
    //                break;
    //            case SpellLevel.TWO:
    //                new IceProjectile(x, y, orb, angle - (Math.Sign((int)orb.Direction) * 15));
    //                new IceProjectile(x, y, orb, angle + (Math.Sign((int)orb.Direction) * 15));
    //                break;
    //            case SpellLevel.THREE:
    //                new IceProjectile(x, y, orb, angle - (Math.Sign((int)orb.Direction) * 15));
    //                new IceProjectile(x, y, orb, angle);
    //                new IceProjectile(x, y, orb, angle + (Math.Sign((int)orb.Direction) * 15));
    //                break;
    //        }
    //    }

    //    public override void Destroy(bool callGC = false)
    //    {
    //        Instances.Remove(this);

    //        if (Instances.Count == 0)
    //        {
    //            orb.Visible = true;
    //        }

    //        FadeOutLight.Create(this, light.Scale);
    //        light.Parent = null;
    //        light.Destroy();

    //        emitter.Active = false;

    //        //orb.Visible = true;

    //        base.Destroy(callGC);
    //    }

    //    public override void Update(GameTime gameTime)
    //    {
    //        base.Update(gameTime);

    //        emitter.Position = player.Position;
    //        emitter.SpawnPosition = Position;

    //        cooldown = Math.Max(cooldown - 1, 0);
    //        if (cooldown > 0)
    //            Damage = 0;
    //        else
    //        {
    //            Damage = (lifeTime > 0) ? 1 : 0;
    //        }

    //        if (lifeTime < (maxLifeTime - 30) && orb.State != OrbState.ATTACK)
    //        {
    //            HeadBack();
    //            lifeTime = 0;
    //        }

    //        if (MathUtil.Euclidean(Position, originalPosition) >= maxDist)
    //        {
    //            HeadBack();
    //        }

    //        var ang = MathUtil.VectorToAngle(new Vector2(orb.X - X, orb.Y - Y));
    //        XVel += spd * (float)(MathUtil.LengthDirX(ang));
    //        YVel += spd * (float)(MathUtil.LengthDirY(ang));

    //        spd = Math.Min(spd + acc, .5f);

    //        XVel = MathUtil.AtMost(XVel, maxVel);
    //        YVel = MathUtil.AtMost(YVel, maxVel);

    //        var coins = this.CollisionBounds<Coin>(X, Y);
    //        foreach (var coin in coins)
    //        {
    //            coin.Take(player);
    //        }

    //        lifeTime = Math.Max(lifeTime - 1, 0);
    //        if (lifeTime == 0)
    //        {
    //            ang = MathUtil.VectorToAngle(new Vector2(orb.X - X, orb.Y - Y));
    //            XVel = 1.5f * (float)(MathUtil.LengthDirX(ang));
    //            YVel = 1.5f * (float)(MathUtil.LengthDirY(ang));
    //            if (this.CollisionBounds(orb, X, Y))
    //                FinishUp();
    //        }

    //        Move(XVel, YVel);
    //        Move(.5f * player.XVel, .5f * player.YVel);

    //        Scale = new Vector2((XVel < 0) ? -scale : scale, scale);

    //        SetAnimation(15, 16, .3f, true);
    //    }

    //    private void HeadBack()
    //    {
    //        if (headBack)
    //            return;

    //        XVel *= .5f;
    //        YVel *= .5f;

    //        var ang = MathUtil.VectorToAngle(new Vector2(XVel, YVel)) + 90 * Math.Sign((int)orb.Direction);
    //        XVel -= 2 * (float)(MathUtil.LengthDirX(ang));
    //        YVel -= 2 * (float)(MathUtil.LengthDirY(ang));

    //        headBack = true;
    //    }

    //    private void FinishUp()
    //    {
    //        if (key != null)
    //        {
    //            player.Stats.KeysAndKeyblocks.Add(key.ID);
    //            player.GetKey();

    //            new KeyBurstEmitter(X, Y, orb);
    //        }
    //        //orb.Position = Position;
    //        orb.Cooldown = 10;
    //        Destroy();
    //    }

    //    public override void Draw(SpriteBatch sb, GameTime gameTime)
    //    {
    //        base.Draw(sb, gameTime);

    //        sb.Draw(AssetManager.Projectiles[17], orb.Position, null, Color, 0, new Vector2(8), Vector2.One, SpriteEffects.None, Depth);

    //        if (key != null)
    //        {
    //            sb.Draw(key.Texture, Position + new Vector2((int)0, 0), null, Color, 0, DrawOffset, Vector2.One, SpriteEffects.None, Depth + .0001f);
    //        }
    //    }

    //    public override void HandleCollision(GameObject obj)
    //    {
    //        if (cooldown == 0)
    //            cooldown = 4;
    //        return;
    //    }
    //}
}
