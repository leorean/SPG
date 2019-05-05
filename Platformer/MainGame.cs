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
using Platformer.Objects.Level;

namespace Platformer
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class MainGame : Microsoft.Xna.Framework.Game
    {
        // visual vars

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private Size viewSize;
        private Size screenSize;
        private float scale;

        // visual unique objects belong to the MainGame

        public HUD HUD { get; private set; }

        // input

        Input input;
        
        private static MainGame instance;
        public static MainGame Current { get => instance; }
        
        public MainGame()
        {
            Content.RootDirectory = "Content";

            // fundamental setup

            instance = this;

            var gm = new GameManager();

            graphics = new GraphicsDeviceManager(this);
            
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

                RoomCamera.Current.ResolutionRenderer.ScreenWidth = w;
                RoomCamera.Current.ResolutionRenderer.ScreenHeight = h;

                graphics.ApplyChanges();

                Debug.WriteLine($"Size changed to {w}x{h}.");
            };

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

            AssetManager.InitializeContent(Content);

            // load map

            XmlDocument xml = Xml.Load("worldMap.tmx");
            
            var map = new GameMap(xml);

            map.TileSet = AssetManager.TileSet;
            map.LayerDepth["FG"] = Globals.LAYER_FG;
            map.LayerDepth["WATER"] = Globals.LAYER_WATER;
            map.LayerDepth["BG"] = Globals.LAYER_BG;
            map.LayerDepth["BG2"] = Globals.LAYER_BG2;

            GameManager.Current.Map = map;

            HUD = new HUD();
            HUD.Texture = AssetManager.HUDSprite;

            Debug.WriteLine("Loaded game in " + sw.ElapsedMilliseconds + "ms");
            sw.Stop();
        }

        /// <summary>
        /// Called AFTER load content
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            var resolutionRenderer = new ResolutionRenderer(graphics.GraphicsDevice, viewSize.Width, viewSize.Height, screenSize.Width, screenSize.Height);

            var cam = new RoomCamera(resolutionRenderer) { MaxZoom = 2f, MinZoom = .5f, Zoom = 1f };

            // first, restrict the bounds to the whole map - will be overridden from the room camera afterwards
            //cam.EnableBounds(new Rectangle(0, 0, gm.Map.Width * Globals.TILE, gm.Map.Height * Globals.TILE));

            GameManager.Current.Initialize();
            GameManager.Current.LoadLevel();
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
                var posX = MathUtil.Div(GameManager.Current.Player.Position.X, Globals.TILE) * Globals.TILE + 8;
                var posY = MathUtil.Div(GameManager.Current.Player.Position.Y, Globals.TILE) * Globals.TILE + 7;

                GameManager.Current.Save(posX, posY);

                Debug.WriteLine("Saved.");
            }
            
            if (input.IsKeyPressed(Keys.C, Input.State.Pressed))
            {
                GameManager.Current.SaveGame.Delete();
                
                Debug.WriteLine("Deleted save game.");
            }

            if (input.IsKeyPressed(Keys.Space, Input.State.Holding))
            {
                ObjectManager.GameSpeed = 120;
            }
            else
            {
                ObjectManager.GameSpeed = 0;
                if (GameManager.Current.Player != null)
                    GameManager.Current.Player.DebugEnabled = false;
            }

            if (input.IsKeyPressed(Keys.R, Input.State.Pressed))
            {
                GameManager.Current.UnloadLevel();
                GameManager.Current.LoadLevel();                
            }

            MouseState mouse = Mouse.GetState();

            if (mouse.LeftButton == ButtonState.Pressed)
            {
                GameManager.Current.Player.Position = RoomCamera.Current.ToVirtual(mouse.Position.ToVector2());
                GameManager.Current.Player.XVel = 0;
                GameManager.Current.Player.YVel = 0;
            }

            GameManager.Current.Update(gameTime);

            // ++++ update camera ++++

            RoomCamera.Current.Update(gameTime);

            // ++++ update objects ++++

            ObjectManager.UpdateObjects(gameTime);
            
            // ++++ update HUD ++++

            HUD.Update(gameTime);
        }
        
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            RoomCamera.Current.ResolutionRenderer.SetupDraw();

            // IMPORTANT HINT: when a texture's alpha is not "pretty", check the Content settings of that texture! Make sure that the texture has premultiplied : true.

            spriteBatch.BeginCamera(RoomCamera.Current, BlendState.NonPremultiplied);
            RoomCamera.Current.DrawBackground(spriteBatch, gameTime);

            GameManager.Current.Map.Draw(spriteBatch, gameTime, RoomCamera.Current);
            ObjectManager.DrawObjects(spriteBatch, gameTime);

            HUD.Draw(spriteBatch, gameTime);

            /*if (initialized)
            {
                DefaultFont.Halign = Font.HorizontalAlignment.Center;
                DefaultFont.Valign = Font.VerticalAlignment.Bottom;

                DefaultFont.Draw(spriteBatch, Player.X, Player.Y - 1 * Globals.TILE, "HelloWorld\nWhat's up!\nTEST.", scale: 1f);
                
                //DefaultFont.Draw(camera.ViewX + 4, camera.ViewY + 4, Player.HP, 0);
            }*/
            spriteBatch.End();
        }
    }
}
