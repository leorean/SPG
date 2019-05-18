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
        public enum OrbState
        {
            FOLLOW,
            IDLE,
            ATTACK
        }

        public OrbState State { get; set; }

        private Player player { get => Parent as Player; }

        private Vector2 targetPosition;

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

            switch (State)
            {
                case OrbState.FOLLOW:

                    targetPosition = player.Position + new Vector2(-Math.Sign((int)player.Direction) * 8, -6);
                    MoveTowards(targetPosition, 12);
                    break;
                case OrbState.IDLE:
                    break;
                case OrbState.ATTACK:
                    break;                
            }            
        }
    }
}
