using System;
using System.Collections.Generic;
using Data;
using Data.DataSaving;
using Project.Scripts.Utils;
using Runtime.Character;
using UnityEngine;
using Utils;

namespace Runtime.GameControllers
{
    public class TeamController: GameControllerBase, ISaveableData
    {

        #region READ-ME

        //The Purpose of this Controller is to manage everything related to player teams per run

        #endregion

        #region Private Fields

        private List<CharacterStatsData> m_savedTeamMembers = new List<CharacterStatsData>();

        #endregion

        #region Accessors

        public Team playerTeam { get; private set; }
        
        #endregion

        #region Unity Events

        private void OnEnable()
        {
            
        }

        private void OnDisable()
        {
            
        }

        #endregion

        #region Class Implementation

        public void AddTeamMember(CharacterBase _teamMember, CharacterStatsData _meepleData)
        {
            if (playerTeam == null)
            {
                playerTeam = new Team();
            }
            
        }

        public void RemoveTeamMember(CharacterBase _teamMemberToRemove, CharacterStatsData _meepleData)
        {
            if (_teamMemberToRemove == null)
            {
                return;
            }
            

        }

        [ContextMenu("Clear Team Members")]
        public void RemoveAllTeamMembers()
        {
            playerTeam.teamMembers.Clear();
        }

        public void SpawnTeamMembers()
        {
           
        }

        #endregion
        
        
        #region ISaveableData Methods

        public void LoadData(SavedGameData _savedGameData)
        {
            m_savedTeamMembers = _savedGameData.savedTeamMembers;
        }

        public void SaveData(ref SavedGameData _savedGameData)
        {
            _savedGameData.savedTeamMembers = m_savedTeamMembers;
        }

        #endregion

    }
}