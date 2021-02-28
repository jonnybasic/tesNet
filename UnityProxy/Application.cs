using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.Assertions
{
    public static class Assert
    { }
}

namespace UnityEngine
{
    public static class Application
    {
        public static bool isPlaying;
        public static string dataPath;
        public static string persistentDataPath;
        public static string temporaryCachePath;
        public static string streamingAssetsPath;

        public static void Quit()
        {
            throw new NotImplementedException();
        }
    }

    public class WWW
    { }

    public static class QualitySettings
    {
        public static string[] names;

        public static void SetQualityLevel(int selectedIndex)
        {
            throw new NotImplementedException();
        }
    }

    public static class Input
    {
        public static bool anyKey;

        public static bool GetKey(KeyCode key)
        {
            throw new NotImplementedException();
        }

        public static bool GetKeyDown(KeyCode nextPageKey)
        {
            throw new NotImplementedException();
        }
    }

    public enum KeyCode
    {
        None,
        LeftControl,
        RightControl,
        LeftShift,
        RightShift,
        LeftAlt,
        RightAlt,
        Tab,
        Escape,
        Return,
        KeypadEnter,
        BackQuote,
        Minus,
        Equals,
        Backslash,
        LeftBracket,
        UpArrow,
        DownArrow,
        RightArrow,
        LeftArrow
    }

    public static class Resources
    {
        private static System.Resources.ResourceManager resourceMan;

        private static global::System.Globalization.CultureInfo resourceCulture;

        [global::System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
        public static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (resourceMan is null)
                {
                    // use the local resource manager
                    return Properties.Resources.ResourceManager;
                }
                return resourceMan;
            }
            set => resourceMan = value;
        }

        [global::System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static System.Globalization.CultureInfo Culture
        {
            get
            {
                if (resourceCulture is null)
                {
                    return Properties.Resources.Culture;
                }
                return resourceCulture;
            }
            set
            {
                resourceCulture = value;
            }
        }

        public static T Load<T>(string name) where T : GameObject, new()
        {
            var temp = Properties.Resources.settings;
            var temp2 = Properties.Resources.ResourceManager.GetObject("settings");

            T result = new T();
            // remove file extensions
            string validName = System.IO.Path.GetFileNameWithoutExtension(name);
            if (result is ITextAsset text)
            {
                string str = ResourceManager.GetString(validName, Culture);
                if (String.IsNullOrEmpty(str))
                {
                    return null;
                }
                text.SetText(str);
                ((IAsset)text).SetBytes(Encoding.ASCII.GetBytes(str));
            }
            else if (result is IAsset asset)
            {
                object obj = ResourceManager.GetObject(validName, Culture);
                if (obj is null)
                {
                    return null;
                }
                asset.SetBytes((byte[])obj);
            }
            return result;
        }

        public static object Load(string name)
        {
            throw new NotImplementedException();
        }

        public static T[] FindObjectsOfTypeAll<T>()
        {
            throw new NotImplementedException();
        }
    }

    internal interface IAsset
    {
        void SetBytes(byte[] bytes);
    }


    internal interface ITextAsset
    {
        void SetText(string text);
    }

    public abstract class Asset : GameObject, IAsset
    {
        public byte[] bytes;

        public void SetBytes(byte[] bytes)
        {
            this.bytes = bytes;
        }
    }

    public class TextAsset : Asset, ITextAsset
    {
        public string text;

        public void SetText(string text)
        {
            this.text = text;
        }
    }

    public class SystemInfo
    {
        public static readonly string deviceName = "unknown";
        public static readonly string deviceUniqueIdentifier = Guid.NewGuid().ToString();
        public static readonly OperatingSystemFamily operatingSystemFamily;
    }
}
