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
    public abstract class Item : RoomObject
    {
        // properties

        /// <summary>
        /// Only set this to true if you handle item destruction in the inheriting class
        /// </summary>
        public bool DestroyOnTaken { get; set; } = true;

        public bool Taken { get; set; }
        /// <summary>
        /// Set to true if the item should respawn after room change
        /// </summary>
        public bool Respawn { get; set; } = false;

        /// <summary>
        /// Set to true if the item should be saved and not respawn anymore once it's saved
        /// </summary>
        public bool Save { get; set; } = false;

        protected bool added;
        protected bool initialized;

        // constructor

        public Item(float x, float y, Room room, string name = null) : base(x, y, room, name)
        {
            DrawOffset = new Vector2(8, 8);
            BoundingBox = new SPG.Util.RectF(-4, -4, 8, 8);
            Depth = Globals.LAYER_ITEM;

            Visible = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!initialized)
            {
                if (Save && GameManager.Current.Player.Stats.Items.ContainsKey(ID))
                {
                    if (DestroyOnTaken)
                        Destroy();
                }
                else if (!Respawn && GameManager.Current.NonRespawnableIDs.Contains(ID))
                {
                    if (DestroyOnTaken)
                        Destroy();
                }

                initialized = true;
                return;
            }

            if (Taken && !added)
            {
                added = true;
                if (!Respawn)
                    GameManager.Current.NonRespawnableIDs.Add(ID);
                if (Save)
                    GameManager.Current.Player.Stats.Items.Add(ID, Name);
            }

            Visible = true;
        }

        // methods

        public abstract void Take(Player player);        
    }
}
