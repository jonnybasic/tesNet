using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace UnityEngine
{
    public enum FilterMode
    {
        Point,
        Bilinear
    }

    public static class Time
    {
        public static float realtimeSinceStartup = 0.0f;
    }

    public static class Shader
    {
        private static int propertyIndex = 0;
        private static Dictionary<string, int> propertyToIDMap = new Dictionary<string, int>();

        public static int PropertyToID(string property)
        {
            if (propertyToIDMap.ContainsKey(property))
            {
                return propertyToIDMap[property];
            }
            int nextId = propertyIndex++;
            propertyToIDMap[property] = nextId;
            return nextId;
        }
    }

    public static class Color32Extensions
    {
        public static Color ToColor(this Color32 color)
        {
            return Color.FromArgb(color.a, color.r, color.b, color.g);
        }

        public static Color32 ToColor(this Color color)
        {
            return new Color32(color.R, color.G, color.B, color.A);
        }
    }
    
}

namespace DaggerfallWorkshopWpf
{
    public class DaggerfallWpf3D : INotifyPropertyChanged
    {
        private string _arena2Path;
        private bool _isReady;

        public string Arena2Path
        {
            get => _arena2Path;
            internal set
            {
                if (_arena2Path != value)
                {
                    _arena2Path = value;
                    _isReady = System.IO.Directory.Exists(value);
                    NotifyPropertyChanged("Arena2Path");
                }
            }
        }
        public bool IsReady
        {
            get => _isReady;
            internal set
            {
                if (_isReady != value)
                {
                    _isReady = value;
                    NotifyPropertyChanged("IsReady");
                }
            }
        }

        private static SettingsManager settingsManager;
        public static SettingsManager Settings
        {
            get { return settingsManager ?? (settingsManager = new SettingsManager()); }
        }

        private MaterialReader materialReader;
        public MaterialReader MaterialReader
        {
            get { return materialReader ?? (materialReader = new MaterialReader()); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        internal void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public static DaggerfallWpf3D Instance
        {
            get
            {
                return Application.Current.FindResource("DaggerfallWpf") as DaggerfallWpf3D;
            }
        }

        public static void LogMessage(String message, bool unknown = false)
        {
            Console.WriteLine(message);
        }
    }


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
            return Math.Max(Math.Min(1.0f, a), 0.0f);
        }

        public static float Clamp(float a, float min, float max)
        {
            return Math.Max(Math.Min(max, a), min);
        }

        public static int Clamp(int a, int min, int max)
        {
            return Math.Max(Math.Min(max, a), min);
        }

    }
  
}

namespace DaggerfallWorkshopWpf.Utility
{

}