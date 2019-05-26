﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platformer.Objects.Effects;
using Platformer.Objects.Main;
using SPG.Objects;
using SPG.Util;

namespace Platformer.Objects.Level
{
    public class NPC : RoomObject
    {
        private string text;
        private string yesText;
        private string noText;

        private int type;

        private Direction direction;

        private Player player;

        private bool centerText;
        private bool lookAtPlayer;

        protected bool decision;
        
        public NPC(float x, float y, Room room, int type, string text, bool centerText = false, Direction dir = Direction.NONE, string name = null, string yesText = null, string noText = null) : base(x, y, room, name)
        {
            this.text = text;
            this.type = type;

            this.yesText = yesText;
            this.noText = noText;

            AnimationTexture = AssetManager.NPCS;
            
            DrawOffset = new Vector2(8, 24);            
            BoundingBox = new RectF(-8, -8, 16, 16);
            Depth = Globals.LAYER_PLAYER - 0.001f;

            if (dir == Direction.NONE)
            {
                direction = Direction.RIGHT;
                lookAtPlayer = true;
            } else
            {
                direction = dir;
            }

            this.centerText = centerText;            
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            //

            var lookDir = GameManager.Current.Player.X < X ? Direction.LEFT : Direction.RIGHT;

            if (lookAtPlayer)
                direction = lookDir;
            
            //

            if (player != null)
            {
                player.Direction = (Direction)(-(int)lookDir);

                var tx = (X - player.X + Math.Sign((int)lookDir) * 8) / 20f;
                player.XVel = tx;

                if (Math.Abs(X - player.X) < 1f)
                    player.XVel = -1;
            }

            
            var xScale = Math.Sign((int)direction);
            Scale = new Vector2(xScale, 1);

            // ++++ draw <-> state logic ++++

            var cols = 4; // how many columns there are in the sheet            
            var fSpd = .04f; // frame speed
            var fAmount = 2; // how many frames
            var loopAnim = true; // loop animation?

            switch (type)
            {
                case 0: // signs
                    fAmount = 1;
                    fSpd = 0;
                    Scale = Vector2.One;
                    break;
                case 1:
                    break;
            }

            SetAnimation(cols * type, cols * type + fAmount - 1, fSpd, loopAnim);            
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);            
        }

        public virtual void Interact(Player player)
        {
            this.player = player;

            this.player.State = Player.PlayerState.BACKFACING;

            if (yesText == null || noText == null)
            {
                var msgBox = new MessageBox(text, centerText, "");
                msgBox.OnCompleted = EndOfConversation;
            } else
            {
                var dialog = new MessageDialog(text, centerText, "");

                dialog.YesAction = () =>
                {
                    decision = true;
                    var msgBox = new MessageBox(yesText, centerText, "");                    
                    msgBox.OnCompleted = EndOfConversation;
                };
                dialog.NoAction = () =>
                {
                    decision = false;
                    var msgBox = new MessageBox(noText, centerText, "");
                    msgBox.OnCompleted = EndOfConversation;
                };                
            }
        }

        public virtual void EndOfConversation()
        {
            this.player.State = Player.PlayerState.IDLE;
            this.player = null;
        }

        internal void ShowToolTip(Player player)
        {
            var toolTip = new ToolTip(this, player, type: 0);
        }
    }
}
