using SPG.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace SPG.Exceptions
{
    public class ObjectException : Exception
    {
        public GameObject GameObject { get; private set; }
        public ObjectException(GameObject gameObject) { GameObject = gameObject; }
        public ObjectException(GameObject gameObject, string message) : base(message) { GameObject = gameObject; }        
    }
}
