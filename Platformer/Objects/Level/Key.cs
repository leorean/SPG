using Microsoft.Xna.Framework;
using Platformer.Objects.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPG.Objects;
using Microsoft.Xna.Framework.Graphics;
using Platformer.Objects.Effects;
using Platformer.Objects.Effects.Emitters;

namespace Platformer.Objects.Level
{    
    public class Key : Platform
    {
        public bool Persistent { get; set; }

        private Player player { get => Parent as Player; }

        private bool stuck = false;

        private int timer = 0;
        private int maxTimer = 10 * 60;

        private Vector2 originalPosition;
        
        public Key(float x, float y, Room room) : base(x, y, room)
        {
            Depth = Globals.LAYER_PLAYER + .001f;

            Texture = GameManager.Current.Map.TileSet[581];

            Visible = true;

            DrawOffset = new Vector2(8, 8);
            BoundingBox = new SPG.Util.RectF(-4, -8, 8, 16);

            originalPosition = Position;
            timer = maxTimer;
        }

        public override void BeginUpdate(GameTime gameTime)
        {
            base.BeginUpdate(gameTime);

            // when re-entering room, destroy original object
            if (GameManager.Current.Player?.KeyObjectID == ID && player == null)
                Destroy();
            
            if (GameManager.Current.Player.Stats.KeysAndKeyblocks.Contains(ID))
                Destroy();                        
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            YVel += .1f;
            XVel *= .95f;
            
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
                XVel *= .5f;
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

            if (player != null)
            {
                XVel = 0;
                YVel = 0;
            }

            var t = 3;
            var keyBlock = this.CollisionRectangle<KeyBlock>(Left - t, Top - t, Right + t, Bottom + t).FirstOrDefault();
            if (keyBlock != null)
            {
                Unlock(keyBlock);
            }                
        }

        public override void EndUpdate(GameTime gameTime)
        {
            base.EndUpdate(gameTime);

            if (player != null)
            {
                Position = player.Position + new Vector2(0 * Math.Sign((int)player.Direction), -12);
            }

            // timer stuff

            if (player == null && (Math.Abs(X - originalPosition.X) > 2 || Math.Abs(Y - originalPosition.Y) > 2))
            {
                timer = Math.Max(timer - 1, 0);

                var a = 1f;
                if (timer < .5f * maxTimer)
                    a = ((timer % 8) < 4) ? 1f : .5f;
                if (timer < .25f * maxTimer)
                    a = ((timer % 4) < 2) ? 1f : .5f;
                Color = new Color(Color, a);

                if (timer == 0)
                {                    
                    new KeyBurstEmitter(Center.X, Center.Y, originalPosition);

                    Color = new Color(Color, 0);

                    Position = originalPosition;
                }
            }
            else
            {
                timer = maxTimer;
                if (!ObjectManager.Exists<KeyBurstEmitter>())
                    Color = new Color(Color, 1f);
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

            if (player != null)
            {
                if (player.State == Player.PlayerState.CARRYOBJECT_IDLE
                    || player.State == Player.PlayerState.CARRYOBJECT_TAKE
                    || player.State == Player.PlayerState.CARRYOBJECT_WALK
                    || player.State == Player.PlayerState.CARRYOBJECT_THROW)
                {
                    player.State = Player.PlayerState.IDLE;
                }

                player.KeyObject = null;
                player.KeyObjectID = -1;
                Parent = null;
            }
        }
        
        public void Take(Player player)
        {
            Parent = player;
            player.KeyObjectID = ID;            
        }

        public void Throw()
        {
            if (player == null)
                return;

            //Position = new Vector2(X + Math.Sign((int)player.Direction) * Globals.TILE, player.Y - Globals.TILE);

            XVel = Math.Sign((int)player.Direction) * 2;
            YVel = -2;

            var emitter = new Effects.Emitters.SaveBurstEmitter(X, Y);

            var col = this.CollisionBounds<Solid>(X + XVel, Y + YVel).FirstOrDefault();

            if (col != null)
                stuck = true;

            player.KeyObject = null;
            
            player.KeyObjectID = -1;
            Parent = null;
        }        
    }
}
