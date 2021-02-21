using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unity.Jobs;

namespace UnityEngine
{
    using CompilerProxy = System.CodeDom.Compiler.CodeCompiler;

    public class AssetBundleRequest : AsyncJob<Asset>
    {
        internal AssetBundleRequest(Task<Asset> t) : base(t)
        {
        }

        public Asset asset
        {
            get => throw new NotImplementedException();
        }
    }

    public class AssetBundleCreateRequest : AsyncJob<AssetBundle>
    {
        internal AssetBundleCreateRequest(Task<AssetBundle> t) : base(t)
        {
        }

        public AssetBundle assetBundle
        {
            get => throw new NotImplementedException();
        }
    }

    public class AssetBundle : GameObject
    {
        public bool Contains(string key) => throw new NotImplementedException();

        public static AssetBundle LoadFromFile(string modFilePath)
        {
            throw new NotImplementedException();
        }

        public void Unload(bool unloadAllAssets)
        {
            throw new NotImplementedException();
        }

        public Object LoadAsset(string assetname)
        {
            throw new NotImplementedException();
        }

        public T LoadAsset<T>(string assetname) where T : Object
        {
            throw new NotImplementedException();
        }

        public AssetBundleRequest LoadAssetAsync(string assetname)
        {
            throw new NotImplementedException();
        }

        public AssetBundleCreateRequest LoadFromFileAsync(string file)
        {
            throw new NotImplementedException();
        }

        public string[] GetAllAssetNames()
        {
            throw new NotImplementedException();
        }
    }

    public static class Compiler
    {
        public static Assembly CompileSource(string[] source, bool b) => throw new NotImplementedException();
    }
}

namespace Wenzil.Console
{
    public static class ConsoleCommandsDatabase
    {
        public static void RegisterCommand(string name, string description, string usage, Func<string[], string> execute)
        {
            throw new NotImplementedException();
        }
    }
}

namespace Wenzil.Console.Commands
{
    public static class HelpCommand
    {
        public static string Execute(string name)
        {
            throw new NotImplementedException();
        }
    }
}
