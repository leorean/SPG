using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace SPG.Util
{
    public static class Draw
    {
        [Obsolete("TODO: find a better implementation to draw a primitive rectangle!")]
        public static void DrawRectangle(Rectangle rect, Color color, float depth)
        {
            Texture2D texRect = new Texture2D(GameManager.Game.GraphicsDevice, rect.Width, rect.Height, false, SurfaceFormat.Color);

            var data = new Color[rect.Width * rect.Height];

            int index = 0;
            for (var i = 0; i < rect.Width; i++)
            {
                for (var j = 0; j < rect.Height; j++)
                {
                    data[index] = Color.Transparent;

                    if (i == 0 || j == 0 || i == rect.Width - 1 || j == rect.Height - 1)
                        data[index] = color;

                    index++;
                }
            }

            texRect.SetData(data);
            GameManager.Game.SpriteBatch.Draw(texRect, rect, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, depth);            
        }
    }
}