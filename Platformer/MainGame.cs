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

namespace Platformer
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class MainGame : SPG.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private Matrix scaleMatrix;

        
        private Size renderSize;
        private Size windowSize;
        private float scale;
        
        private GameMap map;
        private GameObject player;

        Viewport viewPort;

        public override GraphicsDeviceManager GraphicsDeviceManager { get => graphics; }
        public override SpriteBatch SpriteBatch { get => spriteBatch; }

        public MainGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            GameManager.Game = this;

            IsMouseVisible = true;

            renderSize = new Size(480, 288);
            scale = 3.0f;

            windowSize = new Size((int)(renderSize.Width * scale), (int)(renderSize.Height * scale));

            GraphicsDeviceManager.PreferredBackBufferWidth = windowSize.Width;
            GraphicsDeviceManager.PreferredBackBufferHeight = windowSize.Height;

            scaleMatrix = Matrix.CreateScale(new Vector3(scale, scale, 1));

            
        }

        void LoadObjectsFromMap()
        {

            var index = map.LayerInfo.ToList().IndexOf(map.LayerInfo.First(x => x.Key.ToLower() == "fg"));

            var data = map.LayerData.ElementAt(index);
            {
                for(var x = 0; x < data.Width; x++)
                {
                    for (var y = 0; y < data.Height; y++)
                    {
                        var t = data.Get(x, y);

                        if (t == null || t.ID == -1)
                            continue;

                        switch(t.TileType)
                        {
                            case TileType.Solid:
                                var block = new Solid(x * Globals.TILE, y * Globals.TILE);
                                //block.Visible = true;
                                //block.Texture = map.TileSet[28];
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();

            viewPort = new Viewport();            
            viewPort.X = 50;
            viewPort.Y = 50;
            viewPort.Width = 400;
            viewPort.Height = 240;
            viewPort.MinDepth = 0;
            viewPort.MaxDepth = 1;

            GraphicsDevice.Viewport = viewPort;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            var tileSet = TileSet.Load("tiles");

            XmlDocument xml = SPG.Util.Xml.Load("testMap.tmx");
            map = new GameMap(xml);

            map.TileSet = tileSet;
            map.LayerInfo["FG"] = Globals.LAYER_FG;
            map.LayerInfo["WATER"] = Globals.LAYER_WATER;
            map.LayerInfo["BG"] = Globals.LAYER_BG;
            map.LayerInfo["BG2"] = Globals.LAYER_BG2;

            LoadObjectsFromMap();

            // player

            player = new Player(0, 0);
            player.Texture = Content.Load<Texture2D>("player");
            player.DrawOffset = new Vector2(8, 8);
            player.BoundingBox = new RectF(-8, -8, 16, 16);
            player.Depth = Globals.LAYER_FG + 0.0010f;
            player.Debug = true;
            /*
            var block = new Solid(128, 128);
            block.Texture = tileSet[28];
            block.Depth = Globals.LAYER_FG;
            block.SetDebug(true);*/

            /*var r = new Random();
            for (var i = 0; i < 10000; i++)
            {
                var block = new Solid(r.Next(0, 280), r.Next(0, 280));
                block.Texture = tileSet[28];
                block.Depth = Globals.LAYER_FG;
                //block.Enabled = false;
                //block.SetDebug(true);
            }*/

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

            ObjectManager.UpdateObjects();

            KeyboardState keyboard = Keyboard.GetState();
            if (keyboard.IsKeyDown(Keys.Up)) { player.Position += new Vector2(0, -.1f); }
            if (keyboard.IsKeyDown(Keys.Down)) { player.Position += new Vector2(0, .1f); }
            if (keyboard.IsKeyDown(Keys.Left)) { player.Position += new Vector2(-.1f, 0); }
            if (keyboard.IsKeyDown(Keys.Right)) { player.Position += new Vector2(.1f, 0); ; }

            if (keyboard.IsKeyDown(Keys.D0)) { player.Angle += .05f; }

            MouseState mouse = Mouse.GetState();

            if (mouse.LeftButton == ButtonState.Pressed)
            {
                //var v3 = viewPort.Project(new Vector3(mouse.Position.X, mouse.Position.Y, 0), scaleMatrix, Matrix.Identity, Matrix.Identity);

                //player.Position = mouse.Position.ToVector2() / scale;

                player.Position = mouse.Position.ToVector2();
            }

            base.Update(gameTime);
        }
        
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            SpriteBatch.Begin(SpriteSortMode.FrontToBack, null, SamplerState.PointClamp, null, null, null, scaleMatrix);
            
            map.Draw();

            ObjectManager.DrawObjects();

            SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
