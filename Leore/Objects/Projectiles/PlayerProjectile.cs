using Microsoft.Xna.Framework;
using Leore.Main;
using Leore.Objects.Level;
using SPG.Objects;
using Leore.Objects.Level.Blocks;

namespace Leore.Objects.Projectiles
{
    public abstract class PlayerProjectile : SPG.Objects.GameObject
    {
        public int Damage { get; protected set; } = 1;

        public SpellElement Element { get; protected set; } = SpellElement.NONE;

        protected SpellLevel level;

        public PlayerProjectile(float x, float y, SpellLevel level) : base(x, y)
        {
            Depth = Globals.LAYER_PROJECTILE;
            this.level = level;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var destBlocks = this.CollisionBounds<DestroyBlock>(X + XVel, Y + YVel);

            foreach(var destBlock in destBlocks)
            {
                HandleCollisionFromDestroyBlock(destBlock);
            }

            if (this.IsOutsideCurrentRoom(2 * Globals.T))
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
