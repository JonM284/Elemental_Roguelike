using System.Collections.Generic;
using Runtime.Character;
using UnityEngine;

namespace Data.DataSaving
{
    
    [System.Serializable]
    public class SavedGameData
    {
        
        public Team savedTeam;

        public SerializableDictionary<string, CharacterStatsData> allOwnedCharacters;

        public SavedGameData()
        {
            this.savedTeam = new Team();
            this.allOwnedCharacters = new SerializableDictionary<string, CharacterStatsData>();
        }
    }
}