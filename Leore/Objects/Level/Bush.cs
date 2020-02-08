using Microsoft.Xna.Framework;
using Leore.Objects.Effects;
using Leore.Objects.Effects.Emitters;
using Leore.Objects.Projectiles;
using SPG.Objects;

namespace Leore.Objects.Level
{
    public class Bush : RoomObject
    {
        public Bush(float x, float y, Room room) : base(x, y, room)
        {
            //Depth = Globals.LAYER_BG - 0.0001f;
            Depth = Globals.LAYER_BG + 0.0001f;
            BoundingBox = new SPG.Util.RectF(4, 4, 8, 12);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var proj = this.CollisionBoundsFirstOrDefault<PlayerProjectile>(X, Y);

            if (proj != null)
            {
                new SingularEffect(Center.X, Center.Y, 7);
                new DestroyEmitter(Center.X, Center.Y, 2);
                Destroy();
            }
        }
    }
}
