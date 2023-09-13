using System;
using System.Collections.Generic;
using Project.Scripts.Utils;
using UnityEngine;

namespace Runtime.GameControllers
{
    public class MoveableController : GameControllerBase
    {

        #region Static

        public static MoveableController Instance { get; private set; }

        #endregion

        #region Nested Classes

        [Serializable]
        public class MovementObject
        {
            public Transform moveObj;
            public Vector3 moveToPos;
            public Vector3 moveToForward;
            public float percentage;
            public float m_currentTime;
            public bool m_isFinished;
            public Action onFinishCallBack;
        }

        #endregion

        #region Private Fields

        private List<MovementObject> m_moveables = new List<MovementObject>();
        
        private List<MovementObject> m_finishedMoveables = new List<MovementObject>();
        
        private float m_maxTime = 0.5f;

        #endregion

        #region Unity Events

        private void Update()
        {
            if (m_moveables.Count == 0)
            {
                return;
            }

            foreach (var currentMoveable in m_moveables)
            {
                currentMoveable.percentage = currentMoveable.m_currentTime / m_maxTime;
                if (currentMoveable.percentage <= 0.99)
                {
                    currentMoveable.m_currentTime += Time.deltaTime;
                    //Position
                    currentMoveable.moveObj.position = Vector3.Lerp(currentMoveable.moveObj.position,
                        currentMoveable.moveToPos, currentMoveable.percentage);
                    
                    //Rotation
                    currentMoveable.moveObj.forward = Vector3.Lerp(currentMoveable.moveObj.forward,
                        currentMoveable.moveToForward, currentMoveable.percentage);
                }
                else
                {
                    currentMoveable.moveObj.position = currentMoveable.moveToPos;

                    currentMoveable.moveObj.forward = currentMoveable.moveToForward;

                    currentMoveable.m_isFinished = true;
                    
                    m_finishedMoveables.Add(currentMoveable);
                }
            }

            if (m_finishedMoveables.Count > 0)
            {
                foreach (var finishedMoveable in m_finishedMoveables)
                {
                    finishedMoveable.onFinishCallBack?.Invoke();
                    m_moveables.Remove(finishedMoveable);
                }
            
                m_finishedMoveables.Clear();    
            }
            

        }

        #endregion

        #region Class Implementation

        public void CreateNewMoveable(Transform moveable, Vector3 finalPos, Vector3 finalRot, Action _onFinishMovement = null)
        {
            var newMoveable = new MovementObject
            {
                m_currentTime = 0,
                m_isFinished = false,
                percentage = 0, 
                moveObj = moveable, 
                moveToPos = finalPos, 
                moveToForward = finalRot
            };

            if (!_onFinishMovement.IsNull())
            {
                newMoveable.onFinishCallBack = _onFinishMovement;
            }
            
            m_moveables.Add(newMoveable);
        }

        #endregion

        #region Game Controller Base Inherited Methods

        public override void Initialize()
        {
            if (!Instance.IsNull())
            {
                return;
            }
            
            Instance = this;
            base.Initialize();
        }

        #endregion
       
                
    }
}