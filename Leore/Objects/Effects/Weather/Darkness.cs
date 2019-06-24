using Leore.Main;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Objects;
using SPG.View;
using System;
using System.Collections.Generic;
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

        private SpriteBatch sb;

        private DepthStencilState s1, s2;

        private Darkness(GraphicsDevice gd)
        {
            sb = new SpriteBatch(gd);
            
            s1 = new DepthStencilState
            {
                StencilEnable = true,
                StencilFunction = CompareFunction.Always,
                StencilPass = StencilOperation.Replace,
                ReferenceStencil = 1,
                DepthBufferEnable = true,                
            };

            s2 = new DepthStencilState
            {
                StencilEnable = true,
                StencilFunction = CompareFunction.LessEqual,
                StencilPass = StencilOperation.Keep,
                ReferenceStencil = 1,
                DepthBufferEnable = true,
            };
        }
        
        public void Draw(GameTime gameTime)
        {
            var m = Matrix.CreateOrthographicOffCenter(0,
                //sb.GraphicsDevice.PresentationParameters.BackBufferWidth,
                //sb.GraphicsDevice.PresentationParameters.BackBufferHeight,
                camera.ViewWidth,
                camera.ViewHeight,
                0, 0, 1
            );

            var a = new AlphaTestEffect(sb.GraphicsDevice)
            {
                Projection = m//, Alpha = .5f
            };

            Vector2 viewPos = new Vector2(camera.ViewX, camera.ViewY);

            sb.GraphicsDevice.Clear(ClearOptions.Stencil, Color.Transparent, 0, 0);

            //darkness = new RenderTarget2D(GraphicsDevice, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

            //blend.AlphaSourceBlend = Blend.Zero;
            //blend.AlphaDestinationBlend = Blend.InverseSourceColor;
            //blend.ColorSourceBlend = Blend.Zero;
            //blend.ColorDestinationBlend = Blend.InverseSourceColor;

            // mask
            sb.Begin(SpriteSortMode.BackToFront, null, SamplerState.PointClamp, s1, null, a);//, camera.GetViewTransformationMatrix());
            //sb.Draw(AssetManager.DarknessMask, Vector2.Zero, new Color(Color.White, .15f));
            sb.Draw(AssetManager.DarknessMask, player.Position, null, Color.White, 0, new Vector2(32), Vector2.One, SpriteEffects.None, 0);
            sb.End();

            // background
            sb.Begin(SpriteSortMode.BackToFront, null, SamplerState.PointClamp, s2, null, a);//, camera.GetViewTransformationMatrix());
            //sb.Draw(AssetManager.Darkness, Vector2.Zero, Color.White);
            sb.Draw(AssetManager.Darkness, Vector2.Zero, null, Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
            sb.End();            
        }

        public static void Create(GraphicsDevice gd)
        {
            if (Current == null)
                Current = new Darkness(gd);
        }
    }
}
