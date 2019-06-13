using Microsoft.Xna.Framework;
using SPG.Objects;
using SPG.Util;
using System;
using Leore.Objects.Level;
using SPG.Map;
using Leore.Main;

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

        public SpellEXPValue Exp { get; private set; }

        float t = (float)Math.PI;

        public bool CanTake { get; set; }
        public bool Taken { get; set; }
        private float alpha = .75f;

        public SpellEXP(float x, float y, SpellEXPValue value) : base(x, y)
        {
            AnimationTexture = AssetManager.SpellEXP;

            DrawOffset = new Vector2(8, 8);
            BoundingBox = new SPG.Util.RectF(-4, -4, 8, 8);
            Depth = Globals.LAYER_ITEM;

            Angle = (float)(RND.Next * (2 * Math.PI));

            angVel = -.1f + (float)RND.Next * .2f;

            //XVel = -.75f + (float)RND.Next * 1.5f;
            //YVel = -1 - (float)RND.Next * .5f;

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
                    
                    if (GameManager.Current.Map.CollisionTile(X, Y))
                    {
                        MoveTowards(GameManager.Current.Player, 30);
                    }
                    else
                    {
                        if (Math.Abs(XVel) < .1f && Math.Abs(YVel) < .1f)
                            kinetic = false;
                    }
                    
                    //var onGround = CollisionExtensions.MoveAdvanced(this, false);

                    //if (onGround)
                    //{
                    //    angVel = -.1f + (float)RND.Next * .2f;
                    //    YVel = -.8f * yp;

                    //    XVel *= .5f;

                    //    if (Math.Abs(yp) < .2f)
                    //        kinetic = false;
                    //}
                }
                else
                {
                    lifeTime = Math.Max(lifeTime - 1, 0);
                    angVel *= .9f;

                    t = (t + .1f) % (float)(2 * Math.PI);
                    var yv = .05f * (float)Math.Sin(t);
                    Move(0, yv);


                    if (MathUtil.Euclidean(Center, GameManager.Current.Player.Center) > 8)
                    {
                        kinetic = false;
                        XVel = (GameManager.Current.Player.Center.X - Center.X) / 120;
                        YVel = (GameManager.Current.Player.Center.Y - Center.Y) / 120;
                    } else
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
