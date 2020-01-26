using Microsoft.Xna.Framework;
using Leore.Main;
using Leore.Objects.Effects.Emitters;
using System;
using Leore.Objects.Level;
using static Leore.Objects.Effects.Transition;

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

        protected Player player => GameManager.Current.Player;
        protected float maxYDist = Globals.T;

        protected bool flashOnTaken = true;
        protected bool idleOnTaken = true;

        private float yDist = -3;
        protected float sin = 0;
        
        public ObtainShineEmitter ObtainShineEmitter { get; private set; }
        public int Tx { get; set; }
        public int Ty { get; set; }

        protected ObtainParticleEmitter obtainParticleEmitter;

        private string setCondition;
        private string appearCondition;
        private bool appeared;

        private LightSource light;

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

            light = new LightSource(this) { Scale = new Vector2(.75f) };
            
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

            light.Active = true;

            if (state == State.IDLE)
            {
                Visible = true;

                Move(0, (float)Math.Sin(sin) * .1f);                
            }
            if (state == State.TAKING)
            {
                player.SetControlsEnabled(false);

                player.XVel = 0;
                Position = new Vector2(player.X, player.Y);
                Visible = false;

                if (player.OnGround || player.InWater)// || player.OnCeil || player.OnWall)
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
                    if (Text != null)
                    {

                        var msgBox = new MessageBox(Text, name: Name, hiColor: HighlightColor);
                        msgBox.OnCompleted = () =>
                        {
                            state = State.TAKEN;
                        };

                        state = State.RISEN;
                    }
                    else
                    {
                        state = State.TAKEN;
                    }
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
                    if (idleOnTaken)
                    {
                        if (!player.InWater)
                            player.State = Player.PlayerState.IDLE;
                        else
                            player.State = Player.PlayerState.SWIM;
                    }

                    if (flashOnTaken)
                    {
                        var eff = new FlashEmitter(X, Y);
                    }

                    OnObtain?.Invoke();

                    Taken = true;
                    GameManager.Current.AddStoryFlag(setCondition);

                    player.SetControlsEnabled(true);
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
            }
        }        
    }
}
