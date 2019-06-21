using Microsoft.Xna.Framework;
using System;
using System.Linq;
using SPG.Objects;
using Leore.Objects.Effects.Emitters;
using SPG.Map;
using Leore.Main;
using SPG.Util;
using Leore.Objects.Effects;

namespace Leore.Objects.Level
{

    // todo : move 

    public class Dummy : GameObject
    {
        public Dummy(float x, float y, string name = null) : base(x, y, name)
        {
        }
    }

    public class Key : Platform, IMovable
    {
        public bool Persistent { get; set; }

        private Player player { get => Parent as Player; }

        private bool stuck = false;

        private int timer = 0;
        private int maxTimer = 15 * 60;

        private GameObject originalPositionObject;

        public Collider MovingPlatform { get; set; }

        private int initialFallDelay = EnemyBlock.DELAY;
        private bool hasBeenTakenOnce;

        public Key(float x, float y, Room room) : base(x, y, room)
        {
            Depth = Globals.LAYER_PLAYER + .00001f;
            
            Texture = GameManager.Current.Map.TileSet[581];

            Visible = true;
            //DebugEnabled = true;

            DrawOffset = new Vector2(8, 8);
            BoundingBox = new SPG.Util.RectF(-4, -7.5f, 8, 15);

            Gravity = .1f;

            originalPositionObject = new Dummy(X, Y) { Parent = this };
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

            if (initialFallDelay > 0)
            {
                initialFallDelay--;
                return;
            }

            XVel *= .99f;

            var inWater = GameManager.Current.Map.CollisionTile(this, 0, -1, GameMap.WATER_INDEX);
            if (inWater)
            {
                YVel = Math.Max(YVel - Gravity - .01f, -1);
            }


            var onGround = this.MoveAdvanced(true);
            if (onGround)
            {
                XVel *= .5f;
            }

            if (stuck)
            {
                MoveTowards(GameManager.Current.Player, 8);
                
                //var col = this.CollisionBounds<Solid>(X, Y).FirstOrDefault();
                var col = this.CollisionRectangles<Solid>(Left, Top - 1, Right, Bottom + 1).FirstOrDefault();
                if (col == null)
                    stuck = false;
            }

            if (player != null)
            {
                MovingPlatform = null;
                XVel = 0;
                YVel = 0;
            }

            var t = 3;
            var keyBlock = this.CollisionRectangleFirstOrDefault<KeyBlock>(Left - t, Top - t, Right + t, Bottom + t);
            if (keyBlock != null)
            {
                if (!keyBlock.Unlocked)
                    Unlock(keyBlock);
            }

            var keyDoor = this.CollisionRectangleFirstOrDefault<DoorDisabler>(Left - t, Top - t, Right + t, Bottom + t);
            if (keyDoor != null && keyDoor.Type == DoorDisabler.TriggerType.Key && !keyDoor.Open)
            {
                TakeKeyAwayFromPlayer();
                keyDoor.Unlock(X, Y, false);
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

            if (player == null && hasBeenTakenOnce && MathUtil.Euclidean(Position, originalPositionObject.Position) > 2.5f)
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
                    new KeyBurstEmitter(Center.X, Center.Y, originalPositionObject);

                    Color = new Color(Color, 0);

                    Position = originalPositionObject.Position;
                }
            }
            else
            {
                timer = maxTimer;
                if (!ObjectManager.Exists<KeyBurstEmitter>())
                    Color = new Color(Color, 1f);
            }
        }

        private void TakeKeyAwayFromPlayer()
        {
            GameManager.Current.Player.Stats.KeysAndKeyblocks.Add(ID);
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

        public void Unlock(KeyBlock keyBlock)
        {
            TakeKeyAwayFromPlayer();
            keyBlock.Unlock(X, Y);
        }
        
        public void Take(Player player)
        {
            hasBeenTakenOnce = true;
            Parent = player;
            player.KeyObjectID = ID;            
        }

        public void Throw()
        {
            if (player == null)
                return;

            //var sideBlock = ObjectManager.CollisionPoint<Solid>(player.X + Math.Sign((int)player.Direction) * 8, player.Y).FirstOrDefault();
            //if (sideBlock != null)
            //{
            //    Position = new Vector2(X + Math.Sign((int)player.Direction) * 8, Y - 8);
            //}
            //else
            {
                XVel = Math.Sign((int)player.Direction) * 2;
                YVel = -1.75f;
            }
            var emitter = new Effects.Emitters.StarEmitter(X, Y);

            var col = this.CollisionRectangles<Solid>(Left, Top - 1, Right, Bottom + 1).FirstOrDefault();

            if (col != null)
                stuck = true;

            player.KeyObject = null;
            
            player.KeyObjectID = -1;
            Parent = null;
        }        
    }
}
