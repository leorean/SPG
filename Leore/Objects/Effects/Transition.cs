using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Leore.Main;
using System;

namespace Leore.Objects.Effects
{
    public class Transition
    {
        private enum Fade
        {
            NONE = 0, IN = 1, OUT = -1
        }

        public enum TransitionType
        {
            DARK = 0,
            LIGHT = 1,
            LONG_LIGHT = 2
        }
        
        public delegate void TransitionEnd(TransitionType type, Direction direction, string levelName);
        public TransitionEnd OnTransitionEnd;

        //private Texture2D dark = AssetManager.Transition[0];
        private double alpha = 0f;
        private Fade fade;
        private Color color = new Color(Color.White, 0);
        private double vel = .03;

        private TransitionType type;
        private int spriteIndex;
        private Direction direction;
        private string levelName;

        public Transition(TransitionType type = TransitionType.DARK, Direction direction = Direction.NONE, string levelName = null)
        {
            this.type = type;
            this.direction = direction;
            this.levelName = levelName;

            switch (type)
            {
                case TransitionType.DARK:
                    spriteIndex = 0;
                    break;
                case TransitionType.LIGHT:
                    spriteIndex = 1;
                    break;
                case TransitionType.LONG_LIGHT:
                    spriteIndex = 1;
                    break;
                default:
                    break;
            }
        }

        public void FadeIn()
        {
            alpha = 0;
            fade = Fade.IN;
        }
        public void FadeOut()
        {
            alpha = type == TransitionType.LONG_LIGHT ? 3.5 : 1.5;
            fade = Fade.OUT;
        }

        public void Update(GameTime gameTime)
        {
            if (fade == Fade.IN) {
                alpha = Math.Min(alpha + vel, 1);
                if (alpha == 1)
                {
                    fade = Fade.NONE;
                    OnTransitionEnd?.Invoke(type, direction, levelName);
                }
            }

            if (fade == Fade.OUT)
            {
                alpha = Math.Max(alpha - vel, 0);
                if (alpha == 0)
                {
                    fade = Fade.NONE;
                    OnTransitionEnd?.Invoke(type, direction, levelName);
                }
            }

            color = new Color(color, (float)alpha);
        }

        public void Draw(SpriteBatch sb, GameTime gameTime)
        {
            sb.Draw(AssetManager.Transition[spriteIndex], new Vector2(RoomCamera.Current.ViewX, RoomCamera.Current.ViewY), null, color, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, Globals.LAYER_UI + .001f);
        }
    }    
}
