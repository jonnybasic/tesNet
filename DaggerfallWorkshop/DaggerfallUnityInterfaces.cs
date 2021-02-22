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
using UnityEngine;

namespace DaggerfallConnect.Utility
{
    public interface IClimateSwaps
    {
        bool IsExteriorWindow(int archive, int record);
    }

    public static partial class ClimateSwaps
    {
        public static IClimateSwaps Instance
        { get; set; }

        internal static bool IsExteriorWindow(int archive, int record)
        {
            return Instance.IsExteriorWindow(archive, record);
        }
    }
}

namespace DaggerfallWorkshop.Utility.AssetInjection
{
    public enum TextureMap
    {
        Albedo,
        Normal,
        Emission,
        MetallicGloss,
    }

    public enum TextureImport
    {
        None,
        AllLocations,
    }

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

        internal static bool TryImportTexture(int archive, int record, int frame, object normal, TextureImport textureImport, bool mips, out Texture2D texture)
        {
            throw new NotImplementedException();
        }

        internal static bool TryImportTexture(int archive, int record, int mips, out Texture2D texture)
        {
            throw new NotImplementedException();
        }

        internal static bool TryImportTextureArray(int archive, int numSlices, TextureMap albedo, object p, out Texture2DArray textureArray)
        {
            throw new NotImplementedException();
        }

        internal static bool TryImportTexture(int archive, int v1, int v2, TextureMap normal, bool v3, out Texture2D normalMap)
        {
            throw new NotImplementedException();
        }
    }

    public interface IImageProcessing
    { }

    public static partial class ImageProcessing
    {
        public static IImageProcessing Instance
        { get; set; }

        internal static Color32[] Sharpen(ref Color32[] albedoColors, int width, int height)
        {
            throw new NotImplementedException();
        }

        internal static void DilateColors(ref Color32[] albedoColors, DFSize sz)
        {
            throw new NotImplementedException();
        }

        internal static Color32[] GetBumpMap(ref Color32[] albedoColors, int width, int height)
        {
            throw new NotImplementedException();
        }

        internal static Color32[] ConvertBumpToNormals(ref Color32[] normalColors, int width, int height, float normalStrength)
        {
            throw new NotImplementedException();
        }

        internal static void ClampBorder(ref Color32[] albedo, DFSize sz, int borderSize = 0, bool mips = false, bool v2 = false)
        {
            throw new NotImplementedException();
        }

        internal static void WrapBorder(ref Color32[] albedo, DFSize sz, int borderSize = 0, bool mips = false)
        {
            throw new NotImplementedException();
        }

        internal static Color32[] RotateColors(ref Color32[] albedo, int width, int height)
        {
            throw new NotImplementedException();
        }

        internal static Color32[] FlipColors(ref Color32[] albedo, int width, int height)
        {
            throw new NotImplementedException();
        }

        internal static void InsertColors(ref Color32[] albedo, ref Color32[] atlasColors, int x, int y, int width, int height, int atlasDim1, int atlasDim2)
        {
            throw new NotImplementedException();
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

    public interface ITextLocalization
    {
        string GetLocalizedText(string text);
    }

    public interface IUtilitySettings
    {
        int RetroRenderingMode
        { get; }

        bool UseMipMapsInRetroMode
        { get; }
    }

    public interface IMaterialReader
    {
        bool ReadableTextures
        { get; }
    }

    public interface IDaggerfallUnity
    {
        IMaterialReader MaterialReader
        { get; }

        void LogMessage(string message, bool exception = false);
    }

    public interface IContentReader
    { }

    public interface ISpellReader : IContentReader
    {
        void ReadData(byte[] data, out SpellRecord.SpellRecordData parsedData);
    }

    public static partial class TextManager
    {
        public static ITextLocalization Instance
        { get; set; }
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

    public partial class MaterialReader
    { }

    public partial class DaggerfallUnity : IDaggerfallUnity
    {
        public static IDaggerfallUnity Instance
        { get; set; }

        public static IUtilitySettings Settings
        { get; set; }

        public IMaterialReader MaterialReader
        { get; set; }

        void IDaggerfallUnity.LogMessage(string message, bool exception)
        {
            throw new NotImplementedException();
        }

        internal static void LogMessage(string message, bool exception = false)
        {
            Instance.LogMessage(message, exception);
        }
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
