using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platformer.Main;
using Platformer.Objects.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Effects
{
    public class Transition
    {
        private enum Fade
        {
            NONE = 0, IN = 1, OUT = -1
        }
        
        public delegate void TransitionEnd();
        public TransitionEnd OnTransitionEnd;

        private Texture2D dark = AssetManager.Darkness;
        private double alpha = 0f;
        private Fade fade;
        private Color color = new Color(Color.White, 0);
        private double vel = .03;

        public Transition()
        {

        }

        public void FadeIn()
        {
            alpha = 0;
            fade = Fade.IN;
        }
        public void FadeOut()
        {
            alpha = 1.5;
            fade = Fade.OUT;
        }

        public void Update(GameTime gameTime)
        {
            if (fade == Fade.IN) {
                alpha = Math.Min(alpha + vel, 1);
                if (alpha == 1)
                {
                    fade = Fade.NONE;
                    OnTransitionEnd?.Invoke();
                }
            }

            if (fade == Fade.OUT)
            {
                alpha = Math.Max(alpha - vel, 0);
                if (alpha == 0)
                {
                    fade = Fade.NONE;
                    OnTransitionEnd?.Invoke();
                }
            }

            color = new Color(color, (float)alpha);
        }

        public void Draw(SpriteBatch sb, GameTime gameTime)
        {
            sb.Draw(dark, new Vector2(RoomCamera.Current.ViewX, RoomCamera.Current.ViewY), null, color, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, Globals.LAYER_UI - .001f);
        }
    }    
}
