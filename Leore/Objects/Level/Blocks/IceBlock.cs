﻿using Leore.Main;
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

namespace Leore.Objects.Level.Blocks
{
    public class IceBlock : DestroyBlock
    {
        private float maxHp;
        private int regenDelay;

        private float hp;
        private float cooldown;

        public IceBlock(float x, float y, Room room) : base(x, y, room, 6)
        {
            maxHp = HP;
            this.hp = HP;
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
                new DestroyEmitter(X + 8, Y + 8, 4);                
            }
            else
            {
                if (true)
                {
                    var eff = new SingularEffect(Center.X, Center.Y, 18);
                    eff.Depth = Depth + .0001f;
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
                sb.Draw(AssetManager.Particles[11], Position + new Vector2(8), null, col, Angle, new Vector2(8), Scale, SpriteEffects.None, Depth + .0001f);
            }
        }
    }
}
