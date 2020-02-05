using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leore.Main;
using Leore.Objects.Effects;
using Leore.Objects.Effects.Emitters;
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
                player.SetControlsEnabled(false);

                RoomCamera.Current.Shake(2 * 60, () =>
                  {
                      player.SetControlsEnabled(true);

                      Boss boss;

                      switch (type)
                      {
                          case 0:
                              new FlashEmitter(player.X, player.Y, longFlash: true);
                              boss = new BossMirrorSelf(Room.X + Room.BoundingBox.Width - (player.X - Room.X), player.Y, Room, setCondition);
                              boss.ID = ID;
                              break;
                          case 1:
                              new FlashEmitter(player.X, player.Y);
                              boss = new BossGiantBat(Room.X + 5 * Globals.T, Room.Y + 5 * Globals.T, Room, setCondition);
                              boss.ID = ID;
                              break;
                          case 2:
                              new FlashEmitter(player.X, player.Y, longFlash: true);
                              //boss = new BossGiantBat(Room.X + 5 * Globals.T, Room.Y + 5 * Globals.T, Room, setCondition);
                              //boss.ID = ID;
                              break;
                      }
                      Destroy();
                  });
            }
        }
    }
}
