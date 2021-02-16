using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaggerfallWorkshop
{
    internal class DaggerfallUnity
    {
        public Utility.ContentReader ContentReader;
        public Utility.ITextProvider TextProvider;
        public String Arena2Path;

        public DaggerfallUnity()
        {
            // text provider
            TextProvider = new Utility.DefaultTextProvider();
            // create content reader
            ContentReader = new Utility.ContentReader(Arena2Path);
        }

        public static DaggerfallUnity Instance
        {
            get
            {
                if (__Instance == null)
                {
                    __Instance = new DaggerfallUnity();
                }
                return __Instance;
            }
        }
        private static DaggerfallUnity __Instance = null;

        public static void LogMessage(String message, bool unknown = false)
        {
            Console.WriteLine(message);
        }

        public static class Settings
        {
            public static bool SmallerDungeons = true;
        }
    }
}

namespace DaggerfallWorkshop.Utility
{

}

namespace DaggerfallWorkshop.Utility.AssetInjection
{
    internal struct BuildingReplacementData
    {
        public DaggerfallConnect.DFBlock.RmbSubRecord RmbSubRecord;
        public ushort FactionId;
        public int BuildingType;
        public byte[] AutoMapData;
    }

    internal static class WorldDataReplacement
    {
        public static int GetNewDFBlockIndex(String name)
        {
            throw new NotImplementedException();
        }

        public static string GetNewDFBlockName(int block)
        {
            throw new NotImplementedException();
        }

        public static bool GetDFRegionAdditionalLocationData(int region, ref DaggerfallConnect.DFRegion dfRegion)
        {
            throw new NotImplementedException();
        }

        public static bool GetDFBlockReplacementData(int block, String name, out DaggerfallConnect.DFBlock dfBlock)
        {
            throw new NotImplementedException();
        }

        public static bool GetDFLocationReplacementData(int region, int location, out DaggerfallConnect.DFLocation dFLocation)
        {
            throw new NotImplementedException();
        }

        public static bool GetBuildingReplacementData(String name, int block, int index, out BuildingReplacementData data)
        {
            throw new NotImplementedException();
        }
    }
}

namespace DaggerfallWorkshop.Localization
{ }
