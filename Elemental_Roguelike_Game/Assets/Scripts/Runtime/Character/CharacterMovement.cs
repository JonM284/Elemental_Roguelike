using System;
using Data;
using Project.Scripts.Utils;
using UnityEngine;
using UnityEngine.AI;

namespace Runtime.Character
{
    [RequireComponent(typeof(CharacterController))]
    [DisallowMultipleComponent]
    public class CharacterMovement : MonoBehaviour
    {
    
        #region SerializedFields
    
        [SerializeField] private float gravity;

        #endregion

        #region Private Fields

        private Vector3 _velocity;

        private Vector3 m_currentMovePoint;

        private Vector3 m_startPos;

        private Vector3 m_finalPos;

        private int m_currentMovePointIndex;
    
        private bool m_canMove = true;

        private bool m_isMovingOnPath;

        private float m_timeToGetToPoint;

        private float m_startTime;

        private CharacterStatsData m_characterStats;
    
        private CharacterController m_characterController;
    
        private NavMeshPath m_navMeshPath;

        #endregion
    
        #region Accessors

        private float speed => m_characterStats != null ? m_characterStats.baseSpeed : 1f;

        public float battleMoveDistance => m_characterStats != null ? m_characterStats.movementDistance : 1f;

        public float maxGravity => gravity * 20f;

        private CharacterController characterController => CommonUtils.GetRequiredComponent(ref m_characterController, () =>
        {
            var cc = GetComponent<CharacterController>();
            return cc;
        });

        public Vector3 velocity => _velocity;
    
        public bool isInBattle { get; private set; }
    
        public Vector3 relativeRight { get; private set; }

        public Vector3 relativeForward { get; private set; }

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

        public void InitializeCharacterMovement(CharacterStatsData _stats)
        {
            m_characterStats = _stats;
        }

        public void MoveCharacter(Vector3 _movePosition)
        {
            if (m_navMeshPath == null)
            {
                m_navMeshPath = new NavMeshPath();
            }
            
            //Clear any previous paths
            if (m_navMeshPath.corners.Length > 0)
            {
                m_navMeshPath.ClearCorners();
            }

            var sourcePos = NavMesh.SamplePosition(transform.position, out NavMeshHit playerPosition, 100, NavMesh.AllAreas);
            var hasPath = NavMesh.CalculatePath(playerPosition.position, _movePosition, NavMesh.AllAreas, m_navMeshPath);
            if (!hasPath)
            {
                Debug.Log("<color=red>No Path</color>");
                return;
            }

            Debug.Log("Has Created Path");
            m_currentMovePointIndex = 1;
            m_currentMovePoint = m_navMeshPath.corners[m_currentMovePointIndex];
            m_startPos = playerPosition.position;
            m_finalPos = _movePosition;
            m_isMovingOnPath = true;

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
                    return;
                }
            }
            
            characterController.Move(Vector3.ClampMagnitude(_velocity, speed));
        }

        [ContextMenu("Battle On")]
        public void SetCharacterBattleStatus()
        {
            isInBattle = true;
        }

        [ContextMenu("Active char")]
        public void SetCharacterActiveCharacter()
        {
            m_canMove = true;
        }

        public void SetCharacterInactive()
        {
            m_canMove = false;
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
            
            var _fixedTeleportPos = new Vector3(teleportPosition.x, transform.localScale.y / 2, teleportPosition.z);
            characterController.enabled = false;
            transform.position = _fixedTeleportPos;
            characterController.enabled = true;
        }

        #endregion

        #region Unused

        /* This is used if going back to character controller movement
     
    private void InitializeCharacterMovement()
    {
        relativeRight = mainCamera.transform.right.normalized.FlattenVector3Y();
        relativeForward = mainCamera.transform.forward.normalized.FlattenVector3Y();
    }
     
    private void DoGravity()
    {
        _velocity.y -= gravity * Time.deltaTime;
        
        _characterController.Move(Vector3.ClampMagnitude(_velocity, maxGravity) * Time.deltaTime);
    }

    private void FreeMove()
    {
        var _xInput = Input.GetAxisRaw("Horizontal") * speed;
        var _yInput = Input.GetAxisRaw("Vertical") * speed;
        
        _velocity = Vector3.Normalize(relativeRight * _xInput + relativeForward * _yInput).FlattenVector3Y();
        _velocity *= speed * Time.deltaTime;

        _characterController.Move(Vector3.ClampMagnitude(_velocity, speed));
    }

    private void BattleMove()
    {
        FreeMove();
        float mag = Vector3.Magnitude(pivotPosition - transform.position);
        if (mag > battleMoveDistance)
        {
            var dirFromCenter = (transform.position - pivotPosition).normalized;
            var farthestPointInDir = dirFromCenter * battleMoveDistance;
            TeleportCharacter(farthestPointInDir);
        }
    }
    */

        #endregion

   
    }
}
