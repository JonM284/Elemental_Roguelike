using System;
using System.Collections;
using Data.Sides;
using Project.Scripts.Utils;
using UnityEngine;

namespace Runtime.GameControllers
{
    public class WinConditionController : GameControllerBase
    {

        #region Static

        public static WinConditionController Instance { get; private set; }

        #endregion
        
        #region Private Fields

        [SerializeField] private CharacterSide playerSide;

        [SerializeField] private int pointsToWin;
        
        #endregion
        
        #region Accessors

        public int redTeamPoints { get; private set; }
        
        public int blueTeamPoints { get; private set; }

        #endregion

        #region Unity Events

        private void OnEnable()
        {
            TurnController.OnRunEnded += TurnController_OnRunEnded;
        }

        private void OnDisable()
        {
            TurnController.OnRunEnded -= TurnController_OnRunEnded;
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

        private void TurnController_OnRunEnded()
        {
            //Game Over
        }

        private void CheckPointThreshold()
        {
            if (redTeamPoints < pointsToWin && blueTeamPoints < pointsToWin)
            {
                return;
            }

            if (redTeamPoints >= 5)
            {
                //red win
            }else if (blueTeamPoints >= 5)
            {
                //blue win
            }
            
        }

        private void ResetPoints()
        {
            redTeamPoints = 0;
            blueTeamPoints = 0;
        }

        public void GoalScored(CharacterSide _side)
        {
            if (_side.IsNull())
            {
                return;
            }

            if (_side == playerSide)
            {
                redTeamPoints++;
                return;
            }

            blueTeamPoints++;

        }

        #endregion
        
        
    }
}