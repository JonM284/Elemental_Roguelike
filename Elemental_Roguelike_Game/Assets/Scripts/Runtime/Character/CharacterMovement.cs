using System;
using System.Linq;
using Data;
using Project.Scripts.Utils;
using Runtime.Environment;
using Runtime.GameControllers;
using Runtime.Selection;
using UnityEngine;
using UnityEngine.AI;

namespace Runtime.Character
{
    [RequireComponent(typeof(CharacterController))]
    [DisallowMultipleComponent]
    public class CharacterMovement : MonoBehaviour
    {
        #region Events

        private Action OnBeforeMovementCallback;
        
        //local action to use action point when done walking
        private Action OnFinishMovementCallback;

        #endregion
    
        #region SerializedFields
    
        [SerializeField] private float gravity;

        [SerializeField] private LayerMask obstacleLayer;

        [SerializeField] private GameObject movementRangeIndicator;
        
        #endregion

        #region Private Fields

        private Vector3 _velocity;

        private Vector3 m_currentMovePoint;

        private Vector3 m_startPos;

        private Vector3 m_finalPos;

        private int m_currentMovePointIndex;
    
        private bool m_canMove = false;

        private bool m_isMovingOnPath = false;

        private float m_timeToGetToPoint;

        private float m_startTime;

        private bool m_isPerformingMelee;
        
        private CharacterController m_characterController;
    
        private NavMeshPath m_navMeshPath;
        
        #endregion
    
        #region Accessors

        public float speed { get; private set; }

        public float battleMoveDistance { get; private set; }

        public float maxGravity => gravity * 20f;

        private CharacterController characterController => CommonUtils.GetRequiredComponent(ref m_characterController, () =>
        {
            var cc = GetComponent<CharacterController>();
            return cc;
        });
        
        public Vector3 velocity => _velocity;
    
        public bool isInBattle { get; private set; }

        public bool isMoving => m_isMovingOnPath;

        public bool isUsingMoveAction => m_canMove;
        

        #endregion

        #region Unity Events

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, battleMoveDistance);
            if (m_navMeshPath != null)
            {
                if (m_navMeshPath.corners.Length > 0)
                {
                    for (int i = 0; i < m_navMeshPath.corners.Length; i++)
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawWireSphere(m_navMeshPath.corners[i], 0.5f);
                        if (i > 0)
                        {
                            Gizmos.DrawLine(m_navMeshPath.corners[i-1], m_navMeshPath.corners[i]);
                        }
                    }
                    
                }
            }
            
        }

        private void Update()
        {
            if (!m_canMove)
            {
                return;
            }
            
            if (!characterController.isGrounded)
            {
                DoGravity();
            }
            else if(characterController.isGrounded)
            {
                HandleMovement();
            }
        }

        #endregion
    
        #region Class Implementation

        public void InitializeCharacterMovement(float _speed, float _moveDistance)
        {
            speed = _speed;
            battleMoveDistance = _moveDistance;
        }

        public void MoveCharacter(Vector3 _movePosition)
        {
            if (!m_canMove || isMoving)
            {
                Debug.Log($"<color=cyan>Character can't move m_canMove:{m_canMove} // isMoving{isMoving} </color>");
                return;
            }

            if (m_navMeshPath == null)
            {
                m_navMeshPath = new NavMeshPath();
            }
            
            //Clear any previous paths
            if (m_navMeshPath.corners.Length > 0)
            {
                m_navMeshPath.ClearCorners();
            }

            var adjustedEndPos = _movePosition;

            NavMesh.SamplePosition(transform.position, out NavMeshHit playerPosition, 100, NavMesh.AllAreas);
            var hasPath = NavMesh.CalculatePath(playerPosition.position, adjustedEndPos, NavMesh.AllAreas, m_navMeshPath);
            if (!hasPath)
            {
                Debug.Log("<color=red>No Path</color>");
                return;
            }

            Debug.Log("Has Created Path");
            
            movementRangeIndicator.SetActive(false);

            m_currentMovePointIndex = 1;
            m_currentMovePoint = m_navMeshPath.corners[m_currentMovePointIndex];
            m_startPos = playerPosition.position;
            m_finalPos = _movePosition;
            m_isMovingOnPath = true;
            OnBeforeMovementCallback?.Invoke();
        }
    
        private void DoGravity()
        {
            _velocity.y -= gravity * Time.deltaTime;
        
            characterController.Move(Vector3.ClampMagnitude(_velocity, maxGravity) * Time.deltaTime);
        }

        private void HandleMovement()
        {
            if (!m_isMovingOnPath)
            {
                return;
            }

            var diff = m_currentMovePoint - transform.position.FlattenVector3Y();
            if (Vector3.Distance(transform.position.FlattenVector3Y(), m_currentMovePoint.FlattenVector3Y()) > 0.1f) {
                
                _velocity = Vector3.Normalize(diff).FlattenVector3Y();
                _velocity *= speed * Time.deltaTime;
            } else
            {
                m_currentMovePointIndex++;
                if (m_currentMovePointIndex < m_navMeshPath.corners.Length)
                {
                    m_currentMovePoint = m_navMeshPath.corners[m_currentMovePointIndex];
                    m_startPos = m_navMeshPath.corners[m_currentMovePointIndex - 1];
                }
                else
                {
                    m_isMovingOnPath = false;
                    m_canMove = false;    

                    OnFinishMovementCallback?.Invoke();
                    
                    OnFinishMovementCallback = null;
                    OnBeforeMovementCallback = null;
                    
                    return;
                }
            }
            
            characterController.Move(Vector3.ClampMagnitude(_velocity, speed));
        }

        public void SetCharacterBattleStatus(bool _isInBattle)
        {
            isInBattle = _isInBattle;
        }
        
        public void SetCharacterMovable(bool _canMove, Action _beginningAction = null ,Action _finishActionCallback = null)
        {
            m_canMove = _canMove;
            
            movementRangeIndicator.SetActive(_canMove);

            if (!_canMove)
            {
                return;
            }
            
            movementRangeIndicator.transform.localScale = Vector3.one * (battleMoveDistance * 2);
            
            if (_beginningAction != null)
            {
                OnBeforeMovementCallback = _beginningAction;
            }
            
            if (_finishActionCallback != null)
            {
                OnFinishMovementCallback = _finishActionCallback;
            }
        }

        public void TeleportCharacter(Vector3 teleportPosition)
        {
            if (m_navMeshPath != null && m_navMeshPath.corners.Length > 0)
            {
                m_navMeshPath.ClearCorners();
            }

            if (m_isMovingOnPath)
            {
                m_isMovingOnPath = false;
            }
            
            var _fixedTeleportPos = new Vector3(teleportPosition.x, teleportPosition.y + transform.localScale.y/1.35f, teleportPosition.z);
            characterController.enabled = false;
            transform.position = _fixedTeleportPos;
            characterController.enabled = true;
        }

        #endregion


    }
}
