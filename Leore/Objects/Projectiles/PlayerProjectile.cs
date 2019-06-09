﻿using Microsoft.Xna.Framework;
using Leore.Main;
using Leore.Objects.Level;
using SPG.Objects;

namespace Leore.Objects.Projectiles
{
    public abstract class PlayerProjectile : SPG.Objects.GameObject
    {
        public int Damage { get; protected set; } = 1;

        protected SpellLevel level;

        public PlayerProjectile(float x, float y, SpellLevel level) : base(x, y)
        {
            Depth = Globals.LAYER_PROJECTILE;
            this.level = level;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var destBlock = this.CollisionPointFirstOrDefault<DestroyBlock>(X + XVel, Y + YVel);

            if (destBlock != null)
            {
                HandleCollisionFromDestroyBlock(destBlock);
            }

            if (this.IsOutsideCurrentRoom())
                Destroy();

        }

        public virtual void HandleCollisionFromDestroyBlock(DestroyBlock block)
        {
            block.Hit(Damage);
            HandleCollision(block);
        }

        public abstract void HandleCollision(GameObject obj);
    }
}