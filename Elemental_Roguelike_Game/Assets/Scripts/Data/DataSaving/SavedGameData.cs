using System.Collections.Generic;  

namespace Data.DataSaving
{
    
    [System.Serializable]
    public class SavedGameData 
    {
        
        public List<string> savedTeamUIDs;

        public SerializableDictionary<string, CharacterStatsData> allOwnedCharacters;

        public SavedGameData()
        {
            this.savedTeamUIDs = new List<string>();
            this.allOwnedCharacters = new SerializableDictionary<string, CharacterStatsData>();
        }
    }
}