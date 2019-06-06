using Microsoft.Xna.Framework;
using Platformer.Objects.Effects;
using Platformer.Objects.Effects.Emitters;
using Platformer.Objects.Projectiles;
using SPG.Objects;

namespace Platformer.Objects.Level
{
    public class Bush : RoomObject
    {
        public Bush(float x, float y, Room room) : base(x, y, room)
        {
            Depth = Globals.LAYER_BG - 0.0001f;
            BoundingBox = new SPG.Util.RectF(4, 4, 8, 12);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var proj = this.CollisionBoundsFirstOrDefault<PlayerProjectile>(X, Y);

            if (proj != null)
            {
                //var spawn = RND.Choose(true, false);

                //if (spawn)
                //{
                //    float value = (float)Math.Floor(30 * RND.Next);

                //    Coin.Spawn(Center.X, Center.Y, Room, value);

                //    //float value = RND.Choose(Coin.CoinValue.C1, Coin.CoinValue.C2, Coin.CoinValue.C3).Value;
                //    //Coin.Spawn(Center.X, Center.Y, Room, value, true);
                //}

                //proj.HandleCollision();

                new SingularEffect(Center.X, Center.Y, 7);
                new DestroyEmitter(Center.X, Center.Y, 2);
                Destroy();
            }
        }
    }
}
