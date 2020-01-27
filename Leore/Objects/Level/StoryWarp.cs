using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leore.Main;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Objects;
using SPG.Util;
using static Leore.Objects.Effects.Transition;

namespace Leore.Objects.Level
{
    public class StoryWarp : RoomObject
    {
        Player player => GameManager.Current.Player;
        string setCondition;
        int tx;
        int ty;
        string text;

        float alpha = 0;
        float maxAlpha = 1.5f;
        bool touched;

        float warpY;

        public StoryWarp(float x, float y, Room room, string setCondition, int tx, int ty, string text) : base(x, y, room)
        {
            BoundingBox = new RectF(-2, -8, 4, 16);
            DebugEnabled = true;

            this.setCondition = setCondition;
            this.tx = tx;
            this.ty = ty;
            this.text = text;

            warpY = Y;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (GameManager.Current.HasStoryFlag(setCondition))
                return;

            if (!touched)
            {
                if (this.CollisionBounds(player, X, Y))
                {
                    touched = true;                    
                    player.SetControlsEnabled(false);
                    MainGame.Current.HUD.SetVisible(false);
                }
            }
            else
            {
                player.State = Player.PlayerState.JUMP_UP;
                player.XVel = (X - player.X) / 60f;
                player.YVel = (warpY - player.Y) / 60f;

                warpY = Math.Max(warpY - .5f, Y - 48);

                //if (player.Y > Y - 16)
                //{
                //    player.YVel = -player.Gravity - .1f;
                //}
                //player.Position = new Vector2(player.X, Center.Y - 1);

                alpha = Math.Min(alpha + .005f, maxAlpha);
                if (alpha == maxAlpha)
                {
                    player.SetControlsEnabled(true);
                    GameManager.Current.AddStoryFlag(setCondition);
                    new MessageBox(text).OnCompleted = Complete;
                }
            }   
        }

        void Complete()
        {
            var px = MathUtil.Div(X, Globals.T) * Globals.T + 8;
            var py = MathUtil.Div(Y, Globals.T) * Globals.T + 8;
            var pos = new Vector2(px + tx * Globals.T, py + ty * Globals.T);
            RoomCamera.Current.ChangeRoomsToPosition(pos, TransitionType.LIGHT_FLASH_LONG_FADE, GameManager.Current.Player.Direction, null);            
            Destroy();
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);

            Color color = new Color(Color, alpha);
            sb.Draw(AssetManager.Transition[1], new Vector2(RoomCamera.Current.ViewX, RoomCamera.Current.ViewY), null, color, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, Globals.LAYER_FONT - .005f);
        }
    }
}
