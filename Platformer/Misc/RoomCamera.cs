using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platformer.Objects;
using SPG;
using SPG.Map;
using SPG.Objects;
using SPG.Util;
using SPG.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Misc
{
    public class RoomCamera : Camera
    {

        public enum State
        {
            Default,
            PrepareRoomTransition,
            RoomTransition
        }

        private State state = State.Default;
        
        private int viewWidth;
        private int viewHeight;
        
        private GameObject target;
        
        private Room lastRoom;
        private Room currentRoom;
        
        private bool moveX = false;
        private bool moveY = false;

        float curX, curY;

        private float tx0, ty0;
        private float tx, ty;

        private float dirOffsetX = 0;

        private TextureSet _backgrounds;

        public RoomCamera(ResolutionRenderer resolutionRenderer) : base(resolutionRenderer) { }

        public void InitRoomData(List<Dictionary<string, object>> roomData)
        {
            try
            {
                viewWidth = ResolutionRenderer.ViewWidth;
                viewHeight = ResolutionRenderer.ViewHeight;
                
                foreach (var data in roomData)
                {
                    var x = (int)data["x"];
                    var y = (int)data["y"];

                    var w = data.ContainsKey("width") ? (int)data["width"] : viewWidth;
                    var h = data.ContainsKey("height") ? (int)data["height"] : viewHeight;
                    var bg = data.ContainsKey("bg") ? (int)data["bg"] : 0;

                    var room = new Room(x, y, w, h);
                    room.Background = bg;                    
                }
            }
            catch(Exception)
            {
                Debug.WriteLine("Unable to initialize room from data!");
                throw;
            }
        }

        public void SetTarget(GameObject target)
        {
            this.target = target;
            Position = target.Position;
        }

        public override void Update(GameTime gt)
        {
            base.Update(gt);

            if (ObjectManager.ElapsedTime < ObjectManager.GameSpeed)
                return;
            
            if (target == null)
                return;
            
            tx0 = MathUtil.Div(target.X, viewWidth) * viewWidth;
            ty0 = MathUtil.Div(target.Y, viewHeight) * viewHeight;

            // if no room is available, always resort to this state
            if (state == State.Default)
            {
                // if no room is yet found, try to find first room
                if (currentRoom == null)
                {
                    currentRoom = ObjectManager.CollisionPoint<Room>(target, target.X, target.Y).FirstOrDefault();
                    if (currentRoom != null)
                    {
                        lastRoom = currentRoom;
                        EnableBounds(new Rectangle((int)currentRoom.X, (int)currentRoom.Y, (int)currentRoom.BoundingBox.Width, (int)currentRoom.BoundingBox.Height));
                    }
                    else
                        return;
                }
                
                if (target is Player)
                {
                    var dir = (target as Player).Dir;

                    dirOffsetX += Math.Sign((int)dir) * 1f;
                    dirOffsetX = dirOffsetX.Clamp(-4 * Globals.TILE, 4 * Globals.TILE);
                }

                var tarX = Math.Min(Math.Max(target.X + dirOffsetX, currentRoom.X + .5f * viewWidth), currentRoom.X + currentRoom.BoundingBox.Width - .5f * viewWidth);
                var tarY = Math.Min(Math.Max(target.Y - Globals.TILE, currentRoom.Y + .5f * viewHeight), currentRoom.Y + currentRoom.BoundingBox.Height - .5f * viewHeight);

                var vel = new Vector2((tarX - Position.X) / 12f, (tarY - Position.Y) / 12f);

                Position = new Vector2(Position.X + vel.X, Position.Y + vel.Y);


                // if outside view, try to find new room
                //if (!MathUtil.In(target.X, bounds.X, bounds.X + bounds.Width)
                //        || !MathUtil.In(target.Y, bounds.Y, bounds.Y + bounds.Height))
                if (!MathUtil.In(target.X, currentRoom.X, currentRoom.X + currentRoom.BoundingBox.Width)
                        || !MathUtil.In(target.Y, currentRoom.Y, currentRoom.Y + currentRoom.BoundingBox.Height))
                {
                    lastRoom = currentRoom;
                    currentRoom = ObjectManager.CollisionPoint<Room>(target, target.X, target.Y).FirstOrDefault();

                    if (currentRoom != null)
                    {
                        dirOffsetX = 0;
                        state = State.PrepareRoomTransition;
                    }
                }
            }

            // prepare for transition
            if (state == State.PrepareRoomTransition)
            {
                DisableBounds();

                moveX = (currentRoom.X != lastRoom.X);
                moveY = (currentRoom.Y != lastRoom.Y);

                tx = Position.X;
                ty = Position.Y;

                curX = tx;
                curY = ty;

                state = State.RoomTransition;
            }

            // do transition until eligable for default state
            if (state == State.RoomTransition)
            {
                if (currentRoom == null)
                {
                    state = State.Default;
                    return;
                }

                if (!moveX)
                    tx = viewWidth * .5f + tx0;
                else
                {
                    tx = Math.Min(Math.Max(target.X, currentRoom.X + viewWidth * .5f), currentRoom.X + currentRoom.BoundingBox.Width - .5f * viewWidth + .5f);
                }

                if (!moveY)
                    ty = viewHeight * .5f + ty0;
                else
                {
                    ty = Math.Min(Math.Max(target.Y, currentRoom.Y + viewHeight * .5f), currentRoom.Y + currentRoom.BoundingBox.Height - .5f * viewHeight + .5f);
                }

                curX += (tx - curX) / 6f;
                curY += (ty - curY) / 6f;

                if (Math.Abs(curX - tx) < 1)
                    curX = tx;
                if (Math.Abs(curY - ty) < 1)
                    curY = ty;
                
                Vector2 t = new Vector2(curX, curY);                
                Position = t;

                if (curX == tx && curY == ty)
                {
                    state = State.Default;
                    //EnableBounds(new Rectangle((int)currentRoom.X, (int)currentRoom.Y, (int)currentRoom.BoundingBox.Width, (int)currentRoom.BoundingBox.Height));
                }
            }            
        }

        /// <summary>
        /// Loads the backgrounds to the camera so it can display them depending on the room background number.
        /// </summary>
        /// <param name="backgrounds"></param>
        public void SetBackgrounds(TextureSet backgrounds)
        {
            _backgrounds = backgrounds;
        }

        public void DrawBackground(GameTime gt)
        {
            if (_backgrounds != null)
            {
                if (currentRoom != null)
                {
                    GameManager.Game.SpriteBatch.Draw(_backgrounds[currentRoom.Background], Position - new Vector2(viewWidth * .5f, viewHeight *.5f), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                }
            }
        }
    }
}
