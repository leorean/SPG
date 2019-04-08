using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SPG.Map
{    
    public class TileSet : List<Texture2D>
    {
        public static TileSet Load(string fileName)
        {
            try
            {
                var texture = GameManager.Game.Content.Load<Texture2D>(fileName);

                var tileSet = new TileSet();

                for (var j = 0; j < texture.Height / Globals.TILE; j++)
                {
                    for (var i = 0; i < texture.Width / Globals.TILE; i++)
                    {
                        Rectangle sourceRectangle = new Rectangle(i * Globals.TILE, j * Globals.TILE, Globals.TILE, Globals.TILE);
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