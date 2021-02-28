// Project:         Daggerfall Tools For Unity
// Copyright:       Copyright (C) 2009-2020 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Gavin Clayton (interkarma@dfworkshop.net)
// Contributors:    
// 
// Notes:
//

using System;
using DaggerfallConnect;
using DaggerfallConnect.Save;
using DaggerfallConnect.Utility;
using DaggerfallWorkshop.Utility;
using UnityEngine;

namespace DaggerfallConnect.Utility
{
}

namespace DaggerfallWorkshop.Utility.AssetInjection
{
    public interface ITextureReplacement
    {
        bool TryImportImage(string fileName, bool makeNoLongerReadable, out Texture2D texture);
        bool TryImportCifRci(string fileName, int record, int frame, bool makeNoLongerReadable, out Texture2D texture);
    }

    public static partial class TextureReplacement
    {
        public static ITextureReplacement Instance
        { get; set; }

        internal static bool TryImportImage(string fileName, bool makeNoLongerReadable, out Texture2D texture)
        {
            return Instance.TryImportImage(fileName, makeNoLongerReadable, out texture);
        }

        internal static bool TryImportCifRci(string fileName, int record, int frame, bool makeNoLongerReadable, out Texture2D texture)
        {
            return Instance.TryImportCifRci(fileName, record, frame, makeNoLongerReadable, out texture);
        }

        internal static bool TryImportTexture(int archive, int record, int frame, out Texture2D texture)
        {
            System.Diagnostics.Debug.Print("TryImportTexture({0}, {1}, {2}) - NotImplemented", archive, record, frame);
            texture = null;
            return false;
        }

        internal static bool TryImportTexture(int archive, int record, int frame, TextureMap textureMap, bool mips, out Texture2D texture)
        {
            System.Diagnostics.Debug.Print("TryImportTexture({0}, {1}, {2}, {3}, {4}) - NotImplemented", archive, record, frame, textureMap, mips);
            texture = null;
            return false;
        }

        internal static bool TryImportTexture(int archive, int record, int frame, TextureMap textureMap, TextureImport textureImport, bool mips, out Texture2D texture)
        {
            System.Diagnostics.Debug.Print("TryImportTexture({0}, {1}, {2}, {3}, {4}, {5}) - NotImplemented", archive, record, frame, textureMap, textureImport, mips);
            texture = null;
            return false;
        }

        internal static bool TryImportTextureArray(int archive, int numSlices, TextureMap map, object p, out Texture2DArray textureArray)
        {
            throw new NotImplementedException();
        }

        internal static bool TextureExistsAmongLooseFiles(int archive, int record, int frame, TextureMap textureMap = TextureMap.Albedo)
        {
            System.Diagnostics.Debug.Print("TextureExistsAmongLooseFiles({0}, {1}, {2}, {3}) - NotImplemented", archive, record, frame, textureMap);
            return false;
        }

        internal static bool TryImportMaterial(int archive, int record, int frame, out Material material)
        {
            System.Diagnostics.Debug.Print("TryImportMaterial({0}, {1}, {2}) - NotImplemented", archive, record, frame);
            material = null;
            return false;
        }

        internal static GetTextureResults MakeResults(Material material, int archive, int record)
        {
            throw new NotImplementedException();
        }

        internal static void AssignFiltermode(Material material)
        {
            throw new NotImplementedException();
        }

        internal static void CustomizeMaterial(int archive, int record, int frame, Material material)
        {
            System.Diagnostics.Debug.Print("CustomizeMaterial({0}, {1}, {2}, {3}) - NotImplemented", archive, record, frame, material);
        }
    }

    public interface IWorldDataReplacement
    {
        string GetNewDFBlockName(int block);
        int GetNewDFBlockIndex(string name);
        bool GetDFBlockReplacementData(int block, string name, out DFBlock dfBlock);
        bool GetBuildingReplacementData(int block, string name, int index, out BuildingReplacementData data);
        void GetDFRegionAdditionalLocationData(int region, ref DFRegion dFRegion);
        bool GetDFLocationReplacementData(int region, int location, out DFLocation dfLocation);
    }

    public static partial class WorldDataReplacement
    {
        public static IWorldDataReplacement Instance
        { get; set; }

        internal static string GetNewDFBlockName(int block)
        {
            return Instance.GetNewDFBlockName(block);
        }

        internal static int GetNewDFBlockIndex(string name)
        {
            return Instance.GetNewDFBlockIndex(name);
        }

        internal static bool GetDFBlockReplacementData(int block, string name, out DFBlock data)
        {
            return Instance.GetDFBlockReplacementData(block, name, out data);
        }

        internal static bool GetBuildingReplacementData(string name, int block, int index, out BuildingReplacementData data)
        {
            return Instance.GetBuildingReplacementData(block, name, index, out data);
        }

        internal static void GetDFRegionAdditionalLocationData(int region, ref DFRegion dFRegion)
        {
            Instance.GetDFRegionAdditionalLocationData(region, ref dFRegion);
        }

