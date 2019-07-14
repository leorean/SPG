using Leore.Main;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPG.Map;
using SPG.Objects;
using Leore.Objects.Effects;
using SPG.Util;

namespace Leore.Objects.Enemies
{
    public class DefaultEnemyProjectile : EnemyProjectile
    {
        public DefaultEnemyProjectile(float x, float y) : base(x, y)
        {
            Texture = AssetManager.Projectiles[13];
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Angle = (Angle + (float)MathUtil.DegToRad(45)) % 360;
        }
    }

    public abstract class EnemyProjectile : Obstacle
    {
        public EnemyProjectile(float x, float y) : base(x, y, RoomCamera.Current.CurrentRoom)
        {
            Depth = Globals.LAYER_ENEMY + .0005f;

            BoundingBox = new SPG.Util.RectF(-2, -2, 4, 4);
            DrawOffset = new Vector2(8);
            //DebugEnabled = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Move(XVel, YVel);

            var col = GameManager.Current.Map.CollisionTile(X + XVel, Y + YVel);
            if (col)
            {
                Kill();
            }

            if (this.IsOutsideCurrentRoom(Globals.T))
                Destroy();
        }

        public virtual void Kill()
        {
            new SingularEffect(X, Y, 10);
            Destroy();
        }
    }
}
