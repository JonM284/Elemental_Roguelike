using System.Collections.Generic;
using Data;
using Data.CharacterData;

namespace Runtime.Character
{
    [System.Serializable]
    public class Team
    {
        //Data to be saved
        public List<CharacterStatsData> teamMembers = new List<CharacterStatsData>();
    }
}