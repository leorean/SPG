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

namespace Leore.Main
{
    public class GameManager
    {
        public List<Room> LoadedRooms { get; private set; }
        
        public Player Player;
        public GameMap Map;
        
        public SaveGame SaveGame;
        
        private static GameManager instance;
        public static GameManager Current { get => instance; }

        public List<long> NonRespawnableIDs { get; private set; }
        
        public Transition Transition { get; set; }

        private GlobalWaterBubbleEmitter globalWaterEmitter;

        public float CoinsAfterDeath { get; set; }
        private Vector2 originalSpawnPosition;

        private bool manualStateEnabled;

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
        
        /// <summary>
        /// The main Save method.
        /// </summary>
        public void Save(float posX, float posY)
        {
            SaveGame.playerPosition = new Vector2(posX, posY);
            SaveGame.playerDirection = Player.Direction;
            SaveGame.gameStats = Player.Stats;
            SaveGame.currentBG = RoomCamera.Current.CurrentBG;            
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

            // load new room + neighbors
            var neighbors = newRoom.Neighbors();
            RoomObjectLoader.CreateRoomObjects(newRoom);
            
            // create room objects from object data for current room
            var objectData = GameManager.Current.Map.ObjectData.Where(o => !o.Values.Contains("room")).ToList();
            RoomObjectLoader.CreateRoomObjectsFromData(objectData, newRoom);

            foreach (var n in neighbors)
                RoomObjectLoader.CreateRoomObjects(n);

            RoomObjectLoader.CleanObjectsExceptRoom(newRoom);
        }

        

        public void Initialize()
        {
            // handle room changing <-> object loading/unloading
            RoomCamera.Current.OnRoomChange += (sender, rooms) =>
            {
                ChangeRoom(rooms.Item1, rooms.Item2);
            };

            // load room data for the camera
            var roomData = Map.ObjectData.FindDataByTypeName("room");
            RoomObjectLoader.CreateRoom(roomData);

            LoadedRooms = new List<Room>();

            RoomCamera.Current.SetBackgrounds(AssetManager.Backgrounds);            
        }

        public void UnloadRoomObjects(Room room)
        {
            // gather all objects which are inside the specified room
            //var aliveObjects = ObjectManager.Objects.Where(
            //    o => o is RoomObject
            //    && (o as RoomObject).Room == room)
            //    .ToList();

            //foreach (var o in aliveObjects)
            //{
            //    o.Destroy();
            //}

            // TODO: add all objects that are alive and should be killed

            //var alive = ObjectManager.Objects.Where(o => !(o is Room)).ToList();
            var alive = ObjectManager.Objects.Where(
                o => !(o is Room) 
                && !(o is Player)
                && !(o is Weather)
                ).ToList();
            alive.ForEach(o => o.Destroy());

            LoadedRooms.Remove(room);
        }

        /// <summary>
        /// Reloads the whole level.
        /// </summary>
        public void ReloadLevel()
        {
            UnloadLevel();
            LoadLevel();
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

            bool success = SaveManager.Load(ref SaveGame);

            originalSpawnPosition = new Vector2(spawnX, spawnY);

            if (success)
            {
                spawnX = SaveGame.playerPosition.X;
                spawnY = SaveGame.playerPosition.Y;

                if (spawnX == 0 && spawnY == 0)
                {
                    spawnX = originalSpawnPosition.X;
                    spawnY = originalSpawnPosition.Y;
                }
                direction = SaveGame.playerDirection;
            }
            RoomCamera.Current.CurrentBG = SaveGame.currentBG;

            ObjectManager.Enable<Room>();

            // find starting room
            var startRoom = ObjectManager.CollisionPoints<Room>(spawnX, spawnY).FirstOrDefault();

            if (startRoom == null)
            {
                throw new Exception($"No room detected at position {spawnX}x{spawnY}!");
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

            // create player at start position and set camera target

            Player = new Player(spawnX, spawnY);
            Player.Direction = direction;
            Player.AnimationTexture = AssetManager.Player;

            globalWaterEmitter = new GlobalWaterBubbleEmitter(spawnX, spawnY, Player);

            RoomCamera.Current.SetTarget(Player);
            MainGame.Current.HUD.SetPlayer(Player);
            MainGame.Current.HUD.SetBoss(null);

            NonRespawnableIDs = new List<long>();

            // fade-in

            Transition = new Transition();
            Transition.FadeOut();
            Transition.OnTransitionEnd = () => { Transition = null; };

            // death penalty
            if (CoinsAfterDeath > 0)
            {
                Player.Stats.Coins = CoinsAfterDeath;
                CoinsAfterDeath = 0;
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
            //return (!string.IsNullOrEmpty(storyFlag) && Player.Stats.StoryFlags.Contains(storyFlag)) || string.IsNullOrEmpty(storyFlag);
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
            Room[] roomList = new Room[LoadedRooms.Count];
            LoadedRooms.CopyTo(roomList);

            foreach (var room in roomList)
            {
                UnloadRoomObjects(room);
            }

            if (Player.Orb != null)
            {
                Player.Orb.Parent = null;
                Player.Orb.Destroy();
            }

            Player.Destroy();
            Player = null;
            RoomCamera.Current.Reset();

            OverwriteSwitchStateTo(false);

            // reset savegame (will be loaded and updates afterwards)
            SaveGame = new SaveGame(SaveGame.FileName);

            NonRespawnableIDs.Clear();
        }

        public void OverwriteSwitchStateTo(bool enabled)
        {
            manualStateEnabled = enabled;
        }

        public void Update(GameTime gameTime)
        {

            ObjectManager.Disable<GameObject>();
                        
            var room = RoomCamera.Current.CurrentRoom;
            if (room != null)
            {
                ObjectManager.SetRegionEnabled<Collider>(room.X - Globals.T, room.Y - Globals.T, room.BoundingBox.Width + 2 * Globals.T, room.BoundingBox.Height + 2 * Globals.T, true);
            }
            
            if (RoomCamera.Current.CurrentRoom != null)
            {
                ObjectManager.SetRegionEnabled<GameObject>(RoomCamera.Current.CurrentRoom.X, RoomCamera.Current.CurrentRoom.Y, RoomCamera.Current.CurrentRoom.BoundingBox.Width, RoomCamera.Current.CurrentRoom.BoundingBox.Height, true);

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
            ObjectManager.Enable<KeySnatchProjectile>();
            
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
