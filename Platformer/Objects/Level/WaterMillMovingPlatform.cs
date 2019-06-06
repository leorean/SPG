using Microsoft.Xna.Framework;
using Platformer.Main;
using SPG.Util;

namespace Platformer.Objects.Level
{
    public class WaterMillMovingPlatform : MovingPlatform
    {
        private double angle;
        private float dist = 32;

        private Vector2 lastPosition;

        public WaterMillMovingPlatform(WaterMill mill, float angle) :base(mill.X, mill.Y, mill.Room)
        {
            Texture = AssetManager.WaterMillPlatform;

            BoundingBox = new SPG.Util.RectF(-8, 0, 16, 1);
            DrawOffset = new Vector2(8, 0);
            
            Parent = mill;

            Depth = Parent.Depth + .0001f;

            this.angle = angle;

            Position = mill.Position;
            lastPosition = Position;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        
            double ang = (MathUtil.RadToDeg(Parent.Angle) + angle) % 360f;

            Position = new Vector2(Parent.X + dist * (float)MathUtil.LengthDirX((float)ang), Parent.Y + dist * (float)MathUtil.LengthDirY((float)ang));

            Visible = true;

            XVel = (Position.X - lastPosition.X);
            YVel = (Position.Y - lastPosition.Y);
            
            lastPosition = Position;
        }
    }
}
