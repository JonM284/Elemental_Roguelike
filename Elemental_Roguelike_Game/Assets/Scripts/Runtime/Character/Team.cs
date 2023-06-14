using System.Collections.Generic;
using Data;

namespace Runtime.Character
{
    [System.Serializable]
    public class Team
    {
        //Data to be saved
        public List<CharacterStatsData> teamMembers = new List<CharacterStatsData>();

        //Used after instantiated playable characters
        public List<CharacterBase> activeTeamMembers = new List<CharacterBase>();

        //Used after instantiated playable characters
        public List<CharacterBase> allTeamMembersObjects = new List<CharacterBase>();
    }
}