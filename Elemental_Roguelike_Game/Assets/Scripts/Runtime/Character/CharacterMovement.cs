using System;
using Data.Elements;
using Data.Sides;
using Project.Scripts.Utils;
using Runtime.Damage;
using Runtime.GameControllers;
using Runtime.VFX;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

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

        [SerializeField] private VFXPlayer knockbackParticles;
        
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

        private float m_knockbackMod = 1f;

        private float m_knockbackTimer;

        private float m_knockbackForce;

        private float m_originalMoveDistance;

        private bool m_isElementalTackle;

        private bool m_isGoalie;
        
        private float m_floorOffset = 0.1f;

        private Vector3 m_knockbackDir;

        private CharacterController m_characterController;
    
        private NavMeshPath m_navMeshPath;

        private CharacterSide m_characterSide;
        
        private Transform m_goalPivotTransform;

        private CharacterBase m_characterBase;
        
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

        private CharacterSide characterSide => CommonUtils.GetRequiredComponent(ref m_characterSide, () =>
        {
            var s = GetComponent<CharacterBase>().side;
            return s;
        });
        
        private CharacterBase characterBase => CommonUtils.GetRequiredComponent(ref m_characterBase, () =>
        {
            var s = GetComponent<CharacterBase>();
            return s;
        });
        
        public Transform pivotTransform => CommonUtils.GetRequiredComponent(ref m_goalPivotTransform, () =>
        {
            var t = TurnController.Instance.GetTeamManager(characterSide).goalPosition.transform;
            return t;
        });
        
        public Vector3 velocity => _velocity;
    
        public bool isInBattle { get; private set; }

        public bool isMoving => m_isMovingOnPath && !isKnockedBack && !isPaused;

        public bool isRooted { get; private set; }

        public bool isPaused => m_isPaused;

        public bool isUsingMoveAction => m_canMove;

        public bool isKnockedBack => m_isKnockedBack;

        public bool isInReaction { get; private set; }

        public int layerMask => LayerMask.GetMask("WallObstruction");
        
        #endregion

        #region Unity Events

        private void OnDrawGizmos()
        {
            if (m_isPerformingMelee)
            {
                Gizmos.DrawWireSphere(m_finalPos, 0.5f);
            }
        }

        private void Update()
        {
            if (!characterController.isGrounded)
            {
                DoGravity();
            }
            
            if (!m_canMove)
            {
                if (!characterController.isGrounded)
                {
                    DoGravity();
                }
                return;
            }
            
            
            if(characterController.isGrounded)
            {
                HandleMovement();
            }

            CheckKnockback();
        }

        #endregion
    
        #region Class Implementation

        public void InitializeCharacterMovement(float _speed, float _moveDistance, int _tackleDamage, ElementTyping _damageType, bool _isGoalie)
        {
            speed = _speed;
            battleMoveDistance = _moveDistance;
            m_originalMoveDistance = _moveDistance;
            tackleDamage = _tackleDamage;
            tackleDamageType = _damageType;
            m_isGoalie = _isGoalie;
            
            if (m_isGoalie)
            {
                movementRangeIndicator.transform.parent = pivotTransform;
                movementRangeIndicator.transform.position = new Vector3(pivotTransform.position.x, transform.position.y,
                    pivotTransform.position.z);
            }
        }

        public void MarkMovementLocation(Vector3 _position)
        {
            if (movementPositionIndicator.IsNull())
            {
                return;
            }

            var pivotPos = m_isGoalie ? pivotTransform.position : transform.position;

            var distanceToPos = _position - pivotPos;
           
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

            Debug.Log("Has Created Path", gameObject);

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
                    ForceStopMovement(false);
                    return;
                }
            }
            
            characterController.Move(Vector3.ClampMagnitude(_velocity, speed));
        }

        public void SetCharacterBattleStatus(bool _isInBattle)
        {
            isInBattle = _isInBattle;
        }

        public void SetElementTackle(bool _isElementTackle)
        {
            m_isElementalTackle = _isElementTackle;
        }

        public void UpdateTackleDamage(int _newAmount)
        {
            tackleDamage = _newAmount;
        }

        public void PauseMovement(bool _isPaused)
        {
            m_isPaused = _isPaused;
        }

        public void ForceStopMovement(bool _isInReaction)
        {
            isInReaction = _isInReaction;
            
            if (!m_navMeshPath.IsNull() && m_navMeshPath.corners.Length > 0)
            {
                m_navMeshPath.ClearCorners();
            }

            m_isMovingOnPath = false;
            m_canMove = false;

            _velocity = Vector3.zero;

            if (!isInReaction)
            {
                OnFinishMovementCallback?.Invoke();
            }

            m_currentMovePointIndex = 0;
            m_currentMovePoint = transform.position;
            m_finalPos = transform.position;

            if (m_isPaused)
            {
                PauseMovement(false);
            }
                    
            if (m_isPerformingMelee)
            {
                m_isPerformingMelee = false;
                m_hasPerformedMelee = false;
            }
        }

        public void SetKnockbackable(float _newModifier)
        {
            m_knockbackMod = _newModifier;
        }

        public void ApplyKnockback(float _knockbackForce, Vector3 _direction, float _duration)
        {
            if (isRooted)
            {
                return;
            }

            m_knockbackTimer = _duration;
            m_knockbackDir = _direction;
            m_knockbackForce = (_knockbackForce * m_knockbackMod);
            Debug.DrawRay(transform.position, m_knockbackDir.normalized * m_knockbackForce, Color.yellow, 10f);
            m_isKnockedBack = true;
            m_canMove = true;
            
            if (!knockbackParticles.IsNull())
            {
                knockbackParticles.transform.forward = -_direction;
                knockbackParticles.Play();
            }
        }

        private void CheckKnockback()
        {
            if (!m_isKnockedBack || isPaused)
            {
                return;
            }

            if (m_knockbackTimer > 0)
            {
                if (Physics.Raycast(transform.position, m_knockbackDir, 0.5f, layerMask))
                {
                    HitElectricFence();
                }
                
                m_knockbackTimer -= Time.deltaTime;
            }else if (m_knockbackTimer <= 0)
            {
                m_isKnockedBack = false;
                m_canMove = false;
                
                if (isInReaction)
                {
                    isInReaction = false;
                    OnFinishMovementCallback?.Invoke();
                }
                
                if (!knockbackParticles.IsNull())
                {
                    knockbackParticles.Stop();
                }
            }
            
            Vector3 knockbackVelocity = m_knockbackDir.normalized * m_knockbackForce;
            characterController.Move(knockbackVelocity * Time.deltaTime);
        }
        
        public void SetCharacterMovable(bool _canMove, Action _beginningAction = null ,Action _finishActionCallback = null)
        {
            if (isRooted)
            {
                return;
            }
            
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

        public void SetCharacterRooted(bool _isRooted)
        {
            isRooted = _isRooted;
        }

        public void TeleportCharacter(Vector3 teleportPosition)
        {
            if (!m_navMeshPath.IsNull() && m_navMeshPath.corners.Length > 0)
            {
                m_navMeshPath.ClearCorners();
            }

            if (m_isMovingOnPath)
            {
                m_isMovingOnPath = false;
            }
            
            var _fixedTeleportPos = new Vector3(teleportPosition.x, teleportPosition.y + transform.localScale.y/2, teleportPosition.z);
            characterController.enabled = false;
            transform.position = _fixedTeleportPos;
            characterController.enabled = true;
        }

        public void ResetCharacter(Vector3 _position)
        {
            characterController.enabled = false;
            transform.position = _position;
            characterController.enabled = true;
            ForceStopMovement(false);
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

        private void HitElectricFence()
        {
            Debug.Log($"<color=cyan>{this.gameObject.name} has hit electric fence</color>");
            
            m_isKnockedBack = false;
            m_canMove = false;

            m_knockbackTimer = 0;
            
            if (isInReaction)
            {
                isInReaction = false;
                OnFinishMovementCallback?.Invoke();
            }
                
            if (!knockbackParticles.IsNull())
            {
                knockbackParticles.Stop();
            }

            characterBase.HitFence();
        }
        
        private void PerformMelee()
        {
            Collider[] colliders = Physics.OverlapSphere(m_finalPos, 0.5f);

            if (colliders.Length > 0)
            {
                foreach (var collider in colliders)
                {
                    if (collider == characterController)
                    {
                        continue;
                    }

                    collider.TryGetComponent(out CharacterBase _character);
                    if (_character)
                    {
                        if (_character.side == this.characterSide)
                        {
                            continue;
                        }
                    }
                    
                    collider.TryGetComponent(out IDamageable damageable);
                    if (!damageable.IsNull())
                    {
                        var appliableElement = m_isElementalTackle ? tackleDamageType : null;
                        damageable?.OnDealDamage(this.transform, tackleDamage, false, appliableElement, this.transform ,true);
                    }
                }
            }

            m_hasPerformedMelee = true;
        }

        #endregion


    }
}
