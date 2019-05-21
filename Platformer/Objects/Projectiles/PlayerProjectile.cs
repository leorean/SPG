﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Platformer.Main;

namespace Platformer.Objects.Projectiles
{
    public abstract class PlayerProjectile : SPG.Objects.GameObject
    {
        public int Damage { get; protected set; } = 1;

        public PlayerProjectile(float x, float y) : base(x, y)
        {
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.IsOutsideCurrentRoom())
                Destroy();

        }

        public abstract void Kill();
    }
}
