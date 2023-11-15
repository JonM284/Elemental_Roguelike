using System;
using System.Collections.Generic;
using Data.CharacterData;
using Runtime.Perks;

namespace Data
{
    [Serializable]
    public class SavedMemberData
    {
        public string m_characterGUID;
        public CharacterStatsBase m_characterStatsBase;
        public List<PerkBase> perks = new List<PerkBase>();
        public List<string> m_perkGUIDs = new List<string>();
    }
}