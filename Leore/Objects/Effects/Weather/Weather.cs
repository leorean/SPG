using Leore.Objects.Effects.Emitters;
using SPG.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leore.Objects.Effects.Weather
{
    public abstract class Weather : GameObject
    {
        public Weather(float x, float y) : base (x,y, null)
        {
        }        
    }

    public class SnowWeather : Weather
    {
        public SnowWeather(float x, float y) : base(x, y)
        {
            new SnowEmitter(x, y) { Parent = this };
        }
    }
}
