using System;
using System.Collections.Generic;
using Data.Sides;
using NUnit.Framework;
using Project.Scripts.Utils;
using Runtime.GameControllers;
using Runtime.Gameplay;
using Runtime.VFX;
using UnityEngine;


namespace Runtime.Character
{
    
    [RequireComponent(typeof(CharacterController))]
    [DisallowMultipleComponent]
    public class CharacterSlideMovement: MonoBehaviour
    {
        
        #region Events

        private Action OnBeforeMovementCallback;
        
        //local action to use action point when done walking
        private Action OnFinishMovementCallback;

        #endregion
        
        #region SerializedFields
    
        [SerializeField] private float gravity;

        [SerializeField] private float moveForceBase = 1.5f, maxDistance = 4f, wallDetectDisThresh = 0.1f;
        
        [SerializeField] private GameObject movementRangeIndicator;

        [SerializeField] private GameObject moveDirectionIndicator;

        [SerializeField] private VFXPlayer knockbackParticles;

        [SerializeField] private LayerMask characterLayer, wallLayer, ballLayer;
        
        #endregion
        
        #region Private Fields

        private Vector3 currentVelocityRef;

        private Vector3 currentMoveDir;

        private Vector3 currentMovePoint;

        private Vector3 startPos;

        private Vector3 currentHitWallNormal;
        
        private bool canMove = false;
        
        private float timeToGetToPoint;

        private float startTime;

        private float currentScale = 1f;

        private bool isTackling, hasTackled;

        private bool hasEndedMovement;

        private float knockbackMod = 1f, slowDownMod = 1f;
        
        private float knockbackForce;

        private float maxForce, currentForce, savedForce;

        private float knockbackForceReductionMod = 1f;
        
        private float floorOffset = 0.1f;

        private float movementForceThresh = 0.1f;

        private Vector3 knockbackDir, lastFramePosition, directionFromLastPosition;

        private CharacterController characterControllerRef;
        
        private CharacterSide characterSideRef;
        
        private CharacterBase characterBaseRef;

        private Collider[] characterColliders = new Collider[20];
        
        private Collider[] ballColliders = new Collider[20];

        private List<Collider> recentlyHitCharacters = new List<Collider>();

        private int foundCharColliders, foundBalls;

        private Vector3 currentWallBounceNormal, currentWallBoundPosition;
        
        #endregion
    
        #region Accessors

        public float moveForce { get; private set; }

        public float maxMoveDistance { get; private set; }

        public int tackleDamage { get; private set; }
        
        public float maxGravity => gravity * 20f;

        private CharacterController characterController => CommonUtils.GetRequiredComponent(ref characterControllerRef, () =>
        {
            var cc = GetComponent<CharacterController>();
            return cc;
        });

        private CharacterSide characterSide => CommonUtils.GetRequiredComponent(ref characterSideRef, () =>
        {
            var s = GetComponent<CharacterBase>().side;
            return s;
        });
        
        private CharacterBase characterBase => CommonUtils.GetRequiredComponent(ref characterBaseRef, () =>
        {
            var s = GetComponent<CharacterBase>();
            return s;
        });
        
        public Vector3 velocity => currentVelocityRef;
    
        public bool isInBattle { get; private set; }

        public bool isRooted { get; private set; }
        public bool isPaused { get; private set; }

        public bool isUsingMoveAction => canMove;
        public bool isKnockedBack { get; private set; }

        public bool isMoving => currentForce > 0 && !isKnockedBack && !isPaused;

        public bool isInReaction { get; private set; }

        public int layerMask => LayerMask.GetMask("WallObstruction");
        
        #endregion

        #region Class Implementation
        
        /// <summary>
        /// Initializer for movement values. Done before match starts
        /// </summary>
        /// <param name="agilityScore">Read from character stats</param>
        /// <param name="maxTackleDamage">Read from character stats</param>
        public void InitializeCharacterMovement(float agilityScore, int maxTackleDamage)
        {
            maxMoveDistance = moveForceBase + ((agilityScore/100) * maxDistance);
            maxForce = 20f;
            tackleDamage = maxTackleDamage;
        }

