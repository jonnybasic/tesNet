//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace wpfDagger.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.9.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("B:\\Bethesda.net Launcher\\games\\TES Daggerfall\\DF\\DAGGER\\ARENA2")]
        public string Arena2Path {
            get {
                return ((string)(this["Arena2Path"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("#00000000")]
        public global::System.Windows.Media.Color DaggerfallUnityDefaultToolTipBackgroundColor {
            get {
                return ((global::System.Windows.Media.Color)(this["DaggerfallUnityDefaultToolTipBackgroundColor"]));
            }
            set {
                this["DaggerfallUnityDefaultToolTipBackgroundColor"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("#FFFFFFFF")]
        public global::System.Windows.Media.Color DaggerfallUnityDefaultToolTipTextColor {
            get {
                return ((global::System.Windows.Media.Color)(this["DaggerfallUnityDefaultToolTipTextColor"]));
            }
            set {
                this["DaggerfallUnityDefaultToolTipTextColor"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfString xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <string>-arch=compute_61</string>
  <string>-use_fast_math</string>
  <string>-lineinfo</string>
  <string>-default-device</string>
  <string>-rdc=true</string>
  <string>-D__x86_64</string>
</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection CudaCompileOptions {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["CudaCompileOptions"]));
            }
            set {
                this["CudaCompileOptions"] = value;
            }
        }
    }
}
