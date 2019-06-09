using System;
using Microsoft.Xna.Framework;
using Leore.Main;
using Leore.Objects.Effects;
using Leore.Objects.Enemies;
using SPG.Objects;

namespace Leore.Objects.Level
{
    public class EnemyBlock : RoomObject
    {
        private Solid block;

        public bool Active { get; private set; }

        private int activeTimer = 30;

        private bool activatedOnce;

        public EnemyBlock(float x, float y, Room room) : base(x, y, room)
        {
            Texture = GameManager.Current.Map.TileSet[716];
            Visible = false;
            Depth = Globals.LAYER_FG;            
        }

        public override void BeginUpdate(GameTime gameTime)
        {
            base.BeginUpdate(gameTime);

            if (this.IsOutsideCurrentRoom())
                Destroy();

            if (Active && this.CollisionBounds(GameManager.Current.Player, X, Y))
            {
                activeTimer = 60;
                Active = false;
                activatedOnce = false;
            }
            
            if (ObjectManager.Count<Enemy>() == 0 || !activatedOnce)
            {
                //if (!this.CollisionBounds(GameManager.Current.Player, X, Y))
                //if (MathUtil.Euclidean(Center, GameManager.Current.Player.Center) > 1f * Globals.TILE)
                if (!ObjectManager.CollisionRectangle(GameManager.Current.Player, Left - 2, Top - 8, Right + 2, Bottom + 8))
                    activeTimer = Math.Max(activeTimer - 1, 0);
                if (ObjectManager.Exists<Boss>())
                    activeTimer = 0;
            }

            if (Active && block == null)
            {
                block = new Solid(X, Y, Room);
            }
            if (!Active && block != null)
            {
                block.Destroy();
                block = null;
            }

            if (activeTimer == 0)
            {
                if (Active && ObjectManager.Count<Enemy>() == 0)
                {
                    new SingularEffect(X + 8, Y + 8, 2);
                    Active = false;
                }
                else if (!Active && ObjectManager.Count<Enemy>() > 0)
                {
                    activatedOnce = true;
                    new SingularEffect(X + 8, Y + 8, 0);
                    Active = true;
                    activeTimer = 60;
                }
            }

            Visible = Active;
        }
    }
}
