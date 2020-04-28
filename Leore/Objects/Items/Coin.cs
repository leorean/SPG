using Microsoft.Xna.Framework;
using Leore.Objects.Level;
using SPG.Util;
using SPG.Map;
using SPG.Objects;
using System;
using Leore.Main;

namespace Leore.Objects.Items
{
    public static class CoinExtensions
    {
        public static Coin.CoinValue TileIDToCoinValue(this int i)
        {
            if (i == 0) return Coin.CoinValue.C1;
            if (i == 1) return Coin.CoinValue.C2;
            if (i == 2) return Coin.CoinValue.C3;
            if (i == 3) return Coin.CoinValue.C4;
            if (i == 4) return Coin.CoinValue.C5;
            if (i == 5) return Coin.CoinValue.C6;
            if (i == 6) return Coin.CoinValue.C7;

            throw new ArgumentException("Unable to parse tile ID to coin value!");
        }
    }

    public class Coin : Item
    {
        public class CoinValue
        {
            public float Value { get; private set; }

            private CoinValue() { }

            public static CoinValue C1 { get; private set; } = new CoinValue() { Value = V1 };
            public static CoinValue C2 { get; private set; } = new CoinValue() { Value = V2 };
            public static CoinValue C3 { get; private set; } = new CoinValue() { Value = V3 };
            public static CoinValue C4 { get; private set; } = new CoinValue() { Value = V4 };
            public static CoinValue C5 { get; private set; } = new CoinValue() { Value = V5 };
            public static CoinValue C6 { get; private set; } = new CoinValue() { Value = V6 };
            public static CoinValue C7 { get; private set; } = new CoinValue() { Value = V7 };

            public const float V1 = 1f;
            public const float V2 = 2f;
            public const float V3 = 5f;
            public const float V4 = 10f;
            public const float V5 = 50f;
            public const float V6 = 100f;
            public const float V7 = 250f;
        }

        public CoinValue Value { get; private set; }

        private double t;
        private double sin;
        private double alpha = 2;
        private Vector2 pos;
        
        private bool isLoose;
        private int takeDelay;

        public Collider MovingPlatform { get; set; }

        public Coin(float x, float y, Room room, CoinValue v, bool isLoose = false) : base(x, y, room)
        {
            DrawOffset = new Vector2(8);
            
            Visible = false;
            AnimationTexture = AssetManager.Coins;
            t = RND.Next * Math.PI * 2;
            pos = Position;

            Save = false;
            Respawn = false;

            Value = v;
            
            this.isLoose = isLoose;
            if (isLoose)
            {
                XVel = -.5f + (float)(RND.Next * 1f);
                YVel = -1.8f - (float)(RND.Next * .5f);
                takeDelay = 30;
                Respawn = true;
            }
        }
        
        public void SetLoose()
        {
            if (Taken)
                return;
            isLoose = true;
        }

        public static void Spawn(float x, float y, Room room, float value, bool preciseAmount = false)
        {
            if (value < CoinValue.V1)
                return;

            float stack;
            if (!preciseAmount)
                stack = value - CoinValue.V1;
            else
                stack = value;

            var depth = Globals.LAYER_ITEM;

            Coin c = null;

            while(stack > 0)
            {
                var v = CoinValue.C1;

                if (stack >= CoinValue.V7) { v = CoinValue.C7; }
                else if (stack >= CoinValue.V6) { v = CoinValue.C6; }
                else if (stack >= CoinValue.V5) { v = CoinValue.C5; }
                else if (stack >= CoinValue.V4) { v = CoinValue.C4; }
                else if (stack >= CoinValue.V3) { v = CoinValue.C3; }
                else if (stack >= CoinValue.V2) { v = CoinValue.C2; }
                else if (stack >= CoinValue.V1) { v = CoinValue.C1; }
                else return;

                stack -= v.Value;
                c = new Coin(x, y, room, v, true);
                depth += .00001f;
                c.Depth = depth;
            }

            if (!preciseAmount)
            {
                c = new Coin(x, y, room, CoinValue.C1, true);
                depth += .00001f;
                c.Depth = depth;
            }
        }
        
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            takeDelay = Math.Max(takeDelay - 1, 0);

            if (!isLoose)
            {

                if (!Taken)
                {
                    if (t < 0) pos = new Vector2(pos.X, pos.Y - .05f);

                    t = (t + .06) % (2 * Math.PI);
                    sin = Math.Sin(t) * 1.5;
                    Position = new Vector2(pos.X, pos.Y + (float)sin);
                }
                else
                {
                    sin = 0;
                    Move(0, -1f);
                    alpha = Math.Max(alpha - .1, 0);

                    Color = new Color(Color, (float)alpha);

                    if (alpha == 0)
                        Destroy();
                }
            }
            else
            {
                var inWater = GameManager.Current.Map.CollisionTile(X, Y, GameMap.WATER_INDEX);

                var colX = GameManager.Current.Map.CollisionTile(this, XVel, 0);

                if (!colX)
                    Move(XVel, 0);
                else
                    XVel *= -.5f;
                
                var colY = GameManager.Current.Map.CollisionTile(this, 0, YVel);
                Platform p = null;
                if (!colY)
                {
                    if (YVel >= 0)
                        p = this.CollisionBoundsFirstOrDefault<Platform>(X, Y + YVel + 1);
                    if (p != null && Bottom > p.Top)
                        p = null;
                }

                if (!colY && p == null)
                {
                    Move(0, YVel);
                    YVel += .1f;
                }
                else
                {
                    //unstick
                    if (GameManager.Current.Map.CollisionTile(X, Top - 1))
                    {
                        YVel = 0;
                        Move(0, 1);
                    }
                    else
                    {
                        if (Math.Abs(XVel) < 1 && Math.Abs(YVel) < 1)
                        {
                            XVel = 0;
                            YVel = 0;
                            pos = Position;
                            isLoose = false;
                            t = -Math.PI;
                        }

                        XVel *= .9f;
                        YVel *= -.3f;
                    }
                }

                if (inWater)
                {
                    YVel -= .08f;

                    XVel = Math.Sign(XVel) * Math.Min(Math.Abs(XVel), .5f);
                    YVel = Math.Sign(YVel) * Math.Min(Math.Abs(YVel), .5f);
                }

                //unstick
                if (GameManager.Current.Map.CollisionTile(this, 0, 0))
                {
                    MoveTowards(GameManager.Current.Player, 6);
                }
            }

            // draw logic

            int row = 0;
            int cols = 4;
            float fSpd = .13f;
            switch (Value.Value)
            {
                case CoinValue.V1:
                    row = 0;
                    break;
                case CoinValue.V2:
                    row = 1;
                    break;
                case CoinValue.V3:
                    row = 2;
                    break;
                case CoinValue.V4:
                    row = 3;
                    break;
                case CoinValue.V5:
                    row = 4;
                    break;
                case CoinValue.V6:
                    row = 5;
                    break;
                case CoinValue.V7:
                    row = 6;
                    break;                
            }
            
            SetAnimation(cols * row, cols * row + cols - 1, fSpd, true);
        }

        public override void Take(Player player)
        {
            if (!initialized)
                return;

            if (takeDelay > 0)
                return;

            if (!Taken)
            {
                XVel = 0;
                YVel = 0;
                isLoose = false;

                player.Stats.Coins += Value.Value;
                player.CoinCounter += Value.Value;
                Taken = true;

                SoundManager.Play(AssetManager.Coin0);
            }
        }        
    }
}
