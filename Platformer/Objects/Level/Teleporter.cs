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

        private ObtainParticleEmitter emitter;

        private Texture2D segment;
        private float dist;
        private float angSpd = -.5f;
        private float angle;
        private bool wasOpen;
        private float backAlpha = 0;

        public Action OnFinishedAnimation { get; set; }

        public Teleporter(float x, float y, Room room) : base(x, y, room)
        {
            //AnimationTexture = AssetManager.Teleporter;

            segment = AssetManager.Teleporter.Crop(new Rectangle(0, 0, 32, 32));

            Depth = Globals.LAYER_FG - 0.0001f;

            DrawOffset = new Vector2(32);
            BoundingBox = new SPG.Util.RectF(-32, -32, 64, 64);

            emitter = new ObtainParticleEmitter(X, Y, 0);
            emitter.Parent = this;
            emitter.Active = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            angle += angSpd;

            if (Active)
            {
                if (!wasOpen)
                {
                    emitter.Active = true;

                    dist = Math.Min(dist + .1f, 16);
                    angSpd = Math.Min(angSpd + .04f, 10);

                    Depth = GameManager.Current.Player.Depth - .0005f;
                    
                    if (dist == 16)
                    {
                        wasOpen = true;
                    }
                }
                else
                {
                    emitter.Active = false;

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

                if (!GameManager.Current.Player.Stats.Teleporters.Contains(ID))
                    GameManager.Current.Player.Stats.Teleporters.Add(ID);
            }
            else
            {
            }

            angle = angle % 360;

            //Angle = (Angle + .01f) % (float)(2 * Math.PI);
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

            sb.Draw(segment, Position + d1, null, Color, ang1, new Vector2(16), Scale, SpriteEffects.None, Depth);
            sb.Draw(segment, Position + d2, null, Color, ang2, new Vector2(16), Scale, SpriteEffects.None, Depth);
            sb.Draw(segment, Position + d3, null, Color, ang3, new Vector2(16), Scale, SpriteEffects.None, Depth);

            var bgdepth = Globals.LAYER_FG + .0001f;// : GameManager.Current.Player.Depth + .0001f;

            sb.Draw(AssetManager.Transition[0], new Vector2(RoomCamera.Current.ViewX, RoomCamera.Current.ViewY), null, new Color(Color.White, backAlpha), 0, Vector2.Zero, Vector2.One, SpriteEffects.None, bgdepth);

            //sb.Draw(segment, Position + new Vector2((float)MathUtil.LengthDirX(angle + 000) * dist, (float)MathUtil.LengthDirY(angle + 000) * dist), null, Color, ang1, new Vector2(16), Scale, SpriteEffects.None, Depth);
            //sb.Draw(segment, Position + new Vector2((float)MathUtil.LengthDirX(angle + 120) * dist, (float)MathUtil.LengthDirY(angle + 120) * dist), null, Color, ang2, new Vector2(16), Scale, SpriteEffects.None, Depth);
            //sb.Draw(segment, Position + new Vector2((float)MathUtil.LengthDirX(angle + 240) * dist, (float)MathUtil.LengthDirY(angle + 240) * dist), null, Color, ang3, new Vector2(16), Scale, SpriteEffects.None, Depth);

        }
    }
}
