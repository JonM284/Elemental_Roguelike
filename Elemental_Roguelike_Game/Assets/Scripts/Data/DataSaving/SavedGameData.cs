using System.Collections.Generic;  

namespace Data.DataSaving
{
    
    [System.Serializable]
    public class SavedGameData 
    {
        
        public List<CharacterStatsData> savedTeamMembers;

        public int savedFloorNumber;

        public SavedGameData()
        {
            this.savedTeamMembers = new List<CharacterStatsData>();
        }
    }
}