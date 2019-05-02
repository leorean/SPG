using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Util;
using SPG.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace SPG.Map
{

    public static class MapExtensions
    {
        public static string Find(this XmlAttributeCollection attribs, string name)
        {
            foreach(XmlAttribute a in attribs)
            {
                if(a.Name == name)
                {
                    return a.Value;
                }
            }
            return null;
        }

        public static List<XmlNode> ToList(this XmlNode node)
        {
            var nodeList = new List<XmlNode>();

            foreach (XmlNode n in node.ChildNodes)
            {
                nodeList.Add(n);
            }

            return nodeList;
        }

        public static List<Dictionary<string, object>> FindDataByTypeName(this List<Dictionary<string, object>> list, string name)
        {
            return list.Where(x => x.Values.Contains(name)).ToList();
        }

        public static Dictionary<string, object> FindFirstDataByTypeName(this List<Dictionary<string, object>> list, string name)
        {
            return list.Where(x => x.Values.Contains(name)).First();
        }
    }

    /// <summary>
    /// The GameMap holds all data of a level. Interpreting layers, object types from tiles etc. should be done elsewhere.
    /// </summary>
    public class GameMap
    {
        /// <summary>
        /// Gets the width in tile units.
        /// </summary>
        public int Width { get; private set; }
        /// <summary>
        /// Gets the height in tile units.
        /// </summary>
        public int Height { get; private set; }

        public List<Grid<Tile>> LayerData { get; set; } //BG2, BG, WATER, FG

        public Dictionary<string, float> LayerDepth { get; set; }

        public TextureSet TileSet { get; set; }

        /// <summary>
        /// Contains a list of each object including their properties (name, x, y, ...)
        /// </summary>
        public List<Dictionary<string, object>> ObjectData { get; set; }

        /// <summary>
        /// Creates a new map from an XML document.
        /// </summary>
        /// <param name="xml"></param>
        public GameMap(XmlDocument xml)
        {
            try
            {
                var mapElement = xml.GetElementsByTagName("map")[0];
                
                Width = int.Parse(mapElement.Attributes["width"].Value);
                Height = int.Parse(mapElement.Attributes["height"].Value);

                LayerData = new List<Grid<Tile>>();
                LayerDepth = new Dictionary<string, float>();

                var layers = mapElement.ToList().Where(x => x.Name == "layer");
                
                foreach (var layer in layers)
                {
                    var list = layer["data"].ToList();
                    LayerData.Add(ParseTileDataFromElementArray(list));

                    var name = layer.Attributes["name"].Value;

                    LayerDepth.Add(name, 0.0f);
                }
                
                ObjectData = new List<Dictionary<string, object>>();

                foreach (var objectNodes in mapElement.ToList().Where(x => x.Name == "objectgroup")) // multiple object layers
                {
                    foreach (XmlNode objectNode in objectNodes.ChildNodes)
                    {
                        var objProperties = new Dictionary<string, object>();

                        var name = objectNode.Attributes["type"].Value;
                        int x = int.Parse(objectNode.Attributes["x"].Value);
                        int y = int.Parse(objectNode.Attributes["y"].Value);
                        int width = int.Parse(objectNode.Attributes["width"].Value);
                        int height = int.Parse(objectNode.Attributes["height"].Value);

                        objProperties.Add("name", name); // is actually the type name!
                        objProperties.Add("x", x);
                        objProperties.Add("y", y);
                        objProperties.Add("width", width);
                        objProperties.Add("height", height);

                        if (objectNode.HasChildNodes)
                        {
                            foreach (XmlNode node in objectNode.ChildNodes)
                            {
                                foreach (XmlNode prop in node.ChildNodes)
                                {
                                    var propName = prop.Attributes["name"].Value;
                                    var propValueString = prop.Attributes["value"].Value;

                                    var propValueType = prop.Attributes["type"].Value;

                                    object propValue;

                                    switch (propValueType)
                                    {
                                        case "int":
                                            propValue = int.Parse(propValueString);
                                            break;
                                        case "float":
                                            float res;
                                            if (float.TryParse(propValueString, out res))
                                                propValue = res;
                                            else
                                                propValue = float.Parse(propValueString.Replace('.', ','));
                                            break;
                                        case "bool":
                                            propValue = bool.Parse(propValueString);
                                            break;
                                        default:
                                            propValue = propValueString;
                                            break;
                                    }

                                    objProperties.Add(propName, propValue);
                                }
                            }
                        }
                        ObjectData.Add(objProperties);
                    }
                }


            }
            catch (Exception e)
            {
                Debug.WriteLine($"Unable to load map: {e.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Draws parts of the map that are visible to the Game camera (within the camera view bounds).
        /// </summary>
        /// <param name="gameTime"></param>
        public void Draw(SpriteBatch sb, GameTime gameTime, Camera cam)
        {

            if (TileSet == null)
                throw new InvalidOperationException("The map cannot be drawn without a tileset!");
            
            if (cam == null)
                return;

            int minX = (int)Math.Max(cam.Position.X - cam.ViewWidth * .5f, 0f);
            int maxX = (int)Math.Min(cam.Position.X + cam.ViewWidth * .5f + Globals.TILE, Width * Globals.TILE);
            int minY = (int)Math.Max(cam.Position.Y - cam.ViewHeight * .5f, 0f);
            int maxY = (int)Math.Min(cam.Position.Y + cam.ViewHeight * .5f + Globals.TILE, Height * Globals.TILE);

            minX = MathUtil.Div(minX, Globals.TILE);
            minY = MathUtil.Div(minY, Globals.TILE);
            maxX = MathUtil.Div(maxX, Globals.TILE);
            maxY = MathUtil.Div(maxY, Globals.TILE);
            
            for (var l = 0; l < LayerData.Count; l++)
            {
                for (int i = minX; i < maxX; i++)
                {
                    for (int j = minY; j < maxY; j++)
                    {
                        var tile = LayerData[l].Get(i, j);

                        if (tile == null)
                            continue;

                        // approach to draw each texture extra:

                        //Texture2D texture = null;
                        //texture = TileSet.ElementAt(tile.ID);
                        //if (texture != null)
                        //    GameManager.Game.SpriteBatch.Draw(texture, new Vector2(i * Globals.TILE, j * Globals.TILE), null, Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, LayerDepth.ElementAt(l).Value);

                        // approach to draw parts of the original set instead of each texture extra:

                        if (tile.TileOptions == null || tile.TileOptions.Visible)
                        {

                            var tid = tile.ID;

                            var tix = (tid * Globals.TILE) % TileSet.Width;
                            var tiy = MathUtil.Div(tid * Globals.TILE, TileSet.Width) * Globals.TILE;

                            var partRect = new Rectangle(tix, tiy, Globals.TILE, Globals.TILE);

                            sb.Draw(TileSet.OriginalTexture, new Vector2(i * Globals.TILE, j * Globals.TILE), partRect, Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, LayerDepth.ElementAt(l).Value);
                        }
                    }
                }
            }
        }

        private Grid<Tile> ParseTileDataFromElementArray(List<XmlNode> nodeList)
        {
            Grid<Tile> data = new Grid<Tile>(Width, Height);

            // -1 -> no tile found at that position
            // values in XML level have a wrong index (in tileset: 0, in map: 1)

            for (var i = 0; i < nodeList.Count; i++)
            {
                Tile tile = null;
                var attribs = nodeList[i].Attributes;
                var value = attribs.Find("gid");
                if (value != null)
                {
                    var id = int.Parse(value) - 1;
                    tile = new Tile(id);
                }

                data.Set(i, tile);
            }

            return data;
        }
    }
}