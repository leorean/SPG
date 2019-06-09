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

namespace Leore.Objects.Enemies
{
    public class BossMirrorSelf : Boss
    {
        private Player player => GameManager.Current.Player;
        
        private MirrorSelfWallEmitter wallEmitter;
        private EvilEyeEmitter eyeEmitter;

        private float bgAlpha;
        private float xoff;
        private int invincible;
        
        public BossMirrorSelf(float x, float y, Room room) : base(x, y, room)
        {
            wallEmitter = new MirrorSelfWallEmitter(x, y, room);
            wallEmitter.Parent = this;

            eyeEmitter = new EvilEyeEmitter(x, y, this);
            
            HP = GameResources.BossMirrorSelf.HP;
            Damage = GameResources.BossMirrorSelf.Damage;
            EXP = GameResources.BossMirrorSelf.EXP;

            new FlashEmitter(X, Y);

            player.XVel = 0;
            new MessageBox("Foolish boy! You cannot stop the ~Void~!\nI won't let you!", hiColor: GameResources.VoidColor);
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
            
            //var blocks = ObjectManager.FindAll<EnemyBlock>();
            //foreach (var block in blocks)
            //    block.Visible = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            bgAlpha = Math.Min(bgAlpha + .01f, 1);
            Color = new Color(Color, bgAlpha * .5f);

            invincible = Math.Max(invincible - 1, 0);

            player.Position = new Vector2(Math.Max(player.X, Room.X + player.BoundingBox.Width), player.Y);
            Direction = player.Direction.Reverse();
            
            var px = (player.X - Room.X);
            Position = new Vector2(Room.X + Room.BoundingBox.Width - px, player.Y);

            if (!this.CollisionBounds(player, X, Y))
            {
                HP = Math.Max(player.HP, 1);
            }
            else
            {
                HP = 0;
            }
        }

        public override void OnDeath()
        {
            base.OnDeath();

            new FlashEmitter(X, Y, 0);
            new MessageBox("We.. will.. meet.. again...", textSpeed: TextSpeed.SLOW);
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
            var orbPos = player.Position - player.Orb.Position;
            sb.Draw(AssetManager.Orbs[0], Position + new Vector2(orbPos.X, -orbPos.Y), null, Color, 0, player.Orb.DrawOffset, Vector2.One, SpriteEffects.None, Depth + .00015f);

            // bg
            sb.Draw(AssetManager.MirrorBossBG, new Vector2(Room.X + xoff, Room.Y), null, new Color(Color.White, bgAlpha), 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Globals.LAYER_FG + .0002f);            
        }
    }
}
