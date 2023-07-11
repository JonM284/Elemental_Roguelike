using System;
using Project.Scripts.Runtime.LevelGeneration;
using UnityEngine;
using UnityEngine.AI;

namespace Runtime.GameControllers
{
    public class LevelController: GameControllerBase
    {
        #region Events

        public static event Action<RoomTracker> OnRoomChanged;

        #endregion

        #region Serialized Fields
        
        [SerializeField] private bool isOnMenuScene;

        #endregion

        #region Private Fields

        private RoomTracker m_currentRoom;

        #endregion

        #region Inherited Implementation

        public override void Initialize()
        {
            base.Initialize();
            if (isOnMenuScene)
            {
                return;
            }
            StartLevelGeneration();
        }

        #endregion

        #region Unity Events
        

        #endregion

        #region Class Implementation

        public void StartLevelGeneration()
        {
            
        }

        public void OnLevelGenerationFinished(RoomTracker _startingRoom)
        {
            /*levelGenerationManager.levelRooms.ForEach(rt => rt.gameObject.SetActive(false));
            levelGenerationManager.levelRooms.ForEach(rt =>
            {
                rt.gameObject.SetActive(true);
                rt.UpdateRoomNavMesh();
                rt.gameObject.SetActive(false);
            });

            ChangeRoom(_startingRoom);
            */      
        }
    
        
        public void ChangeRoom(RoomTracker _newRoom)
        {
            if (_newRoom == null)
            {
                return;
            }
            /*
            m_currentRoom = _newRoom;
            levelGenerationManager.levelRooms.ForEach(rt => rt.gameObject.SetActive(m_currentRoom == rt));
            OnRoomChanged?.Invoke(m_currentRoom);
            */       
        }
        
        #endregion


    }
}