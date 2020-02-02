using Microsoft.Xna.Framework;
using Leore.Main;
using Leore.Objects.Level;
using SPG.Objects;
using Leore.Objects.Level.Blocks;
using Leore.Objects.Effects.Emitters;

namespace Leore.Objects.Projectiles
{
    public class RollDamageProjectile : PlayerProjectile
    {
        private Player player => GameManager.Current.Player;
        public Direction Direction { get; set; }

        public RollDamageProjectile(float x, float y) : base(x, y, SpellLevel.ONE)
        {
            Damage = 1;
            Element = SpellElement.ROLLDAMAGE;
            BoundingBox = new SPG.Util.RectF(-4, -4, 8, 8);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float roll_x = 0;
            float roll_y = 0;
            switch (Direction)
            {
                case Direction.LEFT:
                    roll_x = -1; roll_y = 0;
                    break;
                case Direction.RIGHT:
                    roll_x = 1; roll_y = 0;
                    break;
                case Direction.UP:
                    roll_x = 0; roll_y = -1;
                    break;
                case Direction.DOWN:
                    roll_x = 0; roll_y = 1;
                    break;
            }
            Position = player.Position + new Vector2(8 * roll_x, 8 * roll_y);
        }
        
        public override void HandleCollision(GameObject obj)
        {
            new StarEmitter(X, Y, 2, 0);

            switch (Direction)
            {
                case Direction.UP:
                case Direction.DOWN:
                    player.YVel *= -1f;
                    break;
                case Direction.LEFT:
                case Direction.RIGHT:
                    player.XVel *= -.5f;
                    break;
            }            
        }
    }
}
