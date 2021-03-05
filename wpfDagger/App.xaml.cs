using DaggerfallWorkshop;
using DaggerfallWorkshop.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using wpfDagger.Properties;

namespace wpfDagger
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static bool isUnitySetup = false;

        public App() : base()
        {
            SetupUnityEngine();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        { }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            System.Diagnostics.Debug.Print("Dispatcher Unhandled Exception: {0}", e);
            MessageBox.Show("Uh Oh!", "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Hand);
        }

        internal static void SetupUnityEngine()
        {
            if (isUnitySetup) return;
            // Unity Globals
            UnityEngine.Application.persistentDataPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Daggerfall Workshop");
            // validate path
            if (!System.IO.Directory.Exists(UnityEngine.Application.persistentDataPath))
            {
                System.IO.Directory.CreateDirectory(UnityEngine.Application.persistentDataPath);
            }
            // Inject the Unity GetComponent function
            UnityEngine.GameObject.AddComponentFunc<DaggerfallUnity>(
                () =>
                {
                    DaggerfallUnity instance = App.Current.FindResource("DaggerfallUnity") as DaggerfallUnity;
                    if (String.IsNullOrEmpty(instance.Arena2Path))
                    {
                        instance.Arena2Path = Settings.Default.Arena2Path;
                    }
                    return instance;
                });
            UnityEngine.GameObject.AddComponentFunc<MeshReader>(
                () => App.Current.FindResource("MeshReader"));
            UnityEngine.GameObject.AddComponentFunc<MaterialReader>(
                () => App.Current.FindResource("MaterialReader"));
            isUnitySetup = true;
        }
    }
}
