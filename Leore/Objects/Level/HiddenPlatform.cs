using Leore.Main;
using Microsoft.Xna.Framework;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leore.Objects.Level
{
    public class HiddenPlatform : Platform
    {
        private float alpha;

        private Player player => GameManager.Current.Player;

        public HiddenPlatform(float x, float y, Room room) : base(x, y, room)
        {
            Depth = Globals.LAYER_FG;
            Visible = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            alpha = 1 - (float)Math.Min(Math.Max(MathUtil.Euclidean(player.Center, Center) / (float)(3 * Globals.T), 0), 1);

            //if (MathUtil.Euclidean(player.Center, Center) < 2 * Globals.T)
            //    alpha = Math.Min(alpha + .03f, 1);
            //else
            //    alpha = Math.Max(alpha - .03f, 0);
            
            Color = new Color(Color, alpha);            
        }
    }
}
