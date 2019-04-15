using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SPG.Map
{    
    /// <summary>
    /// A texture set is a set of equally big textures
    /// </summary>
    public class TextureSet : List<Texture2D>
    {
        private TextureSet()
        {

        }

        public static TextureSet FromTexture(Texture2D texture)
        {
            var tileSet = new TextureSet();

            tileSet.Add(texture);

            return tileSet;
        }

        public static TextureSet Load(string fileName, int? tileWidth = null, int? tileHeight = null)
        {

            if (tileWidth == null) tileWidth = Globals.TILE;
            if (tileHeight == null) tileHeight = Globals.TILE;

            try
            {
                var texture = GameManager.Game.Content.Load<Texture2D>(fileName);

                var tileSet = new TextureSet();

                for (var j = 0; j < texture.Height / tileHeight; j++)
                {
                    for (var i = 0; i < texture.Width / tileWidth; i++)
                    {
                        Rectangle sourceRectangle = new Rectangle(i * (int)tileWidth, j * (int)tileHeight, (int)tileWidth, (int)tileHeight);
                        var newTexture = new Texture2D(GameManager.Game.GraphicsDeviceManager.GraphicsDevice, sourceRectangle.Width, sourceRectangle.Height);
                        Color[] data = new Color[sourceRectangle.Width * sourceRectangle.Height];

                        texture.GetData(0, sourceRectangle, data, 0, data.Length);
                        newTexture.SetData(data);

                        tileSet.Add(newTexture);
                    }
                }

                return tileSet;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Unable to load tileset: {e.Message}");
                throw;
            }
        }
    }    
}