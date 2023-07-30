using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Data.CharacterData;
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

        #region Class Implementation

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

        #region UIBase Inherited Methods

        public override void AssignArguments(params object[] _arguments)
        {
            throw new NotImplementedException();
        }

        #endregion
        
    }
}