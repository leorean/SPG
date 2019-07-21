using Microsoft.Xna.Framework;
using SPG.Objects;
using SPG.Util;
using System;
using Leore.Objects.Level;
using SPG.Map;
using Leore.Main;
using System.Diagnostics;

namespace Leore.Objects.Items
{
    public enum SpellEXPValue
    {
        Small = 1,
        Medium = 2,
        Large = 4
    }

    public class SpellEXP : GameObject, IMovable
    {
        public Collider MovingPlatform { get; set; }

        private float angVel;
        private bool kinetic = true;

        private int lifeTime;

        private Player player { get => GameManager.Current.Player; }

        public SpellEXPValue Exp { get; private set; }

        float t = (float)Math.PI;

        public bool CanTake { get; set; }
        public bool Taken { get; set; }
        private float alpha = .75f;

        float spd = 0;

        public SpellEXP(float x, float y, SpellEXPValue value) : base(x, y)
        {
            AnimationTexture = AssetManager.SpellEXP;

            DrawOffset = new Vector2(8, 8);
            BoundingBox = new SPG.Util.RectF(-4, -4, 8, 8);
            Depth = Globals.LAYER_ITEM;

            Angle = (float)(RND.Next * (2 * Math.PI));

            angVel = -.1f + (float)RND.Next * .2f;
            
            XVel = (1 + (float)RND.Next) * (float)MathUtil.LengthDirX(MathUtil.RadToDeg(Angle));
            YVel = (1 + (float)RND.Next) * (float)MathUtil.LengthDirY(MathUtil.RadToDeg(Angle));

            Gravity = .1f;

            lifeTime = 10 * 60;

            Exp = value;

            Scale = new Vector2(.5f);
        }
        
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.IsOutsideCurrentRoom())
                Destroy();

            CanTake = !kinetic;

            if (!Taken)
            {
                var yp = YVel;
                
                if (kinetic)
                {
                    Move(XVel, YVel);

                    XVel *= .9f;
                    YVel *= .9f;

                    if (Math.Abs(XVel) < .1f && Math.Abs(YVel) < .1f)
                        kinetic = false;
                }
                else
                {
                    lifeTime = Math.Max(lifeTime - 1, 0);
                    angVel *= .9f;

                    t = (t + .1f) % (float)(2 * Math.PI);
                    var yv = .05f * (float)Math.Sin(t);
                    Move(0, yv);
                    
                    if (player.HP > 0 && MathUtil.Euclidean(Center, player.Center) > 1 && MathUtil.Euclidean(Center, player.Center) < 5 * Globals.T)
                    {
                        kinetic = false;

                        var dst = (float)MathUtil.Euclidean(Center, player.Center);

                        spd = Math.Max(spd, 1 - MathUtil.Clamp(dst / (6f * Globals.T), 0, 1));
                        
                        XVel += (player.X - Center.X) * spd / 80;
                        YVel += (player.Y - Center.Y) * spd / 80;

                        if (MathUtil.Euclidean(Center, player.Center) < 4 * Globals.T)
                        {
                            Move(player.XVel, player.YVel);
                        }

                    }
                    else
                    {
                        XVel *= .95f;
                        YVel *= .95f;
                    }

                    Move(XVel, YVel);
                }

                Angle = (Angle + angVel) % (float)(2 * Math.PI);

                if (lifeTime > 2 * 60)
                {
                    Visible = true;
                }
                else
                {
                    Visible = lifeTime % 6 > 3;
                }
            } else // taken
            {
                Visible = true;
                Move(0, -1);
                alpha = Math.Max(alpha - .035f, 0);
            }

            if (lifeTime == 0 || alpha == 0)
                Destroy();

            int row = 0;
            int cols = 4;
            switch(Exp)
            {
                case SpellEXPValue.Small:
                    row = 0;
                    break;
                case SpellEXPValue.Medium:
                    row = 1;
                    break;
                case SpellEXPValue.Large:
                    row = 2;
                    break;
            }
            SetAnimation(row * cols, (row * cols) + 3, .2f, true);
            Color = new Color(Color, alpha);
        }

        ////////////////

        public static void Spawn(float x, float y, int value)
        {
            if ((int)value < (int)SpellEXPValue.Small)
                return;

            var stack = value - (int)SpellEXPValue.Small;

            var depth = Globals.LAYER_ITEM;

            SpellEXP exp = null;
            
            while (stack > 0)
            {
                var v = SpellEXPValue.Small;

                if (stack >= (int)SpellEXPValue.Large) { v = SpellEXPValue.Large; }
                else if (stack >= (int)SpellEXPValue.Medium) { v = SpellEXPValue.Medium; }
                else if (stack >= (int)SpellEXPValue.Small) { v = SpellEXPValue.Small; }
                else return;

                stack -= (int)v;
                exp = new SpellEXP(x, y, v);
                exp.Depth = depth;

                depth += .000001f;
            }

            exp = new SpellEXP(x, y, SpellEXPValue.Small);
            exp.Depth = depth;
        }
    }
}
