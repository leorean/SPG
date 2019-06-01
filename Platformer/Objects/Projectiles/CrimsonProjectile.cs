using Microsoft.Xna.Framework;
using Platformer.Objects.Level;
using Platformer.Objects.Main;
using Platformer.Objects.Main.Orbs;
using SPG.Map;
using SPG.Objects;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Projectiles
{
    public class CrimsonProjectile : PlayerProjectile
    {
        public CrimsonProjectile(float x, float y, SpellLevel level) : base(x, y, level)
        {
            BoundingBox = new SPG.Util.RectF(-2, -2, 4, 4);
            DrawOffset = new Vector2(8);

            switch (level)
            {
                case SpellLevel.ONE:
                    Damage = 1;
                    break;
                case SpellLevel.TWO:
                    Damage = 1;
                    break;
                case SpellLevel.THREE:
                    Damage = 2;
                    break;
            }

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            YVel += .06f;

            var solid = GameManager.Current.Map.CollisionTile(X, Y) || GameManager.Current.Map.CollisionTile(X, Y, GameMap.WATER_INDEX);
            if (!solid)
            {
                solid = ObjectManager.CollisionPointFirstOrDefault<Solid>(X, Y) != null;
            }

            if (solid)
                Destroy();

            /*switch (level)
            {
                case SpellLevel.ONE:
                    if (solid)
                        Destroy();
                    break;
            }*/

            Angle = (float)MathUtil.VectorToAngle(new Vector2(XVel, YVel), true);

            Move(XVel, YVel);

            if (this.IsOutsideCurrentRoom())
                Destroy();
        }

        public override void HandleCollision()
        {
            //throw new NotImplementedException();
        }
    }
}
