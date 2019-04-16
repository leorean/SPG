using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace SPG.Map
{

    public static class XmlExtensions
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
    }

    /// <summary>
    /// The GameMap holds all data of a level. Interpreting layers, object types from tiles etc. should be done elsewhere.
    /// </summary>
    public class GameMap
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public List<Grid<Tile>> LayerData { get; set; } //BG2, BG, WATER, FG

        public Dictionary<string, float> LayerDepth { get; set; }

        public TextureSet TileSet { get; set; }

        /// <summary>
        /// Contains a list of each object including their properties (name, x, y, ...)
        /// </summary>
        public List<Dictionary<string, object>> Objects { get; set; }

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
                
                Objects = new List<Dictionary<string, object>>();

                var objectNodes = mapElement.ToList().Where(x => x.Name == "objectgroup").First();

                foreach (XmlNode objectNode in objectNodes.ChildNodes)
                {
                    var objProperties = new Dictionary<string, object>();

                    var name = objectNode.Attributes["type"].Value;
                    var x = objectNode.Attributes["x"].Value;
                    var y = objectNode.Attributes["y"].Value;

                    objProperties.Add("name", name); // is actually the type name!
                    objProperties.Add("x", x);
                    objProperties.Add("y", y);

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
                    Objects.Add(objProperties);
                }


            }
            catch (Exception e)
            {
                Debug.WriteLine($"Unable to load map: {e.Message}");
                throw;
            }
        }
        
        public void Draw(GameTime gameTime)
        {

            if (TileSet == null)
                throw new InvalidOperationException("The map cannot be drawn without a tileset!");

            for (var l = 0; l < LayerData.Count; l++)
            {
                for (var i = 0; i < Width; i++)
                {
                    for (var j = 0; j < Height; j++)
                    {
                        var tile = LayerData[l].Get(i, j);

                        if (tile == null)
                            continue;

                        var texture = TileSet.ElementAt(tile.ID);
                        
                        GameManager.Game.SpriteBatch.Draw(texture, new Vector2(i * Globals.TILE, j * Globals.TILE), null, Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, LayerDepth.ElementAt(l).Value);                        
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