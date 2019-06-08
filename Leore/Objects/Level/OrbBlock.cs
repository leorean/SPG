using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Leore.Main;
using Leore.Objects.Effects.Emitters;
using SPG.Objects;

namespace Leore.Objects.Level
{
    public class OrbBlock : Solid
    {
        private float alpha = 2;
        private bool active;

        private OrbBlockEmitter emitter;

        public OrbBlock(float x, float y, Room room) : base(x, y, room)
        {
            Texture = GameManager.Current.Map.TileSet[717];
            Visible = false;
            emitter = new OrbBlockEmitter(Center.X, Center.Y);            
        }

        public override void BeginUpdate(GameTime gameTime)
        {
            base.BeginUpdate(gameTime);

            if (this.IsOutsideCurrentRoom())
                Destroy();
            
            if (GameManager.Current.Player.Orb != null)
            {
                //if (MathUtil.Euclidean(Center, GameManager.Current.Player.Orb.Center) < 2 * Globals.TILE)
                if(this.CollisionBounds(GameManager.Current.Player.Orb, X, Y))
                {
                    if (GameManager.Current.Player.Orb.State != OrbState.FOLLOW)
                        //&& GameManager.Current.Player.Stats.Spells.ElementAt(GameManager.Current.Player.Stats.SpellIndex).Key == SpellType.NONE)
                    {
                        active = true;
                        emitter.Active = true;
                    }
                }
            }
            
            if (active)
            {
                alpha = Math.Max(alpha - .02f, 0);
                if (alpha == 0)
                {
                    emitter.SpawnRate = 0;
                    //new SingularEffect(Center.X, Center.Y, 2);
                    Destroy();
                }
            }

            Color = new Color(Color, alpha);
            Visible = true;
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);

            if (active)
            {
                var sin = Math.Sin(alpha * Math.PI);

                sb.Draw(GameManager.Current.Map.TileSet[718], Position, null, new Color(Color, (float)sin), Angle, DrawOffset, Scale, SpriteEffects.None, Depth);                
            }
        }
    }
}
