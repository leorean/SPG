using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Platformer.Main;

namespace Platformer.Objects.Projectiles
{
    public class PlayerProjectile : SPG.Objects.GameObject
    {
        public int Damage { get; protected set; } = 1;

        public PlayerProjectile(float x, float y) : base(x, y)
        {
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var cam = RoomCamera.Current;
            var outOfView = X < cam.ViewX || Y < cam.ViewY || X > cam.ViewX + cam.ViewWidth || Y > cam.ViewY + cam.ViewHeight;
            if (outOfView)
                Destroy();

        }
    }
}
