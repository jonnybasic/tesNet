using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnityEngine
{
    public enum SendMessageOptions
    {
        DontRequireReceiver
    }

    public class WaitForEndOfFrame : Task
    {
        public WaitForEndOfFrame() : base(() => throw new NotImplementedException())
        { }
    }

    public class WaitForSeconds : Task
    {
        public WaitForSeconds(float seconds) : base(() => throw new NotImplementedException())
        { }
    }

    public class MonoBehaviour : GameObject
    {
        protected void SendMessage(string name, SendMessageOptions options)
        {
            throw new NotImplementedException();
        }

        protected void StartCoroutine(IEnumerator coroutine)
        {
            throw new NotImplementedException();
        }

        protected void StopCoroutine(IEnumerator coroutine)
        {
            throw new NotImplementedException();
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

        public static void LogException(Exception e)
        {
            throw new NotImplementedException();
        }

        public static void Log(Exception ex)
        {
            throw new NotImplementedException();
        }
    }
}
