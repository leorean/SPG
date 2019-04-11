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

        
        private Size viewSize;
        private Size screenSize;
        private float scale;
        
        private GameObject player;

        private ResolutionRenderer resolutionRenderer;
        private Camera camera;

        public override GraphicsDeviceManager GraphicsDeviceManager { get => graphics; }
        public override SpriteBatch SpriteBatch { get => spriteBatch; }

        public MainGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            GameManager.Game = this;

            IsMouseVisible = true;

            viewSize = new Size(256, 144);
            scale = 3.0f;

            screenSize = new Size((int)(viewSize.Width * scale), (int)(viewSize.Height * scale));

            GraphicsDeviceManager.PreferredBackBufferWidth = screenSize.Width;
            GraphicsDeviceManager.PreferredBackBufferHeight = screenSize.Height;

            //scaleMatrix = Matrix.CreateScale(new Vector3(scale, scale, 1));

            
        }

        void LoadObjectsFromMap()
        {

            var index = Map.LayerInfo.ToList().IndexOf(Map.LayerInfo.First(x => x.Key.ToLower() == "fg"));

            var data = Map.LayerData.ElementAt(index);
            {
                for(var x = 0; x < data.Width; x++)
                {
                    for (var y = 0; y < data.Height; y++)
                    {
                        var t = data.Get(x, y);

                        if (t == null || t.ID == -1)
                            continue;

                        switch(t.ID)
                        {
                            case 7:
                                t.TileType = TileType.Platform;
                                break;
                            default:
                                t.TileType = TileType.Solid;
                                var solid = new Solid(x * Globals.TILE, y * Globals.TILE);
                                //solid.Visible = true;
                                //solid.Texture = map.TileSet[28];
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
            base.Initialize();

            resolutionRenderer = new ResolutionRenderer(viewSize.Width, viewSize.Height, screenSize.Width, screenSize.Height);
            
            camera = new Camera(resolutionRenderer) { MaxZoom = 10f, MinZoom = .4f, Zoom = 1f };
            camera.SetPosition(Vector2.Zero);

            camera.EnableBounds(new Rectangle(0, 0, Map.Width * Globals.TILE, Map.Height * Globals.TILE));

            //camera.Target = player;
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
            Map = new GameMap(xml);

            Map.TileSet = tileSet;
            Map.LayerInfo["FG"] = Globals.LAYER_FG;
            Map.LayerInfo["WATER"] = Globals.LAYER_WATER;
            Map.LayerInfo["BG"] = Globals.LAYER_BG;
            Map.LayerInfo["BG2"] = Globals.LAYER_BG2;
            
            LoadObjectsFromMap();

            // player

            player = new Player(20 * Globals.TILE, 5 * Globals.TILE);
            player.Texture = Content.Load<Texture2D>("player");
            player.DrawOffset = new Vector2(8, 8);
            player.BoundingBox = new RectF(-4, -4, 8, 12);
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


            KeyboardState keyboard = Keyboard.GetState();
            if (keyboard.IsKeyDown(Keys.Up)) { player.YVel = -2f; }
            //if (keyboard.IsKeyDown(Keys.Down)) { player.YVel += new Vector2(0, 1f); }
            if (keyboard.IsKeyDown(Keys.Left)) { player.XVel = -1f; }
            if (keyboard.IsKeyDown(Keys.Right)) { player.XVel = 1f; }

            if (keyboard.IsKeyDown(Keys.D0)) { player.Angle += .05f; }

            MouseState mouse = Mouse.GetState();

            if (mouse.LeftButton == ButtonState.Pressed)
            {                
                player.Position = camera.ToVirtual(mouse.Position.ToVector2());
            }

            camera.Position = player.Position;

            //camera.Update(gameTime);

            ObjectManager.UpdateObjects(gameTime);

            base.Update(gameTime);
        }
        
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //GameManager.Game.GraphicsDevice.SetRenderTarget(camera.RenderTarget);
            
            resolutionRenderer.SetupDraw();
            
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            SpriteBatch.BeginCamera(camera);

            Map.Draw(gameTime);
            ObjectManager.DrawObjects(gameTime);
            
            SpriteBatch.End();
            
            base.Draw(gameTime);

            

        }
    }
}
