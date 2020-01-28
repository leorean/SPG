using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leore.Main;
using Leore.Objects.Effects.Emitters;
using Leore.Objects.Level;
using Leore.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Objects;
using static Leore.Main.MessageBox;
using SPG.Draw;
using Leore.Util;
using SPG.Util;
using Leore.Objects.Items;
using System.Diagnostics;

namespace Leore.Objects.Enemies
{
    public class BossMirrorSelf : Boss
    {
        private Player player => GameManager.Current.Player;
        
        private MirrorSelfWallEmitter wallEmitter;
        private EvilEyeEmitter eyeEmitter;

        private float bgAlpha;        
        private int invincible;

        private bool hasOrb = true;

        private int orbTimer = 2 * 60;

        private int aliveTimer;
        private bool touching;

        public BossMirrorSelf(float x, float y, Room room, string setCondition) : base(x, y, room, setCondition)
        {
            wallEmitter = new MirrorSelfWallEmitter(x, y, room);
            wallEmitter.Parent = this;

            eyeEmitter = new EvilEyeEmitter(x, y, this);
            
            HP = GameResources.BossMirrorSelf.HP;
            Damage = GameResources.BossMirrorSelf.Damage;
            EXP = GameResources.BossMirrorSelf.EXP;

            new FlashEmitter(X, Y);

            player.XVel = 0;
            //new MessageBox("The [973bba]~Void~ will consume this world!").AppearDelay = 2 * 60;
            //new MessageBox("Where there is light, [973bba]~darkness~ shall take over.").AppearDelay = 2 * 60;
        }

        public override void BeginUpdate(GameTime gameTime)
        {
            base.BeginUpdate(gameTime);
            
            Scale = new Vector2(player.Scale.X * -1, 1);
            BoundingBox = player.BoundingBox;
            DrawOffset = player.DrawOffset;
            Depth = player.Depth;
            Texture = player.Texture;
            Color = Color.Black;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            bgAlpha = Math.Min(bgAlpha + .01f, 1);
            Color = new Color(Color, bgAlpha * .5f);

            invincible = Math.Max(invincible - 1, 0);

            Direction = player.Direction.Reverse();

            // limit player
            player.Position = new Vector2(Math.Min(Math.Max(player.X, Room.X + player.BoundingBox.Width), X - BoundingBox.Width + 4), player.Y);
            if (player.Direction == Direction.RIGHT)
            {
                var dst = ((.5f * Room.BoundingBox.Width) - (player.X - Room.X + 1 * Globals.T));
                var spd = dst / (.5f * Room.BoundingBox.Width);
                player.XVel = Math.Min(player.XVel, spd);

                if (hasOrb)
                {

                    if (player.X >= Room.X + .5f * Room.BoundingBox.Width - 16
                        && player.Stats.Spells.IndexOf(SpellType.NONE) == player.Stats.SpellIndex && player.Orb.State == OrbState.ATTACK)
                    {
                        player.Position = new Vector2(Room.X + .5f * Room.BoundingBox.Width - 12, player.Y);

                        orbTimer = Math.Max(orbTimer - 1, 0);
                        if (orbTimer == 0)
                        {
                            new FlashEmitter(X, Y);
                            hasOrb = false;

                            player.Move(-2 * Globals.T, 0);

                            player.XVel = -3;
                            player.YVel = -2;
                            player.State = Player.PlayerState.HIT_AIR;

                            player.Stats.Abilities &= ~PlayerAbility.ORB;
                            player.Orb.Parent = null;
                            player.Orb.Destroy();
                            player.Orb = null;

                            GameManager.Current.RemoveSpell(SpellType.NONE);

                            var item = new AbilityItem(Room.X + .5f * Room.BoundingBox.Width - 1.5f * Globals.T, Room.Y + Room.BoundingBox.Height - 3 * Globals.T, Room, "Void Orb");
                            item.Texture = AssetManager.Orbs[3];
                            item.DrawOffset = new Vector2(8);
                            item.OnObtain = () =>
                            {
                                player.Stats.Abilities |= PlayerAbility.ORB;
                                GameManager.Current.AddSpell(SpellType.VOID);
                                GameManager.Current.AddStoryFlag("hasVoidOrb"); // maybe important for later...
                            };
                            item.Text = ".....";
                        }
                    }
                }
            }

            // limit orb
            if (player.Direction == Direction.LEFT && hasOrb)
            {
                player.Orb.Position = new Vector2(Math.Min(player.Orb.X, Room.X + .5f * Room.BoundingBox.Width - 8), player.Orb.Y);
            }

            var px = (player.X - Room.X);
            Position = new Vector2(Room.X + Room.BoundingBox.Width - px, player.Y);
            
            if (!this.CollisionBounds(player, X, Y))
            {
                HP = Math.Max(player.HP, 1);
                touching = false;                
            }
            else
            {
                touching = true;                
            }
            
            if (!touching)
            {
                aliveTimer = 2 * 60;
            }
            else
            {
                aliveTimer = Math.Max(aliveTimer - 1, 0);
                if (aliveTimer == 0)
                {
                    HP = 0;                    
                }
            }            
        }

