using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.Rendering
{
    public enum BlendMode
    {
        Zero = 0,
        One,
        SrcAlpha,
        OneMinusSrcAlpha,
    }

    public enum RenderQueue
    {
        AlphaTest,
        Transparent,
    }
}

namespace UnityEngine.PostProcessing
{
    public class PostProcessingSettings
    {
        public bool excludeSkybox;
    }

    public struct FogProfile
    {
        public PostProcessingSettings settings;
    }

    public struct BehaviourProfile
    {
        public FogProfile fog;
    }

    public class PostProcessingBehaviour : GameObject
    {
        public BehaviourProfile profile;
    }
}


namespace UnityEngine
{
    public class Shader : Object
    {
        internal static int lastPropertyId = 0;
        internal static readonly Dictionary<string, Shader> shaderCache = new Dictionary<string, Shader>();
        internal static readonly Dictionary<string, int> propertyIdMap = new Dictionary<string, int>();

        public static int PropertyToID(string property)
        {
            if (!propertyIdMap.ContainsKey(property))
            {
                propertyIdMap[property] = lastPropertyId++;
            }
            return propertyIdMap[property];
        }

        public static Shader Find(string name)
        {
            System.Diagnostics.Debug.Print("Find({0}) - NotImplemented", name);
            if (!shaderCache.ContainsKey(name))
            {
                shaderCache[name] = new Shader();
                shaderCache[name].name = name;
            }
            return shaderCache[name];
        }

        public static void DisableKeyword(string parameter)
        {
            System.Diagnostics.Debug.Print("DisableKeyword({0}) - NotImplemented", parameter);
        }

        public static void EnableKeyword(string parameter)
        {
            System.Diagnostics.Debug.Print("EnableKeyword({0}) - NotImplemented", parameter);
        }
    }

}
