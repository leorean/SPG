using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platformer.Objects.Main;
using SPG.Draw;
using SPG.Objects;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Effects
{
    public class FallingFont : GameObject
    {
        Font font;

        float alpha = 2;
        
        public string Text { get; set; }

        public Color currentColor = Color.White;
        public Color targetColor;

        float r, g, b;

        public FallingFont(float x, float y, string text, Color color, Color? startColor = null) : base(x, y)
        {
            XVel = -1 + (float)RND.Next * 2;
            YVel = -1.5f;

            Gravity = .05f;

            font = AssetManager.DamageFont.Copy();
            font.Halign = Font.HorizontalAlignment.Center;
            font.Valign = Font.VerticalAlignment.Center;

            Text = text;
            targetColor = color;

            currentColor = startColor == null ? Color.White : (Color)startColor;
            r = currentColor.R;
            g = currentColor.G;
            b = currentColor.B;

            Scale = new Vector2(.5f);

            Depth = Globals.LAYER_FONT;            
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            r += ((targetColor.R - r) / 24f);
            g += ((targetColor.G - g) / 24f);
            b += ((targetColor.B - b) / 24f);
            
            currentColor = new Color((int)r, (int)g, (int)b);

            alpha = Math.Max(alpha - .04f, 0);
            Color = new Color(currentColor, alpha);
            font.Color = Color;
            
            Scale = new Vector2(Math.Min(Scale.X + .02f, 1));

            XVel *= .95f;
            YVel += Gravity;
            
            Move(XVel, YVel);
            
            if (this.IsOutsideCurrentRoom() || alpha == 0)
                Destroy();
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);

            font.Draw(sb, X, Y, Text, depth: Depth, scale:Scale.X);
        }
    }
}
