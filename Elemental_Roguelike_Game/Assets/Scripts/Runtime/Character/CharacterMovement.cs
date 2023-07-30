using System;
using System.Linq;
using Data;
using Data.Elements;
using Project.Scripts.Utils;
using Runtime.Damage;
using Runtime.Environment;
using Runtime.GameControllers;
using Runtime.Gameplay;
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
        
        [SerializeField] private GameObject movementRangeIndicator;

        [SerializeField] private GameObject movementPositionIndicator;
        
        #endregion

        #region Private Fields

        private Vector3 _velocity;

        private Vector3 m_currentMovePoint;

        private Vector3 m_startPos;

        private Vector3 m_finalPos;

        private int m_currentMovePointIndex;
    
        private bool m_canMove = false;

        private bool m_isMovingOnPath = false;

        private bool m_isPaused = false;

        private float m_timeToGetToPoint;

        private float m_startTime;

        private bool m_isPerformingMelee;

        private bool m_hasPerformedMelee;

        private bool m_isKnockedBack;

        private float m_knockbackTimer;

        private float m_knockbackForce;

        private float m_originalMoveDistance;

        private float m_floorOffset = 0.1f;

        private Vector3 m_knockbackDir;

        private CharacterController m_characterController;
    
        private NavMeshPath m_navMeshPath;
        
        #endregion
    
        #region Accessors

        public float speed { get; private set; }

        public float battleMoveDistance { get; private set; }

        public int tackleDamage { get; private set; }

        public ElementTyping tackleDamageType { get; private set; }

        public float maxGravity => gravity * 20f;

        private CharacterController characterController => CommonUtils.GetRequiredComponent(ref m_characterController, () =>
        {
            var cc = GetComponent<CharacterController>();
            return cc;
        });
        
        public Vector3 velocity => _velocity;
    
        public bool isInBattle { get; private set; }

        public bool isMoving => m_isMovingOnPath && !isPaused;

        public bool isPaused => m_isPaused;

        public bool isUsingMoveAction => m_canMove;

        #endregion

        #region Unity Events

        private void Update()
        {
            if (!m_canMove)
            {
                if (!characterController.isGrounded)
                {
                    DoGravity();
                }
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

            CheckKnockback();
        }

        #endregion
    
        #region Class Implementation

        public void InitializeCharacterMovement(float _speed, float _moveDistance, int _tackleDamage, ElementTyping _damageType)
        {
            speed = _speed;
            battleMoveDistance = _moveDistance;
            m_originalMoveDistance = _moveDistance;
            tackleDamage = _tackleDamage;
            tackleDamageType = _damageType;
        }

        public void MarkMovementLocation(Vector3 _position)
        {
            if (movementPositionIndicator.IsNull())
            {
                return;
            }

            var distanceToPos = _position - transform.position;
            if (distanceToPos.magnitude >= battleMoveDistance)
            {
                return;
            }
            
            movementPositionIndicator.transform.position = new Vector3(_position.x, m_floorOffset, _position.z);
        }

        public void MoveCharacter(Vector3 _movePosition, bool _isTackle)
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

            SetIndicators(false);
            m_isPerformingMelee = _isTackle;
            m_currentMovePointIndex = 1;
            m_currentMovePoint = m_navMeshPath.corners[m_currentMovePointIndex];
            m_startPos = playerPosition.position;
            m_finalPos = _movePosition;
            m_isMovingOnPath = true;
            
            Debug.Log("Setting to moving on path");
            
            OnBeforeMovementCallback?.Invoke();
        }
    
        private void DoGravity()
        {
            _velocity.y -= gravity * Time.deltaTime;
        
            characterController.Move(Vector3.ClampMagnitude(_velocity, maxGravity) * Time.deltaTime);
        }

        private void HandleMovement()
        {
            if (!m_isMovingOnPath || m_isPaused)
            {
                return;
            }

            if (m_isPerformingMelee)
            {
                if (m_currentMovePointIndex == m_navMeshPath.corners.Length - 1)
                {
                    var percentage = (transform.position - m_startPos).magnitude / (m_finalPos - m_startPos).magnitude;

                    if (percentage > 0.9)
                    {
                        if (!m_hasPerformedMelee)
                        {
                            PerformMelee();
                        }    
                    }
                    
                }
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
                    ForceStopMovement();
                    return;
                }
            }
            
            characterController.Move(Vector3.ClampMagnitude(_velocity, speed));
        }

        public void SetCharacterBattleStatus(bool _isInBattle)
        {
            isInBattle = _isInBattle;
        }

        public void PauseMovement(bool _isPaused)
        {
            m_isPaused = _isPaused;
        }

        public void ForceStopMovement()
        {
            m_isMovingOnPath = false;
            m_canMove = false;

            OnFinishMovementCallback?.Invoke();
                    
            OnFinishMovementCallback = null;
            OnBeforeMovementCallback = null;

            m_currentMovePointIndex = 0;
            m_navMeshPath.ClearCorners();

            if (m_isPaused)
            {
                PauseMovement(false);
            }
                    
            if (m_isPerformingMelee)
            {
                m_isPerformingMelee = false;
                m_hasPerformedMelee = false;
            }

            if (battleMoveDistance > m_originalMoveDistance)
            {
                battleMoveDistance = m_originalMoveDistance;
            }
        }

        public void ApplyKnockback(float _knockbackForce, Vector3 _direction, float _duration)
        {
            m_knockbackTimer = _duration;
            m_knockbackDir = _direction;
            m_knockbackForce = _knockbackForce;
            m_isKnockedBack = true;
            m_canMove = true;
        }

        private void CheckKnockback()
        {
            if (!m_isKnockedBack)
            {
                return;
            }

            if (m_knockbackTimer > 0)
            {
                m_knockbackTimer -= Time.deltaTime;
            }else if (m_knockbackTimer <= 0)
            {
                m_isKnockedBack = false;
                m_canMove = false;
            }
            
            Vector3 knockbackVelocity = m_knockbackDir.normalized * m_knockbackForce;
            characterController.Move(knockbackVelocity * Time.deltaTime);
        }
        
        public void SetCharacterMovable(bool _canMove, Action _beginningAction = null ,Action _finishActionCallback = null)
        {
            m_canMove = _canMove;

            SetIndicators(_canMove);

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

        public void ChangeMovementRange(float _newRange)
        {
            battleMoveDistance = _newRange;
        }

        public void ResetOriginalMoveDistance()
        {
            battleMoveDistance = m_originalMoveDistance;
        }

        private void SetIndicators(bool _isActive)
        {
            if (movementRangeIndicator.IsNull() || movementPositionIndicator.IsNull())
            {
                return;
            }
            
            movementRangeIndicator.SetActive(_isActive);
            movementPositionIndicator.SetActive(_isActive);
        }
        
        private void PerformMelee()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, 1);

            if (colliders.Length > 0)
            {
                foreach (var collider in colliders)
                {
                    if (collider == characterController)
                    {
                        continue;
                    }

                    collider.TryGetComponent(out IDamageable damageable);
                    if (!damageable.IsNull())
                    {
                        damageable?.OnDealDamage(this.transform, tackleDamage, false, tackleDamageType, true);
                    }
                }
            }

            m_hasPerformedMelee = true;
        }

        #endregion


    }
}
