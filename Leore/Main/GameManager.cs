using Microsoft.Xna.Framework;
using Leore.Objects;
using Leore.Objects.Effects;
using Leore.Objects.Effects.Emitters;
using Leore.Objects.Enemies;
using Leore.Objects.Level;
using SPG.Map;
using SPG.Objects;
using SPG.Save;
using System;
using System.Collections.Generic;
using System.Linq;
using Leore.Objects.Projectiles;
using Leore.Objects.Level.Switches;
using Leore.Objects.Effects.Weather;
using Leore.Objects.Effects.Ambience;
using System.Xml;
using SPG.Util;
using System.Diagnostics;

namespace Leore.Main
{
    public class GameManager
    {
        public static readonly string DEFAULT_MAP_NAME = "tutorial2";

        public List<Room> LoadedRooms { get; private set; } = new List<Room>();
        
        public Player Player { get; set; }
        public GameMap Map { get; private set; }

        private List<GameMap> maps = new List<GameMap>();

        public SaveGame SaveGame;

        private Room lastRoom;
        private int currentWeather = -1;

        private GameObject weatherObject;

        private static GameManager instance;
        public static GameManager Current { get => instance; }

        public List<ID> NonRespawnableIDs { get; private set; } = new List<ID>();
        
        public Transition Transition { get; set; }
        
        public float CoinsAfterDeath { get; set; }
        private Vector2 originalSpawnPosition;

        private bool manualStateEnabled;

        private long playTime = 0;
        private TimeSpan prevTime = DateTime.Now.TimeOfDay;

        public static void Create()
        {
            if(instance == null)
            {
                instance = new GameManager();
            }
        }

        private GameManager()
        {
            // game setup
            SaveGame = new SaveGame("save.dat");
        }

        public void AddGameMap(string assetName)
        {
            if (!assetName.EndsWith(".tmx"))
                assetName = assetName + ".tmx";

            XmlDocument xml = Xml.Load(assetName);

            var id = maps.Count;

            var map = new GameMap(xml, assetName.Replace(".tmx", ""), id);

            map.TileSet = AssetManager.TileSet;
            map.LayerDepth["FG"] = Globals.LAYER_FG;
            map.LayerDepth["WATER"] = Globals.LAYER_WATER;
            map.LayerDepth["BG"] = Globals.LAYER_BG;
            map.LayerDepth["BG2"] = Globals.LAYER_BG2;

            maps.Add(map);
        }

        public void SetCurrentGameMap(string mapName)
        {
            if (mapName == null)
                return;

            var obj = maps.Where(x => x.Name == mapName.Replace(".tmx", "")).FirstOrDefault();

            if (obj != null)
            {
                Map = obj;                
            }
            else
            {
                throw new ArgumentException($"Map '{mapName}' could not be found. Is the map loaded & added correctly?");
            }
        }

        /// <summary>
        /// The main Save method.
        /// </summary>
        public void Save(float posX, float posY)
        {
            SaveGame.playTime = playTime;
            SaveGame.playerPosition = new Vector2(posX, posY);
            SaveGame.playerDirection = Player.Direction;
            SaveGame.gameStats = Player.Stats;
            SaveGame.currentBG = RoomCamera.Current.CurrentBG;
            SaveGame.currentWeather = currentWeather;
            SaveGame.levelName = Map.Name;            
            SaveGame.Save();
        }

        public void ChangeRoom(Room oldRoom, Room newRoom)
        {
            // unload all rooms that exist
            Room[] tmp = new Room[LoadedRooms.Count];
            LoadedRooms.CopyTo(tmp);

            foreach (var room in tmp)
                UnloadRoomObjects(room);

            // do this, because else the GC would wait to clean huge resources and create a temporary lag
            GC.Collect();

            OverwriteSwitchStateTo(false);

            // load new room + neighbors
            var neighbors = newRoom.Neighbors();
            RoomObjectLoader.CreateRoomObjects(newRoom);

            //new EmitterSpawner<GlobalWaterBubbleEmitter>(newRoom.X, newRoom.Y, newRoom);

            // create room objects from object data for current room
            var objectData = GameManager.Current.Map.ObjectData.Where(o => !o.Values.Contains("room")).ToList();
            RoomObjectLoader.CreateRoomObjectsFromData(objectData, newRoom);

            foreach (var n in neighbors)
                RoomObjectLoader.CreateRoomObjects(n);

            RoomObjectLoader.CleanObjectsExceptRoom(newRoom);
        }

