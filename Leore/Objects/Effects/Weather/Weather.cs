using Leore.Main;
using Leore.Objects.Effects.Emitters;
using Microsoft.Xna.Framework;
using SPG.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leore.Objects.Effects.Weather
{
    public abstract class Weather : GameObject, IKeepAliveBetweenRooms
    {
        protected GameObject weatherChild;

        public Weather(float x, float y) : base(x, y, null)
        {
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (RoomCamera.Current.CurrentRoom != null)
            {
                Position = RoomCamera.Current.CurrentRoom.Position;
            }

            if (weatherChild != null)
                weatherChild.Position = Position;
        }
    }

    public class SnowWeather : Weather
    {
        public SnowWeather(float x, float y) : base(x, y)
        {
            weatherChild = new SnowWeatherEmitter(x, y) { Parent = this };
        }
        
        public override void Destroy(bool callGC = false)
        {
            base.Destroy(callGC);
        }
    }
}
