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

using UnityEngine;
using System.Collections;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.Items;

namespace DaggerfallWorkshop.Utility
{
    /// <summary>
    /// Implements default text provider.
    /// </summary>
    public class DefaultTextProvider : ITextProvider
    {
        public TextFile.Token[] CreateTokens(TextFile.Formatting formatting, params string[] lines)
        {
            throw new System.NotImplementedException();
        }

        public void EnableLocalizedStringDebug(bool enable)
        {
            throw new System.NotImplementedException();
        }

        public string GetAbbreviatedStatName(DFCareer.Stats stat)
        {
            throw new System.NotImplementedException();
        }

        public string GetArmorMaterialName(ArmorMaterialTypes material)
        {
            throw new System.NotImplementedException();
        }

        public bool GetLocalizedString(string collection, string id, out string result)
        {
            throw new System.NotImplementedException();
        }

        public string GetRandomText(int id)
        {
            throw new System.NotImplementedException();
        }

        public TextFile.Token[] GetRandomTokens(int id, bool dfRand = false)
        {
            throw new System.NotImplementedException();
        }

        public TextFile.Token[] GetRSCTokens(int id)
        {
            throw new System.NotImplementedException();
        }

        public string GetSkillName(DFCareer.Skills skill)
        {
            throw new System.NotImplementedException();
        }

        public int GetStatDescriptionTextID(DFCareer.Stats stat)
        {
            throw new System.NotImplementedException();
        }

        public string GetStatName(DFCareer.Stats stat)
        {
            throw new System.NotImplementedException();
        }

        public string GetText(int id)
        {
            throw new System.NotImplementedException();
        }

        public string GetWeaponMaterialName(WeaponMaterialTypes material)
        {
            throw new System.NotImplementedException();
        }
    }
}