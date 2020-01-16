using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Draw;
using SPG.Objects;
using SPG.Save;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leore.Main
{
    public class TitleMenu : GameObject
    {
        private RoomCamera camera => RoomCamera.Current;
        Vector2 position;

        private Font font = AssetManager.HUDFont;

        double t = 0;
        float z = 0;

        bool saveExists;
        
        public TitleMenu(float x, float y, string name = null) : base(x, y, name)
        {
            var saveGame = new SaveGame("save.dat");
            saveExists = SaveManager.Load(ref saveGame);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            position = new Vector2(camera.ViewX, camera.ViewY);
            t = (t + .025f);
            z = (float)(2 * Math.Sin(t));

            if (MainGame.Current.Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Enter, SPG.Input.State.Pressed))
            {
                // TODO: not reload
                GameManager.Current.ReloadLevel();

                MainGame.Current.State = MainGame.GameState.Running;
            }

            camera.SetTarget(this);
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);

            // BG
            sb.Draw(AssetManager.TitleMenu, position, new Rectangle(256, 0, 256, 144), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.0000f);

            // floaty thing
            sb.Draw(AssetManager.TitleMenu, position + new Vector2(0, z), new Rectangle(0, 0, 256, 144), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.0001f);

            // font
            font.Halign = Font.HorizontalAlignment.Left;
            font.Valign = Font.VerticalAlignment.Top;
            font.Draw(sb, position.X + 25, position.Y + camera.ViewHeight - 2 * Globals.T, saveExists ? $"Load Game" : "New Game", depth: .00003f);
            font.Draw(sb, position.X + 25, position.Y + camera.ViewHeight - 1 * Globals.T, "Delete Game", depth: .00003f);


        }
    }
}
