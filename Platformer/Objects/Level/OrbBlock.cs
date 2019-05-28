using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Platformer.Objects.Effects;
using Platformer.Objects.Enemies;
using Platformer.Objects.Main;
using SPG.Objects;
using SPG.Util;

namespace Platformer.Objects.Level
{
    public class OrbBlock : Solid
    {
        private float alpha = 1;
        private bool active;

        public OrbBlock(float x, float y, Room room) : base(x, y, room)
        {
            Texture = GameManager.Current.Map.TileSet[717];
            Visible = false;
        }

        public override void BeginUpdate(GameTime gameTime)
        {
            base.BeginUpdate(gameTime);

            if (this.IsOutsideCurrentRoom())
                Destroy();
            
            if (GameManager.Current.Player.Orb != null)
            {
                if (MathUtil.Euclidean(Center, GameManager.Current.Player.Orb.Center) < 2 * Globals.TILE)
                {
                    if (GameManager.Current.Player.Orb.State == Main.Orbs.OrbState.ATTACK)
                        active = true;
                }
            }

            if (active)
            {
                alpha = Math.Max(alpha - .02f, 0);
                if (alpha == 0)
                {
                    Destroy();
                }
            }

            Color = new Color(Color, alpha);
            Visible = true;
        }
    }
}
