using System;
using System.Collections.Generic;
using Data;
using Data.DataSaving;
using Project.Scripts.Utils;
using Runtime.Character;
using Runtime.UI.DataReceivers;
using UnityEngine;
using Utils;

namespace Runtime.GameControllers
{
    public class TeamController: GameControllerBase, ISaveableData
    {

        #region READ-ME

        //The Purpose of this Controller is to manage everything related to player teams per run

        #endregion
        
        #region Static

        public static TeamController Instance { get; private set; }

        #endregion

        #region Serialized Fields

        [SerializeField] private int m_teamSize = 5;

        [SerializeField] private int m_generatedTeamSize = 3;

        #endregion

        #region Private Fields

        private List<CharacterStatsData> m_savedTeamMembers = new List<CharacterStatsData>();

        #endregion

        #region Accessors
        
        public int teamSize => m_teamSize;

        public int generatedTeamSize => m_generatedTeamSize;

        public List<CharacterStatsData> savedTeamMembers => m_savedTeamMembers;

        #endregion

        #region Unity Events

        private void OnEnable()
        {
            MeepleTeamSelectionDataModel.TeamMembersConfirmed += OnTeamMembersConfirmed;   
        }

        private void OnDisable()
        {
            MeepleTeamSelectionDataModel.TeamMembersConfirmed -= OnTeamMembersConfirmed;
        }

        #endregion

        #region GameControllerBase Inherited Methods

        public override void Initialize()
        {
            Instance = this;
            base.Initialize();
        }

        #endregion

        #region Class Implementation
        
        private void OnTeamMembersConfirmed(List<CharacterStatsData> _confirmedTeamMembers)
        {
            if (_confirmedTeamMembers.Count == 0)
            {
                Debug.LogError("No Confirmed Members");
                return;
            }

            if (m_savedTeamMembers.Count > 0)
            {
                m_savedTeamMembers.Clear();
            }
            
            _confirmedTeamMembers.ForEach(csd => m_savedTeamMembers.Add(csd));
            
            Debug.Log("<color=#00FF00>Meeples Confirmed</color>");
            foreach (var teamMember in m_savedTeamMembers)
            {
                Debug.Log($"{teamMember.id} //////// {teamMember.meepleElementTypeRef}");
            }
        }

        public void AddTeamMember(CharacterBase _teamMember, CharacterStatsData _meepleData)
        {

        }

        public void RemoveTeamMember(CharacterBase _teamMemberToRemove, CharacterStatsData _meepleData)
        {

        }

        [ContextMenu("Clear Team Members")]
        public void RemoveAllTeamMembers()
        {
            m_savedTeamMembers.Clear();
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