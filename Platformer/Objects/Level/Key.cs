using Microsoft.Xna.Framework;
using Platformer.Objects.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPG.Objects;

namespace Platformer.Objects.Level
{    
    public class Key : Platform
    {
        public bool Persistent { get; set; }

        private Player player { get => Parent as Player; }

        private bool stuck = false;
        
        public Key(float x, float y, Room room) : base(x, y, room)
        {
            Depth = Globals.LAYER_PLAYER + .001f;

            Texture = GameManager.Current.Map.TileSet[581];

            Visible = true;
            
            DrawOffset = new Vector2(8, 8);
            BoundingBox = new SPG.Util.RectF(-4, -8, 8, 16);
            
        }

        public override void BeginUpdate(GameTime gameTime)
        {
            base.BeginUpdate(gameTime);

            if (GameManager.Current.Player?.Stats?.KeyObjectID == ID && player == null)
                Destroy();

            if (GameManager.Current.Player.Stats.KeysAndKeyblocks.Contains(ID))
                Destroy();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            YVel += .1f;
            if (player != null)
            {
                XVel = 0;
                YVel = 0;
                Position = new Vector2(player.X, player.Y - Globals.TILE);
            }

            var colX = this.CollisionBounds<Solid>(X + XVel, Y).FirstOrDefault();

            if (colX == null)
            {
                Move(XVel, 0);
            } else
            {
                XVel = 0;
            }

            var colY = this.CollisionBounds<Solid>(X, Y + YVel).FirstOrDefault();

            if (colY == null)
            {
                Move(0, YVel);
            }
            else
            {
                YVel = 0;
            }

            if (stuck)
            {
                var tx = (GameManager.Current.Player.X - X) / 8f;
                var ty = (GameManager.Current.Player.Y - Y) / 8f;
                Move(tx, ty);

                var col = this.CollisionBounds<Solid>(X, Y).FirstOrDefault();
                if (col == null)
                    stuck = false;
            }            
        }

        public void Unlock(KeyBlock keyBlock)
        {
            GameManager.Current.Player.Stats.KeysAndKeyblocks.Add(ID);

            var emitter = new Effects.Emitters.KeyBurstEmitter(X, Y, keyBlock.Center);
            emitter.OnFinishedAction = () =>
            {
                GameManager.Current.Player.Stats.KeysAndKeyblocks.Add((keyBlock as GameObject).ID);

                new Effects.SingularEffect(keyBlock.X + 8, keyBlock.Y + 8, 2);
                new Effects.Emitters.OuchEmitter(keyBlock.X + 8, keyBlock.Y + 8);
            };
                        
            player.Stats.KeyObjectID = -1;
            Parent = null;
        }

        public void Take(Player player)
        {
            Parent = player;
            player.Stats.KeyObjectID = ID;
        }

        public void Throw()
        {
            if (player == null)
                return;

            Position = new Vector2(X + Math.Sign((int)player.Direction) * Globals.TILE, player.Y - Globals.TILE);
            var emitter = new Effects.Emitters.SaveBurstEmitter(X, Y);

            var col = this.CollisionBounds<Solid>(X + XVel, Y + YVel).FirstOrDefault();

            if (col != null)
                stuck = true;

            player.KeyObject = null;

            player.Stats.KeyObjectID = -1;
            Parent = null;
        }        
    }
}
