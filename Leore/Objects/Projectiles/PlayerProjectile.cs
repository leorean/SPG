using Microsoft.Xna.Framework;
using Leore.Main;
using Leore.Objects.Level;
using SPG.Objects;
using Leore.Objects.Level.Blocks;

namespace Leore.Objects.Projectiles
{
    /// <summary>
    /// Every spell should inherit from this object, else there are problems when holding a spell at the room border or while changing rooms
    /// </summary>
    public abstract class SpellObject : GameObject, IKeepEnabledAcrossRooms
    {
        public SpellObject(float x, float y, string name = null) : base(x, y, name)
        {
        }
    }

    public abstract class PlayerProjectile : SpellObject
    {
        public int Damage { get; set; } = 1;

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

            if (!(this is IceProjectile) && this.IsOutsideCurrentRoom(2 * Globals.T))
                Destroy();

        }

        public virtual void HandleCollisionFromDestroyBlock(DestroyBlock block)
        {
            if (block.Hit(Damage, Element))
            {
                HandleCollision(block);
            }
        }

        public abstract void HandleCollision(GameObject obj);
    }
}
