using Microsoft.Xna.Framework;
using Leore.Main;
using Leore.Objects.Effects.Emitters;
using System;

namespace Leore.Objects.Items
{
    public class AbilityItem : Item
    {
        protected enum State
        {
            IDLE, TAKING, RISING, RISEN, TAKEN
        }
        
        public string Text { get; set; }
        
        public Action OnObtain;

        public Color? HighlightColor { get; set; } = null;

        protected State state = State.IDLE;

        protected Player player;
        protected float maxYDist = 2 * Globals.T;

        protected bool flashOnTaken = true;

        private float yDist = -3;
        private float sin = 0;
        
        public ObtainShineEmitter ObtainShineEmitter { get; private set; }
        protected ObtainParticleEmitter obtainParticleEmitter;

        private string setCondition;
        private string appearCondition;
        private bool appeared;

        public AbilityItem(float x, float y, Room room, string name = null, string setCondition = null, string appearCondition = null) : base(x, y, room, name)
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
            Visible = false;

            this.setCondition = setCondition;
            this.appearCondition = appearCondition;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            sin = (float)((sin + .1f) % (2 * Math.PI));

            if (Taken)
                Destroy();

            if (!GameManager.Current.HasStoryFlag(appearCondition) && !appeared)
            {
                Visible = false;
                ObtainShineEmitter.Active = false;                
                return;
            }
            else
            {
                if (!appeared)
                {
                    Visible = true;
                    ObtainShineEmitter.Active = true;
                    appeared = true;
                }
            }

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

                if (player.OnGround || player.InWater)
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
                    var msgBox = new MessageBox(Text, name:Name, hiColor:HighlightColor);
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
                    GameManager.Current.AddStoryFlag(setCondition);
                }
            }

            ObtainShineEmitter.Position = Position;
            obtainParticleEmitter.Position = Position;            
        }

        public override void Take(Player player)
        {
            if (Taken)
                return;

            if (!GameManager.Current.HasStoryFlag(appearCondition))
                return;

            if (state == State.IDLE)
            {
                state = State.TAKING;
                this.player = player;                
            }
        }        
    }
}
