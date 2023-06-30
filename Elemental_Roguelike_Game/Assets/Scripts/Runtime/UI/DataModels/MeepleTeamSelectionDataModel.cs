using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Project.Scripts.Utils;
using Runtime.GameControllers;
using Runtime.Submodules;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace Runtime.UI.DataReceivers
{
    public class MeepleTeamSelectionDataModel: UIBase
    {

        #region Actions

        public static event Action RerollRequested;
        
        public static event Action<List<CharacterStatsData>> TeamMembersConfirmed;

        #endregion

        #region Events

        public UnityEvent OnTeamConfirmCheck;

        #endregion

        #region Serialized Fields

        [SerializeField] private List<RectTransform> selectedMeepleUIPositions =
            new List<RectTransform>();

        [SerializeField] private List<RectTransform> randomMeepleUIPositions = new List<RectTransform>();

        [SerializeField] private List<MeepleManagerSelectionItem> allMeepleItems = new List<MeepleManagerSelectionItem>();
        
        [SerializeField] private Transform unusedDisplayPool;

        [SerializeField] private PopupDialogData confirmTeamPopupDialog;

        #endregion

        #region Private Fields
        
        private TeamController m_teamController;

        private List<MeepleManagerSelectionItem> m_selectedMeeples = new List<MeepleManagerSelectionItem>();

        private List<MeepleManagerSelectionItem> m_usableMeepleItems = new List<MeepleManagerSelectionItem>();

        #endregion

        #region Accessors

        private TeamController teamController => GameControllerUtils.GetGameController(ref m_teamController);
        
        private int fullTeamSize => teamController.teamSize;
        
        private int generatedTeamSize => teamController.generatedTeamSize;

        #endregion


        #region Unity Events

        private void OnEnable()
        {
            RandomTeamSelectionManager.GeneratedTeamData += OnGeneratedTeamData;
            RandomTeamSelectionManager.SelectedMeepleConfirmed += OnSelectedMeepleConfirmed;
        }

        private void OnDisable()
        {
            RandomTeamSelectionManager.GeneratedTeamData -= OnGeneratedTeamData;
            RandomTeamSelectionManager.SelectedMeepleConfirmed -= OnSelectedMeepleConfirmed;
        }

        #endregion

        #region Class Implementation

        private void OnGeneratedTeamData(List<CharacterStatsData> _randomTeamMembers)
        {
            if (_randomTeamMembers.Count == 0)
            {
                Debug.LogError("No Team Members");
                return;
            }

            m_usableMeepleItems.Clear();
            
            allMeepleItems.ForEach(mmi =>
            {
                if (!mmi.isSelected)
                {
                    m_usableMeepleItems.Add(mmi);
                }
            });


            for (int i = 0; i < generatedTeamSize; i++)
            {
                m_usableMeepleItems[i].gameObject.SetActive(true);
                m_usableMeepleItems[i].TryGetComponent(out RectTransform meepleItemRect);
                if (meepleItemRect)
                {
                    meepleItemRect.parent = randomMeepleUIPositions[i];
                    meepleItemRect.localPosition = Vector3.zero;
                }
                m_usableMeepleItems[i].InitializeSelectionItem(_randomTeamMembers[i]);
            }
            
            
        }
        
        private void OnSelectedMeepleConfirmed(CharacterStatsData _selectedMeeple)
        {
            if (_selectedMeeple.IsNull())
            {
                Debug.LogError("Selected Meeple NULL");
                return;
            }

            var alreadySelectedMeeple = m_selectedMeeples.FirstOrDefault(mmi => mmi.assignedData.id == _selectedMeeple.id);
            if (alreadySelectedMeeple != null)
            {
                m_selectedMeeples.Remove(alreadySelectedMeeple);
                alreadySelectedMeeple.TryGetComponent(out RectTransform removedMeepleRect);
                if (removedMeepleRect)
                {
                    removedMeepleRect.parent = unusedDisplayPool;
                    removedMeepleRect.localPosition = Vector3.zero;
                }
                UpdateSelectedList();
                Debug.Log("Meeple was already selected");
                return;
            }

            var selectedMeepleItem = m_usableMeepleItems.FirstOrDefault(mi => mi.assignedData.id == _selectedMeeple.id);
            
            m_selectedMeeples.Add(selectedMeepleItem);

            UpdateSelectedList();

        }

        private void UpdateSelectedList()
        {
            for (int i = 0; i < m_selectedMeeples.Count; i++)
            {
                m_selectedMeeples[i].TryGetComponent(out RectTransform meepleItemRect);
                if (meepleItemRect)
                {
                    meepleItemRect.parent = selectedMeepleUIPositions[i];
                    meepleItemRect.localPosition = Vector3.zero;
                }
            }
        }

        public void OnRerollSelected()
        {
            RerollRequested?.Invoke();
        }

        public void OnTeamConfirmation()
        {
            if (m_selectedMeeples.Count != fullTeamSize)
            {
                UIUtils.OpenNewPopup(confirmTeamPopupDialog, ConfirmTeam);
                return;
            }
            
            ConfirmTeam();

        }

        private void ConfirmTeam()
        {
            List<CharacterStatsData> confirmedTeamMembers = new List<CharacterStatsData>();

            m_selectedMeeples.ForEach(mmi =>
            {
                confirmedTeamMembers.Add(mmi.assignedData);
            });
            
            TeamMembersConfirmed?.Invoke(confirmedTeamMembers);
            
            UIUtils.CloseUI(this);
            
        }

        #endregion


        public override void AssignArguments(params object[] _arguments)
        {
            throw new NotImplementedException();
        }
    }
}