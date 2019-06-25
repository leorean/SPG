using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Leore.Main;
using Leore.Objects.Effects.Emitters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPG.Draw;
using SPG.Util;
using Leore.Objects.Effects.Weather;

namespace Leore.Objects.Level
{
    public class Teleporter : RoomObject
    {
        public bool Active { get; set; }

        private ObtainParticleEmitter particleEmitter;
        
        private List<Texture2D> segment;
        private int segIndex;
        private float dist;
        private float angSpd;
        private float angle;
        private bool wasOpen;
        private bool closedOnce;
        private float backAlpha = 0;
        private float segAlpha = 1f;

        private float depthBehind;
        private float depthInFront;

        private float angleOffset1, angleOffset2, angleOffset3;

        public Action OnFinishedAnimation { get; set; }

        public Teleporter(float x, float y, Room room) : base(x, y, room)
        {
            segment = new List<Texture2D> {
                AssetManager.Teleporter.Crop(new Rectangle(0, 0, 32, 32)),
                AssetManager.Teleporter.Crop(new Rectangle(32, 0, 32, 32)),
                AssetManager.Teleporter.Crop(new Rectangle(64, 0, 32, 32))
            };

            //Depth = Globals.LAYER_FG - 0.0001f;

            DrawOffset = new Vector2(32);
            BoundingBox = new SPG.Util.RectF(-32, -32, 64, 64);
            
            particleEmitter = new ObtainParticleEmitter(X, Y, 0);
            particleEmitter.Parent = this;
            particleEmitter.Active = false;
            
            Reset();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            depthBehind = GameManager.Current.Player.Depth - .00005f;
            depthInFront = GameManager.Current.Player.Depth + .00005f;

            angle += angSpd;

            if (!GameManager.Current.Player.Stats.Teleporters.ContainsKey(ID))
            {
                //segAlpha = .25f;
                segAlpha = 0;
                angle -= 2f * angSpd;
                dist = 24;

                angleOffset1 = 180;
                angleOffset2 = 180;
                angleOffset3 = 180;
            }
            else
            {
                angleOffset1 *= .9f;
                angleOffset2 *= .9f;
                angleOffset3 *= .9f;

                if (!closedOnce)
                {
                    dist = Math.Max(dist - .5f, 0);
                    if (dist == 0)
                        closedOnce = true;
                }
                segAlpha = Math.Min(segAlpha + .02f, 1);
            }

            if (Active)
            {
                segIndex = 1;

                if (closedOnce)
                {

                    if (!wasOpen)
                    {
                        particleEmitter.Active = true;

                        Depth = depthBehind;

                        if (closedOnce)
                            dist = Math.Min(dist + .1f, 16);
                        angSpd = Math.Min(angSpd + .04f, 10);

                        if (dist == 16)
                        {
                            wasOpen = true;
                        }
                    }
                    else
                    {
                        if (GameManager.Current.Player.Orb != null) GameManager.Current.Player.Orb.Visible = false;
                        particleEmitter.Active = false;

                        Depth = depthInFront;
                        dist = Math.Max(dist - 1f, 0);
                        angSpd = Math.Max(angSpd - .1f, 0);

                        if (angSpd == 0 && dist == 0)
                        {
                            OnFinishedAnimation?.Invoke();
                            Active = false;
                        }
                    }
                }
                backAlpha = Math.Min(backAlpha + .004f, 1);

                //if (!GameManager.Current.Player.Stats.Teleporters.ContainsKey(ID))
                //    GameManager.Current.Player.Stats.Teleporters.Add(ID, Position.ToPoint());
            } else
            {
                if (!wasOpen)
                    Depth = depthBehind;
            }

            angle = angle % 360;
        }

        public void Reset()
        {
            Darkness.Current.Enable();

            segIndex = 0;
            angSpd = -.5f;
            wasOpen = false;
            if (GameManager.Current.Player != null)
                Depth = depthBehind;
            angle = 0;
            backAlpha = 0;
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);

            var ang1 = (float)MathUtil.DegToRad(angle + 000 + angleOffset1);
            var ang2 = (float)MathUtil.DegToRad(angle + 120 + angleOffset2);
            var ang3 = (float)MathUtil.DegToRad(angle + 240 + angleOffset3);

            var d1 = new Vector2((float)MathUtil.LengthDirX(angle + 000 - 90) * dist, (float)MathUtil.LengthDirY(angle - 000 - 90) * dist);
            var d2 = new Vector2((float)MathUtil.LengthDirX(angle + 120 - 90) * dist, (float)MathUtil.LengthDirY(angle + 120 - 90) * dist);
            var d3 = new Vector2((float)MathUtil.LengthDirX(angle + 240 - 90) * dist, (float)MathUtil.LengthDirY(angle + 240 - 90) * dist);

            sb.Draw(segment[segIndex], Position + d1, null, new Color(Color, segAlpha), ang1, new Vector2(16), Scale, SpriteEffects.None, Depth + .00001f);
            sb.Draw(segment[segIndex], Position + d2, null, new Color(Color, segAlpha), ang2, new Vector2(16), Scale, SpriteEffects.None, Depth + .00002f);
            sb.Draw(segment[segIndex], Position + d3, null, new Color(Color, segAlpha), ang3, new Vector2(16), Scale, SpriteEffects.None, Depth + .00003f);
            
            sb.Draw(segment[2], Position + d1, null, new Color(Color, 1f - segAlpha), ang1, new Vector2(16), Scale, SpriteEffects.None, Depth + .000011f);
            sb.Draw(segment[2], Position + d2, null, new Color(Color, 1f - segAlpha), ang2, new Vector2(16), Scale, SpriteEffects.None, Depth + .000021f);
            sb.Draw(segment[2], Position + d3, null, new Color(Color, 1f - segAlpha), ang3, new Vector2(16), Scale, SpriteEffects.None, Depth + .000031f);

            var bgdepth = Globals.LAYER_FG + .0001f;
            sb.Draw(AssetManager.Transition[0], new Vector2(RoomCamera.Current.ViewX, RoomCamera.Current.ViewY), null, new Color(Color.White, backAlpha), 0, Vector2.Zero, Vector2.One, SpriteEffects.None, bgdepth);

            if (backAlpha > 0 && closedOnce)
            {
                var c1 = dist / 16f;
                var c2 = dist / 20f;
                var c3 = dist / 32f;

                sb.Draw(AssetManager.WhiteCircle, Position, null, new Color(Color.Black, backAlpha), 0, new Vector2(32), Math.Min(c1, .9f), SpriteEffects.None, bgdepth + .0001f);
                sb.Draw(AssetManager.WhiteCircle, Position, null, new Color(Color.Black, backAlpha), 0, new Vector2(32), c2, SpriteEffects.None, bgdepth + .0002f);
                sb.Draw(AssetManager.WhiteCircle, Position, null, new Color(Color.Black, backAlpha + .3f), 0, new Vector2(32), c3, SpriteEffects.None, bgdepth + .0003f);
            }
        }
    }
}
