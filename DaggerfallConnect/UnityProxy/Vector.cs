using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    internal class Vector2
    {
        public float x;
        public float y;

        public Vector2(float x = 0.0f, float y = 0.0f)
        {
            this.x = x;
            this.y = y;
        }
    }

    internal class Vector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3(float x = 0.0f, float y = 0.0f, float z = 0.0f)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    internal class Rect
    {
        public float x;
        public float y;
        public float width;
        public float height;

        public Rect(float x = 0.0f, float y = 0.0f, float width = 0.0f, float height = 0.0f)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

    }
}
