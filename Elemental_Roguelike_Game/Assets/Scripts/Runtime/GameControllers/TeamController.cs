using System.Collections.Generic;
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
        
        private List<CharacterStatsBase> m_savedTeamMembers = new List<CharacterStatsBase>();

        private List<string> m_savedGUIDs = new List<string>();

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
        
        private void OnTeamMembersConfirmed(List<CharacterStatsBase> _confirmedTeamMembers, bool _isFirstTime, bool _isRandomTeam)
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
                m_savedGUIDs.Clear();
            }

            _confirmedTeamMembers.ForEach(csb => m_savedTeamMembers.Add(csb));
            _confirmedTeamMembers.ForEach(csb => m_savedGUIDs.Add(csb.characterGUID));
            
            DataController.Instance.SaveGame();
        }

        [ContextMenu("Clear Team Members")]
        public void RemoveAllTeamMembers()
        {
            m_savedTeamMembers.Clear();
            m_savedGUIDs.Clear();
        }

        public List<CharacterStatsBase> GetTeam()
        {
            if (m_savedTeamMembers.Count == 0)
            {
                SearchForSavedTeam();
            }
            
            var duplicate = m_savedTeamMembers.ToList();
            return duplicate;
        }

        public void SearchForSavedTeam()
        {
            List<CharacterStatsBase> references = new List<CharacterStatsBase>();
            foreach (var searchGUID in m_savedGUIDs)
            {
                var _character = CharacterGameController.Instance.GetCharacterByGUID(searchGUID);
                references.Add(_character);
            }

            m_savedTeamMembers = references.ToList();
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
            m_savedGUIDs = _savedGameData.savedTeamMembers;
            m_upgradePoints = _savedGameData.savedUpgradePoints;
            SearchForSavedTeam();
        }

        public void SaveData(ref SavedGameData _savedGameData)
        {
            _savedGameData.savedTeamMembers = m_savedGUIDs;
            _savedGameData.savedUpgradePoints = m_upgradePoints;
        }

        #endregion

    }
}