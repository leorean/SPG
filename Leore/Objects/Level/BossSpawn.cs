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
        private string appearCondition;
        private string setCondition;

        public BossSpawn(float x, float y, Room room, int type, string appearCondition, string setCondition) : base(x, y, room)
        {
            this.type = type;
            this.appearCondition = appearCondition;
            this.setCondition = setCondition;
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

            if (this.CollisionBounds(player, X, Y) && GameManager.Current.HasStoryFlag(appearCondition))
            {
                Boss boss;

                switch (type)
                {
                    case 0:
                        boss = new BossMirrorSelf(Room.X + Room.BoundingBox.Width - (player.X - Room.X), player.Y, Room, setCondition);
                        boss.ID = ID;
                        break;
                    case 1:
                        boss = new BossGiantBat(Room.X + 5 * Globals.T, Room.Y + 5 * Globals.T, Room, setCondition);
                        boss.ID = ID;
                        break;
                }                
                Destroy();
            }
        }
    }
}
