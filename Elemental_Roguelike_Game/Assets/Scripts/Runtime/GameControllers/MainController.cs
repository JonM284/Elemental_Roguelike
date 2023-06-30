﻿using System;
using System.Collections.Generic;
using Runtime.Character;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Runtime.GameControllers
{
    public class MainController: GameControllerBase
    {

        #region Static

        public static MainController Instance { get; private set; }

        #endregion
        
        #region Public Fields

        public List<GameControllerBase> game_controllers = new List<GameControllerBase>();

        #endregion

        #region Unity Events

        private void Awake()
        {
            Initialize();
        }

        #endregion

        #region Class Implementation

        [ContextMenu("Initialize")]
        public override void Initialize()
        {
            Instance = this;
            SetupControllers();
            base.Initialize();
            DontDestroyOnLoad(this.gameObject);
        }

        public void SetupControllers()
        {
            game_controllers.ForEach(gc => gc.Initialize());
        }

        public void CleanupControllers()
        {
            game_controllers.ForEach(gc => gc.Cleanup());
        }


        #endregion


    }
}