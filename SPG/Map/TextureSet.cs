using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using SPG.Util;

namespace SPG.Map
{    
    /// <summary>
    /// A texture set is a list of equally big textures
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
            int partWidth = (tileWidth != null) ? (int)tileWidth : Globals.TILE;
            int partHeight = (tileHeight != null) ? (int)tileHeight: Globals.TILE;
            
            Texture2D original = GameManager.Game.Content.Load<Texture2D>(fileName);
            
            int xCount = original.Width / partWidth;
            int yCount = original.Height / partHeight;

            Texture2D[] r = new Texture2D[xCount * yCount];
            int dataPerPart = partWidth * partHeight;
            
            Color[] originalData = new Color[original.Width * original.Height];
            original.GetData(originalData);

            int index = 0;
            for (int y = 0; y < yCount * partHeight; y += partHeight)
                for (int x = 0; x < xCount * partWidth; x += partWidth)
                {
                    
                    Texture2D part = new Texture2D(original.GraphicsDevice, partWidth, partHeight);
                    Color[] partData = new Color[dataPerPart];

                    for (int py = 0; py < partHeight; py++)
                        for (int px = 0; px < partWidth; px++)
                        {
                            int partIndex = px + py * partWidth;
                            if (y + py >= original.Height || x + px >= original.Width)
                                partData[partIndex] = Color.Transparent;
                            else
                                partData[partIndex] = originalData[(x + px) + (y + py) * original.Width];
                        }
                    part.SetData(partData);                    
                    r[index++] = part;
                }

            TextureSet result = new TextureSet();
            foreach(var element in r.Cast<Texture2D>())
                result.Add(element);
            
            return result;
        }
    }
}