using System.Collections.Generic;
using Data.CharacterData;
using Project.Scripts.Runtime.LevelGeneration;
using UnityEngine;

namespace Data.DataSaving
{
    
    [System.Serializable]
    public class SavedGameData 
    {
        
        public List<CharacterStatsData> savedTeamMembers;

        public SavedGameData()
        {
            this.savedTeamMembers = new List<CharacterStatsData>();
        }
    }
}