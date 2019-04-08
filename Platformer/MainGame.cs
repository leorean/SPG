using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SPG;
using SPG.Map;
using SPG.Objects;
using SPG.Util;
using System.Xml;

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

            // player
            
            player = new GameObject(0, 0, "player");
            player.Texture = Content.Load<Texture2D>("player");
            player.DrawOffset = new Vector2(8, 8);
            player.BoundingBox = new RectF(-8, -8, 16, 16);
            player.Depth = Globals.LAYER_FG + 0.0010f;
            player.SetDebug(true);

            var block = new Solid(128, 128);
            block.Texture = tileSet[28];
            block.Depth = Globals.LAYER_FG;
            block.SetDebug(true);

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

            foreach (var o in ObjectManager.Objects)
            {
                o.Update();
            }

            KeyboardState keyboard = Keyboard.GetState();
            if (keyboard.IsKeyDown(Keys.Up)) { player.Y -= 1; }
            if (keyboard.IsKeyDown(Keys.Down)) { player.Y += 1; }
            if (keyboard.IsKeyDown(Keys.Left)) { player.X -= 1; }
            if (keyboard.IsKeyDown(Keys.Right)) { player.X += 1; }

            if (keyboard.IsKeyDown(Keys.D0)) { player.Angle += .05f; }

            MouseState mouse = Mouse.GetState();

            if (mouse.LeftButton == ButtonState.Pressed)
            {
                player.Position = mouse.Position.ToVector2() / scale;
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

            foreach (var o in ObjectManager.Objects)
            {
                o.Draw();
            }

            SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
