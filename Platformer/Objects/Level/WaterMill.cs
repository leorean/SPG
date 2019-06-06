using Microsoft.Xna.Framework;
using Platformer.Main;
using System;
using System.Collections.Generic;

namespace Platformer.Objects.Level
{
    public class WaterMill : RoomObject
    {
        private List<WaterMillMovingPlatform> platforms = new List<WaterMillMovingPlatform>();

        public float AngleVel { get; set; } = 0.01f;

        public WaterMill(float x, float y, Room room, string name = null) : base(x, y, room, name)
        {
            Texture = AssetManager.WaterMill;

            DrawOffset = new Vector2(64, 64);
            Depth = Globals.LAYER_BG + .0001f;

            for(var i = 0; i < 6; i++)
            {
                var ang = i * (360 / 6f);
                platforms.Add(new WaterMillMovingPlatform(this, ang));
            }

            Scale = new Vector2(.5f);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Angle = (Angle + AngleVel) % (float)(2 * Math.PI);
        }
    }
}
