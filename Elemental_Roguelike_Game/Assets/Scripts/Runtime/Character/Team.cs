using System.Collections.Generic;
using Data;

namespace Runtime.Character
{
    [System.Serializable]
    public class Team
    {
        public List<CharacterStatsData> teamMembers = new List<CharacterStatsData>();

        public List<CharacterBase> activeTeamMembers = new List<CharacterBase>();

        public List<CharacterBase> allTeamMembersObjects = new List<CharacterBase>();
    }
}