        public void LoadLevel(string levelName)
        {
            // clear rooms

            foreach (var room in LoadedRooms.ToList())
            {
                UnloadRoomObjects(room);
                room.Destroy();
            }
            if (LoadedRooms.Count > 0)
                throw new Exception("Loaded rooms should be empty!");

            OverwriteSwitchStateTo(false);
            
            SetCurrentGameMap(levelName);
            
            ObjectManager.Enable<Room>();

            // load room data for the camera
            var mapIndex = Map.MapIndex;
            var roomData = Map.ObjectData.FindDataByTypeName("room");
            RoomObjectLoader.CreateRoom(roomData, mapIndex);
            
            // find starting room
            var startRoom = ObjectManager.CollisionPoints<Room>(Player.X, Player.Y).FirstOrDefault();

            if (startRoom == null)
            {
                throw new Exception($"No room detected at position {Player.X}x{Player.Y}!");
            }

            var neighbours = startRoom.Neighbors();
            RoomObjectLoader.CreateRoomObjects(startRoom);

            // create room objects from object data for current room
            var objectData = GameManager.Current.Map.ObjectData.Where(o => !o.Values.Contains("room")).ToList();
            RoomObjectLoader.CreateRoomObjectsFromData(objectData, startRoom);

            foreach (var n in neighbours)
            {
                RoomObjectLoader.CreateRoomObjects(n);
            }

            RoomObjectLoader.CleanObjectsExceptRoom(startRoom);            
            MainGame.Current.HUD.SetBoss(null);            
        }

        //public void Initialize()
        //{
        //    // handle room changing <-> object loading/unloading
            
        //}

        public void UnloadRoomObjects(Room room)
        {
            var alive = ObjectManager.Objects.Where(
                o => !(o is Room) 
                && !(o is Player)
                && !(o is Weather)
                ).ToList();
            alive.ForEach(o => o.Destroy());
            
            LoadedRooms.Remove(room);
        }

        /// <summary>
        /// Reloads the level.
        /// </summary>
        public void ReloadLevel()
        {
            UnloadLevel();
            CreateLevel();
        }

        public void CreateLevel()
        {
            var spawnX = 0f;
            var spawnY = 0f;
            Direction direction = Direction.RIGHT;

            bool success = SaveManager.Load(ref SaveGame);

            string levelName = DEFAULT_MAP_NAME;

            if (success)
            {
                levelName = SaveGame.levelName;
                playTime = SaveGame.playTime;
            }

            SetCurrentGameMap(SaveGame.levelName);

            var playerData = Map.ObjectData.FindFirstDataByTypeName("player");

            spawnX = (float)(int)playerData["x"] + 8;
            spawnY = (float)(int)playerData["y"] + 7.9f;
            var dir = (int)playerData["direction"];
            direction = (dir == 1) ? Direction.RIGHT : Direction.LEFT;
            
            originalSpawnPosition = new Vector2(spawnX, spawnY);

            if (success)
            {
                spawnX = SaveGame.playerPosition.X;
                spawnY = SaveGame.playerPosition.Y;

                direction = SaveGame.playerDirection;

                RoomCamera.Current.CurrentBG = SaveGame.currentBG;
                currentWeather = SaveGame.currentWeather;
            }

            if (spawnX == 0 && spawnY == 0)
            {
                spawnX = originalSpawnPosition.X;
                spawnY = originalSpawnPosition.Y;
            }

            // create player at start position and set camera target

            Player = new Player(spawnX, spawnY);
            Player.Direction = direction;
            Player.AnimationTexture = AssetManager.Player;

            LoadLevel(levelName);
            
            //globalWaterEmitter = new GlobalWaterBubbleEmitter(spawnX, spawnY, Player);
            //new EmitterSpawner<GlobalWaterBubbleEmitter>(spawnX, spawnY, CurrentRoom);

            RoomCamera.Current.SetTarget(Player);
            MainGame.Current.HUD.SetPlayer(Player);
            MainGame.Current.HUD.SetBoss(null);

            NonRespawnableIDs.Clear();

            // fade-in

            Transition = new Transition();
            Transition.FadeOut();
            Transition.OnTransitionEnd = (t, u, v) => { Transition = null; };

            // death penalty
            if (CoinsAfterDeath > 0)
            {
                Player.Stats.Coins = CoinsAfterDeath;
                CoinsAfterDeath = 0;
            }
        }

