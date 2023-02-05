using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Project.Scripts.Runtime.LevelGeneration
{
    [DisallowMultipleComponent]
    public class DoorChecker: MonoBehaviour
    {

        #region Read-Only

        private readonly string playerTag = "Player";

        #endregion

        #region Nested Classes

        [Serializable]
        class WallObstructionTypes
        {
            public DoorType doorType;
            public GameObject associatedObject;
        }

        #endregion

        #region Serialized Fields

        [SerializeField]
        private List<WallObstructionTypes> _wallObstructionTypesList = new List<WallObstructionTypes>();

        [SerializeField] private Transform _doorCheckPosition;
        
        [SerializeField] private Transform _associatedRoomCheckTransform;
        
        #endregion

        #region Private Fields
        
        [SerializeField]
        private RoomTracker m_connectedRoom;

        #endregion

        #region Accessors

        public Vector3 doorCheckPosition => _doorCheckPosition.position;

        public Transform roomChecker => _associatedRoomCheckTransform;

        public DoorType doorType;

        #endregion

        #region Unity Events

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(playerTag))
            {
                other.gameObject.GetComponent<CharacterMovement>().TeleportCharacter(m_connectedRoom.gameObject.transform.position);
                LevelUtils.ChangeRooms(m_connectedRoom);
            }
        }

        #endregion

        #region Class Implementation

        //Turn on child object
        public void AssignWallDoor(DoorType _doorType)
        {
            if (_wallObstructionTypesList.Count == 0)
            {
                Debug.LogError("No walls assigned");
                return;
            }

            doorType = _doorType;
            _wallObstructionTypesList.ForEach(wot => wot.associatedObject.SetActive(wot.doorType == _doorType));
        }

        public void AssignConnectedRoom(RoomTracker _roomTracker)
        {
            if (_roomTracker == null)
            {
                return;
            }

            m_connectedRoom = _roomTracker;
        }

        public void ResetWalls()
        {
            doorType = DoorType.DOOR;
            m_connectedRoom = null;
            _wallObstructionTypesList.ForEach(wot => wot.associatedObject.SetActive(false));
        }

        #endregion

    }
}