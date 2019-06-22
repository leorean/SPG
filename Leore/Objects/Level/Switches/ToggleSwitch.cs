﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leore.Main;
using Leore.Objects.Effects;
using Leore.Objects.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Objects;

namespace Leore.Objects.Level.Switches
{
    public class ToggleSwitch : RoomObject
    {
        public bool Active { get; set; }

        private bool active;
        private bool defaultOn;

        private PlayerProjectile lastProj;

        float alpha;
        float z;

        public ToggleSwitch(float x, float y, Room room, bool defaultOn) : base(x, y, room)
        {
            this.defaultOn = defaultOn;

            Depth = Globals.LAYER_BG + .0001f;

            DrawOffset = new Vector2(8);
            BoundingBox = new SPG.Util.RectF(-8, -8, 16, 16);            
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            //if (prepared)
            {
                var proj = this.CollisionBoundsFirstOrDefault<PlayerProjectile>(X, Y);
                if (proj != null && lastProj != proj)
                {
                    new SingularEffect(Center.X, Center.Y, 10);

                    proj.HandleCollision(this);
                    lastProj = proj;
                    active = !active;
                    //prepared = false;
                }
            }

            Active = defaultOn ? active : !active;

            if (!active) z = Math.Max(z - .1f, 0);
            else
                z = Math.Min(z + .1f, 4);

            alpha = z / 4;

        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);

            //var frame = active ? 1 : 0;

            sb.Draw(AssetManager.ToggleSwitch[1], Position + new Vector2(0, z), null, new Color(Color, alpha), Angle, DrawOffset, Scale, SpriteEffects.None, Depth);
            sb.Draw(AssetManager.ToggleSwitch[0], Position + new Vector2(0, z), null, new Color(Color, 1 - alpha), Angle, DrawOffset, Scale, SpriteEffects.None, Depth + .0001f);

            sb.Draw(AssetManager.ToggleSwitch[2], Position, null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth + .0002f);
        }
    }
}
