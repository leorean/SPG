using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platformer.Objects.Effects;
using SPG.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Main.Orbs
{
    class SpellFont : FollowFont
    {

        int time;

        public enum SpellChange
        {
            LVUP, LVDOWN
        }

        private SpellChange spellChange;

        private Color color;
        private Color colorUp = new Color(255, 203, 0);
        private Color colorDown = new Color(168, 0, 0);

        public SpellFont(GameObject target, float offx, float offy, SpellChange spellChange) : base(target.X + offx, target.Y + offy, "")
        {
            this.spellChange = spellChange;
            Target = target;

            if (spellChange == SpellChange.LVUP)
                text = "Spell Up!";
            if (spellChange == SpellChange.LVDOWN)
                text = "Spell Down!";
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            time++;            
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            Color color = Color.White;

            if (spellChange == SpellChange.LVUP) color = (time % 6 < 3) ? colorUp : Color.White;
            if (spellChange == SpellChange.LVDOWN) color = (time % 6 < 3) ? colorDown : Color.Red;

            font.Color = color;
            font.Draw(sb, X, Y, text, scale: 1);
        }
    }
}
