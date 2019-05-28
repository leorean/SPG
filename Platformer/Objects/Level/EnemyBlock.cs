using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Platformer.Objects.Effects;
using Platformer.Objects.Enemies;
using Platformer.Objects.Main;
using SPG.Objects;

namespace Platformer.Objects.Level
{
    public class EnemyBlock : Solid
    {
        public EnemyBlock(float x, float y, Room room) : base(x, y, room)
        {
            Texture = GameManager.Current.Map.TileSet[716];
            Visible = false;
        }

        public override void BeginUpdate(GameTime gameTime)
        {
            base.BeginUpdate(gameTime);

            if (this.IsOutsideCurrentRoom())
                Destroy();
            
            if (ObjectManager.Count<Enemy>() == 0)
            {
                //if (this.CollisionBounds(GameManager.Current.Player, X, Y))
                //    destroyTimer = 0;
                new SingularEffect(X + 8, Y + 8, 2);
                Destroy();
                
            }

            Visible = true;
        }
    }
}
