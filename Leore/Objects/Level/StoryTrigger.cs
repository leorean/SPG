using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leore.Main;
using Microsoft.Xna.Framework;
using SPG.Objects;

namespace Leore.Objects.Level
{
    public class StoryTrigger : RoomObject
    {
        private string setCondition;

        public StoryTrigger(float x, float y, Room room, string setCondition) : base(x, y, room)
        {
            this.setCondition = setCondition;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.CollisionBounds(GameManager.Current.Player, X, Y))
            {
                GameManager.Current.AddStoryFlag(setCondition);
                Destroy();
            }
        }
    }
}
