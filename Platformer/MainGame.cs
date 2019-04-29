using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SPG;
using SPG.Map;
using SPG.Objects;
using SPG.Util;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System;
using SPG.View;
using System.Diagnostics;
using Platformer.Main;
using Platformer.Objects;
using SPG.Draw;
using SPG.Save;
using Platformer.Objects.Enemy;
using System.Threading.Tasks;
using System.Threading;
using Platformer.Objects.Effects;
using Platformer.Objects.Main;

namespace Platformer
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class MainGame : SPG.Game
    {
        // visual vars

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private Size viewSize;
        private Size screenSize;
        private float scale;

        // game variables

        private RoomCamera camera;
        public override Camera Camera { get => camera; protected set => camera = value as RoomCamera; }
        public override GraphicsDeviceManager GraphicsDeviceManager { get => graphics; }
        //public SpriteBatch SpriteBatch { get => spriteBatch; }
        private GameMap map;
        public override GameMap Map { get => map; protected set => map = value; }

        // input

        Input input;

        // common textures & objects

        public TextureSet TileSet { get; private set; }
        public TextureSet PlayerSprites { get; private set; }
        public TextureSet SaveStatueSprites { get; private set; }

        public Font DefaultFont { get; private set; }
        public Font DamageFont { get; private set; }

        // common objects

        public HUD HUD { get; private set; }

        public Player Player { get; private set; }

        // stores room data
        private List<Room> loadedRooms;

        // save data

        private SaveGame saveGame;
        public SaveGame SaveGame { get => saveGame; private set => saveGame = value; }
                
        private bool initialized;

        private static MainGame instance;
        public static MainGame Current { get => instance; }

        public MainGame()
        {
            // fundamental setup

            instance = this;

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            GameManager.Game = this;

            input = new Input();

            // window & screen setup

            viewSize = new Size(256, 144);
            scale = 4.0f;
            screenSize = new Size((int)(viewSize.Width * scale), (int)(viewSize.Height * scale));

            graphics.PreferredBackBufferWidth = screenSize.Width;
            graphics.PreferredBackBufferHeight = screenSize.Height;

            IsMouseVisible = true;
            Window.AllowUserResizing = true;

            // change window & camera parameters when changing window size
            Window.ClientSizeChanged += (s, args) =>
            {
                var w = Window.ClientBounds.Width;
                var h = Window.ClientBounds.Height;

                graphics.PreferredBackBufferWidth = w;
                graphics.PreferredBackBufferHeight = h;

                camera.ResolutionRenderer.ScreenWidth = w;
                camera.ResolutionRenderer.ScreenHeight = h;

                graphics.ApplyChanges();

                Debug.WriteLine($"Size changed to {w}x{h}.");
            };

            // game setup

            SaveGame = new SaveGame("save.dat");

            HUD = new HUD();
        }

        /// <summary>
        /// The main Save method.
        /// </summary>
        public void Save(float posX, float posY)
        {
            SaveGame.playerPosition = new Vector2(posX, posY);
            SaveGame.playerDirection = Player.Direction;
            SaveGame.playerStats = Player.Stats;
            SaveGame.Save();
        }

        void UnloadRoomObjects(Room room)
        {
            // gather all objects which are inside the specified room
            var aliveObjects = ObjectManager.Objects.Where(
                o => o is RoomDependentdObject
                && (o as RoomDependentdObject).Room == room)
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
        /// loads objects from a given room. 
        /// </summary>
        void LoadRoomObjects(Room room)
        {
            if (room == null || loadedRooms.Contains(room))
                return;

            var x = MathUtil.Div(room.X, camera.ViewWidth) * 16;
            var y = MathUtil.Div(room.Y, camera.ViewHeight) * 9;
            var w = MathUtil.Div(room.BoundingBox.Width, camera.ViewWidth) * 16;
            var h = MathUtil.Div(room.BoundingBox.Height, camera.ViewHeight) * 9;
            
            var index = Map.LayerDepth.ToList().IndexOf(Map.LayerDepth.First(o => o.Key.ToLower() == "fg"));

            var data = Map.LayerData.ElementAt(index);

            x = (int)((float)x).Clamp(0, data.Width);
            y = (int)((float)y).Clamp(0, data.Height);
            
            var solidCount = 0;

            for(int i = x; i < x + w; i++)
            {
                for (int j = y; j < y + h; j++)
                {
                    var t = data.Get(i, j);

                    if (t == null || t.ID == -1)
                        continue;

                    switch(t.ID)
                    {
                        case 0: // platforms
                            var platform = new Platform(i * Globals.TILE, j * Globals.TILE, room);                            
                            break;
                        case 576: //save-statues
                            var saveSatue = new SaveStatue(i * Globals.TILE, j * Globals.TILE, room);
                            t.Hide();
                            break;
                        case 512: //spikes
                            var spike = new SpikeBottom(i * Globals.TILE, j * Globals.TILE, room)
                            {
                                Enabled = false,
                                Texture = TileSet[512]
                            };
                            t.Hide();
                            break;
                        default:                            
                            var solid = new Solid(i * Globals.TILE, j * Globals.TILE, room)
                            {
                                Enabled = false
                            };
                            solidCount++;
                            break;
                    }
                }
            }

            Debug.WriteLine("Created " + solidCount + " solid objects.");
            loadedRooms.Add(room);
        }

        /// <summary>
        /// Unloads the whole level.
        /// </summary>
        public void UnloadLevel()
        {
            Room[] roomList = new Room[loadedRooms.Count];
            loadedRooms.CopyTo(roomList);

            foreach(var room in roomList)
            {
                UnloadRoomObjects(room);
            }

            Player.Destroy();

            Player = null;
            camera.Reset();            
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
                MaxMagic = 100,
                MagicRegen = 1
            };

            bool success = SaveManager.Load(ref saveGame);

            if (success)
            {
                spawnX = SaveGame.playerPosition.X;
                spawnY = SaveGame.playerPosition.Y;
                direction = SaveGame.playerDirection;
                stats = SaveGame.playerStats;
            }

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
            Player.AnimationTexture = PlayerSprites;
            camera.SetTarget(Player);

            HUD.SetTarget(Player);

        }

        /// <summary>
        /// called BEFORE initialize
        /// </summary>
        protected override void LoadContent()
        {
            Stopwatch sw = Stopwatch.StartNew();

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // load textures & texture sets

            TileSet = TextureSet.Load("tiles");
            SaveStatueSprites = TextureSet.Load("save");
            PlayerSprites = TextureSet.Load("player", 16, 32);
            HUD.Texture = Content.Load<Texture2D>("hud");

            // load map

            XmlDocument xml = Xml.Load("worldMap.tmx");
            
            Map = new GameMap(xml);

            Map.TileSet = TileSet;
            Map.LayerDepth["FG"] = Globals.LAYER_FG;
            Map.LayerDepth["WATER"] = Globals.LAYER_WATER;
            Map.LayerDepth["BG"] = Globals.LAYER_BG;
            Map.LayerDepth["BG2"] = Globals.LAYER_BG2;
            
            // load fonts

            var defaultFont = TextureSet.Load("font", 10, 10);
            var damageFont = TextureSet.Load("damageFont", 10, 10);

            DefaultFont = new Font(defaultFont, ' ');
            DamageFont = new Font(damageFont, ' ');

            initialized = true;
            Debug.WriteLine("Loaded game in " + sw.ElapsedMilliseconds + "ms");
            sw.Stop();
        }

        /// <summary>
        /// Called AFTER load content
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            var resolutionRenderer = new ResolutionRenderer(viewSize.Width, viewSize.Height, screenSize.Width, screenSize.Height);

            camera = new RoomCamera(resolutionRenderer) { MaxZoom = 2f, MinZoom = .5f, Zoom = 1f };
            camera.SetPosition(Vector2.Zero);

            // first, restrict the bounds to the whole map - will be overridden from the room camera afterwards
            camera.EnableBounds(new Rectangle(0, 0, Map.Width * Globals.TILE, Map.Height * Globals.TILE));

            // handle room changing <-> object loading/unloading
            camera.OnRoomChange += (sender, rooms) =>
            {
                // unload all rooms that exist
                Room[] tmp = new Room[loadedRooms.Count];
                loadedRooms.CopyTo(tmp);

                foreach(var room in tmp)
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
            camera.InitRoomData(roomData);

            loadedRooms = new List<Room>();

            //PlayerSet = TextureSet.Load("player", 16, 32);

            var backgrounds = TextureSet.Load("background", 16 * Globals.TILE, 9 * Globals.TILE);
            camera.SetBackgrounds(backgrounds);

            LoadLevel();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // ++++ debug input ++++

            input.Update(gameTime);

            if (input.IsKeyPressed(Keys.D0, Input.State.Pressed))
            {
                var posX = MathUtil.Div(Player.Position.X, Globals.TILE) * Globals.TILE + 8;
                var posY = MathUtil.Div(Player.Position.Y, Globals.TILE) * Globals.TILE + 7;

                Save(posX, posY);

                Debug.WriteLine("Saved.");
            }

            if (input.IsKeyPressed(Keys.D9, Input.State.Pressed))
            {
                SaveManager.Load(ref saveGame);
                
                Debug.WriteLine("Loaded.");
            }

            if (input.IsKeyPressed(Keys.C, Input.State.Pressed))
            {
                SaveGame.Delete();
                
                Debug.WriteLine("Deleted save game.");
            }

            if (input.IsKeyPressed(Keys.Space, Input.State.Holding))
            {
                ObjectManager.GameSpeed = 120;
            }
            else
            {
                ObjectManager.GameSpeed = 0;
                if (Player != null)
                    Player.DebugEnabled = false;
            }

            if (input.IsKeyPressed(Keys.R, Input.State.Pressed))
            {
                UnloadLevel();
                LoadLevel();                
            }

            MouseState mouse = Mouse.GetState();

            if (mouse.LeftButton == ButtonState.Pressed)
            {                
                Player.Position = camera.ToVirtual(mouse.Position.ToVector2());
                Player.XVel = 0;
                Player.YVel = 0;
            }

            // enable all solids from neighbors
            foreach (var room in loadedRooms)
            {
                ObjectManager.SetRegionEnabled<Solid>(room.X, room.Y, room.BoundingBox.Width, room.BoundingBox.Height, true);
            }

            if (camera?.CurrentRoom != null)
            {
                ObjectManager.SetRegionEnabled<GameObject>(camera.CurrentRoom.X, camera.CurrentRoom.Y, camera.CurrentRoom.BoundingBox.Width, camera.CurrentRoom.BoundingBox.Height, true);
            }
            
            // ++++ update objects ++++

            ObjectManager.UpdateObjects(gameTime);

            // ++++ update camera ++++

            camera.Update(gameTime);

            // ++++ update HUD ++++

            HUD.Update(gameTime);
        }
        
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            camera.ResolutionRenderer.SetupDraw();

            // IMPORTANT HINT: when a texture's alpha is not "pretty", check the Content settings of that texture! Make sure that the texture has premultiplied : true.

            spriteBatch.BeginCamera(camera, BlendState.NonPremultiplied);
            camera.DrawBackground(spriteBatch, gameTime);

            Map.Draw(spriteBatch, gameTime);
            ObjectManager.DrawObjects(spriteBatch, gameTime);

            HUD.Draw(spriteBatch, gameTime);

            /*if (initialized)
            {
                DefaultFont.Halign = Font.HorizontalAlignment.Center;
                DefaultFont.Valign = Font.VerticalAlignment.Top;

                DefaultFont.Draw(6 * Globals.TILE, 2 * Globals.TILE, "HelloWorld\nWhat's up!\nTEST.", 32);

                DefaultFont.Halign = Font.HorizontalAlignment.Left;
                DefaultFont.Valign = Font.VerticalAlignment.Top;

                DefaultFont.Draw(camera.ViewX + 4, camera.ViewY + 4, Player.Stats.HP, 0);
            }*/
            spriteBatch.End();
        }
    }
}
