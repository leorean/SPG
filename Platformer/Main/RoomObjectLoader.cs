using Platformer.Objects.Enemies;
using Platformer.Objects.Items;
using Platformer.Objects.Level;
using SPG.Objects;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SPG.Map;
using Platformer.Objects.Effects.Emitters;
using Microsoft.Xna.Framework;
using static Platformer.Objects.Items.StatUpItem;
using Platformer.Objects;

namespace Platformer.Main
{
    public static class RoomObjectLoader
    {

        /// <summary>
        /// loads objects from a given room. 
        /// </summary>
        public static void CreateRoomObjects(Room room)
        {
            if (room == null || GameManager.Current.LoadedRooms.Contains(room))
                return;

            var x = MathUtil.Div(room.X, RoomCamera.Current.ViewWidth) * 16;
            var y = MathUtil.Div(room.Y, RoomCamera.Current.ViewHeight) * 9;
            var w = MathUtil.Div(room.BoundingBox.Width, RoomCamera.Current.ViewWidth) * 16;
            var h = MathUtil.Div(room.BoundingBox.Height, RoomCamera.Current.ViewHeight) * 9;

            var index = GameManager.Current.Map.LayerDepth.ToList().IndexOf(GameManager.Current.Map.LayerDepth.First(o => o.Key.ToLower() == "fg"));

            var data = GameManager.Current.Map.LayerData.ElementAt(index);

            x = (int)((float)x).Clamp(0, data.Width);
            y = (int)((float)y).Clamp(0, data.Height);

            // load objects from tile data
            
            for (int i = x; i < x + w; i++)
            {
                for (int j = y; j < y + h; j++)
                {
                    var t = data.Get(i, j);

                    if (t == null || t.ID == -1)
                        continue;
                    
                    switch (t.ID)
                    {
                        case 0: // platforms
                        case 12:
                        case 34:
                        case 535:
                        case 612:
                        case 646:
                            var platform = new Platform(i * Globals.TILE, j * Globals.TILE, room);
                            t.TileOptions.Solid = false;
                            if (t.ID == 646) t.TileOptions.Visible = false; // <- invisible platform
                            break;
                        case 387: // mushrooms
                            var mushroom = new Mushroom(i * Globals.TILE, j * Globals.TILE, room)
                            {
                                Texture = GameManager.Current.Map.TileSet[t.ID]
                            };
                            t.TileOptions.Visible = false;
                            t.TileOptions.Solid = false;                            
                            break;
                        case 576: // save-statues
                            var saveSatue = new SaveStatue(i * Globals.TILE, j * Globals.TILE, room);
                            t.TileOptions.Visible = false;
                            t.TileOptions.Solid = false;
                            break;
                        case 512: // spikes (bottom)
                            new SpikeBottom(i * Globals.TILE, j * Globals.TILE, room);
                            t.TileOptions.Visible = true;
                            t.TileOptions.Solid = false;
                            break;
                        case 513: // spikes (top)
                            new SpikeTop(i * Globals.TILE, j * Globals.TILE, room);
                            t.TileOptions.Visible = true;
                            t.TileOptions.Solid = false;
                            break;
                        case 514: // spikes (right)
                            new SpikeRight(i * Globals.TILE, j * Globals.TILE, room);
                            t.TileOptions.Visible = true;
                            t.TileOptions.Solid = false;
                            break;
                        case 515: // spikes (left)
                            new SpikeLeft(i * Globals.TILE, j * Globals.TILE, room);
                            t.TileOptions.Visible = true;
                            t.TileOptions.Solid = false;
                            break;
                        case 577: // BIG spikes (deadly)
                            var bigSpike = new BigSpike(i * Globals.TILE, j * Globals.TILE, room);
                            t.TileOptions.Visible = true;
                            t.TileOptions.Solid = false;
                            break;
                        case 578: case 641: case 642:
                            t.TileOptions.Visible = true;
                            t.TileOptions.Solid = false;
                            break;
                        case 599: // chimney smoke 
                            new Smoke(i * Globals.TILE + 8, j * Globals.TILE + 8, room);
                            t.TileOptions.Visible = false;
                            t.TileOptions.Solid = false;
                            break;
                        case 640: // push-blocks
                            var pushBlock = new PushBlock(i * Globals.TILE, j * Globals.TILE, room);
                            pushBlock.Texture = GameManager.Current.Map.TileSet[t.ID];
                            t.TileOptions.Visible = false;
                            t.TileOptions.Solid = false;
                            break;
                        case 643: // switches (ground)
                            new GroundSwitch(i * Globals.TILE, j * Globals.TILE, false, room);
                            t.TileOptions.Visible = false;
                            t.TileOptions.Solid = false;
                            break;
                        case 644: // switches (ground) - activate once
                            new GroundSwitch(i * Globals.TILE, j * Globals.TILE, true, room);
                            t.TileOptions.Visible = false;
                            t.TileOptions.Solid = false;
                            break;
                        case 648: // waterfall (bright)
                        case 649: // waterfall (cave)
                            new WaterFall(i * Globals.TILE, j * Globals.TILE, room, (t.ID - 648));
                            t.TileOptions.Visible = false;
                            t.TileOptions.Solid = false;
                            break;
                        case 579: // hp potion
                            new Potion(i * Globals.TILE + 8, j * Globals.TILE + 8, room, PotionType.HP);
                            t.TileOptions.Visible = false;
                            t.TileOptions.Solid = false;
                            break;
                        case 580: // mp potion
                            new Potion(i * Globals.TILE + 8, j * Globals.TILE + 8, room, PotionType.MP);
                            t.TileOptions.Visible = false;
                            t.TileOptions.Solid = false;
                            break;
                        case 581: // key
                            var key = new Key(i * Globals.TILE + 8, j * Globals.TILE + 8, room);
                            t.TileOptions.Visible = false;
                            t.TileOptions.Solid = false;
                            break;
                        case 582: // keyblock
                            new KeyBlock(i * Globals.TILE, j * Globals.TILE, room);
                            t.TileOptions.Visible = false;
                            t.TileOptions.Solid = false;
                            break;
                        case 647: // control disabler
                            new JumpControlDisabler(i * Globals.TILE, j * Globals.TILE, room);
                            t.TileOptions.Visible = false;
                            t.TileOptions.Solid = false;
                            break;
                        // coins
                        case 704: case 705: case 706: case 707: case 708: case 709: case 710:
                            var coin = new Coin(i * Globals.TILE + 8, j * Globals.TILE + 8, room, (t.ID - 704).TileIDToCoinValue());
                            t.TileOptions.Visible = false;
                            t.TileOptions.Solid = false;
                            break;
                        case 711: // max HP
                            new StatUpItem(i * Globals.TILE + 8, j * Globals.TILE + 8, room, StatType.HP);
                            t.TileOptions.Visible = false;
                            t.TileOptions.Solid = false;
                            break;
                        case 712: // max MP
                            new StatUpItem(i * Globals.TILE + 8, j * Globals.TILE + 8, room, StatType.MP);
                            t.TileOptions.Visible = false;
                            t.TileOptions.Solid = false;
                            break;
                        case 713: // MP Regen +
                            new StatUpItem(i * Globals.TILE + 8, j * Globals.TILE + 8, room, StatType.Regen);
                            t.TileOptions.Visible = false;
                            t.TileOptions.Solid = false;
                            break;
                        case 714: // destroy blocks
                        case 715:
                            var destroyBlock = new DestroyBlock(i * Globals.TILE, j * Globals.TILE, room, t.ID - 714 + 1);
                            t.TileOptions.Visible = false;
                            t.TileOptions.Solid = false;
                            break;
                        case 716: // enemy block
                            t.TileOptions.Visible = false;
                            t.TileOptions.Solid = false;
                            new EnemyBlock(i * Globals.TILE, j * Globals.TILE, room);
                            break;
                        case 717: // orb block
                            t.TileOptions.Visible = false;
                            t.TileOptions.Solid = false;
                            new OrbBlock(i * Globals.TILE, j * Globals.TILE, room);
                            break;
                        case 723:
                            t.TileOptions.Visible = false;
                            t.TileOptions.Solid = false;
                            var abs = new AirBubbleSpawner(i * Globals.TILE, j * Globals.TILE, room);
                            abs.Texture = GameManager.Current.Map.TileSet[t.ID];
                            break;
                        case 768: // enemy Bat
                            t.TileOptions.Visible = false;
                            t.TileOptions.Solid = false;
                            new EnemyBat(i * Globals.TILE + 8, j * Globals.TILE + 8, room);
                            break;
                        case 769: // enemy Grassy
                            new EnemyGrassy(i * Globals.TILE + 8, j * Globals.TILE + 8, room);
                            t.TileOptions.Visible = false;
                            t.TileOptions.Solid = false;
                            break;
                        case 770: // enemy Voidling (without shield)
                            new EnemyVoidling(i * Globals.TILE + 8, j * Globals.TILE + 8, room, 0);
                            t.TileOptions.Visible = false;
                            t.TileOptions.Solid = false;
                            break;
                        case 771: // enemy Voidling (with shield)
                            new EnemyVoidling(i * Globals.TILE + 8, j * Globals.TILE + 8, room, 1);
                            t.TileOptions.Visible = false;
                            t.TileOptions.Solid = false;
                            break;
                        case 719: // switch block (default: on)
                            new SwitchBlock(i * Globals.TILE, j * Globals.TILE, room, 1);
                            t.TileOptions.Visible = false;
                            t.TileOptions.Solid = false;
                            break;
                        case 720: // switch block (default: off)
                            new SwitchBlock(i * Globals.TILE, j * Globals.TILE, room, 0);
                            t.TileOptions.Visible = false;
                            t.TileOptions.Solid = false;
                            break;
                        case 721: // pots
                            var pot = new Pot(i * Globals.TILE, j * Globals.TILE, room);
                            pot.Texture = GameManager.Current.Map.TileSet[t.ID];
                            t.TileOptions.Visible = false;
                            t.TileOptions.Solid = false;
                            break;
                        case 722: // bushes
                            var bush = new Bush(i * Globals.TILE, j * Globals.TILE, room);
                            bush.Texture = GameManager.Current.Map.TileSet[t.ID];
                            t.TileOptions.Visible = false;
                            t.TileOptions.Solid = false;
                            break;
                        case 832:
                            new Teleporter(i * Globals.TILE + 8, j * Globals.TILE + 8, room);
                            t.TileOptions.Visible = false;
                            t.TileOptions.Solid = false;
                            break;
                        default:
                            var solid = new Solid(i * Globals.TILE, j * Globals.TILE, room);
                            if (t.ID == 645) t.TileOptions.Visible = false;  // <- invisible blocks
                            break;
                    }
                }
            }
            
            //Debug.WriteLine("Created " + solidCount + " solid objects.");
            GameManager.Current.LoadedRooms.Add(room);
        }

