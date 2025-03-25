using System;
using Project.Scripts.Utils;
using UnityEngine;

namespace Runtime.Character.AI.EnemyAI
{
    
    [RequireComponent(typeof(SphereCollider))]
    public class Sensor: MonoBehaviour
    {

        #region Serialized Fields

        [SerializeField] private float detectionRadius = 5f;
        
        [SerializeField] private float timerInterval = 1f;

        #endregion

        #region Private Fields

        private SphereCollider detectionRange;

        private GameObject target;

        private Vector3 lastKnownPosition;

        #endregion

        #region Actions

        public event Action OnTargetChanged = delegate { };

        #endregion

        #region Accessors

        public Vector3 targetPosition => !target.IsNull() ? target.transform.position : Vector3.zero;

        public bool isTargetInRange => targetPosition != Vector3.zero;

        #endregion

        #region Unity Events

        private void Awake()
        {
            detectionRange = GetComponent<SphereCollider>();
            detectionRange.isTrigger = true;
            detectionRange.radius = detectionRadius;
        }

        #endregion


        #region Class Implementation

        public void CheckSurroundings(float _distance)
        {
            
        }

        public void UpdateTargetPosition(GameObject _target = null)
        {
            this.target = _target;

            if (!isTargetInRange)
            {
                return;
            }

            if (lastKnownPosition == targetPosition || lastKnownPosition == Vector3.zero)
            {
                return;
            }
            
            lastKnownPosition = targetPosition;
            OnTargetChanged?.Invoke();
        }

        #endregion


    }
}