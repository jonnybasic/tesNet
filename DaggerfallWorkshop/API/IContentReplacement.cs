using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DaggerfallConnect;
using DaggerfallConnect.Save;
using DaggerfallWorkshop.Game.Entity;

namespace DaggerfallConnect
{
    public struct BlockRecordKey
    {
        public int blockIndex;
        public int recordIndex;
        public string variant;
    }

    public struct BuildingReplacementData
    {
        public ushort FactionId;
        public int BuildingType;
        public byte Quality;
        public DFBlock.RmbSubRecord RmbSubRecord;
        public byte[] AutoMapData;      // for coloured map (optional)
    }

    public interface IContentReplacement
    { }

    public interface IDFBlockReplacement : IContentReplacement
    {
        string GetBlockName(int block);
        int GetBlockIndex(string name);
        bool GetData(int block, string name, out DFBlock dfBlock);
    }

    public interface IBuildingReplacement : IContentReplacement
    {
        bool GetData(int block, string name, int index, out BuildingReplacementData data);
    }

    public static class WorldDataReplacement
    {
        public static IDFBlockReplacement BlockReplacement
        { get; set; }

        public static IBuildingReplacement BuildingReplacement
        { get; set; }

        internal static string GetNewDFBlockName(int block)
        {
            return BlockReplacement.GetBlockName(block);
        }

        internal static int GetNewDFBlockIndex(string name)
        {
            return BlockReplacement.GetBlockIndex(name);
        }

        internal static bool GetDFBlockReplacementData(int block, string name, out DFBlock data)
        {
            return BlockReplacement.GetData(block, name, out data);
        }

        internal static bool GetBuildingReplacementData(string name, int block, int index, out BuildingReplacementData data)
        {
            return BuildingReplacement.GetData(block, name, index, out data);
        }

        internal static void GetDFRegionAdditionalLocationData(int region, ref DFRegion dFRegion)
        {
            throw new NotImplementedException();
        }

        internal static bool GetDFLocationReplacementData(int region, int location, out DFLocation dfLocation)
        {
            throw new NotImplementedException();
        }
    }
}

namespace DaggerfallWorkshop
{
    public interface ITextLocalization
    {
        string GetLocalizedText(string text);
    }

    public static class TextManager
    {
        public static ITextLocalization Instance
        { get; set; }
    }

    public interface IUtilityProxy
    {
        void LogMessage(string message, bool isError = false);
    }


    public interface IEntityTemplate
    {
        int NumberBodyParts { get; }

        DFCareer GetClassCareerTemplate(ClassCareers mage);
    }

    public interface IContentReader
    { }

    public interface ISpellReader : IContentReader
    {
        void ReadData(byte[] data, out SpellRecord.SpellRecordData parsedData);
    }

    public static class DaggerfallContentReader
    {
        public static ISpellReader SpellReader
        { get; set; }

        internal static void ReadSpellData(byte[] data, out SpellRecord.SpellRecordData parsedData)
        {
            SpellReader.ReadData(data, out parsedData);
        }
    }

    public static class DaggerfallProxy
    {
        public static IUtilityProxy Utility
        { get; set; }

        public static IEntityTemplate EntityTemplate
        { get; set; }
    }
}
