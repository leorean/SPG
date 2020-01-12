using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leore.Main
{
    public struct ID
    {
        public long Identifier { get; private set; }
        public string Name { get; private set; }
        public ID(long identifier, string name)
        {
            Identifier = identifier;
            Name = name;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != GetType())
                return false;

            return Identifier == ((ID)obj).Identifier && Name == ((ID)obj).Name;
        }

        public override int GetHashCode()
        {
            var hashCode = 1452607508;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + Identifier.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            return hashCode;
        }

        public static ID Generate(long id)
        {
            return new ID(id, GameManager.Current.Map.Name);
        }
    }    
}
