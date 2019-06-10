using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leore.Main;
using Leore.Objects.Level;
using Microsoft.Xna.Framework;
using SPG.Objects;

namespace Leore.Objects.Projectiles
{
    public class VoidProjectile : PlayerProjectile
    {
        float alpha = 1;
        public VoidProjectile(float x, float y, Orb parent) : base(x, y, parent.Level)
        {
            Parent = parent;

            Texture = AssetManager.WhiteCircle;
            DrawOffset = new Vector2(32);
            Scale = Vector2.Zero;
            Color = Color.Black;

            //DebugEnabled = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Position = Parent.Position;

            Scale = new Vector2(Math.Min(Scale.X + .03f, 1));

            alpha = 1 - Scale.X / 1;
            Color = new Color(Color, alpha);

            var radius = Math.Max((32 - 6) * Scale.X, 0);
            var bx = -radius;
            var bw = 2 * radius;             
            BoundingBox = new SPG.Util.RectF(bx, bx, bw, bw);

            if (Scale.X == 1)
            {
                Parent = null;
                Destroy();
            }
        }

        public override void HandleCollision(GameObject obj)
        {
            //throw new NotImplementedException();
        }

        public override void HandleCollisionFromDestroyBlock(DestroyBlock block)
        {
            //base.HandleCollisionFromDestroyBlock(block);
            block.Hit(Damage);
        }

        
    }
}
