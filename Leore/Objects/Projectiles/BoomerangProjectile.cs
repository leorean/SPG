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
    public class BoomerangProjectile : PlayerProjectile
    {
        private static List<BoomerangProjectile> Instances = new List<BoomerangProjectile>();

        private Player player => GameManager.Current.Player;
        private Orb orb;

        private LightSource light;
        private Key key;

        private bool headBack;

        private float spd;
        private float acc;
        private float maxVel;
        private float maxDist;

        //private float degAngle;
        //private float angSpd;

        private Vector2 originalPosition;

        private int cooldown;

        IceEmitter emitter;

        int lifeTime;
        int maxLifeTime;

        private BoomerangProjectile(float x, float y, Orb orb, float angle) : base(x, y, orb.Level)
        {
            Instances.Add(this);

            this.orb = orb;

            AnimationTexture = AssetManager.Projectiles;

            Element = SpellElement.ICE;

            Depth = orb.Depth + .0002f;
            BoundingBox = new RectF(-4, -4, 8, 8);
            DrawOffset = new Vector2(8);
            
            Scale = new Vector2((int)player.Direction, 1);
            light = new LightSource(this) { Active = true, Scale = new Vector2(.45f) };

            emitter = new IceEmitter(X, Y);
            emitter.Active = true;
            emitter.Depth = Depth - .0001f;

            originalPosition = Position;
            
            spd = 0;
            maxDist = 48;

            maxLifeTime = 5 * 60;
            lifeTime = maxLifeTime;
            
            switch (orb.Level)
            {
                case SpellLevel.ONE:
                    maxDist = 48;                    
                    acc = .01f;
                    maxVel = 3.5f;
                    //angSpd = 10f;
                    break;
                case SpellLevel.TWO:                    
                    acc = .01f;
                    maxVel = 3.5f;
                    maxDist = 64;
                    //angSpd = 15f;
                    break;
                case SpellLevel.THREE:                    
                    acc = .01f;
                    maxVel = 3.5f;
                    maxDist = 64;
                    //angSpd = 25f;
                    break;
            }

            //var ang = 0;

            //if (player.Direction == Direction.LEFT && player.LookDirection == Direction.UP) ang = 180 + 45;
            //if (player.Direction == Direction.LEFT && player.LookDirection == Direction.NONE) ang = 180;
            //if (player.Direction == Direction.LEFT && player.LookDirection == Direction.DOWN) ang = 180 - 45;
            //if (player.Direction == Direction.RIGHT && player.LookDirection == Direction.UP) ang = 0 - 45;
            //if (player.Direction == Direction.RIGHT && player.LookDirection == Direction.NONE) ang = 0;
            //if (player.Direction == Direction.RIGHT && player.LookDirection == Direction.DOWN) ang = 0 + 45;

            //angSpd = Math.Sign((int)player.Direction) * Math.Abs(angSpd);
            
            //XVel = .9f * player.XVel + (float)MathUtil.LengthDirX(ang) * s;
            //YVel = .3f * player.YVel + (float)MathUtil.LengthDirY(ang) * s;

            //if (player.LookDirection == Direction.UP)
            //    Angle = (float)MathUtil.DegToRad(player.Direction == Direction.LEFT ? 45 : -45);

            XVel = (float)MathUtil.LengthDirX(angle) * maxVel;
            YVel = (float)MathUtil.LengthDirY(angle) * maxVel;
        }

        public static void Create(float x, float y, Orb orb)
        {
            if (Instances.Count > 0)
            {
                Instances[0].player.MP += GameResources.MPCost[Instances[0].orb.Type][Instances[0].orb.Level];
                return;
            }

            var angle = (float)MathUtil.VectorToAngle(new Vector2(orb.TargetPosition.X - x, orb.TargetPosition.Y - y));

            switch (orb.Level)
            {
                case SpellLevel.ONE:
                    new BoomerangProjectile(x, y, orb, angle);
                    break;
                case SpellLevel.TWO:
                    new BoomerangProjectile(x, y, orb, angle - (Math.Sign((int)orb.Direction) * 15));
                    new BoomerangProjectile(x, y, orb, angle + (Math.Sign((int)orb.Direction) * 15));
                    break;
                case SpellLevel.THREE:
                    new BoomerangProjectile(x, y, orb, angle - (Math.Sign((int)orb.Direction) * 15));
                    new BoomerangProjectile(x, y, orb, angle);
                    new BoomerangProjectile(x, y, orb, angle + (Math.Sign((int)orb.Direction) * 15));
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

            //orb.Visible = true;

            base.Destroy(callGC);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            emitter.Position = orb.Position;
            emitter.SpawnPosition = Position;
            
            cooldown = Math.Max(cooldown - 1, 0);
            if (cooldown > 0)
                Damage = 0;
            else
            {
                //Damage = (orb.Level == SpellLevel.ONE) ? 2 : (orb.Level == SpellLevel.TWO) ? 2 : 3;
                Damage = (lifeTime > 0) ? 1 : 0;
            }

            if (lifeTime < (maxLifeTime - 30) && orb.State != OrbState.ATTACK)
            {
                HeadBack();
                lifeTime = 0;
            }

            //if (touchedPlayer)
            //{
            //    if (MathUtil.Euclidean(Position, originalPosition) > Globals.T)
            //    {
            //        touchedPlayer = false;
            //    }
            //}
            //else {
            //    if ((this.CollisionBounds(orb, X, Y)/* || this.CollisionBounds(player, X, Y)*/) && orb.State != OrbState.ATTACK)
            //    {
            //        //FinishUp();
            //        //return;
            //        lifeTime = 0;
            //    }
            //}

            if (MathUtil.Euclidean(Position, originalPosition) >= maxDist)
            {
                HeadBack();
            }

            var ang = MathUtil.VectorToAngle(new Vector2(orb.X - X, orb.Y - Y));
            XVel += spd * (float)(MathUtil.LengthDirX(ang));
            YVel += spd * (float)(MathUtil.LengthDirY(ang));

            spd = Math.Min(spd + acc, .5f);
            
            XVel = MathUtil.AtMost(XVel, maxVel);
            YVel = MathUtil.AtMost(YVel, maxVel);

            var coins = this.CollisionBounds<Coin>(X, Y);
            foreach (var coin in coins)
            {
                coin.Take(player);
            }

            //if (true)
            //{
            //    foreach(var other in Instances)
            //    {
            //        if (other == this) continue;

            //        if(this.CollisionBounds(other, X, Y))
            //        {
            //            var kb = MathUtil.VectorToAngle(new Vector2(X - other.X, Y - other.Y));
            //            XVel += 1.5f * (float)MathUtil.LengthDirX(kb);
            //            YVel += 1.5f * (float)MathUtil.LengthDirY(kb);
            //        }
            //    }
            //}

            //if (key == null)
            //{
            //    // let's not give this spell the power to collect keys..
            //    //key = this.CollisionBoundsFirstOrDefault<Key>(X, Y);

            //    //if (key != null)
            //    //{
            //    //    key.Destroy();
            //    //    HeadBack();
            //    //}

            //    //if (player.HasAtLeastOneKey())
            //    //{
            //    //    var keyBlock = this.CollisionBoundsFirstOrDefault<KeyBlock>(X, Y);
            //    //    if (keyBlock != null && !keyBlock.Unlocked)
            //    //    {
            //    //        new SingularEffect(keyBlock.Center.X, keyBlock.Center.Y, 9);
            //    //        keyBlock.Unlock(keyBlock.Center.X, keyBlock.Center.Y);
            //    //        player.UseKeyFromInventory();
            //    //        HeadBack();
            //    //    }
            //    //}
            //}

            ////if (!headBack)
            ////{
            ////    //var laser = this.CollisionBoundsFirstOrDefault<LaserObstacle>(X, Y);
            ////    //if (laser != null)
            ////    //{
            ////    //    new StarEmitter(X, Y, 5);
            ////    //    HeadBack();
            ////    //}
            ////}
            ////else
            ////{
            ////    //if (orb.State != OrbState.ATTACK)
            ////    //{
            ////    //    var dist = MathUtil.Euclidean(Position, player.Position);
            ////    //    if (dist < 8)
            ////    //    {
            ////    //        FinishUp();
            ////    //    }
            ////    //}
            ////}

            lifeTime = Math.Max(lifeTime - 1, 0);
            if (lifeTime == 0)
            {
                ang = MathUtil.VectorToAngle(new Vector2(orb.X - X, orb.Y - Y));
                XVel = 1.5f * (float)(MathUtil.LengthDirX(ang));
                YVel = 1.5f * (float)(MathUtil.LengthDirY(ang));
                if (this.CollisionBounds(orb, X, Y))
                    FinishUp();
            }

            Move(XVel, YVel);
            Move(.5f * player.XVel, .5f * player.YVel);

            //degAngle = (degAngle + angSpd) % 360;
            //Angle = (float)MathUtil.DegToRad(degAngle);            
            //Angle = (float)MathUtil.VectorToAngle(new Vector2(XVel, YVel), true);

            Scale = new Vector2((XVel < 0) ? -1 : 1, 1);
            SetAnimation(15, 16, .3f, true);
        }

        private void HeadBack()
        {
            if (headBack)
                return;

            XVel *= .5f;
            YVel *= .5f;

            var ang = MathUtil.VectorToAngle(new Vector2(XVel, YVel)) + 90 * Math.Sign((int)orb.Direction);
            XVel -= 2 * (float)(MathUtil.LengthDirX(ang));
            YVel -= 2 * (float)(MathUtil.LengthDirY(ang));

            headBack = true;            
        }

        private void FinishUp()
        {
            if (key != null)
            {
                player.Stats.KeysAndKeyblocks.Add(key.ID);
                player.GetKey();

                new KeyBurstEmitter(X, Y, orb);
            }
            //orb.Position = Position;
            orb.Cooldown = 10;
            Destroy();
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);

            if (key != null)
            {
                sb.Draw(key.Texture, Position + new Vector2((int)0, 0), null, Color, 0, DrawOffset, Vector2.One, SpriteEffects.None, Depth + .0001f);
            }
        }

        public override void HandleCollision(GameObject obj)
        {
            if (cooldown == 0)
                cooldown = 4;
            return;            
        }
    }
}
