using System;
using Data;
using Runtime.GameControllers;
using TMPro;
using UnityEngine;
using Utils;

namespace Runtime.UI.DataModels
{
    public class TournamentMatchResultDataModel: MonoBehaviour
    {

        #region Serialized Fields

        [SerializeField] private TMP_Text resultText;

        [SerializeField] private UIWindowData nextMatchDisplayData;

        [SerializeField] private UIWindowDialog windowDialog;

        #endregion

        #region Private Fields

        private int playerPoints;

        private int enemyPoints;

        private bool playerVictory;

        #endregion

        #region Accessors

        private WinConditionController winConController => WinConditionController.Instance;

        #endregion

        #region Unity Events

        private void OnEnable()
        {
            Initialize();
        }

        #endregion

        #region Class Implementation

        private void Initialize()
        {

            playerPoints = winConController.blueTeamPoints;

            enemyPoints = winConController.redTeamPoints;

            playerVictory = winConController.isPlayerVictory;

            string vicDef = playerVictory ? "Victory" : "Defeat";

            resultText.text = $"{playerPoints} -- {enemyPoints} \n <size=150%>{vicDef}</size>";

        }

        public void OnConfirm()
        {
            UIUtils.OpenUI(nextMatchDisplayData);
            windowDialog.Close();   
        }

        #endregion
        
        
    }
}