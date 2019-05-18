using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Objects;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Main.Orbs
{
    public enum OrbState
    {
        FOLLOW,
        IDLE,
        ATTACK
    }

    public class Orb : GameObject
    {
        public Direction Direction { get; set; }
        public OrbState State { get; set; }
        private Player player { get => Parent as Player; }
        private Vector2 targetPosition;
        private int headBackTimer;


        private int cooldown;

        public Orb(Player player) : base(player.X, player.Y)
        {
            //Texture = AssetManager.Orb;
            Scale = new Vector2(.5f);

            BoundingBox = new RectF(-4, -4, 8, 8);
            DrawOffset = new Vector2(16, 16);
            Depth = player.Depth + .0001f;

            Parent = player;
            targetPosition = player.Position;
        }

        public override void BeginUpdate(GameTime gameTime)
        {
            base.BeginUpdate(gameTime);

            targetPosition += new Vector2(player.XVel, player.YVel);

            cooldown = Math.Max(cooldown - 1, 0);

            switch (State)
            {
                case OrbState.FOLLOW:

                    targetPosition = player.Position + new Vector2(-Math.Sign((int)player.Direction) * 8, -6);
                    MoveTowards(targetPosition, 12);
                    break;
                case OrbState.IDLE:

                    MoveTowards(targetPosition, 6);
                    
                    headBackTimer = Math.Max(headBackTimer - 1, 0);
                    if (headBackTimer == 0)
                    {
                        State = OrbState.FOLLOW;
                    }

                    break;
                case OrbState.ATTACK:

                    headBackTimer = 20;

                    targetPosition = player.Position + new Vector2(Math.Sign((int)player.Direction) * 12, 12 * Math.Sign((int)player.LookDirection));
                    MoveTowards(targetPosition, 6);
                    Move(player.XVel, player.YVel);

                    // attack projectiles

                    if (cooldown == 0)
                    {
                        cooldown = 10;

                        var proj = new StarProjectile(X, Y);

                        var degAngle = MathUtil.VectorToAngle(new Vector2(targetPosition.X - player.X, targetPosition.Y - player.Y));
                        
                        proj.XVel = (float)MathUtil.LengthDirX(degAngle) * 3;
                        proj.YVel = (float)MathUtil.LengthDirY(degAngle) * 3;
                    }

                    break;                
            }            
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            //base.Draw(sb, gameTime);

            sb.Draw(AssetManager.Orb, Position, null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth);
            //if (State == OrbState.ATTACK)
            //    sb.Draw(AssetManager.OrbReflection, Position, null, new Color(Color, .75f), Angle, DrawOffset, Scale, SpriteEffects.None, Depth + .0001f);
        }
    }
}
