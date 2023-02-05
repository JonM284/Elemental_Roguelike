﻿using System;
using Project.Scripts.Runtime.LevelGeneration;
using UnityEngine;

namespace Runtime.GameControllers
{
    public class LevelController: GameControllerBase
    {

        #region Serialized Fields

        [SerializeField] private LevelGenerationManager levelGenerationManager;

        #endregion

        #region Private Fields

        private RoomTracker m_currentRoom;

        #endregion

        #region Inherited Implementation

        public override void Initialize()
        {
            base.Initialize();
            StartLevelGeneration();
        }

        #endregion

        #region Unity Events

        private void OnEnable()
        {
            LevelGenerationManager.LevelGenerationFinished += ChangeRoom;
        }

        #endregion

        #region Class Implementation

        public void StartLevelGeneration()
        {
            levelGenerationManager.GenerateLevel();
        }
        
        public void ChangeRoom(RoomTracker _newRoom)
        {
            m_currentRoom = _newRoom;
            levelGenerationManager.levelRooms.ForEach(rt => rt.gameObject.SetActive(m_currentRoom == rt));
        }

        #endregion


    }
}