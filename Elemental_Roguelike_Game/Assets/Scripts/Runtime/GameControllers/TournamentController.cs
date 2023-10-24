using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Data.DataSaving;
using Data.EnemyData;
using Project.Scripts.Utils;
using Utils;
using UnityEngine;

namespace Runtime.GameControllers
{
    public class TournamentController: GameControllerBase, ISaveableData
    {

        #region Static

        public static TournamentController Instance { get; private set; }

        #endregion

        #region Nested Classes

        [Serializable]
        public class TournamentParticipant
        {
            public bool isPlayerTeam;
            public string savedTeamGUID;
            public int wins;
            public int loses;
            public int points;
            public int currentPosition;
            public int previousPosition;

            public TournamentParticipant()
            {
                isPlayerTeam = false;
                savedTeamGUID = "";
                wins = 0;
                loses = 0;
                points = 0;
                currentPosition = 0;
                previousPosition = 0;
            }
        }

        #endregion
        
        #region Serialized Fields

        [SerializeField] private List<TournamentData> allTournaments = new List<TournamentData>();

        #endregion

        #region Private Fields

        private List<EnemyAITeamData> tournamentEnemyTeams = new List<EnemyAITeamData>();

        private List<TournamentParticipant> m_tournamentParticipants = new List<TournamentParticipant>();

        private List<TournamentParticipant> m_teamsCompetedAgainst = new List<TournamentParticipant>();

        private int currentMatchIndex;

        private int currentTournamentIndex;

        #endregion

        #region Accessors

        public TournamentData selectedTournament { get; private set; }

        #endregion

        #region Unity Events

        private void OnEnable()
        {
            WinConditionController.ReportMatchResults += OnMatchResultsReported;
        }

        private void OnDisable()
        {
            WinConditionController.ReportMatchResults -= OnMatchResultsReported;
        }

        #endregion

        #region GameControllerBase Inherited Methods

        public override void Initialize()
        {
            if (!Instance.IsNull())
            {
                return;
            }
            
            Instance = this;
            base.Initialize();
        }

        #endregion

        #region Class Implementation
        
        private void OnMatchResultsReported(bool _isPlayerVictory)
        {
            currentMatchIndex++;

            if (currentMatchIndex >= tournamentEnemyTeams.Count)
            {
                EndTournament();
            }
            
        }

        public List<TournamentData> GetAllAvailableTournaments()
        {
            var returnedList = CommonUtils.ToList(allTournaments.Where(td => td.isUnlocked));
            return returnedList;
        }

        public List<TournamentData> GetAllTournaments()
        {
            return CommonUtils.ToList(allTournaments);
        }

        public void SetSelectedTournament(TournamentData _data)
        {
            if (_data.IsNull())
            {
                return;
            }

            selectedTournament = _data;

            currentTournamentIndex = allTournaments.IndexOf(selectedTournament);
            
            SetupTournamentTeams();
        }

        public void SetupTournamentTeams()
        {
            if (selectedTournament.IsNull())
            {
                return;
            }

            currentMatchIndex = 0;

            tournamentEnemyTeams = CommonUtils.ToList(selectedTournament.tournamentTeams);

            if (m_tournamentParticipants.Count > 0)
            {
                m_tournamentParticipants.Clear();
            }
            
            tournamentEnemyTeams.ForEach(eatd =>
            {
                TournamentParticipant newParticipant = new TournamentParticipant();
                newParticipant.savedTeamGUID = eatd.tournamentGuid;
                m_tournamentParticipants.Add(newParticipant);
            });
            
        }

        public void EndTournament()
        {
            selectedTournament.isCompleted = true;
        }

        public List<TournamentParticipant> GetAllTeams()
        {
            return CommonUtils.ToList(m_tournamentParticipants);
        }

        public EnemyAITeamData GetCurrentEnemyTeam()
        {
            return tournamentEnemyTeams[currentMatchIndex];
        }

        public void UnlockNextTournament(TournamentData _data)
        {
            if (_data.IsNull())
            {
                return;
            }

            var nextTournamentIndex = allTournaments.IndexOf(_data) + 1;

            if (allTournaments[nextTournamentIndex].IsNull())
            {
                return;
            }

            allTournaments[nextTournamentIndex].isUnlocked = true;
        }

        #endregion

        #region ISaveableData Inherited Methods

        public void LoadData(SavedGameData _savedGameData)
        {
            currentMatchIndex = _savedGameData.currentMatchIndex;
            m_tournamentParticipants = _savedGameData.tournamentParticipants;
            m_teamsCompetedAgainst = _savedGameData.teamsCompetedAgainst;
            currentTournamentIndex = _savedGameData.currentTournamentIndex;
        }

        public void SaveData(ref SavedGameData _savedGameData)
        {
            _savedGameData.currentMatchIndex = currentMatchIndex;
            _savedGameData.tournamentParticipants = m_tournamentParticipants;
            _savedGameData.teamsCompetedAgainst = m_teamsCompetedAgainst;
            _savedGameData.currentTournamentIndex = currentTournamentIndex;
        }
        
        #endregion

    }
}