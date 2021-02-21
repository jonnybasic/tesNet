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
        public static T Load<T>(string name) where T : GameObject
        {
            throw new NotImplementedException();
        }

        public static T[] FindObjectsOfTypeAll<T>()
        {
            throw new NotImplementedException();
        }
    }

    public abstract class Asset : GameObject
    {
        public byte[] bytes;
    }

    public class TextAsset : Asset
    {
        public string text;
    }

    public class SystemInfo
    {
        public static readonly string deviceName = "unknown";
        public static readonly string deviceUniqueIdentifier = Guid.NewGuid().ToString();
        public static readonly OperatingSystemFamily operatingSystemFamily;
    }
}
