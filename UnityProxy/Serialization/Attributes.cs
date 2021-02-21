using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace UnityEngine
{
    public class ReadOnlyAttribute : Attribute
    { }

    public class WriteOnlyAttribute : Attribute
    { }

}

namespace UnityEngine.Serialization
{
    public class FormerlySerializedAsAttribute : Attribute
    {
        public string Name
        { get; private set; }

        public FormerlySerializedAsAttribute(string name)
        {
            Name = name;
        }
    }

}