        public override void OnDeath()
        {
            new FlashEmitter(X, Y, 0);
            //player.Move(-1 * Globals.T, 0);
            player.XVel = -1;
            player.YVel = -1f;
            player.State = Player.PlayerState.HIT_AIR;

            //new MessageBox("You.. can't.. stop.. it...", textSpeed: TextSpeed.SLOW);
            new MessageBox("...you feel strange.").AppearDelay = 2 * 60;

            base.OnDeath();            
        }
        
        public override void Hit(int hitPoints, float degAngle)
        {
            if (invincible > 0)
                return;

            base.Hit(hitPoints, degAngle);            
            player.Hit(hitPoints);
            player.XVel -= 1;
            player.YVel -= .5f;
            invincible = 60;
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);

            // orb
            if (hasOrb)
            {
                var dp = player.Position - player.Orb.Position;
                var orbPos = Position + new Vector2(dp.X, -dp.Y);
                sb.Draw(AssetManager.Orbs[0], orbPos, null, Color, 0, player.Orb.DrawOffset, Vector2.One, SpriteEffects.None, Depth + .00015f);

                if (player.Stats.Spells.IndexOf(SpellType.NONE) == player.Stats.SpellIndex && player.Orb.State == OrbState.ATTACK)
                {
                    //sb.DrawLightning(orbPos, player.Orb.Position, Color.White, Depth + .0001f);
                    if(MathUtil.Euclidean(orbPos, player.Orb.Position) < 12f)
                    {
                        var ang1 = (float)RND.Next * 2 * Math.PI;
                        var ang2 = (float)RND.Next * 2 * Math.PI;
                        var pos1 = orbPos + new Vector2(32 * (float)MathUtil.LengthDirX(MathUtil.RadToDeg(ang1)), 32 * (float)MathUtil.LengthDirY(MathUtil.RadToDeg(ang1)));
                        var pos2 = orbPos + new Vector2(32 * (float)MathUtil.LengthDirX(MathUtil.RadToDeg(ang2)), 32 * (float)MathUtil.LengthDirY(MathUtil.RadToDeg(ang2)));

                        //new StarEmitter(orbPos.X, orbPos.Y, 1);

                        sb.DrawLightning(player.Orb.Position, pos1, Color.White, Depth + .0001f);
                        sb.DrawLightning(orbPos, pos2, Color.Black, Depth + .0001f);

                        player.Orb.Position = new Vector2(player.Orb.X - 1 + (float)(RND.Next * 2), player.Orb.Y - 1 + (float)(RND.Next * 2));
                    }

                    player.Orb.Position = new Vector2(Math.Min(player.Orb.X, Room.X + .5f * Room.BoundingBox.Width), player.Orb.Y);                    
                }
            }

            //if (touching)
            //{
            //    for (int i = 0; i < 3; i++)
            //    {
            //        var ang1 = (float)RND.Next * 2 * Math.PI;
            //        var ang2 = (float)RND.Next * 2 * Math.PI;
            //        var rp = new Vector2(Room.X + .5f * Room.BoundingBox.Width, Y);
            //        var pos1 = rp + new Vector2(32 * (float)MathUtil.LengthDirX(MathUtil.RadToDeg(ang1)), 48 * (float)MathUtil.LengthDirY(MathUtil.RadToDeg(ang1)));
            //        var pos2 = rp + new Vector2(32 * (float)MathUtil.LengthDirX(MathUtil.RadToDeg(ang2)), 48 * (float)MathUtil.LengthDirY(MathUtil.RadToDeg(ang2)));
                    
            //        sb.DrawLightning(rp, pos1, Color.White, Depth + .0001f);
            //        sb.DrawLightning(rp, pos2, Color.Black, Depth + .0001f);
            //    }
            //}

            // bg
            sb.Draw(AssetManager.MirrorBossBG, new Vector2(Room.X, Room.Y), null, new Color(Color.White, bgAlpha), 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Globals.LAYER_FG + .0002f);            
        }
    }
}
