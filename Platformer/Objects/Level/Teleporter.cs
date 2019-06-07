using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platformer.Main;
using Platformer.Objects.Effects.Emitters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPG.Draw;
using SPG.Util;

namespace Platformer.Objects.Level
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
        private float backAlpha = 0;

        public Action OnFinishedAnimation { get; set; }

        public Teleporter(float x, float y, Room room) : base(x, y, room)
        {
            segment = new List<Texture2D> { AssetManager.Teleporter.Crop(new Rectangle(0, 0, 32, 32)), AssetManager.Teleporter.Crop(new Rectangle(32, 0, 32, 32)) };

            Depth = Globals.LAYER_FG - 0.0001f;

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
            
            angle += angSpd;

            if (Active)
            {
                segIndex = 1;

                if (!wasOpen)
                {
                    particleEmitter.Active = true;

                    Depth = GameManager.Current.Player.Depth - .0005f;

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

                    Depth = GameManager.Current.Player.Depth + .0005f;
                    dist = Math.Max(dist - 1f, 0);
                    angSpd = Math.Max(angSpd - .1f, 0);

                    if (angSpd == 0 && dist == 0)
                    {
                        OnFinishedAnimation?.Invoke();
                        Active = false;
                    }
                }

                backAlpha = Math.Min(backAlpha + .004f, 1);

                if (!GameManager.Current.Player.Stats.Teleporters.ContainsKey(ID))
                    GameManager.Current.Player.Stats.Teleporters.Add(ID, Position.ToPoint());
            }

            angle = angle % 360;
        }

        public void Reset()
        {
            segIndex = 0;
            angSpd = -.5f;
            wasOpen = false;
            if (GameManager.Current.Player != null)
                Depth = GameManager.Current.Player.Depth - .0005f;
            angle = 0;
            backAlpha = 0;
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);

            var ang1 = (float)MathUtil.DegToRad(angle + 000);
            var ang2 = (float)MathUtil.DegToRad(angle + 120);
            var ang3 = (float)MathUtil.DegToRad(angle + 240);

            var d1 = new Vector2((float)MathUtil.LengthDirX(angle + 000 - 90) * dist, (float)MathUtil.LengthDirY(angle - 000 - 90) * dist);
            var d2 = new Vector2((float)MathUtil.LengthDirX(angle + 120 - 90) * dist, (float)MathUtil.LengthDirY(angle + 120 - 90) * dist);
            var d3 = new Vector2((float)MathUtil.LengthDirX(angle + 240 - 90) * dist, (float)MathUtil.LengthDirY(angle + 240 - 90) * dist);
            
            sb.Draw(segment[segIndex], Position + d1, null, Color, ang1, new Vector2(16), Scale, SpriteEffects.None, Depth + .00001f);
            sb.Draw(segment[segIndex], Position + d2, null, Color, ang2, new Vector2(16), Scale, SpriteEffects.None, Depth + .00002f);
            sb.Draw(segment[segIndex], Position + d3, null, Color, ang3, new Vector2(16), Scale, SpriteEffects.None, Depth + .00003f);

            var bgdepth = Globals.LAYER_FG + .0001f;
            sb.Draw(AssetManager.Transition[0], new Vector2(RoomCamera.Current.ViewX, RoomCamera.Current.ViewY), null, new Color(Color.White, backAlpha), 0, Vector2.Zero, Vector2.One, SpriteEffects.None, bgdepth);

            if (backAlpha > 0)
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
