﻿using DaggerfallWorkshopWpf;
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
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            DaggerfallWpf3D dagger = FindResource("DaggerfallWpf") as DaggerfallWpf3D;
            dagger.Arena2Path = Settings.Default.Arena2Path;
        }
    }
}