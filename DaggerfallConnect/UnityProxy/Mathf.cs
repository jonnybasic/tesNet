using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    internal static class Mathf
    {
        public static float Pow(float a, float e)
        {
            return (float)Math.Pow(a, e);
        }

        public static float Lerp(float a, float b, float t)
        {
            return a + t * (b - a);
        }

        public static float Clamp01(float a)
        {
            return (float)Math.Max(Math.Min(1.0, a), 0.0);
        }
    }
}
