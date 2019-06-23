using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leore.Main;
using Leore.Objects.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Objects;

namespace Leore.Objects.Level.Blocks
{
    public class FallingPlatformSpawner : RoomObject
    {
        private int timer = 3 * 60;

        private Vector2 originalPosition;

        private float alpha = 1;
        private float dst;

        public FallingPlatformSpawner(float x, float y, Room room) : base(x, y, room)
        {
            originalPosition = Position;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            timer = Math.Max(timer - 1, 0);

            if (timer == 0)
            {
                //new SingularEffect(Center.X, Center.Y, 7);
                new SingularEffect(Center.X, Center.Y, 13);
                new FallingPlatform(originalPosition.X, originalPosition.Y, Room) { Texture = this.Texture };
                Destroy();
            }
            
            YVel = Math.Min(YVel + .15f, 3);
            alpha = Math.Max(alpha - .015f, 0);

            dst += YVel;

            DrawOffset = new Vector2(0, -dst);
            
            Color = new Color(Color, alpha);
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);
        }
    }

    public class FallingPlatform : Platform
    {

        private int timer;
        private int maxTimer = 60;
        private double t;

        private bool loose;

        public FallingPlatform(float x, float y, Room room) : base(x, y, room)
        {
            Visible = true;
            BoundingBox = new SPG.Util.RectF(0, 0, 16, 1);            
            Depth = Globals.LAYER_FG;

            timer = maxTimer;

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (loose)
            {
                timer = Math.Max(timer - 1, 0);

                t = (t + 1) % (2 * Math.PI);
                DrawOffset = new Vector2((float)Math.Sin(t) * .5f, 0);
            }
            
            if (this.CollisionBounds(GameManager.Current.Player, X, Y - 4) && GameManager.Current.Player.OnGround)
            {
                loose = true;
            }

            if (timer == 0)
            {
                new FallingPlatformSpawner(X, Y, Room) { Texture = this.Texture, Depth = this.Depth, BoundingBox = this.BoundingBox };
                Destroy();
            }
        }
    }
}
