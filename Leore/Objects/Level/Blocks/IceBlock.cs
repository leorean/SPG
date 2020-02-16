using Leore.Main;
using Leore.Objects.Effects;
using Leore.Objects.Effects.Emitters;
using Leore.Objects.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPG.Objects;
using SPG.Util;

namespace Leore.Objects.Level.Blocks
{
    public class IceBlock : DestroyBlock
    {
        private float maxHp;
        private int regenDelay;

        private float hp;
        private float cooldown;

        private int shimmerTimer;

        public IceBlock(float x, float y, Room room) : base(x, y, room, 6)
        {
            maxHp = HP;
            this.hp = HP;

            BoundingBox = new RectF(-8, -8, 16, 16);

            DrawOffset = new Vector2(8);
            
            shimmerTimer = RND.Int(2 * 60);
        }

        public override void Update(GameTime gameTime)
        {
            //base.Update(gameTime);

            HP = (int)Math.Ceiling(hp);

            cooldown = Math.Max(cooldown - 1, 0);

            regenDelay = Math.Max(regenDelay - 1, 0);
            if (cooldown == 0&& regenDelay == 0)
            {
                hp = Math.Min(hp + .1f, maxHp);
                regenDelay = 10;
            }

            shimmerTimer = Math.Max(shimmerTimer - 1, 0);
            if (shimmerTimer == 0)
            {
                var eff = new SingularEffect(Center.X, Center.Y, 18);
                eff.Depth = Depth + .0001f;

                shimmerTimer = 60 + RND.Int(2 * 60);
            }

            Scale = new Vector2(Math.Min(Scale.X + .02f, 1));
            //Angle *= .9f;

            if (hp == 0)
            {
                Destroy();
            }
        }

        public override bool Hit(PlayerProjectile projectile)
        {
            if (projectile.Damage == 0 || HP == 0 || projectile.Element != SpellElement.FIRE || cooldown > 0)
                return false;

            cooldown = 10;

            var dmg = projectile.Damage;
            
            if (hp <= dmg)
            {
                new DestroyEmitter(X, Y, 4);                
            }
            else
            {
                if (true)
                {
                    Scale = new Vector2(.9f);
                    //Angle = (float)MathUtil.DegToRad(-6 + RND.Int(12));
                    /*var eff = new SingularEffect(Center.X, Center.Y, 19);
                    eff.Depth = Depth + .0001f;*/
                }
            }
            
            hp = Math.Max(hp - dmg, 0);

            return true;
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);

            if (hp > 0)
            {
                var alpha = hp / maxHp;
                var col = new Color(Color, .5f - .5f * alpha);
                sb.Draw(AssetManager.Particles[11], Position, null, col, Angle, new Vector2(8), Scale, SpriteEffects.None, Depth + .0001f);
            }
        }
    }
}
