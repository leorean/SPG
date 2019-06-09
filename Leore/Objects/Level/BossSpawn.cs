using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leore.Main;
using Leore.Objects.Enemies;
using Microsoft.Xna.Framework;
using SPG.Objects;

namespace Leore.Objects.Level
{
    public class BossSpawn : RoomObject
    {
        private int type;
        public BossSpawn(float x, float y, Room room, int type) : base(x, y, room)
        {
            this.type = type;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            var player = GameManager.Current.Player;

            if (player.Stats.Bosses.Contains(ID))
            {
                Destroy();
                return;
            }

            if (this.CollisionBounds(player, X, Y))
            {
                Boss boss;

                switch (type)
                {
                    case 0:
                        if (player.Orb != null)
                        {
                            boss = new BossMirrorSelf(Room.X + Room.BoundingBox.Width - (player.X - Room.X), player.Y, Room);
                            boss.ID = ID;
                        }
                        break;
                }
                Destroy();
            }
        }
    }
}
