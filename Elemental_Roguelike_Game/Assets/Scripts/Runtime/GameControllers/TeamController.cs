using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Data.CharacterData;
using Data.DataSaving;
using Project.Scripts.Utils;
using Runtime.Character;
using Runtime.UI.DataModels;
using UnityEngine;

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

        private List<SavedMemberData> m_savedTeamMembers = new List<SavedMemberData>();

        #endregion

        #region Accessors
        
        public int teamSize => m_teamSize;

        public int generatedTeamSize => m_generatedTeamSize;

        public int upgradePoints => m_upgradePoints;
        
        #endregion

        #region Unity Events

        private void OnEnable()
        {
            TeamSelectionUIDataModel.TeamConfirmed += OnTeamMembersConfirmed;
        }

        private void OnDisable()
        {
            TeamSelectionUIDataModel.TeamConfirmed -= OnTeamMembersConfirmed;
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
        
        private void OnTeamMembersConfirmed(List<SavedMemberData> _confirmedTeamMembers, bool _isFirstTime, bool _isRandomTeam)
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

            _confirmedTeamMembers.ForEach(csb => m_savedTeamMembers.Add(csb));
            
            DataController.Instance.SaveGame();
        }

        [ContextMenu("Clear Team Members")]
        public void RemoveAllTeamMembers()
        {
            m_savedTeamMembers.Clear();
        }

        public List<SavedMemberData> GetTeam()
        {
            if (m_savedTeamMembers.Count == 0)
            {
                SearchForSavedTeam();
            }

            if (Enumerable.FirstOrDefault(m_savedTeamMembers).m_characterStatsBase.IsNull())
            {
                SearchForSavedTeam();
            }
            
            var duplicate = CommonUtils.ToList(m_savedTeamMembers);
            return duplicate;
        }

        public void SearchForSavedTeam()
        {
            List<SavedMemberData> references = new List<SavedMemberData>();
            foreach (var _teamMember in m_savedTeamMembers)
            {
                SavedMemberData _foundMember = new SavedMemberData();
                
                var _characterData = CharacterGameController.Instance.GetCharacterByGUID(_teamMember.m_characterGUID);

                _foundMember.m_characterGUID = _teamMember.m_characterGUID;
                _foundMember.m_characterStatsBase = _characterData;

                if(_teamMember.m_perkGUIDs.Count > 0){

                    foreach (var _perkGUID  in _teamMember.m_perkGUIDs)
                    {
                        var _foundPerk = PerkController.Instance.GetPerkByGUID(_perkGUID);
                        
                        _foundMember.perks.Add(_foundPerk);
                    }
                    
                }
                
                references.Add(_foundMember);
            }

            m_savedTeamMembers = CommonUtils.ToList(references);
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
            SearchForSavedTeam();
        }

        public void SaveData(ref SavedGameData _savedGameData)
        {
            _savedGameData.savedTeamMembers = m_savedTeamMembers;
            _savedGameData.savedUpgradePoints = m_upgradePoints;
        }

        #endregion

    }
}