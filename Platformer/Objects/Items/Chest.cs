﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Platformer.Objects.Effects.Emitters;
using Platformer.Objects.Items;
using Platformer.Objects.Main;

namespace Platformer.Objects.Items
{
    public class Chest : Item
    {
        private float value;

        public Chest(float x, float y, Room room, float value) : base(x, y, room)
        {
            this.value = value;
            Visible = false;

            BoundingBox = new SPG.Util.RectF(0, 0, 16, 16);
            DrawOffset = Vector2.Zero;

            Depth = Globals.LAYER_BG + 0.0001f;

            Respawn = true;
            Save = true;
            DestroyOnTaken = false;
        }
        
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (GameManager.Current.Player.Stats.Items.ContainsKey(ID))
            {
                added = true;
                Taken = true;
            }

            Texture = AssetManager.Chest[Convert.ToInt32(Taken)];

            Visible = true;
        }

        public override void Take(Player player)
        {
            if (Taken)
                return;

            new OuchEmitter(X + 8, Y + 8);            
            Coin.Spawn(X + 8, Y + 8, Room, value);

            Taken = true;
        }        
    }
}