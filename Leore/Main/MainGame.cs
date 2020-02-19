using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SPG;
using SPG.Map;
using SPG.Objects;
using SPG.Util;
using System.Xml;
using SPG.View;
using System.Diagnostics;
using Leore.Main;
using SPG.Save;
using Leore.Objects.Items;
using Leore.Objects.Level;
using Leore.Objects.Effects.Weather;
using System.Collections.Generic;
using System.IO;
using Leore.Util;

namespace Leore.Main
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class MainGame : Microsoft.Xna.Framework.Game
    {
        // 

        public enum GameState
        {
            InTitleMenu,
            Running,
            Paused
        }

        public GameState State { get; set; } = GameState.Running;

        // visual vars

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private Size viewSize;
        private Size screenSize;
        private float scale;

        // visual unique objects belong to the MainGame
        public HUD HUD { get; private set; }

        // input

        public InputManager Input { get; private set; }
        
        private static MainGame instance;
        public static MainGame Current { get => instance; }
        
        public MainGame()
        {
            Content = new EmbeddedResourceContentManager(Services);

            Content.RootDirectory = "Content";

            // fundamental setup

            Input = new InputManager();
            Input.PreferGamePad = Properties.Settings.Default.PreferGamePad;

            instance = this;

            GameManager.Create();

            graphics = new GraphicsDeviceManager(this);
            
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

            Darkness.Create();

            // load textures & texture sets

            AssetManager.InitializeContent(Content);

            // ++++ Maps +++++

            GameManager.Current.AddGameMap(ContentUtil.GetResourceStream("debug"));

            GameManager.Current.AddGameMap(ContentUtil.GetResourceStream("sanctuary"));
            GameManager.Current.AddGameMap(ContentUtil.GetResourceStream("lybianna"));
            GameManager.Current.AddGameMap(ContentUtil.GetResourceStream("tealglade_woods"));
            GameManager.Current.AddGameMap(ContentUtil.GetResourceStream("forest_temple"));
            GameManager.Current.AddGameMap(ContentUtil.GetResourceStream("great_cavern"));
            GameManager.Current.AddGameMap(ContentUtil.GetResourceStream("nethervault_temple"));
            GameManager.Current.AddGameMap(ContentUtil.GetResourceStream("mount_ascent"));
            GameManager.Current.AddGameMap(ContentUtil.GetResourceStream("pine_woods"));

            //GameManager.Current.AddGameMap("sanctuary");
            //GameManager.Current.AddGameMap("lybianna");
            //GameManager.Current.AddGameMap("tealglade_woods");
            //GameManager.Current.AddGameMap("forest_temple");
            //GameManager.Current.AddGameMap("great_cavern");
            //GameManager.Current.AddGameMap("nethervault_temple");
            //GameManager.Current.AddGameMap("pine_woods");

            HUD = new HUD();
            HUD.Texture = AssetManager.HUD;

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

            // creates camera!

            new RoomCamera(resolutionRenderer) { MaxZoom = 2f, MinZoom = .5f, Zoom = 1f };
            
            // adds room change events to game manager
            RoomCamera.Current.OnRoomChange += (sender, rooms) =>
            {
                GameManager.Current.ChangeRoom(rooms.Item1, rooms.Item2);
            };
            RoomCamera.Current.SetBackgrounds(AssetManager.Backgrounds);
            
            new TitleMenu(0, 0);
            State = GameState.InTitleMenu;
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

            Input.Update(gameTime);
            MouseState mouse = Mouse.GetState();


            // STATE-independent logic here:

            if (Properties.Settings.Default.IsDebugBuild)
            {
                if (InputMapping.KeyPressed(InputMapping.ResetLevel))
                {
                    GameManager.Current.ReloadLevel();
                }
            }

            if (InputMapping.KeyPressed(InputMapping.Pause))
            {
                if (State == GameState.Running)
                    State = GameState.Paused;
                else if (State == GameState.Paused)
                    State = GameState.Running;
            }

            // STATE-dependent logic here:

            if (State == GameState.Running)
            {

                // ++++ debug input ++++

                if (Properties.Settings.Default.IsDebugBuild)
                {

                    if (Input.IsKeyPressed(Keys.D0, InputManager.State.Pressed))
                    {
                        var posX = MathUtil.Div(GameManager.Current.Player.Position.X, Globals.T) * Globals.T + 8;
                        var posY = MathUtil.Div(GameManager.Current.Player.Position.Y, Globals.T) * Globals.T + 8;

                        GameManager.Current.Save(posX, posY, GameManager.Current.Map.Name);

                        Debug.WriteLine("Saved.");
                    }

                    if (Input.IsKeyPressed(Keys.L, InputManager.State.Pressed))
                    {
                        RoomCamera.Current.ChangeRoomsToPosition(new Vector2(2 * Globals.T - 8, 8 * Globals.T - 8), Objects.Effects.Transition.TransitionType.LIGHT, Direction.NONE, "debug", null);
                    }

                    //if (Input.IsKeyPressed(Keys.D5, Input.State.Pressed))
                    //{
                    //    RoomCamera.Current.Zoom -= .1f;
                    //    Debug.WriteLine(RoomCamera.Current.Zoom);
                    //}
                    //if (Input.IsKeyPressed(Keys.D6, Input.State.Pressed))
                    //{
                    //    RoomCamera.Current.Zoom += .1f;
                    //    Debug.WriteLine(RoomCamera.Current.Zoom);
                    //}

                    if (Input.IsKeyPressed(Keys.C, InputManager.State.Pressed))
                    {
                        var dialog = new MessageDialog("Delete save game?");
                        dialog.YesAction = () =>
                        {
                            GameManager.Current.SaveGame.Delete();
                            Debug.WriteLine("Deleted save game.");
                        };
                    }

                    if (GameManager.Current.Player != null)
                    {


                        if (Input.IsKeyPressed(Keys.H, InputManager.State.Pressed))
                        {
                            GameManager.Current.Player.Hit(1);
                            GameManager.Current.Player.HP++;
                        }

                        if (Input.IsKeyPressed(Keys.D9, InputManager.State.Pressed))
                        {
                            //stats.Abilities = PlayerAbility.NONE;

                            // add: flags |= flag
                            // remove: flags &= ~flag
                            // toggle: flags ^= flag

                            GameManager.Current.Player.Stats.Abilities |= PlayerAbility.DOUBLE_JUMP;

                            GameManager.Current.Player.Stats.Abilities |= PlayerAbility.ORB;
                            GameManager.Current.AddStoryFlag("hasOrb");

                            GameManager.Current.Player.Stats.Abilities |= PlayerAbility.PUSH;
                            GameManager.Current.Player.Stats.Abilities |= PlayerAbility.NO_FALL_DAMAGE;

                            //GameManager.Current.Player.Stats.Abilities |= PlayerAbility.LEVITATE;

                            GameManager.Current.Player.Stats.Abilities |= PlayerAbility.CLIMB_WALL;
                            GameManager.Current.Player.Stats.Abilities |= PlayerAbility.CLIMB_CEIL;
                            GameManager.Current.Player.Stats.Abilities |= PlayerAbility.ROLL;

                            GameManager.Current.AddSpell(SpellType.STAR);
                            GameManager.Current.AddSpell(SpellType.SNATCH_KEYS);
                            GameManager.Current.AddSpell(SpellType.CRIMSON_ARC);

                            GameManager.Current.AddSpell(SpellType.FIRE);

                            //GameManager.Current.RemoveSpell(SpellType.NONE);

                            GameManager.Current.AddSpell(SpellType.VOID);

                            //GameManager.Current.Player.Stats.MaxHP = 5;
                            //GameManager.Current.Player.Stats.MaxMP = 30;
                            //GameManager.Current.Player.Stats.MPRegen = .1f;
                            //GameManager.Current.Player.Stats.MaxHP = 14;
                            //GameManager.Current.Player.Stats.MaxMP = 40;
                            //GameManager.Current.Player.Stats.MPRegen = .4f;

                            Debug.WriteLine("Added abilities");
                        }

                        if (Input.IsKeyPressed(Keys.D8, InputManager.State.Pressed))
                        {
                            //GameManager.Current.Player.Stats.Abilities &= ~PlayerAbility.DOUBLE_JUMP;
                            //GameManager.Current.Player.Stats.Abilities &= ~PlayerAbility.PUSH;
                            //GameManager.Current.Player.Stats.Abilities &= ~PlayerAbility.NO_FALL_DAMAGE;

                            GameManager.Current.Player.Stats.Abilities &= ~PlayerAbility.LEVITATE;
                            GameManager.Current.Player.Stats.Abilities &= ~PlayerAbility.CLIMB_WALL;
                            GameManager.Current.Player.Stats.Abilities &= ~PlayerAbility.CLIMB_CEIL;
                            GameManager.Current.Player.Stats.Abilities &= ~PlayerAbility.ROLL;

                            //GameManager.Current.RemoveSpell(SpellType.VOID);
                            //GameManager.Current.RemoveSpell(SpellType.FIRE);
                            //GameManager.Current.AddSpell(SpellType.NONE);

                            Debug.WriteLine("Removed abilities");
                        }

                        if (Input.IsKeyPressed(Keys.O, InputManager.State.Pressed))
                        {
                            Coin.Spawn(GameManager.Current.Player.X, GameManager.Current.Player.Y, RoomCamera.Current.CurrentRoom, 2000);
                            Debug.WriteLine($"{ObjectManager.Count<Coin>()} coins exist. (Blocks: {ObjectManager.Count<Solid>()}, active: {ObjectManager.ActiveObjects.Count}, overall: {ObjectManager.Count<GameObject>()})");

                            GameManager.Current.Player.Stats.KeysAndKeyblocks.Clear();
                            GameManager.Current.Player.Stats.Items.Clear();
                            GameManager.Current.Player.Stats.Teleporters.Clear();
                            GameManager.Current.Player.Stats.StoryFlags.Clear();
                            GameManager.Current.Player.Stats.Bosses.Clear();
                            GameManager.Current.Player.Stats.ItemsBought.Clear();
                            GameManager.Current.Player.Stats.HeldKeys = new Dictionary<string, int>();

                            Debug.WriteLine("cleared saved lists!");
                        }

                        if (Input.IsKeyPressed(Keys.M, InputManager.State.Pressed))
                        {
                            var dialog = new MessageDialog("Do you like message dialogs?");

                            dialog.YesAction = () =>
                            {
                                Coin.Spawn(GameManager.Current.Player.X, GameManager.Current.Player.Y, RoomCamera.Current.CurrentRoom, 100);
                            };
                            dialog.NoAction = () =>
                            {
                                GameManager.Current.Player.HP = 0;
                            };
                        }

                        if (mouse.RightButton == ButtonState.Pressed)
                        {
                            var sep = RoomCamera.Current.ToVirtual(mouse.Position.ToVector2());
                            if (ObjectManager.CollisionPointFirstOrDefault<SpellEXP>(sep.X, sep.Y) == null)
                            {
                                SpellEXP.Spawn(sep.X, sep.Y, 11);
                            }
                        }

                        if (Input.IsKeyPressed(Keys.Space, InputManager.State.Holding))
                        {
                            ObjectManager.GameDelay = 120;
                            Debug.WriteLine("delay " + RND.Next);
                        }
                        else
                        {
                            ObjectManager.GameDelay = 0;
                        }

                        if (mouse.LeftButton == ButtonState.Pressed)
                        {
                            GameManager.Current.Player.Position = RoomCamera.Current.ToVirtual(mouse.Position.ToVector2());
                            GameManager.Current.Player.XVel = 0;
                            GameManager.Current.Player.YVel = 0;
                        }
                    }
                }
            }

            // ++++ HANDLES ALL OBJECTS ++++

            if (State != GameState.Paused)
            {
                GameManager.Current.Update(gameTime);
            }
            
            // ++++ update HUD ++++

            HUD.Update(gameTime);

            // ++++ update transition ++++
            if (State != GameState.Paused)
            {
                GameManager.Current.Transition?.Update(gameTime);
            }
        }
        
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // IMPORTANT HINT: when a texture's alpha is not "pretty", check the Content settings of that texture! Make sure that the texture has premultiplied : true.

            // ++++++++++++++++ darkness ++++++++++++++++
            
            Darkness.Current.PrepareDraw(spriteBatch);

            GraphicsDevice.SetRenderTarget(null);

            // ++++++++++++++++

            RoomCamera.Current.ResolutionRenderer.SetupDraw();
            
            // actual object drawing etc.

            spriteBatch.BeginCamera(RoomCamera.Current, BlendState.NonPremultiplied, DepthStencilState.None);

            Darkness.Current.Draw(spriteBatch, gameTime);

            RoomCamera.Current.Draw(spriteBatch, gameTime);

            var visibleRect = new Rectangle((int)RoomCamera.Current.ViewX, (int)RoomCamera.Current.ViewY, RoomCamera.Current.ViewWidth, RoomCamera.Current.ViewHeight);

            GameManager.Current.Map?.Draw(spriteBatch, gameTime, RoomCamera.Current);
            ObjectManager.DrawObjects(spriteBatch, gameTime, visibleRect);

            HUD.Draw(spriteBatch, gameTime);
            
            GameManager.Current.Transition?.Draw(spriteBatch, gameTime);
            
            spriteBatch.End();
        }
    }
}
