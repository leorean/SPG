using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPG.Map;
using SPG.Objects;
using Leore.Main;
using Leore.Objects.Effects;
using Leore.Resources;

namespace Leore.Objects.Level
{
    public class AirBubbleSpawner : RoomObject
    {
        int delay;
        public AirBubbleSpawner(float x, float y, Room room) : base(x, y, room)
        {
            Visible = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            delay = Math.Max(delay - 1, 0);

            if (delay == 0)
            {
                delay = 3 * 60;
                var bubble = new AirBubble(Center.X, Center.Y, Room);
                bubble.Texture = Texture;
            }
        }
    }

    public class AirBubble : RoomObject
    {
        float alpha;
        double t = Math.PI * .5f;

        public AirBubble(float x, float y, Room room) : base(x, y, room)
        {
            DrawOffset = new Vector2(8);
            Scale = new Vector2(.1f);
            Depth = Globals.LAYER_PLAYER + .0001f;

            BoundingBox = new SPG.Util.RectF(-2, -2, 4, 4);

            YVel = 0;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            t = (t + .1f) % (2 * Math.PI);
            XVel = .1f * Scale.X * (float)Math.Sin(t);

            YVel = Math.Max(YVel - .001f, -.3f);

            Move(XVel, YVel);
            
            if (!GameManager.Current.Map.CollisionTile(X, Y - 4, GameMap.WATER_INDEX) || GameManager.Current.Map.CollisionTile(X, Y - 4, GameMap.FG_INDEX))
            {
                Destroy();
            }

            
                if (Scale.X > .2f && this.CollisionBounds(GameManager.Current.Player, X, Y))
                {
                    if (!GameManager.Current.Player.Stats.Abilities.HasFlag(PlayerAbility.BREATHE_UNDERWATER))
                    {
                        GameManager.Current.Player.Oxygen = GameManager.Current.Player.MaxOxygen + 60;
                        var fnt = new FollowFont(GameManager.Current.Player.X, GameManager.Current.Player.Y - 8, "+Air");
                        fnt.Color = GameResources.OxygenColors[0];
                    }
                    Destroy();
                }
            
            alpha = Math.Min(alpha + .01f, 1);
            Scale = new Vector2(Math.Min(Scale.X + .003f, 1));
            Color = new Color(Color, alpha);
        }

        public override void Destroy(bool callGC = false)
        {
            var eff = new SingularEffect(X, Y, 9);
            eff.Scale = new Vector2(.5f);
            base.Destroy(callGC);
        }
    }
}
