using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Objects;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Main
{
    public class Orb : GameObject
    {
        private Player player { get => Parent as Player; }

        public Orb(Player player) : base(player.X, player.Y)
        {
            Texture = AssetManager.Orb;
            Scale = new Vector2(.5f);

            BoundingBox = new RectF(-4, -4, 8, 8);
            DrawOffset = new Vector2(16, 16);
            Depth = player.Depth + .0001f;

            Parent = player;

            //DebugEnabled = true;
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);
        }
    }
}
