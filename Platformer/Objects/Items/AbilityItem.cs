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
            IDLE,
            TAKING,
            RISING,
            TAKEN
        }

        public PlayerAbility Ability { get; private set; }

        private State state = State.IDLE;

        private Player player;
        private float yDist = -3;

        private float sin = 0;

        public ObtainShineEmitter ObtainShineEmitter { get; private set; }
        private ObtainParticleEmitter obtainParticleEmitter;
        private float lightScale = .2f;
        
        public AbilityItem(float x, float y, Room room, PlayerAbility ability, string name = null) : base(x, y, room, name)
        {
            Ability = ability;
            Visible = false;

            ObtainShineEmitter = new ObtainShineEmitter(X, Y);
            ObtainShineEmitter.Active = false;
            ObtainShineEmitter.Depth = Depth - .001f;
            ObtainShineEmitter.Parent = this;            
            
            obtainParticleEmitter = new ObtainParticleEmitter(X, Y);
            obtainParticleEmitter.Active = false;
            obtainParticleEmitter.Parent = this;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (GameManager.Current.Player.Stats.Abilities.HasFlag(Ability))
            {
                Destroy();
            }
            Visible = true;

            sin = (float)((sin + .1f) % (2 * Math.PI));
            
            if (state == State.IDLE)
            {
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
                }
            }
            if (state == State.RISING)
            {
                Position = new Vector2(player.X, player.Y + yDist);
                yDist = Math.Max(yDist - .3f, -2 * Globals.TILE);

                ObtainShineEmitter.Active = true;
                obtainParticleEmitter.Active = true;

                //lightScale = Math.Min(lightScale + .005f, 1f);
                //ObtainShineEmitter.GlowScale = lightScale;
                //ObtainShineEmitter.GlowAlpha = Math.Min(ObtainShineEmitter.GlowAlpha + .001f, .125f);
            }

            ObtainShineEmitter.Position = Position;
            obtainParticleEmitter.Position = Position;
        }

        public override void Take()
        {
            // save to item list so it won't respawn
        }
    }
}
