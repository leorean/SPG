using Leore.Main;
using Leore.Objects.Level;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Objects;
using SPG.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leore.Objects.Effects.Weather
{
    public class Darkness
    {
        private RoomCamera camera => RoomCamera.Current;
        private Room room => RoomCamera.Current.CurrentRoom;
        private Player player => GameManager.Current.Player;

        public static Darkness Current { get; private set; }
        
        private RenderTarget2D darkness;
        private BlendState blend;

        private double t;

        public float Alpha { get; private set; }

        private float globalAlpha;
        private float alpha;
        private float targetAlpha;

        private bool isRoomDark;
        private float x, y;

        bool enabled = true;

        private Darkness()
        {
            blend = new BlendState
            {
                ColorSourceBlend = Blend.Zero, // multiplier of the source color
                ColorBlendFunction = BlendFunction.Add, // function to combine colors
                ColorDestinationBlend = Blend.SourceAlpha, // multiplier of the destination color
                AlphaBlendFunction = BlendFunction.Add, // function to combine alpha
                AlphaSourceBlend = Blend.Zero, // multiplier of the source alpha
                AlphaDestinationBlend = Blend.InverseSourceAlpha, // multiplier of the destination alpha
            };
        }

        public static void Create()
        {
            if (Current == null)
                Current = new Darkness();
        }

        public void PrepareDraw(SpriteBatch spriteBatch)
        {
            // update

            t = (t + .02f) % (2 * Math.PI);

            // draw

            if (darkness == null)
                darkness = new RenderTarget2D(spriteBatch.GraphicsDevice, RoomCamera.Current.ViewWidth, RoomCamera.Current.ViewHeight);

            spriteBatch.GraphicsDevice.SetRenderTarget(darkness);
            spriteBatch.GraphicsDevice.Clear(Color.Black);
            
            spriteBatch.Begin(SpriteSortMode.Immediate, blend, SamplerState.PointClamp, null, null, null, null);

            ObjectManager.Enable<LightSource>();
            var lightSources = ObjectManager.FindAll<LightSource>().Where(o => !o.Parent.IsOutsideCurrentRoom(64)).ToList();

            if (RoomCamera.Current.CurrentRoom != null)
            {
                x = RoomCamera.Current.ViewX;
                y = RoomCamera.Current.ViewY;
                isRoomDark = RoomCamera.Current.CurrentRoom.IsDark;
            }

            if (!isRoomDark || !enabled)
            {
                globalAlpha = Math.Max(globalAlpha - .025f, 0);
                if (globalAlpha == 0)
                {
                    spriteBatch.End();
                    return;
                }
            } else
            {
                globalAlpha = Math.Min(globalAlpha + .03f, 1);
            }
            
            float a = 0;
            if (lightSources.Count >= 0)
            {
                // calculate brightness
                foreach (var source in lightSources)
                {
                    if (source.Active) {
                        if (source.State == LightSource.LightState.Bright)
                            a += .15f;
                        if (source.State == LightSource.LightState.FullRoom)
                        {
                            a = 1;
                            break;
                        }
                    }
                    if (source.State == LightSource.LightState.Ambient)
                    {
                        a += .15f;
                    }
                }
                alpha = Math.Min(alpha, 1);                

                alpha = 1 - a;
                
                if (lightSources.Count == 0)
                    alpha = 0;

                Alpha = alpha;
                
                // draw masks
                foreach (var source in lightSources)
                {
                    if (!source.Active)
                        continue;

                    var pos = new Vector2(source.Parent.Center.X, source.Parent.Center.Y)
                         - new Vector2(x, y);

                    spriteBatch.Draw(AssetManager.DarknessMask, pos, null, Color.White, 0, new Vector2(64), new Vector2(.9f + .05f * (float)Math.Sin(t)) * .5f * source.Scale.X, SpriteEffects.None, Globals.LAYER_UI - .001f);                    
                }                
            }

            spriteBatch.End();
        }

        public void Draw(SpriteBatch sb, GameTime gameTime)
        {
            targetAlpha += (alpha - targetAlpha) / 60f;

            sb.Draw(darkness, new Vector2(x, y), null, new Color(Color.White, globalAlpha * targetAlpha), 0, new Vector2(0), Vector2.One, SpriteEffects.None, Globals.LAYER_UI - .001f);            
        }

        public void Disable()
        {
            enabled = false;
        }

        public void Enable()
        {
            enabled = true;
        }
    }
}