        internal static bool GetDFLocationReplacementData(int region, int location, out DFLocation dfLocation)
        {
            return Instance.GetDFLocationReplacementData(region, location, out dfLocation);
        }
    }
}

namespace DaggerfallWorkshop
{
    /// <summary>
    /// Interface to map samples resource for StreamingWorld.
    /// </summary>
    public interface IMapSamplesResource
    { }

    public interface IUtilitySettings
    {
        int RetroRenderingMode
        { get; }

        bool UseMipMapsInRetroMode
        { get; }
        bool SDFFontRendering { get; set; }
    }

    public interface IContentReader
    { }

    public interface ISpellReader : IContentReader
    {
        void ReadData(byte[] data, out SpellRecord.SpellRecordData parsedData);
    }

    public static partial class DaggerfallContentReader
    {
        public static ISpellReader SpellReader
        { get; set; }

        internal static void ReadSpellData(byte[] data, out SpellRecord.SpellRecordData parsedData)
        {
            SpellReader.ReadData(data, out parsedData);
        }
    }

#if true
    public partial class DaggerfallUnity
    {
        public static DaggerfallUnity Instance
        { get; set; }

        public static SettingsManager Settings
        { get; set; }

        public MaterialReader MaterialReader
        { get; set; }

        public ITextProvider TextProvider
        { get; set; }

        public string Arena2Path
        { get; set; }

        public bool IsReady
        {
            get => !String.IsNullOrEmpty(Arena2Path)
                && System.IO.Directory.Exists(Arena2Path);
        }

        public static void LogMessage(string message, bool exception = false)
        {
            if (exception) Debug.LogError(message);
            else Debug.Log(message);
        }
    }
#endif
}

namespace DaggerfallWorkshop.Game
{
    public partial class DaggerfallUI
    {
        public static DaggerfallUI Instance
        { get; set; }

        public Material PixelFontMaterial
        { get; set; }
        public Material SDFFontMaterial
        { get; set; }

        public string FontsFolder
        {
            get { return System.IO.Path.Combine(Application.streamingAssetsPath, fontsFolderName); }
        }

        const string fontsFolderName = "Fonts";
        const string parchmentBorderRCIFile = "SPOP.RCI";
        const string splashVideo = "ANIM0001.VID";
        const string deathVideo = "ANIM0012.VID";

        public static Color DaggerfallDefaultTextColor = new Color32(243, 239, 44, 255);
        public static Color DaggerfallDefaultInputTextColor = new Color32(227, 223, 0, 255);
        public static Color DaggerfallHighlightTextColor = new Color32(219, 130, 40, 255);
        public static Color DaggerfallAlternateHighlightTextColor = new Color32(255, 130, 40, 255);
        public static Color DaggerfallQuestionTextColor = new Color(0.698f, 0.812f, 1.0f);
        public static Color DaggerfallAnswerTextColor = DaggerfallDefaultInputTextColor;
        public static Color DaggerfallDefaultShadowColor = new Color32(93, 77, 12, 255);
        public static Color DaggerfallAlternateShadowColor1 = new Color32(44, 60, 60, 255);
        public static Color DaggerfallDefaultSelectedTextColor = new Color32(162, 36, 12, 255);
        public static Color DaggerfallBrighterSelectedTextColor = new Color32(254, 56, 18, 255);
        public static Color DaggerfallForcedEnchantmentTextColor = new Color32(186, 207, 125, 255);
        public static Color DaggerfallUnityStatDrainedTextColor = new Color32(190, 85, 24, 255);
        public static Color DaggerfallUnityStatIncreasedTextColor = new Color32(178, 207, 255, 255);
        public static Color DaggerfallDefaultTextCursorColor = new Color32(154, 134, 0, 200);
        public static Color DaggerfallUnityDefaultToolTipBackgroundColor = new Color32(64, 64, 64, 210);
        public static Color DaggerfallUnityDefaultToolTipTextColor = new Color32(230, 230, 200, 255);
        public static Color DaggerfallUnityDefaultCheckboxToggleColor = new Color32(146, 12, 4, 255);
        public static Color DaggerfallUnityNotImplementedColor = new Color(1, 0, 0, 0.5f);
        public static Color DaggerfallPrisonDaysUntilFreedomColor = new Color32(232, 196, 76, 255);
        public static Color DaggerfallPrisonDaysUntilFreedomShadowColor = new Color32(48, 36, 20, 255);
        public static Vector2 DaggerfallDefaultShadowPos = Vector2.one;
    }
}

namespace DaggerfallWorkshop.Game.Entity
{
    public interface IDaggerfallEntity
    {
        int NumberBodyParts { get; }

        DFCareer GetClassCareerTemplate(ClassCareers careers);
    }

    public static partial class DaggerfallEntity
    {
        public static IDaggerfallEntity Instance
        { get; set; }

        public static int NumberBodyParts
        {
            get => Instance.NumberBodyParts;
        }

        internal static DFCareer GetClassCareerTemplate(ClassCareers careers)
        {
            return Instance.GetClassCareerTemplate(careers);
        }
    }
}
