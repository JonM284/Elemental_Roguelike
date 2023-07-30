﻿using System;
using System.Collections;
using Runtime.GameControllers;
using Runtime.Submodules;
using UnityEngine;

namespace Runtime.Managers
{
    public class GameShowManager: MonoBehaviour
    {

        #region Serialized Fields

        [SerializeField] private RandomTeamSelectionManager randomTeamSelectionManager;

        [SerializeField] private MapController mapController;

        #endregion

        #region Unity Events

        private IEnumerator Start()
        {
            if (!MainController.Instance.allInitialized)
            {
                yield return new WaitUntil(() => MainController.Instance.allInitialized);
            }

            CheckOptions();

        }

        #endregion
        
        
        #region Class Implementation

        private void CheckOptions()
        {
            if (TeamController.Instance.savedTeamMembers.Count == 0)
            {
                randomTeamSelectionManager.SetupRandomTeamGenerator();
                return;
            }

            mapController.DisplayMap();

        }

        #endregion

    }
}