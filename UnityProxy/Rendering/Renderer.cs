using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    public class RenderSettings
    {
        public static Color fogColor;
        public static FogMode fogMode;
        public static float fogDensity;
        public static float fogStartDistance;
        public static float fogEndDistance;
    }

    public enum ShadowCastingMode
    {
        TwoSided,
        Off
    }

    public enum WindZoneMode
    {
        Directional
    }

    public class WindZone : GameObject
    {
        public WindZoneMode mode;
        public float windMain;
        public float windTurbulence;
        public int windPulseMagnitude;
        public float windPulseFrequency;
    }

    public class Renderer : GameObject
    {
        public Material material;

        public Material[] materials;
    }
}

namespace UnityEngine.Video
{
    public enum VideoRenderMode
    {
        APIOnly
    }

    public class VideoClip : GameObject
    {

    }

    public class VideoPlayer : GameObject
    {
        public bool isPlaying;
        public bool playOnAwake;
        public VideoRenderMode renderMode;

        public void SetTargetAudioSource(int v, AudioSource audioSource)
        {
            throw new NotImplementedException();
        }
    }
}
