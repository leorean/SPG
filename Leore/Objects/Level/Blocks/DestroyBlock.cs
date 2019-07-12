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
            if (projectile.Damage == 0 || HP == 0)
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
