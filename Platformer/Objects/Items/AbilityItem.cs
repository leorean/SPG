using Microsoft.Xna.Framework;
using Platformer.Objects.Effects.Emitters;
using Platformer.Objects.Main;
using SPG.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Objects.Items
{
    public class AbilityItem : Item
    {
        enum State
        {
            IDLE, TAKING, RISING, RISEN, TAKEN
        }

        public PlayerAbility Ability { get; private set; }

        public string Text { get; set; }

        private State state = State.IDLE;

        private Player player;
        private float yDist = -3;
        private float sin = 0;
        
        public ObtainShineEmitter ObtainShineEmitter { get; private set; }
        private ObtainParticleEmitter obtainParticleEmitter;
        
        public AbilityItem(float x, float y, Room room, PlayerAbility ability, string name = null) : base(x, y, room, name)
        {
            Ability = ability;
            Visible = false;

            ObtainShineEmitter = new ObtainShineEmitter(X, Y);
            ObtainShineEmitter.Active = true;
            ObtainShineEmitter.Depth = Depth - .001f;
            ObtainShineEmitter.Parent = this;            
            
            obtainParticleEmitter = new ObtainParticleEmitter(X, Y);
            obtainParticleEmitter.Active = false;
            obtainParticleEmitter.Parent = this;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            sin = (float)((sin + .1f) % (2 * Math.PI));
            
            if (state == State.IDLE)
            {
                if (GameManager.Current.Player.Stats.Abilities.HasFlag(Ability))
                    Destroy();
                Visible = true;

                Move(0, (float)Math.Sin(sin) * .1f);

                player = this.CollisionBounds<Player>(X, Y).FirstOrDefault();
                if (player != null)
                {
                    state = State.TAKING;
                }
            }
            if (state == State.TAKING)
            {
                player.XVel = 0;
                Position = new Vector2(player.X, player.Y);
                Visible = false;

                if (player.OnGround)
                {
                    player.State = Player.PlayerState.OBTAIN;
                    Visible = true;
                    state = State.RISING;
                    ObtainShineEmitter.Active = true;
                    obtainParticleEmitter.Active = true;                    
                }
            }
            if (state == State.RISING)
            {
                Position = new Vector2(player.X, player.Y + yDist);
                yDist = Math.Max(yDist - .4f, -2 * Globals.TILE);

                if (yDist == -2 * Globals.TILE)
                {
                    var msgBox = new MessageBox(Text, Name);
                    msgBox.OnCompleted = () => {

                        player.Stats.Abilities |= Ability;
                        state = State.TAKEN;
                    };

                    state = State.RISEN;
                }
            }
            if (state == State.RISEN)
            {

            }
            if (state == State.TAKEN)
            {
                ObtainShineEmitter.Active = false;
                obtainParticleEmitter.Active = false;

                var ty = player.Y - 8;

                XVel = (player.X - Position.X) / 16f + .8f * player.XVel;
                YVel = (ty - Position.Y) / 16f + .8f * player.YVel;
                Move(XVel, YVel);
                
                if (Math.Abs(X - player.X) < 2 && Math.Abs(Y - ty) < 2)
                {
                    player.State = Player.PlayerState.IDLE;
                    
                    var eff = new FlashEmitter(X, Y);

                    Destroy();
                }
            }

            ObtainShineEmitter.Position = Position;
            obtainParticleEmitter.Position = Position;            
        }

        public override void Take(Player player)
        {
            // save to item list so it won't respawn
            // -> for ability items the check is done via ability flag already
        }
    }
}
