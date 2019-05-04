using Microsoft.Xna.Framework;
using Platformer.Objects.Main;
using SPG.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Items
{
    public class AbilityItem : Item
    {
        public PlayerAbility Ability { get; private set; }

        private bool taken = false;

        public AbilityItem(float x, float y, Room room, PlayerAbility ability, string name = null) : base(x, y, room, name)
        {
            Ability = ability;
            Visible = false;
            DebugEnabled = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (GameManager.Current.Player.Stats.Abilities.HasFlag(Ability))
            {
                Destroy();
            }

            Visible = true;

            if (!taken)
            {
                var player = this.CollisionBounds<Player>(X, Y).FirstOrDefault();

                if (player != null)
                {
                    player.State = Player.PlayerState.OBTAIN;
                }
            }
        }

        public override void Take()
        {
            // save to item list so it won't respawn
        }
    }
}
