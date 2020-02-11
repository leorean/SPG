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
        private static BoomerangProjectile Current;

        private Player player => GameManager.Current.Player;
        private Orb orb;

        private LightSource light;
        private Key key;

        private bool touchedPlayer;
        private bool headBack;

        private float spd;
        private float acc;
        private float maxVel;
        private float maxDist;

        private float degAngle;
        private float angSpd;

        private Vector2 originalPosition;

        private int cooldown;

        IceEmitter emitter;

        private BoomerangProjectile(float x, float y, Orb orb) : base(x, y, orb.Level)
        {
            this.orb = orb;

            AnimationTexture = AssetManager.Projectiles;

            Depth = orb.Depth + .0002f;
            BoundingBox = new RectF(-4, -4, 8, 8);
            DrawOffset = new Vector2(8);
            
            Scale = new Vector2((int)player.Direction, 1);
            light = new LightSource(this) { Active = true, Scale = new Vector2(.45f) };

            emitter = new IceEmitter(X, Y);
            emitter.Active = true;

            originalPosition = Position;

            touchedPlayer = true;

            spd = 0;
            maxDist = 48;

            var s = 0;
            
            switch (orb.Level)
            {
                case SpellLevel.ONE:
                    //Texture = AssetManager.Projectiles[15];
                    maxDist = 48;
                    s = 4;
                    acc = .01f;
                    maxVel = 3.5f;
                    angSpd = 10f;
                    break;
                case SpellLevel.TWO:
                    //Texture = AssetManager.Projectiles[16];
                    s = 8;
                    acc = .01f;
                    maxVel = 4f;
                    maxDist = 64;
                    angSpd = 15f;
                    break;
                case SpellLevel.THREE:
                    //Texture = AssetManager.Projectiles[17];
                    s = 16;
                    acc = .01f;
                    maxVel = 5f;
                    maxDist = 64;
                    angSpd = 25f;
                    break;
            }

            var ang = 0;

            if (player.Direction == Direction.LEFT && player.LookDirection == Direction.UP) ang = 180 + 45;
            if (player.Direction == Direction.LEFT && player.LookDirection == Direction.NONE) ang = 180;
            if (player.Direction == Direction.LEFT && player.LookDirection == Direction.DOWN) ang = 180 - 45;
            if (player.Direction == Direction.RIGHT && player.LookDirection == Direction.UP) ang = 0 - 45;
            if (player.Direction == Direction.RIGHT && player.LookDirection == Direction.NONE) ang = 0;
            if (player.Direction == Direction.RIGHT && player.LookDirection == Direction.DOWN) ang = 0 + 45;

            angSpd = Math.Sign((int)player.Direction) * Math.Abs(angSpd);


            ObjectManager.Enable(emitter);

            //XVel = .9f * player.XVel + (float)MathUtil.LengthDirX(ang) * s;
            //YVel = .3f * player.YVel + (float)MathUtil.LengthDirY(ang) * s;

            //if (player.LookDirection == Direction.UP)
            //    Angle = (float)MathUtil.DegToRad(player.Direction == Direction.LEFT ? 45 : -45);

            XVel = (float)MathUtil.LengthDirX(ang) * s;
            YVel = (float)MathUtil.LengthDirY(ang) * s;            
        }

        public static BoomerangProjectile Create(float x, float y, Orb parent)
        {
            if (Current != null)
            {
                //Current.player.MP += Math.Max(Current.player.MP - GameResources.MPCost[Current.orb.Type][Current.orb.Level], 0);
                Current.player.MP += GameResources.MPCost[Current.orb.Type][Current.orb.Level];
                return Current;
            }

            Current = new BoomerangProjectile(x, y, parent);

            //new CrimsonBurstEmitter(x, y) { ParticleColors = new List<Color>() { Color.Black } };
            return Current;
        }

        public override void Destroy(bool callGC = false)
        {
            if (Current == this)
                Current = null;
            
            FadeOutLight.Create(this, light.Scale);
            light.Parent = null;
            light.Destroy();

            emitter.Active = false;

            orb.Visible = true;

            base.Destroy(callGC);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            emitter.Position = Position;

            cooldown = Math.Max(cooldown - 1, 0);
            if (cooldown > 0)
                Damage = 0;
            else
            {
                Damage = (orb.Level == SpellLevel.ONE) ? 1 : (orb.Level == SpellLevel.TWO) ? 2 : 3;
            }

            if (touchedPlayer)
            {
                if (MathUtil.Euclidean(Position, player.Position) > Globals.T)
                {
                    touchedPlayer = false;
                }
            }
            else {
                if (this.CollisionBounds(player, X, Y))
                {
                    FinishUp();
                    return;
                }
            }

            if (MathUtil.Euclidean(Position, originalPosition) >= maxDist)
            {
                HeadBack();
            }

            var ang = MathUtil.VectorToAngle(new Vector2(player.X - X, player.Y - Y));
            XVel += spd * (float)(MathUtil.LengthDirX(ang));
            YVel += spd * (float)(MathUtil.LengthDirY(ang));

            spd = Math.Min(spd + acc, 1);
            
            XVel = MathUtil.AtMost(XVel, maxVel);
            YVel = MathUtil.AtMost(YVel, maxVel);

            var coins = this.CollisionBounds<Coin>(X, Y);
            foreach (var coin in coins)
            {
                coin.Take(player);
            }

            if (key == null)
            {

                key = this.CollisionBoundsFirstOrDefault<Key>(X, Y);

                if (key != null)
                {
                    key.Destroy();
                    HeadBack();
                }

                if (player.HasAtLeastOneKey())
                {
                    var keyBlock = this.CollisionBoundsFirstOrDefault<KeyBlock>(X, Y);
                    if (keyBlock != null && !keyBlock.Unlocked)
                    {
                        new SingularEffect(keyBlock.Center.X, keyBlock.Center.Y, 9);
                        keyBlock.Unlock(keyBlock.Center.X, keyBlock.Center.Y);
                        player.UseKeyFromInventory();
                        HeadBack();
                    }
                }
            }

            if (!headBack)
            {
                var laser = this.CollisionBoundsFirstOrDefault<LaserObstacle>(X, Y);
                if (laser != null)
                {
                    new StarEmitter(X, Y, 5);
                    HeadBack();
                }             
            }
            else
            {
                var dist = MathUtil.Euclidean(Position, player.Position);
                if (dist < 8)
                {
                    FinishUp();
                }
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
            orb.Position = Position;
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
            //throw new NotImplementedException();
            if (cooldown == 0)
                cooldown = 5;

            return;

            //if (obj is IIgnoreRollKnockback)
            //    return;

            //if (orb.Level == SpellLevel.THREE)
            //    return;

            if (!headBack)
            {
                XVel *= -.5f;
                YVel *= -.5f;
                headBack = true;
            }
        }
    }
}
