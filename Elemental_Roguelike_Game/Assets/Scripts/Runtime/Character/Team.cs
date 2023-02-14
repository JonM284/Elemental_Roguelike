using System.Collections.Generic;

namespace Runtime.Character
{
    [System.Serializable]
    public class Team
    {
        public List<CharacterBase> teamMembers = new List<CharacterBase>();
    }
}