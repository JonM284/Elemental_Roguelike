using System.Collections.Generic;
using Data.CharacterData;
using Project.Scripts.Runtime.LevelGeneration;
using Runtime.GameControllers;
using Runtime.Managers;
using UnityEngine;

namespace Data.DataSaving
{
    
    [System.Serializable]
    public class SavedGameData
    {

        public List<SavedMemberData> savedTeamMembers;

        public int savedUpgradePoints;

        public int savedMapSelectionLevel;

        public string m_currentEventIdetifier;

        public int currentTournamentIndex;

        public int currentMatchIndex;

        public List<TournamentController.TournamentParticipant> tournamentParticipants;
        
        public List<TournamentController.TournamentParticipant> teamsCompetedAgainst;

        public Vector3 m_lastPressedPOIpoisiton;
        
        public List<MapController.RowData> savedMap;

        public SavedGameData()
        {
            this.savedTeamMembers = new List<SavedMemberData>();
            this.savedUpgradePoints = 0;
            this.savedMapSelectionLevel = 0;
            this.currentTournamentIndex = 0;
            this.currentMatchIndex = 0;
            this.m_currentEventIdetifier = "";
            this.tournamentParticipants = new List<TournamentController.TournamentParticipant>();
            this.teamsCompetedAgainst = new List<TournamentController.TournamentParticipant>();
            this.m_lastPressedPOIpoisiton = Vector3.zero;
            this.savedMap = new List<MapController.RowData>();
        }
    }
}