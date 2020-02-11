using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leore.Main;
using Leore.Objects.Effects;
using Leore.Objects.Effects.Emitters;
using Leore.Objects.Enemies;
using Leore.Objects.Items;
using Leore.Objects.Level;
using Leore.Objects.Level.Blocks;
using Leore.Objects.Obstacles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Objects;
using SPG.Util;

namespace Leore.Objects.Projectiles
{
    public class KeySnatchProjectile : PlayerProjectile
    {
        private static KeySnatchProjectile instance;

        private Orb orb => GameManager.Current.Player.Orb;
        private Player player => GameManager.Current.Player;

        private float dist;
        private float maxDist = 3 * Globals.T;
        private Direction direction;

        private bool headBack;
        private Key key;
        private int cooldown;

        private float yspd;

        private Vector2 originalPosition;

        private Vector2 maxPosition;

        private LightSource light;

        private KeySnatchProjectile(float x, float y) : base(x, y, SpellLevel.ONE)
        {
            originalPosition = orb.TargetPosition;

            //orb.Visible = false;
            Depth = orb.Depth + .0002f;
            BoundingBox = new RectF(-4, -4, 8, 8);
            DrawOffset = new Vector2(8);
            direction = player.Direction;
            yspd = Math.Sign((int)player.LookDirection);

            if (yspd != 0)
                maxDist *= .75f;

            light = new LightSource(this) { Active = true, Scale = new Vector2(.45f) };
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            //orb.Position = originalPosition;

            cooldown = Math.Max(cooldown - 1, 0);
            if (cooldown > 0)
                Damage = 0;
            else
                Damage = 1;

            Scale = new Vector2((int)direction, 1);
            Angle = (float)MathUtil.VectorToAngle(new Vector2(1, yspd * Scale.X), true);

            if (!headBack)
            {

                var coins = this.CollisionBounds<Coin>(X, Y);
                foreach(var coin in coins)
                {
                    coin.Take(player);
                }

                if (key == null)
                    key = this.CollisionBoundsFirstOrDefault<Key>(X, Y);

                if (key != null)
                {
                    key.Destroy();
                    headBack = true;
                }

                if (player.HasAtLeastOneKey())
                {
                    var keyBlock = this.CollisionBoundsFirstOrDefault<KeyBlock>(X, Y);
                    if (keyBlock != null && !keyBlock.Unlocked)
                    {
                        new SingularEffect(keyBlock.Center.X, keyBlock.Center.Y, 9);
                        keyBlock.Unlock(keyBlock.Center.X, keyBlock.Center.Y);
                        player.UseKeyFromInventory();
                        headBack = true;
                    }
                }

                var laser = this.CollisionBoundsFirstOrDefault<LaserObstacle>(orb.TargetPosition.X + (int)direction * (dist + 0), orb.TargetPosition.Y + yspd * (dist + 0));
                if (laser != null)
                {
                    new StarEmitter(X, Y, 5);
                    headBack = true;
                }
                
                dist = Math.Min(dist + 3, maxDist);
                if (dist == maxDist)
                    headBack = true;
            }
            else
            {
                dist = Math.Max(dist - 3, 0);
                if (dist == 0)
                {
                    if (key != null)
                    {
                        player.Stats.KeysAndKeyblocks.Add(key.ID);
                        player.GetKey();

                        new KeyBurstEmitter(X, Y, orb);                        
                    }
                    orb.Cooldown = 10;
                    Destroy();
                }
            }

            Position = orb.Position + new Vector2((int)direction *  dist, yspd * dist);

            maxPosition = orb.Position + new Vector2((int)direction * maxDist, yspd * maxDist);

            //if (orb.State != OrbState.ATTACK || player.Direction != direction)
            if (player.Direction != direction)
            {
                    headBack = true;                
            }
        }

        public override void Destroy(bool callGC = false)
        {
            //orb.Visible = true;
            instance = null;
            base.Destroy(callGC);
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);

            //sb.Draw(AssetManager.Projectiles[7], orb.Position, null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, orb.Depth);

            //for (var i = 0; i < dist; i+= 2)
            //{
            //    sb.Draw(AssetManager.Projectiles[8], orb.Position + new Vector2((int)direction * i, Math.Sign(yspd) * i), null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth);
            //}

            var texture = headBack ? AssetManager.Projectiles[9] : AssetManager.Projectiles[8];
            
            //sb.Draw(texture, Position, null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth);
            
            if (!headBack)
            {
                sb.Draw(AssetManager.Projectiles[7], Position, null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth);
                sb.Draw(texture, maxPosition, null, new Color(Color, 1 * (dist / maxDist)), Angle, DrawOffset, Scale, SpriteEffects.None, Depth - .0001f);
            }
            else
            {
                sb.Draw(texture, Position, null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth);
            }

            if (key != null)
            {
                sb.Draw(key.Texture, Position + new Vector2((int)direction * 8, 0), null, Color, Angle, DrawOffset, Vector2.One, SpriteEffects.None, Depth + .0001f);
            }
        }
        
        public static void Create(float x, float y)
        {
            if (instance == null)
            {
                instance = new KeySnatchProjectile(x, y);
            }
        }

        public override void HandleCollision(GameObject obj)
        {
            if (cooldown == 0)
                cooldown = 5;
        }
    }
}