        /// <summary>
        /// Run movement when character is using move action
        /// </summary>
        public void UpdateMovement()
        {
            if (currentForce <= 0 || !canMove)
            {
                return;
            }
            
            if (HasReachedDistance() || HasSetOffBackupCheck())
            {
                Debug.Log("Has Reached Distance");
                ReflectOnWall();
            }

            CheckForPlayers();
            CheckForBall();
            
            currentVelocityRef = currentMoveDir.normalized * (currentForce * Time.deltaTime);
            characterController.Move(currentVelocityRef);

            currentForce -= Time.deltaTime * slowDownMod;
            lastFramePosition = transform.position;
            
            if (!hasEndedMovement && currentForce <= 0)
            {
                OnEndMovement();
            }
        }

        private void OnEndMovement()
        {
            Debug.Log("End Movement called");
            isKnockedBack = false;
            canMove = false;
            hasEndedMovement = true;
            
            if (isInReaction)
            {
                isInReaction = false;
            }
                
            if (!knockbackParticles.IsNull())
            {
                knockbackParticles.Stop();
            }
            
            OnFinishMovementCallback?.Invoke();
        }
        
        /// <summary>
        /// Movement selection visuals
        /// </summary>
        /// <param name="_position">User Input, hover position</param>
        public void MarkMovementLocation(Vector3 _position)
        {
            if (moveDirectionIndicator.IsNull())
            {
                return;
            }
            
            var distanceToPos = _position - transform.position;
           
            if (distanceToPos.magnitude >= maxMoveDistance)
            {
                return;
            }
            
            //ToDo: make something that shows direction and strength
            moveDirectionIndicator.transform.position = new Vector3(_position.x, floorOffset, _position.z);
        }

        /// <summary>
        /// User input selected position. Flick character towards position. Force = percentage from selected position and max distance
        /// </summary>
        /// <param name="selectedPosition">User Input</param>
        public void MoveCharacter(Vector3 selectedPosition)
        {
            if (!canMove || isMoving)
            {
                Debug.Log($"<color=cyan>Character can't move m_canMove:{canMove} // isMoving{isMoving} </color>");
                return;
            }

            currentMoveDir = (selectedPosition - transform.position).FlattenVector3Y().normalized;
            currentForce = (currentMoveDir.magnitude / maxDistance) * maxForce;
            hasEndedMovement = false;
            Debug.Log("Has Direction", gameObject);
            
            FindNextWallHitPoint(currentMoveDir);

            SetIndicators(false);
            //ToDo: Flick character

            Debug.Log("Has Flicked Character in Direction");
            Debug.Log($"Direction: {currentMoveDir} /// Force:{currentForce} ");
            
            OnBeforeMovementCallback?.Invoke();
        }
    
        private void DoGravity()
        {
            currentVelocityRef.y -= gravity * Time.deltaTime;
        
            characterController.Move(Vector3.ClampMagnitude(currentVelocityRef, maxGravity) * Time.deltaTime);
        }

        public void SetCharacterBattleStatus(bool _isInBattle)
        {
            isInBattle = _isInBattle;
        }

        public void UpdateTackleDamage(int _newAmount)
        {
            tackleDamage = _newAmount;
        }

        public void PauseMovement(bool _isPaused)
        {
            isPaused = _isPaused;
        }

        public void ForceStopMovement(bool _isInReaction)
        {
            isInReaction = _isInReaction;

            canMove = false;

            currentMoveDir = Vector3.zero;

            if (!isInReaction)
            {
                OnFinishMovementCallback?.Invoke();
            }

            if (isPaused)
            {
                PauseMovement(false);
            }
        }

        public void SetKnockbackMod(float _newModifier)
        {
            knockbackMod = _newModifier;
        }

        public void ApplyKnockback(float knockbackForce, Vector3 direction)
        {
            if (isRooted)
            {
                return;
            }

            currentMoveDir = direction.normalized;
            currentForce = knockbackForce;
            hasEndedMovement = false;
            Debug.Log("Has Direction", gameObject);
            
            FindNextWallHitPoint(currentMoveDir);
            
            Debug.DrawRay(transform.position, knockbackDir.normalized * knockbackForce, Color.yellow, 10f);
            isKnockedBack = true;
            canMove = true;

            if (knockbackParticles.IsNull())
            {
                return;
            }
            
            knockbackParticles.transform.forward = -direction;
            knockbackParticles.Play();
        }

