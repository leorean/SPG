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

        public float Alpha { get; set; }
        private float alpha;
        private float targetAlpha = 1;

        private int weather;

        private Darkness()
        {
            blend = new BlendState();

            blend.ColorBlendFunction = BlendFunction.Add;
            blend.ColorSourceBlend = Blend.Zero;
            blend.ColorDestinationBlend = Blend.InverseSourceAlpha;
            blend.AlphaBlendFunction = BlendFunction.Add;
            blend.AlphaSourceBlend = Blend.Zero;
            blend.AlphaDestinationBlend = Blend.InverseSourceAlpha;
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
            var lightSources = ObjectManager.FindAll<LightSource>().Where(o => !o.Parent.IsOutsideCurrentRoom()).ToList();

            if (RoomCamera.Current.CurrentRoom != null)
                weather = RoomCamera.Current.CurrentRoom.Weather;

            if (weather != 1)
            {
                Alpha = Math.Max(Alpha - .1f, 0);
                if (Alpha == 0)
                {
                    spriteBatch.End();
                    return;
                }
            } else
            {
                Alpha = Math.Min(Alpha + .1f, 1);
            }
            
            if (lightSources.Count >= 0)
            {

                int activeSources = -1;
                foreach (var source in lightSources)
                {
                    if (source.Active)
                        activeSources++;
                }

                alpha = 1 - Math.Max(activeSources, 0) / (float)lightSources.Count;
                
                if (activeSources == 0)
                    alpha = 1;

                if (lightSources.Count == 0)
                    alpha = 0;

                foreach (var source in lightSources)
                {
                    if (!source.Active)
                        continue;

                    var pos = new Vector2(source.Parent.Center.X, source.Parent.Center.Y)
                         - new Vector2(RoomCamera.Current.ViewX, RoomCamera.Current.ViewY);

                    spriteBatch.Draw(AssetManager.DarknessMask, pos, null, Color.White, 0, new Vector2(32), new Vector2(.9f + .05f * (float)Math.Sin(t)), SpriteEffects.None, Globals.LAYER_UI - .001f);

                    //spriteBatch.Draw(AssetManager.DarknessMask, new Vector2(source.Parent.Center.X, source.Parent.Center.Y)
                    //     - new Vector2(32) - new Vector2(RoomCamera.Current.ViewX, RoomCamera.Current.ViewY), Color.White, );
                }                
            }

            spriteBatch.End();
        }

        public void Draw(SpriteBatch sb, GameTime gameTime)
        {
            targetAlpha += (alpha - targetAlpha) / 60f;

            sb.Draw(darkness, new Vector2(RoomCamera.Current.ViewX, RoomCamera.Current.ViewY), null, new Color(Color.White, Alpha * targetAlpha), 0, new Vector2(0), Vector2.One, SpriteEffects.None, Globals.LAYER_UI - .001f);            
        }
    }
}
