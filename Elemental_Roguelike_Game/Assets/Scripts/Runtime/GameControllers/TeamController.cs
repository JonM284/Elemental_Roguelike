using System;
using System.Collections.Generic;
using Data;
using Data.DataSaving;
using Runtime.Character;
using UnityEngine;
using Utils;

namespace Runtime.GameControllers
{
    public class TeamController: GameControllerBase, ISaveableData
    {

        #region Private Fields

        private List<string> savedTeamUIDs = new List<string>();

        #endregion

        #region Accessors

        public Team playerTeam { get; private set; }
        
        #endregion

        #region Unity Events

        private void OnEnable()
        {
            MeepleController.PlayerMeepleCreated += OnPlayerMeepleCreated;
        }

        private void OnDisable()
        {
            MeepleController.PlayerMeepleCreated -= OnPlayerMeepleCreated;
        }

        #endregion

        #region Class Implementation
        
        private void OnPlayerMeepleCreated(CharacterBase _playerMeeple, CharacterStatsData _meepleData)
        {
            if (playerTeam == null || playerTeam.teamMembers.Count < 3)
            {
                AddTeamMember(_playerMeeple, _meepleData);
            }
            
        }

        public void AddTeamMember(CharacterBase _teamMember, CharacterStatsData _meepleData)
        {
            if (playerTeam == null)
            {
                playerTeam = new Team();
            }
            
            if (savedTeamUIDs.Contains(_meepleData.id) && !playerTeam.teamMembers.Contains(_teamMember))
            {
                Debug.Log("Team member added");
                playerTeam.teamMembers.Add(_teamMember);
            }else if (!savedTeamUIDs.Contains(_meepleData.id) && !playerTeam.teamMembers.Contains(_teamMember))
            {
                Debug.Log("Team member && Saved ID added");
                savedTeamUIDs.Add(_meepleData.id);
                playerTeam.teamMembers.Add(_teamMember);
            }
        }

        public void RemoveTeamMember(CharacterBase _teamMemberToRemove, CharacterStatsData _meepleData)
        {
            if (_teamMemberToRemove == null)
            {
                return;
            }

            savedTeamUIDs.Remove(_meepleData.id);
            playerTeam.teamMembers.Remove(_teamMemberToRemove);

        }

        [ContextMenu("Clear Team Members")]
        public void RemoveAllTeamMembers()
        {
            savedTeamUIDs.Clear();
            playerTeam.teamMembers.Clear();
        }

        #endregion

        #region ISaveableData Methods

        public void LoadData(SavedGameData _savedGameData)
        {
            this.savedTeamUIDs = _savedGameData.savedTeamUIDs;
        }

        public void SaveData(ref SavedGameData _savedGameData)
        {
            _savedGameData.savedTeamUIDs = this.savedTeamUIDs;
        }

        #endregion
        
    }
}