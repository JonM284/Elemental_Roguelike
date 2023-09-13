using System.Collections.Generic;
using Data.CharacterData;
using Project.Scripts.Runtime.LevelGeneration;
using Runtime.Managers;
using UnityEngine;

namespace Data.DataSaving
{
    
    [System.Serializable]
    public class SavedGameData 
    {
        
        public List<CharacterStatsData> savedTeamMembers;

        public int savedUpgradePoints;

        public int savedMapSelectionLevel;

        public string m_currentEventIdetifier;

        public Vector3 m_lastPressedPOIpoisiton;
        
        public List<MapController.RowData> savedMap;

        public SavedGameData()
        {
            this.savedTeamMembers = new List<CharacterStatsData>();
            this.savedUpgradePoints = 0;
            this.savedMapSelectionLevel = 0;
            this.m_currentEventIdetifier = "";
            this.m_lastPressedPOIpoisiton = Vector3.zero;
            this.savedMap = new List<MapController.RowData>();
        }
    }
}