using Platformer.Main;
using Platformer.Objects.Items;
using SPG.Objects;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Main
{
    public static class RoomObjectLoader
    {
        public static void CreateRoomObjects(List<Dictionary<string, object>> objectData, Room room)
        {
            var camera = RoomCamera.Current;

            try
            {
                foreach (var data in objectData)
                {
                    var x = (int)data["x"];
                    var y = (int)data["y"];

                    if (!((float)x).In(room.X, room.X + room.BoundingBox.Width) || !((float)y).In(room.Y, room.Y + room.BoundingBox.Height))
                    {
                        continue;
                    }

                    var type = data["name"].ToString();

                    if (type == "item")
                    {
                        var itemType = data.ContainsKey("itemType") ? (int)data["itemType"] : -1;
                        var itemName = data.ContainsKey("itemName") ? data["itemName"].ToString() : "-unknown-";

                        switch (itemType)
                        {
                            case 0: // ability item: push
                                var item = new AbilityItem(x + 8, y + 8, room, PlayerAbility.PUSH, itemName);
                                item.Texture = AssetManager.ItemSprites[0];                                
                                break;
                            // TODO: add other item types, collectables etc.
                        }                        
                    }                    
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Unable to initialize objects: " + e.Message);
                throw;
            }
        }

        /// <summary>
        /// Creates a room object and adds it to the camera room list.
        /// </summary>
        /// <param name="roomData"></param>
        public static void CreateRoom(List<Dictionary<string, object>> roomData)
        {
            var camera = RoomCamera.Current;

            try
            {
                foreach (var data in roomData)
                {
                    var x = (int)data["x"];
                    var y = (int)data["y"];

                    var w = data.ContainsKey("width") ? (int)data["width"] : camera.ViewWidth;
                    var h = data.ContainsKey("height") ? (int)data["height"] : camera.ViewHeight;
                    var bg = data.ContainsKey("bg") ? (int)data["bg"] : -1;

                    int remX, remY, remW, remH;
                    Math.DivRem(x, camera.ViewWidth, out remX);
                    Math.DivRem(y, camera.ViewHeight, out remY);
                    Math.DivRem(w, camera.ViewWidth, out remW);
                    Math.DivRem(h, camera.ViewHeight, out remH);

                    if (remX != 0 || remY != 0 || remW != 0 || remH != 0)
                        throw new ArgumentException($"The room at ({x},{y}) has an incorrect size or position!");

                    var room = new Room(x, y, w, h);
                    room.Background = bg;
                    camera.Rooms.Add(room);
                }

                // load rooms of standard size when there is none
                for (var i = 0; i < GameManager.Current.Map.Width * Globals.TILE; i += camera.ViewWidth)
                {
                    for (var j = 0; j < GameManager.Current.Map.Height * Globals.TILE; j += camera.ViewHeight)
                    {
                        if (ObjectManager.CollisionPoint<Room>(i + Globals.TILE, j + Globals.TILE).Count == 0)
                        {
                            var room = new Room(i, j, camera.ViewWidth, camera.ViewHeight);
                            camera.Rooms.Add(room);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Unable to initialize room from data: " + e.Message);
                throw;
            }
        }
    }
}
