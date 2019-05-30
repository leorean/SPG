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
        protected enum State
        {
            IDLE, TAKING, RISING, RISEN, TAKEN
        }
        
        public string Text { get; set; }
        
        public Action OnObtain;

        protected Color? hiColor = null;

        protected State state = State.IDLE;

        protected Player player;
        protected float maxYDist = 2 * Globals.TILE;

        protected bool flashOnTaken = true;

        private float yDist = -3;
        private float sin = 0;
        
        public ObtainShineEmitter ObtainShineEmitter { get; private set; }
        protected ObtainParticleEmitter obtainParticleEmitter;
                
        public AbilityItem(float x, float y, Room room, string name = null) : base(x, y, room, name)
        {
            Visible = false;

            ObtainShineEmitter = new ObtainShineEmitter(X, Y);
            ObtainShineEmitter.Active = true;
            ObtainShineEmitter.Depth = Depth - .001f;
            ObtainShineEmitter.Parent = this;            
            
            obtainParticleEmitter = new ObtainParticleEmitter(X, Y);
            obtainParticleEmitter.Active = false;
            obtainParticleEmitter.Parent = this;

            Respawn = false;
            Save = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            sin = (float)((sin + .1f) % (2 * Math.PI));

            if (Taken)
                Destroy();

            if (state == State.IDLE)
            {
                Visible = true;

                Move(0, (float)Math.Sin(sin) * .1f);                
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
                yDist = Math.Max(yDist - .4f, -maxYDist);

                if (yDist == -maxYDist)
                {
                    //ObjectManager.DestroyAll<MessageBox>();
                    var msgBox = new MessageBox(Text, name:Name, hiColor:hiColor);
                    msgBox.OnCompleted = () => {                        
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
                if (Taken)
                    return;

                ObtainShineEmitter.Active = false;
                obtainParticleEmitter.Active = false;

                var ty = player.Y - 8;

                XVel = (player.X - Position.X) / 16f + .8f * player.XVel;
                YVel = (ty - Position.Y) / 16f + .8f * player.YVel;
                Move(XVel, YVel);
                
                if (Math.Abs(X - player.X) < 2 && Math.Abs(Y - ty) < 2)
                {
                    player.State = Player.PlayerState.IDLE;

                    if (flashOnTaken)
                    {
                        var eff = new FlashEmitter(X, Y);
                    }

                    OnObtain?.Invoke();

                    Taken = true;
                }
            }

            ObtainShineEmitter.Position = Position;
            obtainParticleEmitter.Position = Position;            
        }

        public override void Take(Player player)
        {
            if (Taken)
                return;

            if (state == State.IDLE)
            {
                state = State.TAKING;
                this.player = player;
            }
        }        
    }
}
