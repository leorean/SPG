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
    public abstract class SpellObject : GameObject, IKeepEnabledAcrossRooms, IKeepAliveBetweenRooms
    {
        protected SpellLevel level;

        public SpellObject(float x, float y, SpellLevel level) : base(x, y)
        {
            this.level = level;
        }
    }

    public abstract class PlayerProjectile : SpellObject
    {
        public int Damage { get; set; } = 1;

        public SpellElement Element { get; protected set; } = SpellElement.NONE;
        
        public PlayerProjectile(float x, float y, SpellLevel level) : base(x, y, level)
        {
            Depth = Globals.LAYER_PROJECTILE;            
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var destBlocks = this.CollisionBounds<DestroyBlock>(X + XVel, Y + YVel);

            foreach(var destBlock in destBlocks)
            {
                HandleCollisionFromDestroyBlock(destBlock);
            }

            //if (!(this is IKeepAliveBetweenRooms) && this.IsOutsideCurrentRoom(2 * Globals.T))
            //    Destroy();

        }

        public virtual void HandleCollisionFromDestroyBlock(DestroyBlock block)
        {
            if (block.Hit(this))
            {
                HandleCollision(block);
            }
        }

        public abstract void HandleCollision(GameObject obj);
    }
}
