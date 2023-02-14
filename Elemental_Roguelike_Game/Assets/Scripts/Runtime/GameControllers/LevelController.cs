using System;
using Project.Scripts.Runtime.LevelGeneration;
using UnityEngine;
using UnityEngine.AI;

namespace Runtime.GameControllers
{
    public class LevelController: GameControllerBase
    {
        #region Events

        public static Action<RoomTracker> OnRoomChanged;

        #endregion

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
            LevelGenerationManager.LevelGenerationFinished += OnLevelGenerationFinished;
        }

        private void OnDisable()
        {
            LevelGenerationManager.LevelGenerationFinished -= OnLevelGenerationFinished;
        }

        #endregion

        #region Class Implementation

        public void StartLevelGeneration()
        {
            levelGenerationManager.GenerateLevel();
        }

        public void OnLevelGenerationFinished(RoomTracker _startingRoom)
        {
            levelGenerationManager.levelRooms.ForEach(rt => rt.gameObject.SetActive(false));
            levelGenerationManager.levelRooms.ForEach(rt =>
            {
                rt.gameObject.SetActive(true);
                rt.UpdateRoomNavMesh();
                rt.gameObject.SetActive(false);
            });

            ChangeRoom(_startingRoom);
        }
        
        public void ChangeRoom(RoomTracker _newRoom)
        {
            if (_newRoom == null)
            {
                return;
            }
            
            m_currentRoom = _newRoom;
            levelGenerationManager.levelRooms.ForEach(rt => rt.gameObject.SetActive(m_currentRoom == rt));
            OnRoomChanged?.Invoke(m_currentRoom);
        }

        #endregion


    }
}