using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    //public static class UnityEngine
    //{ }

    public static class Graphics
    {
        public static void DrawTexture(Rect targetRect, Texture2D atlasTexture, Rect atlasRect, int v1, int v2, int v3, int v4, Color color, Material pixelFontMaterial)
        {
            throw new NotImplementedException();
        }
    }

    public enum CursorMode
    {
        Auto,
        ForceSoftware
    }

    public enum OperatingSystemFamily
    {
        Windows
    }

    public static class Random
    {
        public struct State
        { }

        public static State state;

        public static int Range(int start, int end)
        {
            throw new NotImplementedException();
        }

        public static float Range(float start, float end)
        {
            throw new NotImplementedException();
        }

        public static float value
        {
            get => throw new NotImplementedException();
        }

        public static void InitState(int x)
        {
            throw new NotImplementedException();
        }
    }

    public class AnimationCurve : GameObject
    {
        public float Evaluate(float time)
        {
            throw new NotImplementedException();
        }
    }

    public class ParticleSystem : GameObject
    {
        public void Play() => throw new NotImplementedException();
        public void Stop() => throw new NotImplementedException();
    }

    public class Light : GameObject
    {
        public float range;
        public float intensity;
        public float shadowStrength;
        public Color color;
    }

    public class ScriptableObject : Object
    { }

    public class Time
    {
        public static float realtimeSinceStartup;
        public static float deltaTime;
        public static float fixedDeltaTime;
        public static float smoothDeltaTime;
        public static float unscaledDeltaTime;
    }

    public enum AudioDataLoadState
    {
        Loaded
    }

    public enum AudioRolloffMode
    {
        Linear
    }

    public static class AudioSettings
    {
        public static object driverCapabilities;

        public static void GetDSPBufferSize(out int bufferLength, out int numBuffers)
        {
            throw new NotImplementedException();
        }
    }

    public class AudioSource : GameObject
    {
        public float volume;
        public float pitch;
        public float spatialBlend;
        public float maxDistance;
        public bool isPlaying;
        public bool playOnAwake;
        public bool loop;

        public AudioClip clip;
        public int timeSamples;
        public int dopplerLevel;

        public void Play()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }

    public class AudioClip : GameObject
    {
        public float length;
        public int samples;
        public AudioDataLoadState loadState;

        public static AudioClip Create(string name, int length, int v1, int sampleRate, bool v2)
        {
            throw new NotImplementedException();
        }

        public void SetData(float[] data, int v)
        {
            throw new NotImplementedException();
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class ExecuteInEditModeAttribute : Attribute
    { }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class HideInInspectorAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public class RequireComponentAttribute : Attribute
    {
        public Type ComponentType
        { get; private set; }

        public RequireComponentAttribute(Type componentType)
        {
            ComponentType = componentType;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Struct)]
    public class SerializeFieldAttribute : Attribute
    { }
}

namespace UnityEngine.AddressableAssets
{
    public static class Addressables
    {
        public static string RuntimePath
        {
            get
            {
                return System.IO.Directory.GetCurrentDirectory();
            }
        }
    }
}
