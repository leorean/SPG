using Leore.Main;
using Leore.Objects.Effects.Emitters;
using Leore.Objects.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if (projectile.Damage == 0 || HP == 0 || projectile.Element != SpellElement.FIRE)
                return;
            
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
}