        public int GetStatUpItemCount(string key)
        {
            if (!Player.Stats.ItemsBought.ContainsKey(key))
                return 0;
            return Player.Stats.ItemsBought[key];
        }

        public void AddStatUpItemCount(string key)
        {
            if (!Player.Stats.ItemsBought.ContainsKey(key))
                Player.Stats.ItemsBought.Add(key, 1);
            else
            {
                Player.Stats.ItemsBought[key] = Player.Stats.ItemsBought[key] + 1;
            }
        }

        /// <summary>
        /// Returns true if one or more storyflags are set (separated by ',')
        /// </summary>
        /// <param name="storyFlag"></param>
        /// <returns></returns>
        public bool HasStoryFlag(string storyFlag)
        {
            if (string.IsNullOrEmpty(storyFlag))
                return true;
            
            storyFlag = storyFlag.Replace(" ", "");

            string[] storyFlags = storyFlag.Split(',');

            foreach(var flag in storyFlags)
            {
                if (Player.Stats.StoryFlags.Contains(flag))
                    return true;                
            }

            return false;
        }

        /// <summary>
        /// Adds one or more flags (separated by ',')
        /// </summary>
        /// <param name="storyFlag"></param>
        public void AddStoryFlag(string storyFlag)
        {
            if (string.IsNullOrEmpty(storyFlag))
                return;

            storyFlag = storyFlag.Replace(" ", "");
            string[] storyFlags = storyFlag.Split(',');

            foreach (var flag in storyFlags)
            {
                if (!Player.Stats.StoryFlags.Contains(flag))
                    Player.Stats.StoryFlags.Add(storyFlag);
            }            
        }

        /// <summary>
        /// Removes one or more flags (separated by ',')
        /// </summary>
        /// <param name="storyFlag"></param>
        public void RemoveStoryFlag(string storyFlag)
        {
            if (string.IsNullOrEmpty(storyFlag))
                return;

            storyFlag = storyFlag.Replace(" ", "");
            string[] storyFlags = storyFlag.Split(',');

            foreach (var flag in storyFlags)
            {
                if (Player.Stats.StoryFlags.Contains(flag))
                    Player.Stats.StoryFlags.Remove(storyFlag);
            }
        }

        /// <summary>
        /// Unloads the whole level.
        /// </summary>
        public void UnloadLevel()
        {
            Debug.WriteLine("Objects: " + ObjectManager.Objects.Count);

            foreach (var room in LoadedRooms.ToList())
            {
                UnloadRoomObjects(room);
                room.Destroy(); //
            }

            if (Player?.Orb != null)
            {
                Player.Orb.Parent = null;
                Player.Orb.Destroy();
            }

            Player?.Destroy();
            Player = null;
            RoomCamera.Current.Reset();

            // reset the weather 

            if (weatherObject != null)
                weatherObject.Destroy();
            weatherObject = null;
            currentWeather = -1;
            lastRoom = null;

            OverwriteSwitchStateTo(false);

            // reset savegame (will be loaded and updates afterwards)
            SaveGame = new SaveGame(SaveGame.FileName);

            NonRespawnableIDs.Clear();

            SetCurrentGameMap(DEFAULT_MAP_NAME);

            Debug.WriteLine("Objects: " + ObjectManager.Objects.Count);
            Debug.WriteLine("killing all...");

            foreach(var obj in ObjectManager.Objects.ToList())
            {
                obj.Destroy();
            }
            Debug.WriteLine("Objects: " + ObjectManager.Objects.Count);
            Debug.WriteLine("-----");

        }

        public void OverwriteSwitchStateTo(bool enabled)
        {
            manualStateEnabled = enabled;
        }
        
