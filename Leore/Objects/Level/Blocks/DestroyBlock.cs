using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Leore.Main;
using Leore.Objects.Effects;
using Leore.Objects.Effects.Emitters;
using SPG.Util;
using Leore.Objects.Projectiles;
using Leore.Objects.Enemies;

namespace Leore.Objects.Level.Blocks
{
    public class IceBlock : DestroyBlock
    {
        private float maxHp;
        private int regenDelay;

        public IceBlock(float x, float y, Room room) : base(x, y, room, 6)
        {
            maxHp = HP;            
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            regenDelay = Math.Max(regenDelay - 1, 0);
            if (regenDelay == 0)
            {
                HP = (int)Math.Min(HP + 1, maxHp);
                regenDelay = 30;
            }            
        }
        
        public override void Hit(PlayerProjectile projectile)
        {
            if (projectile.Damage == 0 || projectile.Element != SpellElement.FIRE)
                return;

            // TODO: effect

            if (HP <= projectile.Damage)
                new DestroyEmitter(X + 8, Y + 8, 4);

            HP = Math.Max(HP - projectile.Damage, 0);
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);

            var alpha = HP / maxHp;
            var col = new Color(Color, .5f - .5f * alpha);            
            sb.Draw(AssetManager.Particles[11], Position + new Vector2(8), null, col, Angle, new Vector2(8), Scale, SpriteEffects.None, Depth + .0001f);
        }
    }

    public class FireBlock : DestroyBlock
    {
        private float maxHp;

        private Obstacle obstacle;

        public FireBlock(float x, float y, Room room) : base(x, y, room, 5)
        {
            maxHp = HP;

            obstacle = new ObstacleBlock(x, y, room) { Parent = this };
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Hit(PlayerProjectile projectile)
        {
            if (projectile.Damage == 0)
            //if (projectile.Damage == 0 || projectile.Element != SpellElement.ICE)
            //if (projectile.Damage == 0 || (projectile.Element != SpellElement.ICE && projectile.Element != SpellElement.WATER))
                return;

            // TODO: effect

            if (HP <= projectile.Damage)
                new DestroyEmitter(X + 8, Y + 8, 6);
            
            HP = Math.Max(HP - projectile.Damage, 0);
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);

            var alpha = HP / maxHp;
            var col = new Color(Color, 1f - .5f * alpha);
            sb.Draw(AssetManager.Particles[12], Position + new Vector2(8), null, col, Angle, new Vector2(8), Scale, SpriteEffects.None, Depth + .0001f);
        }
    }

    public class DestroyBlock : Solid
    {
        public int HP { get; set; }

        private Texture2D tex1 = GameManager.Current.Map.TileSet[714];
        private Texture2D tex2 = GameManager.Current.Map.TileSet[715];

        public DestroyBlock(float x, float y, Room room, int hp) : base(x, y, room)
        {
            Depth = Globals.LAYER_FG;
            Visible = true;
            HP = hp;

            if (HP == 1) Texture = tex1;
            if (HP == 2) Texture = tex2;
        }

        public virtual void Hit(PlayerProjectile projectile)
        {
            if (projectile.Damage == 0)
                return;

            if (HP > projectile.Damage)
                new SingularEffect(Center.X - 4 + (float)(RND.Next * 8), Center.Y - 4 + (float)(RND.Next * 8), 5);
            else
                new DestroyEmitter(X + 8, Y + 8);
            
            HP = Math.Max(HP - projectile.Damage, 0);

            if (HP == 1) Texture = tex1;
            if (HP == 2) Texture = tex2;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (HP == 0)
            {
                new SingularEffect(X + 8, Y + 8);
                Destroy();
            }            
        }
    }
}
