using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Platformer.Objects.Effects;
using Platformer.Objects.Effects.Emitters;
using Platformer.Objects.Items;
using Platformer.Objects.Projectiles;
using SPG.Objects;
using SPG.Util;

namespace Platformer.Objects.Level
{
    public class Pot : RoomObject
    {
        public Pot(float x, float y, Room room) : base(x, y, room)
        {
            Depth = Globals.LAYER_FG;
            BoundingBox = new SPG.Util.RectF(4, 4, 8, 12);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var proj = this.CollisionBoundsFirstOrDefault<PlayerProjectile>(X, Y);

            if (proj != null)
            {
                //var spawn = RND.Choose(true, false, false);
                //float value = (float)Math.Floor(30 * RND.Next);

                var value = RND.Choose(0, 0, 0, 1, 2, 5, 5, 10, 15, 20, 25, 30);

                if (value > 0)
                {
                    Coin.Spawn(Center.X, Center.Y, Room, value);

                    //float value = RND.Choose(Coin.CoinValue.C1, Coin.CoinValue.C2, Coin.CoinValue.C3).Value;
                    //Coin.Spawn(Center.X, Center.Y, Room, value, true);
                }

                proj.HandleCollision(this);
                new SingularEffect(Center.X, Center.Y, 7);
                new DestroyEmitter(Center.X, Center.Y, 1);
                Destroy();
            }
        }
    }
}
