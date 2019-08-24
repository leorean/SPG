using Leore.Main;
using Leore.Objects.Level;
using Leore.Objects.Level.Obstacles;
using Leore.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Leore.Objects.Obstacles.Laser;

namespace Leore.Objects.Obstacles
{
    public class Laser : RoomObject
    {
        public enum Orientation { Horizontal, Vertical }

        public bool Active { get; set; } = true;

        private bool canBeToggled;
        bool defaultValue;

        private Orientation orientation;
        private LaserObstacle laser;
        private float z1, z2, z3;
        private int T;

        private float alpha;
        
        public Laser(float x, float y, Room room, Orientation orientation, bool canBeToggled = false, bool defaultValue = true) : base(x, y, room)
        {
            this.orientation = orientation;
            this.canBeToggled = canBeToggled;
            this.defaultValue = defaultValue;

            Depth = Globals.LAYER_WATER + .0005f;

            DrawOffset = new Vector2(16);

            T = Globals.T;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (canBeToggled)
                Active = defaultValue ? !Room.SwitchState : Room.SwitchState;

            if (!Active)
            {
                alpha = Math.Max(alpha - .1f, 0);

                if (laser != null)
                {
                    laser.Destroy();
                    laser = null;
                }
            }

            if (Active)
            {
                alpha = Math.Min(alpha + .1f, 1);

                if (laser == null)
                    laser = new LaserObstacle(X, Y, Room, orientation);
            }
            
            z1 = (z1 + .2f) % T;
            z2 = (z2 + .3f) % T;
            z3 = (z3 + .6f) % T;
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);


            if (orientation == Orientation.Vertical)
            {
                sb.Draw(AssetManager.Laser[0], Position + new Vector2(16), new Rectangle(8, (int)z1, 16, 16), new Color(Color, .2f * Math.Max(alpha, .5f)), 0, DrawOffset, Scale, SpriteEffects.None, Depth + .0002f);
                sb.Draw(AssetManager.Laser[1], Position + new Vector2(16), new Rectangle(8, 16 - (int)z2, 16, 16), new Color(Color, .5f * Math.Max(alpha, .25f)), 0, DrawOffset, Scale, SpriteEffects.None, Depth);
                sb.Draw(AssetManager.Laser[2], Position + new Vector2(16), new Rectangle(8, (int)z3, 16, 16), new Color(Color, .7f * alpha), 0, DrawOffset, Scale, SpriteEffects.None, Depth + .0001f);
            }
            else if (orientation == Orientation.Horizontal)
            {
                var ang = (float)MathUtil.DegToRad(90);
                sb.Draw(AssetManager.Laser[0], Position + new Vector2(0, 16), new Rectangle(8, (int)z1, 16, 16), new Color(Color, .2f * Math.Max(alpha, .5f)), ang, DrawOffset, Scale, SpriteEffects.None, Depth + .0002f);
                sb.Draw(AssetManager.Laser[1], Position + new Vector2(0, 16), new Rectangle(8, 16 - (int)z2, 16, 16), new Color(Color, .5f * Math.Max(alpha, .25f)), ang, DrawOffset, Scale, SpriteEffects.None, Depth);
                sb.Draw(AssetManager.Laser[2], Position + new Vector2(0, 16), new Rectangle(8, (int)z3, 16, 16), new Color(Color, .7f * alpha), ang, DrawOffset, Scale, SpriteEffects.None, Depth + .0001f);
            }
        }
    }

    public class LaserObstacle : Obstacle
    {
        public LaserObstacle(float x, float y, Room room, Orientation orientation) : base(x, y, room)
        {            
            switch (orientation)
            {
                case Orientation.Vertical:
                    BoundingBox = new SPG.Util.RectF(6, 0, 4, 16);
                    break;
                case Orientation.Horizontal:
                    BoundingBox = new SPG.Util.RectF(0, 6, 16, 4);
                    break;
            }                        
        }        
    }
}
