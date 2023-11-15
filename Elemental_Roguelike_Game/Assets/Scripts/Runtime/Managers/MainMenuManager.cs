using System;
using Data;
using Runtime.GameControllers;
using UnityEngine;
using Utils;

namespace Runtime.Managers
{
    public class MainMenuManager: MonoBehaviour
    {

        #region Serialized Fields
        
        [SerializeField] private UIWindowData matchResultData;
        
        #endregion
        
        #region Unity Events

        private void OnEnable()
        {
            WinConditionController.ReportMatchResults += OnReportMatchResults;
            SceneController.OnLevelFinishedLoading += OnLevelFinishedLoading;
        }

        private void OnDisable()
        {
            WinConditionController.ReportMatchResults -= OnReportMatchResults;
            SceneController.OnLevelFinishedLoading -= OnLevelFinishedLoading;
        }

        #endregion

        #region Class Implementation

        private void OnReportMatchResults(bool _isPlayerVictory)
        {
            UIUtils.OpenUI(matchResultData);
        }
        
        private void OnLevelFinishedLoading(SceneName _name, bool _isMatchScene)
        {
            UIUtils.FadeBlack(false);
        }

        #endregion

    }
}