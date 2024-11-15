﻿using System;
using Microsoft.Xna.Framework;
using Leore.Main;
using SPG.Objects;

namespace Leore.Objects.Effects
{
    public class ToolTip : GameObject
    {
        private double alpha = 0;

        private Player player;
        private GameObject obj;

        public int Type { get; set; }

        private Vector2 collisionOffset;

        public ToolTip(GameObject obj, Player player, Vector2? offset = null, int type = 0) : base(0, 0)
        {
            Depth = Globals.LAYER_EFFECT;
            DrawOffset = new Vector2(16, 16);

            AnimationTexture = AssetManager.ToolTip;

            Visible = false;

            Type = type;
            this.player = player;
            this.obj = obj;
            collisionOffset = offset != null ? (Vector2)offset : Vector2.Zero;
            
            if (ObjectManager.Count<ToolTip>() > 1)
                Destroy();
        }
        
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            Position = player.Position + new Vector2(0, -16);

            Visible = true;
            
            var cols = 4; // how many columns there are in the sheet
            var fSpd = .1f; // frame speed
            
            SetAnimation(cols * Type, cols * Type + 2, fSpd, true);

            if (player.CollisionBounds(obj, player.Center.X + collisionOffset.X, player.Center.Y + collisionOffset.Y))
            {
                alpha = Math.Min(alpha + .02f, .5f);
            } else
            {
                alpha = Math.Max(alpha - .03f, 0);
                if (alpha == 0)
                    Destroy();
            }

            if (ObjectManager.Exists<MessageBox>())
                Destroy();

            Color = new Color(Color, (float)alpha);
        }
    }
}
