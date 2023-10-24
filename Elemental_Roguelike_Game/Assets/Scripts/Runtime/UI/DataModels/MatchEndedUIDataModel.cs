using System;
using Data;
using Runtime.GameControllers;
using UnityEngine;
using Utils;

namespace Runtime.UI.DataModels
{
    public class MatchEndedUIDataModel: MonoBehaviour
    {

        #region Serialized Fields

        [SerializeField] private UIBase window;

        #endregion
        
        #region Unity Events

        private void OnEnable()
        {
            WinConditionController.ReportMatchResults += WinConditionControllerOnReportMatchResults;
        }
        
        private void OnDisable()
        {
            WinConditionController.ReportMatchResults -= WinConditionControllerOnReportMatchResults;
        }

        #endregion
        
        #region Class Implementation
        
        private void WinConditionControllerOnReportMatchResults(bool _isMatchEvent)
        {
            window.Close();
        }


        public void EndMatch()
        {
            Debug.Log("PRESSED END MATCH");
            SceneController.Instance.LoadScene(SceneName.MainMenu, false);
        }

        #endregion
        
    }
}