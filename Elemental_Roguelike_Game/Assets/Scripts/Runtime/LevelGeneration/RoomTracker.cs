using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Project.Scripts.Runtime.LevelGeneration
{
    [DisallowMultipleComponent]
    public class RoomTracker: MonoBehaviour
    {

        #region Public Fields

        public int level;

        public RoomType roomType;

        public List<DoorChecker> modifiableDoorCheckers = new List<DoorChecker>();

        #endregion

        #region Serialized Fields

        [SerializeField]
        private List<DoorChecker> doorCheckers = new List<DoorChecker>();

        [SerializeField] private NavMeshSurface navMeshSurface;

        [SerializeField] private Transform decorationHolder;

        #endregion

        #region Accessor
        
        public bool hasBuiltNavmesh { get; private set; }

        public Transform decorationTransform => decorationHolder;

        #endregion

        #region Class Implementation

        public void ResetRoom()
        {
            level = 0;
            roomType = RoomType.FOUR_DOOR;
            doorCheckers.ForEach(dc =>
            {
                dc.ResetWalls();
                if (!modifiableDoorCheckers.Contains(dc))
                {
                    modifiableDoorCheckers.Add(dc);
                }
            });
        }

        public void UpdateRoomNavMesh()
        {
            navMeshSurface.BuildNavMesh();
            hasBuiltNavmesh = true;
        }

        public void LocationSelected()
        {
            
        }

        #endregion

    }
}