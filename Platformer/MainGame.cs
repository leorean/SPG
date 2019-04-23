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
using Platformer.Misc;
using Platformer.Objects;
using SPG.Draw;

namespace Platformer
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class MainGame : SPG.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        //private Matrix scaleMatrix;

        private RoomCamera camera;

        public override Camera Camera {
            get
            {
                return camera;
            }
            protected set
            {
                camera = value as RoomCamera;
            }
        }
        
        private Size viewSize;
        private Size screenSize;
        private float scale;

        Input input;

        private TextureSet tileSet;
        private TextureSet playerSet;
        private Player player;

        private int playerX = 16 * 8;
        private int playerY = 16 * 4;

        private List<Room> loadedRooms;
        
        public override GraphicsDeviceManager GraphicsDeviceManager { get => graphics; }
        public override SpriteBatch SpriteBatch { get => spriteBatch; }

        public Font font;

        public MainGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            GameManager.Game = this;

            IsMouseVisible = true;

            viewSize = new Size(256, 144);
            scale = 4.0f;

            screenSize = new Size((int)(viewSize.Width * scale), (int)(viewSize.Height * scale));

            GraphicsDeviceManager.PreferredBackBufferWidth = screenSize.Width;
            GraphicsDeviceManager.PreferredBackBufferHeight = screenSize.Height;

            input = new Input();
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
                ObjectManager.Remove(o);
            }
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
                        case 7:
                            t.TileType = TileType.Platform;
                            break;
                        default:
                            t.TileType = TileType.Solid;
                            var solid = new Solid(i * Globals.TILE, j * Globals.TILE, room);
                            solid.Enabled = false;                            
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
            
            ObjectManager.Remove(player);

            player = null;
            camera.Reset();            
        }

        /// <summary>
        /// Loads the whole level.
        /// </summary>
        public void LoadLevel()
        {
            // TODO: load from save/load coordinates
            
            var startRoom = camera.Rooms.Where(r => r.X == 0 && r.Y == 0).FirstOrDefault();

            var neighbours = startRoom.Neighbors();
            LoadRoomObjects(startRoom);
            foreach (var n in neighbours)
                LoadRoomObjects(n);

            // player

            //var playerData = Map.ObjectData.FindFirstByTypeName("player");
            //var playerX = (int)playerData["x"] + 8;
            //var playerY = (int)playerData["y"] + 7;

            player = new Player(playerX, playerY);
            player.AnimationTexture = playerSet;
            camera.SetTarget(player);
            
        }
        
        /// <summary>
        /// called BEFORE initialize
        /// </summary>
        protected override void LoadContent()
        {
            Stopwatch sw = Stopwatch.StartNew();

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // load default tileset

            tileSet = TextureSet.Load("tiles");
                        
            // load map

            XmlDocument xml = SPG.Util.Xml.Load("testMap.tmx");
            
            Map = new GameMap(xml);

            Map.TileSet = tileSet;
            Map.LayerDepth["FG"] = Globals.LAYER_FG;
            Map.LayerDepth["WATER"] = Globals.LAYER_WATER;
            Map.LayerDepth["BG"] = Globals.LAYER_BG;
            Map.LayerDepth["BG2"] = Globals.LAYER_BG2;

            // load font

            var fontTexture = TextureSet.Load("font", 10, 10);

            font = new Font(fontTexture, ' ');

            sw.Stop();
            Debug.WriteLine("Loaded game in " + sw.ElapsedMilliseconds + "ms");            
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
            camera.OnBeginRoomChange += (sender, rooms) =>
            {
                // unload old rooms
                var oldNeighbors = rooms.Item1.Neighbors();                
                foreach (var n in oldNeighbors)
                    UnloadRoomObjects(n);

                // do this, because else the GC would wait to clean huge resources and create a temporary lag
                GC.Collect();

                var neighbors = rooms.Item2.Neighbors();
                LoadRoomObjects(rooms.Item2);
                foreach (var n in neighbors)
                    LoadRoomObjects(n);
            };

            // load room data for the camera
            var roomData = Map.ObjectData.FindByTypeName("room");
            camera.InitRoomData(roomData);

            loadedRooms = new List<Room>();

            playerSet = TextureSet.Load("player", 16, 32);

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

            if (input.IsKeyPressed(Keys.Space, Input.State.Holding))
            {
                ObjectManager.GameSpeed = 120;
            }
            else
            {
                ObjectManager.GameSpeed = 0;
                player.DebugEnabled = false;
            }

            if (input.IsKeyPressed(Keys.R, Input.State.Pressed))
            {
                UnloadLevel();
                LoadLevel();                
            }

            MouseState mouse = Mouse.GetState();

            if (mouse.LeftButton == ButtonState.Pressed)
            {                
                player.Position = camera.ToVirtual(mouse.Position.ToVector2());
                player.XVel = 0;
                player.YVel = 0;
            }

            // ++++ update objects ++++
            
            ObjectManager.UpdateObjects(gameTime);
            
            // ++++ update camera ++++

            camera.Update(gameTime);            
        }
        
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            camera.ResolutionRenderer.SetupDraw();
            
            //GraphicsDevice.Clear(Color.CornflowerBlue);
            
            SpriteBatch.BeginCamera(camera);

            camera.DrawBackground(gameTime);

            Map.Draw(gameTime);
            ObjectManager.DrawObjects(gameTime);

            font.Halign = Font.HorizontalAlignment.Center;
            font.Valign = Font.VerticalAlignment.Top;

            font.Draw(6 * Globals.TILE, 2 * Globals.TILE, "HelloWorld\nWhat's up!\nTEST.", 32);

            font.Halign = Font.HorizontalAlignment.Left;
            font.Valign = Font.VerticalAlignment.Top;

            font.Draw(camera.ViewX + 4, camera.ViewY + 4, player.HP, 0);

            SpriteBatch.End();            
        }
    }
}
