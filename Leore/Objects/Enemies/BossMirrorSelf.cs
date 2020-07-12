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
using Leore.Objects.Projectiles;

namespace Leore.Objects.Enemies
{
    public class BossMirrorSelf : Boss
    {
        private Player player => GameManager.Current.Player;
        
        private MirrorSelfWallEmitter wallEmitter;
        private EvilEyeEmitter eyeEmitter;

        private float bgAlpha = -1;
        private int invincible;

        private bool hasOrb = true;

        private int orbTimer = 2 * 60;

        private int aliveTimer;
        private bool touching;

        private bool deadFromOwnSpell;

        public BossMirrorSelf(float x, float y, Room room, string setCondition) : base(x, y, room, setCondition)
        {
            wallEmitter = new MirrorSelfWallEmitter(x, y, room);
            wallEmitter.Parent = this;

            eyeEmitter = new EvilEyeEmitter(x, y, this);
            
            HP = GameResources.BossMirrorSelf.HP;
            Damage = GameResources.BossMirrorSelf.Damage;
            EXP = GameResources.BossMirrorSelf.EXP;
            
            player.XVel = 0;
            //new MessageBox("[973bba]~Consume...~", textSpeed:TextSpeed.SLOW).AppearDelay = 4 * 60;
            //new MessageBox("Where there is light, [973bba]~darkness~ shall take over.").AppearDelay = 2 * 60;

            new MessageBox("[973bba]~Do not resist us.~", textSpeed:TextSpeed.NORMAL).AppearDelay = 2 * 60;
        }

        public override void BeginUpdate(GameTime gameTime)
        {
            base.BeginUpdate(gameTime);
            
            BoundingBox = player.BoundingBox;
            DrawOffset = player.DrawOffset;
            Depth = player.Depth + .0003f;
            
            if (player.Stats.Abilities.HasFlag(PlayerAbility.ORB))
            {
                Scale = new Vector2(player.Scale.X * -1, 1);
                Texture = player.Texture;
            }
                
            Color = Color.Black;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            bgAlpha = Math.Min(bgAlpha + .01f, 1);
            Color = new Color(Color, bgAlpha * .5f);

            invincible = Math.Max(invincible - 1, 0);

            Direction = player.Direction.Reverse();

            eyeEmitter.Active = (player.HP > 0 && bgAlpha == 1) && player.State != Player.PlayerState.HIT_GROUND && player.State != Player.PlayerState.OBTAIN && player.Stats.Abilities.HasFlag(PlayerAbility.ORB);
            
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

                            var item = new AbilityItem(Room.X + .5f * Room.BoundingBox.Width - 1.5f * Globals.T, Room.Y + Room.BoundingBox.Height - 3 * Globals.T, Room, "Spell: Vortex");
                            item.Texture = AssetManager.Orbs[3];
                            item.DrawOffset = new Vector2(8);
                            item.OnObtain = () =>
                            {
                                player.Stats.Abilities |= PlayerAbility.ORB;
                                GameManager.Current.AddSpell(SpellType.VOID);
                                GameManager.Current.AddStoryFlag("hasVoidOrb"); // maybe important for later...                                
                            };
                            item.Text = "Learned spell: [973bba]~Vortex~";
                        }
                    }
                }
            }

            // limit orb
            if (player.Direction == Direction.LEFT && hasOrb)
            {
                player.Orb.Position = new Vector2(Math.Min(player.Orb.X, Room.X + .5f * Room.BoundingBox.Width - 8), player.Orb.Y);
            }

            if (deadFromOwnSpell)
            {
                if (aliveTimer > 0)
                {
                    player.State = Player.PlayerState.HIT_AIR;
                    player.YVel = -.115f;
                    player.SetControlsEnabled(false);
                    player.XVel = -.2f + (float)RND.Next * .4f;
                }
                else
                {
                    player.SetControlsEnabled(true);
                }
            }

            if (player.Stats.Abilities.HasFlag(PlayerAbility.ORB))
            {
                var px = (player.X - Room.X);
                Position = new Vector2(Room.X + Room.BoundingBox.Width - px, player.Y);
            }
            
            if (!this.CollisionBounds(player, X, Y) && !deadFromOwnSpell)
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
                aliveTimer = 30;
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
            player.XVel = -1;
            player.YVel = -1f;
            player.State = Player.PlayerState.HIT_AIR;

            new MessageBox("...you feel strange.").AppearDelay = 3 * 60;

            base.OnDeath();            
        }
        
        public override void Hit<T>(T hitObj, float degAngle)
        {
            if (invincible > 0)
                return;
            
            if (hitObj is PlayerProjectile proj)
            {
                if (proj.Element != SpellElement.DARK)
                {
                    base.Hit(hitObj, degAngle);

                    player.Hit(proj.Damage);
                    player.XVel -= 1;
                    player.YVel -= .5f;
                    invincible = 60;
                }
                else
                {
                    ((PlayerProjectile)(object)hitObj).Destroy();
                    aliveTimer = 3 * 60;
                    deadFromOwnSpell = true;
                    player.Position = new Vector2(player.Position.X, player.Position.Y - 1);

                    //new FlashEmitter(X, Y, 1 * 60, false);
                    //new FlashEmitter(X, Y, 2 * 60, false);
                    //new FlashEmitter(X, Y, 2 * 60 + 30, false);
                    //new FlashEmitter(X, Y, 3 * 60, false);
                }
            }            
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);

            // orb
            if (hasOrb)
            {
                var dp = player.Position - player.Orb.Position;
                var orbPos = Position + new Vector2(dp.X, -dp.Y);
                sb.Draw(AssetManager.Orbs[0], orbPos, null, Color, 0, player.Orb.DrawOffset, Vector2.One, SpriteEffects.None, player.Orb.Depth + .00015f);

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

            if (deadFromOwnSpell)
            {
                var ang1 = (float)RND.Next * 2 * Math.PI;
                var ang2 = (float)RND.Next * 2 * Math.PI;
                var pos1 = Position + new Vector2(32 * (float)MathUtil.LengthDirX(MathUtil.RadToDeg(ang1)), 32 * (float)MathUtil.LengthDirY(MathUtil.RadToDeg(ang1)));
                var pos2 = Position + new Vector2(32 * (float)MathUtil.LengthDirX(MathUtil.RadToDeg(ang2)), 32 * (float)MathUtil.LengthDirY(MathUtil.RadToDeg(ang2)));

                sb.DrawLightning(player.Orb.Position, pos1, Color.White, Depth + .0001f);
                sb.DrawLightning(player.Orb.Position, pos2, Color.Black, Depth + .0001f);
            }

            // bg
            sb.Draw(AssetManager.MirrorBossBG[0], new Vector2(Room.X, Room.Y), null, new Color(Color.White, bgAlpha), 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Globals.LAYER_FG + .0002f);
            sb.Draw(AssetManager.MirrorBossBG[1], new Vector2(Room.X, Room.Y), null, new Color(Color.White, bgAlpha), 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Depth + .0002f);
        }
    }
}
