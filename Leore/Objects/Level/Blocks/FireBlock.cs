using Leore.Main;
using Leore.Objects.Effects;
using Leore.Objects.Effects.Emitters;
using Leore.Objects.Enemies;
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
    public class FireBlock : DestroyBlock
    {
        private float maxHp;

        private Obstacle obstacle;

        public FireBlock(float x, float y, Room room) : base(x, y, room, 1)
        {
            maxHp = HP;

            obstacle = new ObstacleBlock(x, y, room) { Parent = this };
        }

        public override void Update(GameTime gameTime)
        {
            //base.Update(gameTime);
            if (HP == 0)
                Destroy();
        }

        public override void Hit(PlayerProjectile projectile)
        {
            if (projectile.Damage == 0 || HP == 0)
                return;

            if (projectile.Element != SpellElement.ICE)
                return;

            if (HP <= projectile.Damage)
            {
                var eff = new DestroyEmitter(Center.X, Center.Y, 6);
                new SingularEffect(Center.X, Center.Y, 15) { Depth = eff.Depth + .0001f, Scale = new Vector2(.5f) };

            }

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
}