        public void Update(GameTime gameTime)
        {
            if (MainGame.Current.State == MainGame.GameState.Running)
            {
                DateTime elapsedTime = DateTime.Now - prevTime;
                prevTime = DateTime.Now.TimeOfDay;
                playTime += elapsedTime.Millisecond;
                Debug.WriteLine("Time elapsed: " + TimeSpan.FromMilliseconds(playTime));
            }            

            ObjectManager.Disable<GameObject>();

            var room = RoomCamera.Current.CurrentRoom;
            if (room != null)
            {
                ObjectManager.SetRegionEnabled<Collider>(room.X - Globals.T, room.Y - Globals.T, room.BoundingBox.Width + 2 * Globals.T, room.BoundingBox.Height + 2 * Globals.T, true);            
                ObjectManager.SetRegionEnabled<GameObject>(RoomCamera.Current.CurrentRoom.X, RoomCamera.Current.CurrentRoom.Y, RoomCamera.Current.CurrentRoom.BoundingBox.Width, RoomCamera.Current.CurrentRoom.BoundingBox.Height, true);

                // weather

                if (lastRoom != room)
                {
                    if (weatherObject == null || currentWeather != room.Weather)
                    {
                        if (room.Weather != -1)
                        {
                            currentWeather = room.Weather;

                            if (weatherObject != null)
                            {
                                weatherObject.Destroy();
                                weatherObject = null;
                            }
                        }
                        
                        if (weatherObject == null)
                        {
                            switch (currentWeather)
                            {
                                case 0: // no weather
                                    break;
                                case 1: // snow
                                    weatherObject = new SnowWeather(room.X, room.Y);
                                    break;
                            }
                        }
                    }                    
                    lastRoom = room;
                }

                // switch state
                var switches = ObjectManager.FindAll<GroundSwitch>();
                bool found = false;
                foreach (var s in switches)
                {
                    if (s.Active)
                    {
                        found = true;
                        RoomCamera.Current.CurrentRoom.SwitchState = true || manualStateEnabled;
                        break;
                    }
                }
                if (!found)
                    RoomCamera.Current.CurrentRoom.SwitchState = false || manualStateEnabled;

                // switch from torches

                var torches = ObjectManager.FindAll<Torch>().Where(t => t.TriggerSwitch == true);
                if (torches.Count() > 0)
                {
                    if (RoomCamera.Current.CurrentRoom.SwitchState == false)
                    {
                        RoomCamera.Current.CurrentRoom.SwitchState = torches.Count() == torches.Where(t => t.Active == true).Count();
                    }
                }
                
                // switch from toggleSwitches (overrides switch behaviour)

                var toggleSwitches = ObjectManager.FindAll<ToggleSwitch>();
                if (toggleSwitches.Count > 0) {
                    RoomCamera.Current.CurrentRoom.SwitchState = false || manualStateEnabled;

                    int activeCounter = 0;
                    foreach (var s in toggleSwitches)
                    {
                        if (s.Active)
                            activeCounter++;
                    }
                    if (activeCounter == toggleSwitches.Count)
                    {
                        RoomCamera.Current.CurrentRoom.SwitchState = true || manualStateEnabled;
                    }
                }
            }

            // ++++ enable global objects ++++

            ObjectManager.Enable<Room>();            
            ObjectManager.Enable<MessageBox>();
            ObjectManager.Enable<GlobalWaterBubbleEmitter>();
            ObjectManager.Enable<PlayerGhost>();
            ObjectManager.Enable<Orb>();
            ObjectManager.Enable<Player>();
            ObjectManager.Enable<PlayerProjectile>();
            
            // todo: instead of those, let them inherit from spell and activate spell
            ObjectManager.Enable<FireSpell>();
            ObjectManager.Enable<CrimsonSpell>();
            //ObjectManager.Enable<KeySnatchProjectile>();
            
            // todo: solve more smoothly?
            foreach (var o in ObjectManager.Objects.Where(o => o is Enemy && (o as RoomObject).Room == room)) {
                ObjectManager.Enable(o);
            }

            // ++++ update camera ++++

            RoomCamera.Current.Update(gameTime);

            ObjectManager.Disable<Room>();
            ObjectManager.Enable(RoomCamera.Current.CurrentRoom);
            
            // ++++ update objects ++++
            
            ObjectManager.UpdateObjects(gameTime);

        }

        public void AddSpell(SpellType spellType)
        {
            if (!Player.Stats.Spells.ContainsKey(spellType))
            {
                Player.Stats.Spells.Add(spellType, SpellLevel.ONE);
                Player.Stats.SpellEXP.Add(spellType, 0);                
                Player.Stats.SpellIndex = Player.Stats.Spells.IndexOf(spellType);
            }
        }

        public void RemoveSpell(SpellType spellType)
        {
            if (Player.Stats.Spells.ContainsKey(spellType))
            {
                Player.Stats.Spells.Remove(spellType);
                Player.Stats.SpellEXP.Remove(spellType);
                Player.Stats.SpellIndex = 0;
            }
        }
    }
}
