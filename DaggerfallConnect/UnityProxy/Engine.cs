using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    internal class AssetBase
    {
        /// <summary>
        /// Raw bytes for asset
        /// </summary>
        public byte[] bytes;
    }

    internal class TextAsset : AssetBase
    { }

    internal class Resources
    {
        public static AssetBase Load(String resourceName)
        {
            throw new NotImplementedException();
        }

        public static TAsset Load<TAsset>(String resourceName) where TAsset : class
        {
            return null;
            //throw new NotImplementedException();
        }
    }

    public static class Debug
    {
        public static void Log(String message)
        {
            Console.WriteLine(message);
        }

        public static void LogError(String message)
        {
            Console.WriteLine(message);
        }

        public static void LogWarning(String message)
        {
            Console.WriteLine(message);
        }

        public static void LogFormat(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public static void LogErrorFormat(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public static void LogWarningFormat(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }
    }

    internal class Application
    {
        public static string streamingAssetsPath = "./";
    }
}