        public static void CreateRoomObjectsFromData(List<Dictionary<string, object>> objectData, Room room)
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
                        var itemText = data.ContainsKey("text") ? data["text"].ToString() : "-unknown-";

                        AbilityItem item = null;

                        switch (itemType)
                        {
                            case 0: // ability item: push
                                item = new AbilityItem(x + 8, y + 8, room, itemName);
                                item.Texture = AssetManager.Items[0];
                                item.Text = itemText;
                                item.OnObtain = () => { GameManager.Current.Player.Stats.Abilities |= PlayerAbility.PUSH; };
                                break;
                            case 1: // ability item: orb
                                item = new AbilityItem(x + 8, y + 8, room, itemName);
                                item.Texture = AssetManager.Orbs[0];
                                item.DrawOffset = new Vector2(8);
                                item.OnObtain = () => { GameManager.Current.Player.Stats.Abilities |= PlayerAbility.ORB; };
                                item.Text = itemText;
                                break;
                            case 2: // spell: shoot star
                                item = new AbilityItem(x + 8, y + 8, room, itemName);
                                item.Texture = AssetManager.Items[4];
                                item.OnObtain = () =>
                                {
                                    GameManager.Current.AddSpell(SpellType.STAR);
                                };
                                item.Text = itemText;
                                break;                            
                                // TODO: add other item types, collectables etc.
                        }                        
                    }
                    if (type == "door")
                    {
                        var tx = data.ContainsKey("tx") ? (int)data["tx"] : 0;
                        var ty = data.ContainsKey("ty") ? (int)data["ty"] : 0;

                        var door = new Door(x, y, room, tx, ty);
                    }
                    if (type == "npc")
                    {
                        var npcText = data.ContainsKey("text") ? data["text"].ToString() : "-unknown-";
                        var npcYesText = data.ContainsKey("yesText") ? data["yesText"].ToString() : null;
                        var npcNoText = data.ContainsKey("noText") ? data["noText"].ToString() : null;
                        var npcType = data.ContainsKey("npcType") ? (int)data["npcType"] : -1;
                        var npcDirection = data.ContainsKey("direction") ? (Direction)(int)data["direction"] : Direction.NONE;
                        var npcCenterText = data.ContainsKey("centerText") ? (bool)data["centerText"] : false;

                        var npc = new NPC(x + 8, y + 8, room, npcType, npcText, npcCenterText, npcDirection, yesText:npcYesText, noText:npcNoText);                        
                    }
                    if (type == "shopItem")
                    {
                        var shopItemText = data.ContainsKey("text") ? data["text"].ToString() : "-unknown-";
                        var shopItemName = data.ContainsKey("itemName") ? data["itemName"].ToString() : "-unknown-";
                        var shopItemType = data.ContainsKey("itemType") ? (int)data["itemType"] : 0;
                        var shopItemRespawn = data.ContainsKey("respawn") ? (bool)data["respawn"] : false;
                        var shopItemPrice = data.ContainsKey("price") ? (int)data["price"] : 0;

                        var shopItem = new ShopItem(x + 8, y + 8, room, shopItemName, shopItemType, shopItemPrice, shopItemText, shopItemRespawn);
                    }
                    if (type == "chest")
                    {
                        var chestValue = data.ContainsKey("value") ? (float)data["value"] : 0.0f;
                        var chest = new Chest(x, y, room, chestValue);
                    }
                    if (type == "movingPlatform")
                    {
                        var movXvel = data.ContainsKey("xVel") ? (float)data["xVel"] : 0f;
                        var movYvel = data.ContainsKey("yVel") ? (float)data["yVel"] : 0f;
                        // in tile units
                        var movXrange = data.ContainsKey("xRange") ? (int)data["xRange"] : 0;
                        var movYrange = data.ContainsKey("yRange") ? (int)data["yRange"] : 0;

                        var moveTimeout = data.ContainsKey("timeOut") ? (int)data["timeOut"] : -1;
                        var activatable = data.ContainsKey("activatable") ? (bool)data["activatable"] : false;

                        var movingPlatform = new LinearMovingPlatform(x, y, movXvel, movYvel, movXrange, movYrange, activatable, moveTimeout, room);
                    }
                    if (type == "waterMill")
                    {
                        new WaterMill(x + 8, y + 8, room);
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
                    //camera.Rooms.Add(room);
                }

                // load rooms of standard size when there is none
                for (var i = 0; i < GameManager.Current.Map.Width * Globals.TILE; i += camera.ViewWidth)
                {
                    for (var j = 0; j < GameManager.Current.Map.Height * Globals.TILE; j += camera.ViewHeight)
                    {
                        if (ObjectManager.CollisionPoints<Room>(i + Globals.TILE, j + Globals.TILE).Count == 0)
                        {
                            var room = new Room(i, j, camera.ViewWidth, camera.ViewHeight);
                            //camera.Rooms.Add(room);
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

        /// <summary>
        /// After rooms + neighbours are created, call this method to clean all room objects except for the active room
        /// </summary>
        /// <param name="room"></param>
        [Obsolete("TODO: Find a better solution to the inefficient object loading problem!")]
        public static void CleanObjectsExceptRoom(Room room)
        {
            var toDelete = ObjectManager.Objects.Where(o => o is RoomObject && !(o is Collider) && (o as RoomObject).Room != room).ToList();

            var arr = new GameObject[toDelete.Count];
            toDelete.CopyTo(arr);

            foreach (var del in arr)
                del.Destroy();            
        }
    }
}

