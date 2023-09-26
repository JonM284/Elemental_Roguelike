using System;
using System.Collections.Generic;
using Data;
using Data.CharacterData;
using Data.DataSaving;
using Project.Scripts.Utils;
using Runtime.Character;
using Runtime.Submodules;
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

        [SerializeField] private int m_teamSize = 4;

        [SerializeField] private int m_generatedTeamSize = 3;

        #endregion

        #region Private Fields

        private int m_upgradePoints;
        
        private List<CharacterStatsData> m_savedTeamMembers = new List<CharacterStatsData>();

        #endregion

        #region Accessors
        
        public int teamSize => m_teamSize;

        public int generatedTeamSize => m_generatedTeamSize;

        public int upgradePoints => m_upgradePoints;
        
        #endregion

        #region Unity Events

        private void OnEnable() 
        {
            RandomTeamSelectionManager.TeamMembersConfirmed += OnTeamMembersConfirmed;   
        }

        private void OnDisable()
        {
            RandomTeamSelectionManager.TeamMembersConfirmed -= OnTeamMembersConfirmed;
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
        
        private void OnTeamMembersConfirmed(List<CharacterStatsData> _confirmedTeamMembers, bool _isFirstTime)
        {
            if (!is_Initialized)
            {
                return;
            }
            
            if (_confirmedTeamMembers.Count == 0)
            {
                Debug.Log("No Confirmed Members");
                return;
            }

            if (m_savedTeamMembers.Count > 0)
            {
                Debug.Log("Clearing Team Members");
                m_savedTeamMembers.Clear();
            }

            _confirmedTeamMembers.ForEach(csd => m_savedTeamMembers.Add(csd));
            
            DataController.Instance.SaveGame();
        }

        [ContextMenu("Clear Team Members")]
        public void RemoveAllTeamMembers()
        {
            m_savedTeamMembers.Clear();
        }

        public List<CharacterStatsData> GetTeam()
        {
            var duplicate = m_savedTeamMembers.ToList();
            return duplicate;
        }

        public void UpdateTeamMemberStats(CharacterStatsData _character, CharacterStatsEnum _stat, int _amount)
        {
            if (_character.IsNull())
            {
                return;
            }

            if (_amount == 0)
            {
                return;
            }

            switch (_stat)
            {
                case CharacterStatsEnum.TACKLE:
                    _character.damageScore += _amount;
                    break;
                case CharacterStatsEnum.AGILITY:
                    _character.agilityScore += _amount;
                    break;
                case CharacterStatsEnum.SHOOTING:
                    _character.shootingScore += _amount;
                    break;
            }
        }

        public void UpdateCharacterVitality(CharacterStatsData _character, int _amount)
        {
            if (_character.IsNull())
            {
                return;
            }

            _character.baseHealth += _amount;
        }

        public void UpdateCharacterShields(CharacterStatsData _character, int _amount)
        {
            if (_character.IsNull())
            {
                return;
            }

            _character.baseShields += _amount;
        }

        #endregion
        
        
        #region ISaveableData Methods

        public void LoadData(SavedGameData _savedGameData)
        {
            m_savedTeamMembers = _savedGameData.savedTeamMembers;
            m_upgradePoints = _savedGameData.savedUpgradePoints;
        }

        public void SaveData(ref SavedGameData _savedGameData)
        {
            _savedGameData.savedTeamMembers = m_savedTeamMembers;
            _savedGameData.savedUpgradePoints = m_upgradePoints;
        }

        #endregion

    }
}