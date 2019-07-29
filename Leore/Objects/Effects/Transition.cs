﻿using Microsoft.Xna.Framework;
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
        
        public delegate void TransitionEnd(int type, Direction direction);
        public TransitionEnd OnTransitionEnd;

        //private Texture2D dark = AssetManager.Transition[0];
        private double alpha = 0f;
        private Fade fade;
        private Color color = new Color(Color.White, 0);
        private double vel = .03;

        private int type;
        private Direction direction;

        public Transition(int type = 0, Direction direction = Direction.NONE)
        {
            this.type = type;
            this.direction = direction;
        }

        public void FadeIn()
        {
            alpha = 0;
            fade = Fade.IN;
        }
        public void FadeOut()
        {
            alpha = type == 1 ? 3.5 : 1.5;
            fade = Fade.OUT;
        }

        public void Update(GameTime gameTime)
        {
            if (fade == Fade.IN) {
                alpha = Math.Min(alpha + vel, 1);
                if (alpha == 1)
                {
                    fade = Fade.NONE;
                    OnTransitionEnd?.Invoke(type, direction);
                }
            }

            if (fade == Fade.OUT)
            {
                alpha = Math.Max(alpha - vel, 0);
                if (alpha == 0)
                {
                    fade = Fade.NONE;
                    OnTransitionEnd?.Invoke(type, direction);
                }
            }

            color = new Color(color, (float)alpha);
        }

        public void Draw(SpriteBatch sb, GameTime gameTime)
        {
            sb.Draw(AssetManager.Transition[type], new Vector2(RoomCamera.Current.ViewX, RoomCamera.Current.ViewY), null, color, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, Globals.LAYER_UI + .001f);
        }
    }    
}
