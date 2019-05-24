﻿using Microsoft.Xna.Framework;
using Platformer.Objects.Main;
using SPG.Objects;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platformer.Objects;
using Platformer.Objects.Level;

namespace Platformer.Objects.Items
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

        public bool Taken { get; set; }
        private float alpha = 2;

        public SpellEXP(float x, float y, SpellEXPValue value) : base(x, y)
        {
            AnimationTexture = AssetManager.SpellEXP;

            DrawOffset = new Vector2(8, 8);
            BoundingBox = new SPG.Util.RectF(-4, -4, 8, 8);
            Depth = Globals.LAYER_ITEM;

            Angle = (float)(RND.Next * (2 * Math.PI));

            angVel = -.1f + (float)RND.Next * .2f;

            XVel = -.75f + (float)RND.Next * 1.5f;
            YVel = -1 - (float)RND.Next * .5f;

            Gravity = .1f;

            lifeTime = 6 * 60;

            Exp = value;

            Scale = new Vector2(.5f);
        }
        
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.IsOutsideCurrentRoom())
                Destroy();

            if (!Taken)
            {
                var yp = YVel;

                if (kinetic)
                {

                    var onGround = CollisionExtensions.MoveAdvanced(this, false);

                    if (onGround)
                    {
                        angVel = -.1f + (float)RND.Next * .2f;
                        YVel = -.8f * yp;

                        XVel *= .5f;

                        if (Math.Abs(yp) < .2f)
                            kinetic = false;
                    }
                }
                else
                {
                    lifeTime = Math.Max(lifeTime - 1, 0);
                    angVel *= .9f;

                    t = (t + .1f) % (float)(2 * Math.PI);
                    YVel = .05f * (float)Math.Sin(t);
                    Move(0, YVel);
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
                alpha = Math.Max(alpha - .1f, 0);
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
            //SetAnimation(row * cols, (row * cols) + 3, 0, true);
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