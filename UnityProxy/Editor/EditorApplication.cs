using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityEditor
{
    public class InitializeOnLoadAttribute : Attribute
    { }

    public static class EditorApplication
    {
        //public event playmodeStateChanged;

        public static bool isPlayingOrWillChangePlaymode;
        public static bool isPlaying;
    }

    public class Editor : MonoBehaviour
    { }

    public static class EditorUtility
    {
        public static void SetDirty(ScriptableObject obj)
        {
            throw new NotImplementedException();
        }
    }
}

namespace UnityEngine
{
    public class CreateAssetMenuAttribute : Attribute
    {
        public string menuName;
    }

    public class CustomEditoAttributer : Attribute
    {
        public Type Type
        { get; private set; }

        public CustomEditoAttributer(Type type)
        {
            Type = type;
        }
    }

    public class HelpURL : Attribute
    {
        public Uri URL
        { get; private set; }

        public HelpURL(string url)
        {
            URL = new Uri(url);
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ContextMenuAttribute : Attribute
    {
        public string Label
        { get; private set; }

        public ContextMenuAttribute(string label)
        {
            Label = label;
        }
    }


    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class HeaderAttribute : Attribute
    {
        public string Label
        { get; private set; }

        public HeaderAttribute(string label)
        {
            Label = label;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class RangeAttribute : Attribute
    {
        public float Min
        { get; private set; }
        public float Max
        { get; private set; }

        public RangeAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }

    public class ColorUsageAttribute : Attribute
    {
        public ColorUsageAttribute(bool a, bool b)
        { }
    }

    public class TooltipAttribute : Attribute
    {
        public string Label
        { get; private set; }

        public TooltipAttribute(string label)
        {
            Label = label;
        }
    }
}
