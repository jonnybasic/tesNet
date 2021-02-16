using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaggerfallWorkshop
{
    internal class DaggerfallDungeon
    {
        public static bool IsMainStoryDungeon(int mapId)
        {
            return false;
        }
    }
}

namespace DaggerfallWorkshop.Game
{

}

namespace DaggerfallWorkshop.Game.Items
{
    public enum WeaponMaterialTypes
    { }

    public enum ArmorMaterialTypes
    { }
}

namespace DaggerfallWorkshop.Game.Entity
{

}

namespace DaggerfallWorkshop.Game.Banking
{

}

namespace DaggerfallWorkshop.Game.Player
{
}

namespace DaggerfallWorkshop.Game.Entity
{
}

namespace DaggerfallWorkshop.Game.Formulas
{
}

namespace DaggerfallWorkshop.Game.Utility.ModSupport
{
}

namespace DaggerfallWorkshop.Game.Questing
{
    internal enum QuestSmallerDungeonsState
    {
        Disabled,
        Enabled,
    }

    internal struct SiteLink
    {
        public int questUID;
    }

    internal class Quest
    {
        public QuestSmallerDungeonsState SmallerDungeonsState
        {
            get { return QuestSmallerDungeonsState.Disabled; }
        }
    }

    internal enum SiteTypes
    {
        Dungeon,
    }

    internal class QuestMachine
    {
        public static QuestMachine Instance = new QuestMachine();

        public Quest GetQuest(int questId)
        {
            return new Quest();
        }

        public SiteLink[] GetSiteLinks(SiteTypes type, int mapId)
        {
            return new SiteLink[1];
        }
    }
}
