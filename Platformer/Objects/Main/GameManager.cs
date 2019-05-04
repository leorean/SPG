using Microsoft.Xna.Framework;
using Platformer.Main;
using Platformer.Objects.Enemy;
using Platformer.Objects.Level;
using SPG.Map;
using SPG.Objects;
using SPG.Save;
using SPG.Util;
using SPG.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Main
{    
    public class GameManager
    {
        private List<Room> loadedRooms;
        
        public Player Player;
        public GameMap Map;
        
        public SaveGame SaveGame;
        
        private static GameManager instance;
        public static GameManager Current { get => instance; }

        public GameManager()
        {
            instance = this;

            // game setup

            SaveGame = new SaveGame("save.dat");            
        }

        /// <summary>
        /// The main Save method.
        /// </summary>
        public void Save(float posX, float posY)
        {
            SaveGame.playerPosition = new Vector2(posX, posY);
            SaveGame.playerDirection = Player.Direction;
            SaveGame.playerStats = Player.Stats;
            SaveGame.currentBG = RoomCamera.Current.CurrentBG;
            SaveGame.Save();
        }

        /// <summary>
        /// loads objects from a given room. 
        /// </summary>
        public void LoadRoomObjects(Room room)
        {
            if (room == null || loadedRooms.Contains(room))
                return;

            var x = MathUtil.Div(room.X, RoomCamera.Current.ViewWidth) * 16;
            var y = MathUtil.Div(room.Y, RoomCamera.Current.ViewHeight) * 9;
            var w = MathUtil.Div(room.BoundingBox.Width, RoomCamera.Current.ViewWidth) * 16;
            var h = MathUtil.Div(room.BoundingBox.Height, RoomCamera.Current.ViewHeight) * 9;

            var index = Map.LayerDepth.ToList().IndexOf(Map.LayerDepth.First(o => o.Key.ToLower() == "fg"));

            var data = Map.LayerData.ElementAt(index);

            x = (int)((float)x).Clamp(0, data.Width);
            y = (int)((float)y).Clamp(0, data.Height);

            // load objects from tile data

            var solidCount = 0;

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
                            var platform = new Platform(i * Globals.TILE, j * Globals.TILE, room);
                            break;
                        case 387: // mushrooms
                            var mushroom = new Mushroom(i * Globals.TILE, j * Globals.TILE, room)
                            {
                                Texture = Map.TileSet[t.ID]
                            };
                            t.Hide();
                            break;
                        case 576: // save-statues
                            var saveSatue = new SaveStatue(i * Globals.TILE, j * Globals.TILE, room);
                            t.Hide();
                            break;
                        case 512: // spikes
                            var spike = new SpikeBottom(i * Globals.TILE, j * Globals.TILE, room);
                            break;
                        case 577:
                            var bigSpike = new BigSpike(i * Globals.TILE, j * Globals.TILE, room);
                            break;
                        case 578: case 641: case 642: break;
                        case 640: // push-blocks
                            var pushBlock = new PushBlock(i * Globals.TILE, j * Globals.TILE, room);
                            pushBlock.Texture = Map.TileSet[t.ID];
                            t.Hide();
                            break;
                        default:
                            var solid = new Solid(i * Globals.TILE, j * Globals.TILE, room)
                            {
                            };
                            solidCount++;
                            break;
                    }
                }
            }

            // create room objects from object data for current room

            var itemData = Map.ObjectData.FindDataByTypeName("item");

            RoomObjectLoader.CreateRoomObjects(itemData, room);

            //Debug.WriteLine("Created " + solidCount + " solid objects.");
            loadedRooms.Add(room);
        }

        public void Initialize()
        {
            // handle room changing <-> object loading/unloading
            RoomCamera.Current.OnRoomChange += (sender, rooms) =>
            {
                // unload all rooms that exist
                Room[] tmp = new Room[loadedRooms.Count];
                loadedRooms.CopyTo(tmp);

                foreach (var room in tmp)
                    UnloadRoomObjects(room);

                // do this, because else the GC would wait to clean huge resources and create a temporary lag
                GC.Collect();

                // load new room + neighbors
                var neighbors = rooms.Item2.Neighbors();
                LoadRoomObjects(rooms.Item2);
                foreach (var n in neighbors)
                    LoadRoomObjects(n);
            };

            // load room data for the camera
            var roomData = Map.ObjectData.FindDataByTypeName("room");
            RoomObjectLoader.CreateRoom(roomData);

            loadedRooms = new List<Room>();

            RoomCamera.Current.SetBackgrounds(AssetManager.Backgrounds);
        }

        public void UnloadRoomObjects(Room room)
        {
            // gather all objects which are inside the specified room
            var aliveObjects = ObjectManager.Objects.Where(
                o => o is RoomObject
                && (o as RoomObject).Room == room)
                .ToList();

            foreach (var o in aliveObjects)
            {
                o.Destroy();
            }

            // TODO: add all objects that are alive and should be killed

            //var alive = ObjectManager.Objects.Where(o => !(o is Room)).ToList();
            var alive = ObjectManager.Objects.Where(o => !(o is Room) && !(o is Player)).ToList();
            alive.ForEach(o => o.Destroy());

            loadedRooms.Remove(room);
        }

        /// <summary>
        /// Loads the whole level.
        /// </summary>
        public void LoadLevel()
        {
            var playerData = Map.ObjectData.FindFirstDataByTypeName("player");
            var spawnX = (float)(int)playerData["x"] + 8;
            var spawnY = (float)(int)playerData["y"] + 7.9f;
            var dir = (int)playerData["direction"];
            var direction = (dir == 1) ? Direction.RIGHT : Direction.LEFT;
            var stats = new PlayerStats
            {
                MaxHP = 5,
                MaxMP = 100,
                MPRegen = 1
            };

            bool success = SaveManager.Load(ref SaveGame);

            if (success)
            {
                spawnX = SaveGame.playerPosition.X;
                spawnY = SaveGame.playerPosition.Y;
                direction = SaveGame.playerDirection;
                stats = SaveGame.playerStats;
            }
            RoomCamera.Current.CurrentBG = SaveGame.currentBG;

            ObjectManager.Enable<Room>();

            // find starting room
            var startRoom = ObjectManager.CollisionPoint<Room>(spawnX, spawnY).FirstOrDefault();

            if (startRoom == null)
            {
                throw new Exception($"No room detected at position {spawnX}x{spawnY}!");
            }

            var neighbours = startRoom.Neighbors();
            LoadRoomObjects(startRoom);
            foreach (var n in neighbours)
                LoadRoomObjects(n);

            // create player at start position and set camera target

            Player = new Player(spawnX, spawnY);
            Player.Stats = stats;
            Player.Direction = direction;
            Player.AnimationTexture = AssetManager.PlayerSprites;

            RoomCamera.Current.SetTarget(Player);
            MainGame.Current.HUD.SetTarget(Player);
        }

        /// <summary>
        /// Unloads the whole level.
        /// </summary>
        public void UnloadLevel()
        {
            Room[] roomList = new Room[loadedRooms.Count];
            loadedRooms.CopyTo(roomList);

            foreach (var room in roomList)
            {
                UnloadRoomObjects(room);
            }

            Player.Destroy();

            Player = null;
            RoomCamera.Current.Reset();
        }

        internal void Update(GameTime gameTime)
        {
            // enable all solids from neighbors
            foreach (var room in loadedRooms)
            {
                ObjectManager.SetRegionEnabled<Solid>(room.X, room.Y, room.BoundingBox.Width, room.BoundingBox.Height, true);
            }

            if (RoomCamera.Current.CurrentRoom != null)
            {
                ObjectManager.SetRegionEnabled<GameObject>(RoomCamera.Current.CurrentRoom.X, RoomCamera.Current.CurrentRoom.Y, RoomCamera.Current.CurrentRoom.BoundingBox.Width, RoomCamera.Current.CurrentRoom.BoundingBox.Height, true);
            }
        }
    }
}