        private void CheckKnockback()
        {
            if (!isKnockedBack || isPaused)
            {
                return;
            }

            if (knockbackForce > 0)
            {
                knockbackForce -= Time.deltaTime * knockbackForceReductionMod;
            }else
            {
                isKnockedBack = false;
                canMove = false;
                
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
            
            Vector3 knockbackVelocity = knockbackDir.normalized * knockbackForce;
            characterController.Move(knockbackVelocity * Time.deltaTime);
        }
        
        public void SetCharacterMovable(bool canMove, Action _beginningAction = null ,Action _finishActionCallback = null)
        {
            if (isRooted)
            {
                return;
            }
            
            this.canMove = canMove;

            SetIndicators(canMove);

            if (!canMove)
            {
                return;
            }
            
            movementRangeIndicator.transform.localScale = Vector3.one * (maxMoveDistance * 2);
            
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
            maxMoveDistance = _newRange;
        }

        public void ResetMoveForce()
        {
            currentForce = savedForce;
        }

        private void SetIndicators(bool _isActive)
        {
            if (movementRangeIndicator.IsNull() || moveDirectionIndicator.IsNull())
            {
                return;
            }
            
            movementRangeIndicator.SetActive(_isActive);
            moveDirectionIndicator.SetActive(_isActive);
        }
        
        private void CheckForPlayers()
        {
            foundCharColliders = Physics.OverlapSphereNonAlloc(transform.position, 0.5f, characterColliders, characterLayer);

            if (foundCharColliders == 0)
            {
               return; 
            }

            Vector3 _reflectedPoint = transform.position;
            
            for (int i = 0; i < foundCharColliders; i++)
            {
                //If the detected collider is 
                if (characterColliders[i] == characterController || recentlyHitCharacters.Contains(characterColliders[i]))
                {
                    continue;
                }

                characterColliders[i].TryGetComponent(out CharacterBase _character);
                
                if (_character.IsNull())
                {
                    continue;
                }

                _reflectedPoint = _character.transform.position;
                _character.OnDealDamage(this.transform, tackleDamage, false, null, this.transform ,true);
                ReflectOnCharacter(_reflectedPoint);
                recentlyHitCharacters.Add(characterColliders[i]);
                TickGameController.Instance.CreateNewTimer("Hit_Player", 0.7f, false, RemoveCharacterFromRecentlyHit);
            }
            
        }
        
        private void RemoveCharacterFromRecentlyHit()
        {
            if (recentlyHitCharacters.Count == 0)
            {
                return;
            }
                    
            recentlyHitCharacters.RemoveAt(0);
        }

        private void CheckForBall()
        {
            foundBalls = Physics.OverlapSphereNonAlloc(transform.position, 0.5f, ballColliders, ballLayer);

            if (foundBalls == 0)
            {
                return; 
            }
            
            for (int i = 0; i < foundBalls; i++)
            {
                //If the detected collider is 
                ballColliders[i].TryGetComponent(out BallBehavior _ball);
                
                if (_ball.IsNull())
                {
                    continue;
                }

                var dir = (_ball.transform.position - transform.position).FlattenVector3Y().normalized;
                
                //ToDo: change throw force and thrown ball stat
                _ball.ThrowBall(dir, 10, true, this.characterBase, 100);
            }
            
        }
        
        private bool HasReachedDistance()
        {
            return Vector3.SqrMagnitude(transform.position - currentWallBoundPosition) <= wallDetectDisThresh * currentScale;
        }
        
        private bool HasSetOffBackupCheck()
        {
            directionFromLastPosition = transform.position - lastFramePosition;
            return Physics.Raycast(lastFramePosition, directionFromLastPosition, directionFromLastPosition.magnitude, wallLayer);
        }

        private void ReflectOnCharacter(Vector3 _reflectPoint)
        {
            currentMoveDir = Vector3.Reflect(currentMoveDir, transform.position - _reflectPoint);
            FindNextWallHitPoint(currentMoveDir);
        }
        
        private void FindNextWallHitPoint(Vector3 _direction)
        {
            if (!Physics.Raycast(transform.position, _direction, out RaycastHit _hit, Mathf.Infinity, wallLayer))
            {
                Debug.Log($"Something Wrong: {_direction}");
                return;
            }

            currentWallBoundPosition = _hit.point;
            currentWallBounceNormal = _hit.normal;
        }
        
        private void ReflectOnWall()
        {
            //PlayWallHitSFX();
            
            currentMoveDir = Vector3.Reflect(currentMoveDir, currentWallBounceNormal);
            FindNextWallHitPoint(currentMoveDir);
        }

        #endregion
        
    }
}