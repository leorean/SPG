﻿using Microsoft.Xna.Framework;
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

    public struct LayerInfo
    {
        public string Name;
        public float Depth;

        public LayerInfo(string name, float depth)
        {
            Name = name;
            Depth = depth;
        }
    }

    /// <summary>
    /// The GameMap holds all data of the level. Interpreting layers, object types from tiles etc. should be done elsewhere.
    /// </summary>
    public class GameMap
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        
        private List<Grid<Tile>> layerData; //BG2, BG, WATER, FG

        private List<LayerInfo> layerInfo;

        public TileSet TileSet { get; set; }

        public GameMap(XmlDocument xml)
        {
            try
            {
                //var mapElement = xml.Element("map").Elements();
                var mapElement = xml.GetElementsByTagName("map")[0];
                
                Width = int.Parse(mapElement.Attributes["width"].Value);
                Height = int.Parse(mapElement.Attributes["height"].Value);

                layerData = new List<Grid<Tile>>();
                layerInfo = new List<LayerInfo>();

                var layers = mapElement.ToList().Where(x => x.Name == "layer");
                
                foreach (var layer in layers)
                {
                    var list = layer["data"].ToList();
                    layerData.Add(ParseTileDataFromElementArray(list));

                    var name = layer.Attributes["name"].Value;

                    layerInfo.Add(new LayerInfo(name, 0.0f));
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Unable to load map: {e.Message}");
                throw;
            }
        }

        public void Draw()
        {

            if (TileSet == null)
                throw new InvalidOperationException("The map cannot be drawn without a tileset!");

            for (var l = 0; l < layerData.Count; l++)
            {
                for (var i = 0; i < Width; i++)
                {
                    for (var j = 0; j < Height; j++)
                    {
                        var tile = layerData[l].Get(i, j);

                        if (tile == null)
                            continue;

                        var texture = TileSet.ElementAt(tile.ID);
                        
                        GameManager.Game.SpriteBatch.Draw(texture, new Vector2(i * Globals.TILE, j * Globals.TILE), null, Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, layerInfo[l].Depth);                        
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

                    TileType type = TileType.Block;

                    // todo: refactor
                    switch (id)
                    {
                        case 0:
                        case 7:
                            type = TileType.Platform;
                            break;
                    }

                    tile = new Tile(id, type);
                }

                data.Set(i, tile);
            }

            return data;
        }
    }
}