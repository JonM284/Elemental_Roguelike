using System;
using System.Collections;
using UnityEngine;

namespace Runtime.GameControllers
{
    public class WinConditionController : GameControllerBase
    {

        #region Private Fields

        
        
        #endregion
        
        #region Accessors

        public bool winCondition { get; private set; }

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


        #region Class Implementation

        private void TurnController_OnRunEnded()
        {
            //Game Over
        }

        #endregion
        
        
    }
}