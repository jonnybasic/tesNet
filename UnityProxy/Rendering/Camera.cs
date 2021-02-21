using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    public enum FogMode
    {
        Linear,
        Exponential
    }

    public class Camera : GameObject
    {
        public CameraClearFlags clearFlags;
        public Color backgroundColor;

        public static Camera main;
    }
}
