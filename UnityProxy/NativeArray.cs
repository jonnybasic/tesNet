using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    public class NativeArray<T> where T : struct
    {
        internal readonly IList<T> array;

        public int Length { get; set; }

        public T this[int i]
        {
            get => array[i];
            set => array[i] = value;
        }

        public NativeArray(int length)
        {
            array = new List<T>(length);
        }
    }
}
