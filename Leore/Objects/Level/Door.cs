using Leore.Main;
using Leore.Objects.Effects;
using Leore.Objects.Effects.Emitters;
using Leore.Objects.Enemies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG.Objects;

namespace Leore.Objects.Level
{
    public class DoorDisabler : RoomObject
    {

        public enum TriggerType
        {
            Switch, Key, Enemy
        }

        public bool Open { get; set; }
        private Door door;
        public TriggerType Type { get; private set; }

        public bool Unlocked { get; private set; } // <- just for key type

        public DoorDisabler(float x, float y, Room room, TriggerType type) : base(x, y, room)
        {
            Depth = Globals.LAYER_BG + .00001f;
            this.Type = type;
            Visible = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (door == null) {
                door = this.CollisionBoundsFirstOrDefault<Door>(X, Y);
                return;
            }

            var open = false;

            switch (Type)
            {
                case TriggerType.Switch:
                    open = Room.SwitchState;
                    break;
                case TriggerType.Enemy:
                    open = ObjectManager.Count<Enemy>() == 0;
                    break;
                case TriggerType.Key:
                    open = GameManager.Current.Player.Stats.KeysAndKeyblocks.Contains(ID);
                    break;
            }

            var wasOpen = Open && !open && !Visible;
            var wasClosed = !Open && open && Visible;

            if (wasOpen || wasClosed)
            {                
                new SingularEffect(Center.X, Center.Y);                
            }

            Open = open;
            Visible = !Open;
            door.Open = Open;
        }

        public void Unlock(float x, float y)
        {
            if (Unlocked)
                return;

            Unlocked = true;
            GameManager.Current.Player.UseKey();

            var emitter = new KeyBurstEmitter(x, y, this);
            emitter.OnFinishedAction = () =>
            {
                GameManager.Current.Player.Stats.KeysAndKeyblocks.Add(ID);

                //new Effects.SingularEffect(X + 8, Y + 8, 2);
                //new StarEmitter(X + 8, Y + 8);
            };
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);

            //sb.Draw(Texture, Position, null, Color, Angle, DrawOffset, Scale, SpriteEffects.None, Depth);
        }

    }

    public class Door : RoomObject
    {
        public int Tx { get; private set; }
        public int Ty { get; private set; }

        public bool Open { get; set; } = true;

        public Door(float x, float y, Room room, int tx, int ty, string name = null) : base(x, y, room, name)
        {
            Depth = Globals.LAYER_BG + .001f;
            
            BoundingBox = new SPG.Util.RectF(6, 0, 4, 16);
            Texture = AssetManager.Door;

            Tx = tx;
            Ty = ty;
        }
    }
}
