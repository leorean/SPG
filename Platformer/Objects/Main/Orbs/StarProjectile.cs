using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Platformer.Main;
using SPG.Map;
using SPG.Objects;
using SPG.Util;

namespace Platformer.Objects.Main.Orbs
{
    public class StarProjectile : GameObject
    {
        public StarProjectile(float x, float y) : base(x, y)
        {
            //Texture = AssetManager.Ouch;

            BoundingBox = new RectF(-2, -2, 4, 4);
            DebugEnabled = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Move(XVel, YVel);

            var solid = GameManager.Current.Map.CollisionTile(X, Y);

            var cam = RoomCamera.Current;
            var outOfView = X < cam.ViewX || Y < cam.ViewY || X > cam.ViewX + cam.ViewWidth || Y > cam.ViewY + cam.ViewHeight;

            if (solid || outOfView)
                Destroy();
        }
    }
}
