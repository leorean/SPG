using Leore.Main;
using Leore.Objects.Effects;
using Leore.Objects.Effects.Emitters;
using Leore.Objects.Enemies;
using Leore.Objects.Level.Obstacles;
using Leore.Objects.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Leore.Objects.Level.Blocks
{
    public class FireBlock : DestroyBlock
    {
        private int maxHp;

        private Obstacle obstacle;

        public int OriginalTextureID { get; set; }

        private float hp;

        public FireBlock(float x, float y, Room room, int hp) : base(x, y, room, hp)
        {
            maxHp = HP;
            obstacle = new ObstacleBlock(x, y, room) { Parent = this };

            this.hp = HP;
        }

        public override void Update(GameTime gameTime)
        {
            //base.Update(gameTime);
            
            Texture = GameManager.Current.Map.TileSet[OriginalTextureID - maxHp + HP];

            HP = (int)Math.Ceiling(hp);
            
            if (HP <= 1 && obstacle != null)
            {
                obstacle.Parent = null;
                obstacle.Destroy();
                obstacle = null;
            }            
        }

        public override bool Hit(PlayerProjectile projectile)
        {
            if (projectile.Damage == 0 || hp == 0)
                return false;

            if (projectile.Element != SpellElement.ICE)
                return false;

            var dmg = .5f;//damage / 10f;

            hp = Math.Max(hp - dmg, 0);

            if (hp <= dmg)
            {
                var eff = new DestroyEmitter(Center.X, Center.Y, 6);
                new SingularEffect(Center.X, Center.Y, 15) { Depth = eff.Depth + .0001f, Scale = new Vector2(.5f) };
                Destroy();
            }
            else
            {
                var eff = new SingularEffect(Center.X, Center.Y, 16);
                eff.Depth = Depth + .0001f;
            }

            //HP = Math.Max(HP - damage, 0);
            
            return true;
        }        
    }
}
