using System;
using System.Collections;
using Data.CharacterData;
using Data.Elements;
using Data.Sides;
using Project.Scripts.Utils;
using Runtime.Damage;
using Runtime.GameControllers;
using Runtime.Gameplay;
using Runtime.Selection;
using Runtime.Status;
using Runtime.VFX;
using UnityEngine;
using UnityEngine.Events;
using Utils;
using Random = UnityEngine.Random;
using TransformUtils = Project.Scripts.Utils.TransformUtils;

namespace Runtime.Character
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterAbilityManager))]
    [RequireComponent(typeof(CharacterLifeManager))]
    [RequireComponent(typeof(CharacterMovement))]
    [RequireComponent(typeof(CharacterRotation))]
    [RequireComponent(typeof(CharacterVisuals))]
    [RequireComponent(typeof(CharacterWeaponManager))]
    [RequireComponent(typeof(CharacterClassManager))]
    public abstract class CharacterBase: MonoBehaviour, ISelectable, IDamageable, IEffectable, IBallInteractable, IReactor
    {

        #region Nested Classes

        public class AppliedStatus
        {
            public Status.Status status;
            public int roundTimer;
        }

        #endregion
        
        #region Events

        public static event Action<CharacterBase> CharacterEndedTurn;

        public static event Action<CharacterBase> CharacterSelected;
        
        public static event Action<bool, CharacterBase> CharacterHovered;
        
        public static event Action<CharacterBase> BallPickedUp;

        public static event Action<CharacterBase> StatusAdded;

        public static event Action<CharacterBase> StatusRemoved;

        public static event Action<CharacterBase> CharacterReset;

        public static event Action<CharacterBase, int> CharacterUsedActionPoint;
        
        #endregion

        #region Read-Only

        protected static readonly int EmisColor = Shader.PropertyToID("_EmisColor");

        #endregion

        #region Serialized Fields

        [SerializeField] protected CharacterStatsBase m_characterStatsBase;

        [SerializeField] protected int m_maxActionPoints = 2;

        [SerializeField] protected bool isGoalie;

        [SerializeField] protected string characterSideRef;

        [SerializeField] private VFXPlayer deathVFX;

        [SerializeField] private VFXPlayer damageVFX;

        [SerializeField] private VFXPlayer healVFX;

        [SerializeField] protected GameObject ballOwnerIndicator;

        [SerializeField] protected GameObject activePlayerIndicator;

        [SerializeField] protected LineRenderer ballThrowIndicator;
        
        [SerializeField] private MeshRenderer characterClassMarker;

        [Header("Camera Shake on Damage")]
        [SerializeField] private float shakeDuration = 0.1f;

        [SerializeField] private float shakeStrength = 0.1f;

        [SerializeField] private int shakeVibrationAmount = 1;

        [Range(0,90)]
        [SerializeField] private float shakeRandomness = 90f;

        [SerializeField] private LayerMask goalLayer;

        #endregion

        #region Unity Public Events

        public UnityEvent onHighlight;

        #endregion
        
        #region Protected Fields
        
        protected bool m_finishedTurn;
        
        protected CharacterAbilityManager m_characterAbilityManager;
        
        protected CharacterLifeManager m_characterLifeManager;
        
        protected CharacterMovement m_characterMovement;
        
        protected CharacterVisuals m_characterVisuals;

        protected CharacterAnimations m_characterAnimations;

        protected CharacterWeaponManager m_characterWeaponManager;
        
        protected CharacterClassManager m_characterClassManager;

        protected CharacterRotation m_characterRotation;
        
        protected Transform m_statusEffectTransform;
        
        protected float shootSpeed = 8f;

        protected bool m_canPickupBall = true;

        protected bool m_canUseAbilities = true;

        protected Color m_passColor;

        protected Color m_shotColor;
        
        private bool isPerformingReaction1;

        protected float maxMovementDistance = 6f;

        public int maxActionPoints => m_maxActionPoints;

        private LayerMask displayLayerVal => LayerMask.NameToLayer("DISPLAY");

        private LayerMask charLayerVal => LayerMask.NameToLayer("CHARACTER");

        #endregion

        #region Accessors

        public CharacterAbilityManager characterAbilityManager => CommonUtils.GetRequiredComponent(ref m_characterAbilityManager,
            () =>
            {
                var cam = GetComponent<CharacterAbilityManager>();
                return cam;
            });
        
        public CharacterLifeManager characterLifeManager => CommonUtils.GetRequiredComponent(ref m_characterLifeManager,
            () =>
            {
                var clm = GetComponent<CharacterLifeManager>();
                return clm;
            });
        
        public CharacterMovement characterMovement => CommonUtils.GetRequiredComponent(ref m_characterMovement,
            () =>
            {
                var cm = GetComponent<CharacterMovement>();
                return cm;
            });

        public CharacterVisuals characterVisuals => CommonUtils.GetRequiredComponent(ref m_characterVisuals,
            () =>
            {
                var cv = GetComponent<CharacterVisuals>();
                return cv;
            });

        public Transform statusEffectTransform => CommonUtils.GetRequiredComponent(ref m_statusEffectTransform, () =>
        {
            var t = TransformUtils.CreatePool(transform, true);
            t.RenameTransform("VFX_POOL");
            return t;
        });

        public CharacterAnimations characterAnimations => CommonUtils.GetRequiredComponent(ref m_characterAnimations,
            () =>
            {
                var cam = GetComponentInChildren<CharacterAnimations>();
                return cam;
            });

        public CharacterWeaponManager characterWeaponManager => CommonUtils.GetRequiredComponent(
            ref m_characterWeaponManager,
            () =>
            {
                var cwm = GetComponent<CharacterWeaponManager>();
                return cwm;
            });
        
        public CharacterClassManager characterClassManager => CommonUtils.GetRequiredComponent(
        ref m_characterClassManager,
        () =>
        {
            var ccm = GetComponent<CharacterClassManager>();
            return ccm;
        });
    
        public CharacterRotation characterRotation => CommonUtils.GetRequiredComponent(
        ref m_characterRotation,
        () =>
        {
            var cr = GetComponent<CharacterRotation>();
            return cr;
        });

        public Color shotColor => CommonUtils.GetRequiredComponent(ref m_shotColor, () =>
        {
            var c = CharacterGameController.Instance.shotColor;
            return c;
        });
        
        public Color passColor => CommonUtils.GetRequiredComponent(ref m_passColor, () =>
        {
            var c = CharacterGameController.Instance.passColor;
            return c;
        });

        public bool isAlive => characterLifeManager.isAlive;

        public bool isActiveCharacter { get; private set; }

        public bool finishedTurn => m_finishedTurn;

        public bool isInBattle => characterMovement.isInBattle;

        public bool isGoalieCharacter => isGoalie;

        public int characterActionPoints { get; private set; }

        public float baseSpeed => GetBaseSpeed();
        
        public CharacterSide side { get; protected set; }

        public bool isBusy => characterMovement.isMoving;

        public bool isDoingAction =>
            characterAbilityManager.isUsingAbilityAction || characterMovement.isUsingMoveAction 
            || isSetupThrowBall;

        public AppliedStatus appliedStatus { get; private set; }

        public CharacterStatsBase characterStatsBase => m_characterStatsBase;

        public BallBehavior heldBall { get; private set; }

        public bool isSetupThrowBall { get; private set; }

        public bool isTargetable { get; private set; } = true;

        public bool canPickupBall => m_canPickupBall && !characterMovement.isKnockedBack;

        public float shotStrength => (characterClassManager.currentMaxShootingScore/(float)CharacterGameController.Instance.GetStatMax()) * shootSpeed;

        public float passStrength => (characterClassManager.currentMaxPassingScore / (float)CharacterGameController.Instance.GetStatMax()) * shootSpeed;

        public bool isSilenced => !m_canUseAbilities;

        #endregion

        #region Unity Events

        private void OnEnable()
        {
            TurnController.OnBattleEnded += OnBattleEnded;
            NavigationSelectable.SelectPosition += CheckAllAction;
            TurnController.OnChangeActiveCharacter += OnChangeActiveCharacter;
            TurnController.OnChangeActiveTeam += OnChangeTeam;
            CharacterSelected += OnOtherCharacterSelected;
        }

        private void OnDisable()
        {
            TurnController.OnBattleEnded -= OnBattleEnded;
            NavigationSelectable.SelectPosition -= CheckAllAction;
            TurnController.OnChangeActiveCharacter -= OnChangeActiveCharacter;
            TurnController.OnChangeActiveTeam -= OnChangeTeam;
            CharacterSelected -= OnOtherCharacterSelected;
        }

        #endregion
        
        #region Class Implementation


        public abstract void InitializeCharacter(CharacterStatsBase _characterStatsBase);

        public abstract float GetBaseSpeed();


        protected abstract void OnBattleEnded();

        protected void InitializeCharacterMarker()
        {
            if (characterClassMarker.IsNull())
            {
                return;
            }

            var _clonedMat = new Material(characterClassMarker.material);

            var _color = SettingsController.Instance.GetSideColor(characterSideRef);

            _clonedMat.SetColor(EmisColor ,_color);

            characterClassMarker.material = _clonedMat;
        }
        
        protected void CharacterDeath()
        {
            TurnController.Instance.HideCharacter(this);
        }
        
        private void OnOtherCharacterSelected(CharacterBase _character)
        {
            if (_character.IsNull())
            {
                return;
            }
            
            if (_character == this)
            {
                return;
            }

            if (characterMovement.isMoving)
            {
                return;
            }

            if (_character.side.sideGUID != this.side.sideGUID)
            {
                if (TurnController.Instance.GetActiveTeamSide().sideGUID != this.side.sideGUID)
                {
                    return;
                }
                
                if (characterMovement.isUsingMoveAction)
                {
                    var correctPivot = !isGoalie ? transform.position : characterMovement.pivotTransform.position.FlattenVector3Y();
                    var _dir = _character.transform.position.FlattenVector3Y() - correctPivot;
                    
                    if (_dir.magnitude > characterMovement.battleMoveDistance)
                    {
                        return;
                    }
                    
                    characterMovement.MoveCharacter(_character.transform.position, true);
                    return;
                }   
            } else if (_character.side.sideGUID == this.side.sideGUID)
            {
                if (isSetupThrowBall)
                {
                    ThrowBall(_character.transform.position - transform.position, false);
                    return;
                }
                
                if (characterMovement.isUsingMoveAction)
                {
                    characterMovement.SetCharacterMovable(false);
                    _character.OnSelect();
                }
            }


        }

        protected virtual void OnBeginWalkAction()
        {
            
        }

        protected void OnWalkActionEnded()
        {
            UseActionPoint();

            if (!isAlive)
            {
                return;
            }
            
            if(characterActionPoints != 0)
            {
                SetCharacterWalkAction();
            }
        }

        public void HitFence()
        {
            //Get the status to apply, deal damage
            OnDealDamage(null, ScriptableDataController.Instance.GetFenceDamage(), false, null, null, false);
            SetCharacterUsable(false);
        }

        public void CheckDeath()
        {
            if (isAlive)
            {
                return;
            }
            
            ReactionQueueController.Instance.QueueReaction(this, DoDeathAction);
        }

        private void DoDeathAction()
        {
            StartCoroutine(C_DoDeathAction());
        }

        public void StopAllActions()
        {
            if (characterMovement.isUsingMoveAction)
            {
                characterMovement.SetCharacterMovable(false);
            }

            if (characterAbilityManager.isUsingAbilityAction)
            {
                characterAbilityManager.CancelAbilityUse();
            }
            
            characterWeaponManager.CancelWeaponUse();

            if (isSetupThrowBall)
            {
                CancelThrowAction();
            }
        }

        private IEnumerator C_DoDeathAction()
        {
            characterVisuals.SetNewLayer(displayLayerVal);
            
            yield return StartCoroutine(JuiceController.Instance.C_DoDeathAnimation(this.characterClassManager.reactionCameraPoint));
            
            if (!heldBall.IsNull())
            {
                KnockBallAway();
            }
            
            CheckActiveAfterDeath();
            PlayDeathEffect();
            RemoveEffect();
            CharacterDeath();
            characterVisuals.SetNewLayer(charLayerVal);
            Debug.Log($"<color=red>{this} has died</color>");
        }

        private void CheckActiveAfterDeath()
        {
            if (!isActiveCharacter)
            {
                return;
            }

            characterActionPoints = 0;
            EndTurn();
        }

        public void MarkHighlightArea(Vector3 _selectedPosition)
        {
            if (!isActiveCharacter || !isDoingAction)
            {
                return;
            }

            if (characterMovement.isUsingMoveAction)
            {
                characterMovement.MarkMovementLocation(_selectedPosition);
            }else if (characterAbilityManager.isUsingAbilityAction)
            {
                characterAbilityManager.MarkAbility(_selectedPosition);
            }else if (isSetupThrowBall)
            {
                MarkThrowBall(_selectedPosition);
            }
        }

        public void CheckAllAction(Vector3 _selectedPosition, bool _isReaction)
        {
            if (!isActiveCharacter && !_isReaction)
            {
                return;
            }

            if (characterMovement.isUsingMoveAction)
            {
                //check if this will be a tackle
                var _correctPivot = !isGoalie ? transform.position : !_isReaction ? characterMovement.pivotTransform.position.FlattenVector3Y() : transform.position;
                var dirToTarget = _selectedPosition - _correctPivot;

                if (dirToTarget.magnitude > characterMovement.battleMoveDistance)
                {
                    return;
                }
                
                RaycastHit[] hits = Physics.CapsuleCastAll(transform.position, _selectedPosition, 0.2f, dirToTarget, dirToTarget.magnitude);

                bool _isTackle = false;
                var _adjustedFinalPosition = _selectedPosition;
                
                foreach (RaycastHit hit in hits)
                {
                    
                    //if they are running into an enemy character, make them stop at that character and perform melee
                    if (hit.collider.TryGetComponent(out CharacterBase otherCharacter))
                    {
                        if (!otherCharacter.isTargetable)
                        {
                            continue;
                        }
                        
                        if (otherCharacter.side != side && otherCharacter != this)
                        {
                            _adjustedFinalPosition = otherCharacter.transform.position;
                            _isTackle = true;
                            break;
                        }
                    }
                }

                characterMovement.MoveCharacter(_adjustedFinalPosition, _isTackle);
            }else if (characterAbilityManager.isUsingAbilityAction)
            {
                characterAbilityManager.SelectAbilityTarget(_selectedPosition);
            }else if (characterWeaponManager.isUsingWeapon)
            {
                characterWeaponManager.SelectWeaponTarget(_selectedPosition);
            }else if (isSetupThrowBall)
            {
                ThrowBall(_selectedPosition - transform.position, IsShot(_selectedPosition));
            }
            
            CameraUtils.SetCameraTrackPos(transform, true);

        }

        private void PlayDamageVFX()
        {
            if (damageVFX == null)
            {
                return;
            }

            damageVFX.PlayAt(transform.position, Quaternion.identity);
        }

        private void PlayHealEffect()
        {
            if (healVFX == null)
            {
                return;
            }

            healVFX.PlayAt(transform.position, Quaternion.identity);
        }
        
        public void PlayDeathEffect()
        {
            if (deathVFX == null)
            {
                return;
            }

            deathVFX.PlayAt(transform.position, Quaternion.identity);
        }

        public void InitializeCharacterBattle(bool _isInBattle)
        {
            characterMovement.SetCharacterBattleStatus(_isInBattle);
        }

        public void UseCharacterAbility(int _abilityIndex)
        {
            if (characterActionPoints <= 0)
            {
                return;
            }

            if (!m_canUseAbilities)
            {
                //ToDo: visual queue
                Debug.Log("Character Can't use abilities");
                return;
            }

            if (characterAbilityManager.isUsingAbilityAction)
            {
                characterAbilityManager.CancelAbilityUse();
                return;
            }

            if (isSetupThrowBall)
            {
                CancelThrowAction();
            }

            if (characterMovement.isUsingMoveAction)
            {
                characterMovement.SetCharacterMovable(false);
            }

            characterAbilityManager.UseAssignedAbility(_abilityIndex, OnAbilityUsed);
        }

        private void OnAbilityUsed()
        {
            characterAnimations.AbilityAnim(characterAbilityManager.GetActiveAbilityIndex(), true);
        }

        public void UseCharacterWeapon()
        {
            if (characterActionPoints <= 0)
            {
                return;
            }
            
            characterWeaponManager.SetupWeaponAction(!characterWeaponManager.isUsingWeapon,OnAttackUsed);
        }

        private void OnAttackUsed()
        {
            if (characterAnimations != null)
            {
                characterAnimations.AttackAnim(true);
            }
            
            UseActionPoint();
        }

        public void SetCharacterWalkAction()
        {
            if (characterActionPoints <= 0)
            {
                return;
            }

            if (characterMovement.isUsingMoveAction)
            {
                characterMovement.SetCharacterMovable(false);
                return;
            }

            if (isSetupThrowBall)
            {
                CancelThrowAction();
            }

            if (characterAbilityManager.isUsingAbilityAction)
            {
                characterAbilityManager.CancelAbilityUse();
            }

            characterMovement.SetCharacterMovable(true, OnBeginWalkAction, OnWalkActionEnded);
        }

        public void SetCharacterThrowAction()
        {
            if (characterActionPoints <= 0)
            {
                return;
            }

            if (heldBall.IsNull())
            {
                return;
            }

            isSetupThrowBall = !isSetupThrowBall;

            if (isSetupThrowBall)
            {
                if (characterMovement.isUsingMoveAction)
                {
                    characterMovement.SetCharacterMovable(false);
                }

                if (characterAbilityManager.isUsingAbilityAction)
                {
                    characterAbilityManager.CancelAbilityUse();
                }
            }
            
            if (!ballThrowIndicator.IsNull())
            {
                ballThrowIndicator.gameObject.SetActive(isSetupThrowBall);
            }
        }

        public void CancelThrowAction()
        {
            isSetupThrowBall = false;
            
            if (!ballThrowIndicator.IsNull())
            {
                ballThrowIndicator.gameObject.SetActive(isSetupThrowBall);
            }
        }

        public void UseActionPoint()
        {
            characterActionPoints--;
            CharacterUsedActionPoint?.Invoke(this, characterActionPoints);
            
            Debug.Log($"<color=yellow>Used action point // Action Points:{characterActionPoints} left</color>");
            
            if (characterActionPoints == 0)
            {
                Debug.Log($"character finished turn");
                EndTurn();
            }
            
        }


        protected void OnChangeActiveCharacter(CharacterBase _characterBase)
        {
            if (_characterBase != this)
            {
                if (isActiveCharacter)
                {
                    isActiveCharacter = false;
                    activePlayerIndicator.SetActive(isActiveCharacter);
                }
                return;
            }

            if (!isAlive)
            {
                m_finishedTurn = true;
                return;
            }
            
            Debug.Log("Player start");
            ActivePlayer();
        }

        private void ActivePlayer()
        {
            if (!isAlive)
            {
                //ToDo: do death things
                return;
            }

            isActiveCharacter = true;
            activePlayerIndicator.SetActive(isActiveCharacter);
            SetCharacterWalkAction();
        }

        private void OnChangeTeam(CharacterSide _side)
        {
            if (_side != this.side)
            {
                return;
            }
            
            ResetCharacterActions();
        }

        [ContextMenu("Reset Actions")]
        public void ResetCharacterActions()
        {
            if (!isAlive)
            {
                //ToDo: do death things
                return;
            }
            
            m_finishedTurn = false;
            characterActionPoints = maxActionPoints;
            characterAbilityManager.CheckAbilityCooldown();
            CheckStatus();
        }

        public void ResetCharacter(Vector3 _position)
        {
            characterLifeManager.FullReviveCharacter();
            RemoveEffect();
            characterMovement.ResetCharacter(_position);
            StopAllActions();
            CharacterReset?.Invoke(this);
        }

        public void SetCharacterUsable(bool _isUseable)
        {
            characterActionPoints = _isUseable ? maxActionPoints : 0;
        }

        public void SetTargetable(bool _isTargetable)
        {
            isTargetable = _isTargetable;

            if (!isTargetable)
            {
                SetCanPickupBall(isTargetable);
                
                if (!heldBall.IsNull())
                {
                    ThrowBall(Vector3.zero, true);
                }
            }
        }

        private void SetCanPickupBall(bool _canPickupBall)
        {
            m_canPickupBall = _canPickupBall;
        }

        public void SetCharacterCanUseAbilities(bool _canUse)
        {
            m_canUseAbilities = _canUse;
        }
        
        private void CheckStatus()
        {
            if (appliedStatus.IsNull())
            {
                return;
            }
            
            if (appliedStatus.roundTimer <= 0)
            {
                RemoveEffect();
                return;
            }

            if (appliedStatus.status.playVFXOnTrigger)
            {
                appliedStatus.status.statusOneTimeVFX.PlayAt(transform.position, Quaternion.identity, statusEffectTransform);
            }
            
            appliedStatus.status.TriggerStatusEffect(this);

            if (!isAlive)
            {
                return;
            }
            
            appliedStatus.roundTimer--;
        }

        [ContextMenu("Skip Turn")]
        public void EndTurn()
        {
            Debug.Log($"{this} has ended turn", this);
            if (isActiveCharacter)
            {
                isActiveCharacter = false;
                activePlayerIndicator.SetActive(isActiveCharacter);
            }
            m_finishedTurn = true;
            characterActionPoints = 0;
            StopAllActions();
            CharacterEndedTurn?.Invoke(this);
        }

        private void MarkThrowBall(Vector3 _position)
        {
            if (ballThrowIndicator.IsNull())
            {
                return;
            }
            //ToDo: update points if the player is aiming at a wall

            bool _isShot = IsShot(_position);
            var _throwingStrength = _isShot ? shotStrength : passStrength;
            
            var dir = transform.InverseTransformDirection(_position - transform.position);
            var _decelTime = 1.9f;
            var _decel = (_throwingStrength)/_decelTime;
            var _dist = (0.5f * _decel) * (_decelTime * _decelTime);
            var furthestPoint = (dir.normalized * _dist);

            ballThrowIndicator.SetPosition(1, new Vector3(furthestPoint.x, furthestPoint.z, 0));
        }

        private void TransferBall(CharacterBase _ballHolder, CharacterBase _ballStealer)
        {
            var _ball = TurnController.Instance.ball;
            _ballHolder.DetachBall();
            _ballStealer.PickUpBall(_ball);
        }

        private bool IsShot(Vector3 _endPos)
        {
            var _dir = _endPos - transform.position;
            var strongerStrength = shotStrength > passStrength ? shotStrength : passStrength;
            var _furthestPoint = transform.position + (_dir.normalized * strongerStrength);
            var _mag = _furthestPoint - transform.position;
            RaycastHit[] hits = Physics.RaycastAll(transform.position, _dir, _mag.magnitude, goalLayer, QueryTriggerInteraction.Collide);

            if (hits.Length == 0)
            {
                Debug.DrawLine(transform.position, _furthestPoint, Color.red);
                return false;
            }
            
            Debug.DrawLine(transform.position, _furthestPoint, Color.green);
            return true;
            
        }

        #endregion

        #region ISelectable Inherited Methods

        public void OnSelect()
        {
            if (isDoingAction)
            {
                return;
            }
            
            CharacterSelected?.Invoke(this);
        }

        public void OnUnselected()
        {
            
        }

        public void OnHover()
        {
            if (isDoingAction)
            {
                return;
            }
            
            CharacterHovered?.Invoke(true,this);
            onHighlight?.Invoke();
            characterVisuals.SetHighlight();
        }

        public void OnUnHover()
        {
            CharacterHovered?.Invoke(false, this);
            characterVisuals.SetUnHighlight();
        }

        #endregion

        #region IDamageable Inherited Methods

        public void OnRevive()
        {
            characterLifeManager.FullReviveCharacter();
        }

        public void OnHeal(int _healAmount, bool _isHealArmor)
        {
            characterLifeManager.HealCharacter(_healAmount, _isHealArmor);
            PlayHealEffect();
        }

        public void OnDealDamage(Transform _attacker, int _damageAmount, bool _armorPiercing ,ElementTyping _damageElementType, Transform _knockbackAttacker ,bool _hasKnockback)
        {
            characterLifeManager.DealDamage(_attacker, _damageAmount, _armorPiercing, _damageElementType);
            
            if (_damageAmount > 0)
            {
                PlayDamageVFX();
            }

            if (_hasKnockback)
            {
                var _direction = transform.position - _knockbackAttacker.position;
                
                var _knockbackForce = (characterClassManager.currentMaxTacklingScore / CharacterGameController.Instance.GetStatMax()) * 10;

                if (isGoalie)
                {
                    _knockbackForce /= 2;
                }
                
                characterMovement.ApplyKnockback(_knockbackForce, _direction.FlattenVector3Y(), 0.5f);
               
                JuiceController.Instance.DoCameraShake(shakeDuration, shakeStrength, shakeVibrationAmount, shakeRandomness);
               
                if (!heldBall.IsNull())
                {
                    _knockbackAttacker.TryGetComponent(out CharacterBase _character);

                    if (!_character.IsNull() && _character != this && _character.characterClassManager.assignedClass.classType == CharacterClass.PLAYMAKER)
                    {
                        if (_character.characterClassManager.CheckStealBall(this))
                        {
                            //Succeeded
                            TransferBall(this, _character);
                        }
                        else
                        {
                            //Failed
                            KnockBallAway(_attacker);
                        }
                    }
                    else
                    {
                        KnockBallAway(_attacker);
                    }

                }
                else
                {
                    _knockbackAttacker.TryGetComponent(out CharacterBase _character);
                    if (!_character.IsNull() && _character != this)
                    {
                        characterAbilityManager.CheckAbilityCooldown();
                    }
                    
                }
            }
            else
            {
                if (_damageAmount > 0)
                {
                    JuiceController.Instance.DoCameraShake(shakeDuration, shakeStrength/2, shakeVibrationAmount, shakeRandomness);
                }
            }
            
            if (!characterAnimations.IsNull())
            {
                if (_damageAmount > 0)
                {
                    characterAnimations.DamageAnim(true);
                }
            }
        }

        #endregion

        #region IEffectable Inherited Methods

        Status.Status IEffectable.currentStatus
        {
            get => appliedStatus.status;
            set => appliedStatus.status = value;
        }

        public void ApplyEffect(Status.Status _newStatus)
        {
            if (_newStatus.IsNull())
            {
                return;
            }
            
            Debug.Log($"<color=cyan>Applied {_newStatus.statusName} to {this.transform.name}</color>");
            if (!appliedStatus.IsNull())
            {
                RemoveEffect();
            }

            appliedStatus = new AppliedStatus
            {
                status = _newStatus,
                roundTimer = _newStatus.roundCooldownTimer
            };

            if (!_newStatus.statusOneTimeVFX.IsNull())
            {
                _newStatus.statusOneTimeVFX.PlayAt(transform.position, Quaternion.identity, statusEffectTransform);    
            }

            if (!_newStatus.statusStayVFX.IsNull())
            {
                _newStatus.statusStayVFX.PlayAt(transform.position, Quaternion.identity, statusEffectTransform);
            }
            
            StatusAdded?.Invoke(this);
            
        }

        public void RemoveEffect()
        {
            if (appliedStatus.IsNull())
            {
                return;
            }
            
            Debug.Log($"<color=red>Removed {appliedStatus.status.statusName} to {this.transform.name}</color>");
            
            appliedStatus.status.ResetStatusEffect(this);

            for (int i = 0; i < statusEffectTransform.childCount; i++)
            {
                Debug.Log("Getting Child Transform");
                statusEffectTransform.GetChild(i).TryGetComponent(out VFXPlayer vfx);
                if (!vfx.IsNull())
                {
                    vfx.Stop();
                }
            }

            appliedStatus = null;
            
            StatusRemoved?.Invoke(this);
        }

        #endregion

        #region IBallInteractalbe Inherited Methods

        public void PickUpBall(BallBehavior ball)
        {
            if (!canPickupBall)
            {
                return;
            }
            
            if(isGoalie)
            {
                characterActionPoints++;
            }
            
            heldBall = ball;
            heldBall.SetFollowTransform(characterWeaponManager.handPos, this);
            ballOwnerIndicator.SetActive(true);
            BallPickedUp?.Invoke(this);
        }

        public void DetachBall()
        {
            heldBall = null;
            ballOwnerIndicator.SetActive(false);
        }

        public void KnockBallAway(Transform attacker = null)
        {
            if (heldBall.IsNull())
            {
                return;
            }

            var randomPos = Random.insideUnitCircle;
            var direction = attacker.IsNull() ? (transform.position + new Vector3(randomPos.x, 0 , randomPos.y) - transform.position) : attacker.position - transform.position;
            Debug.Log(shotStrength);
            heldBall.ThrowBall(direction, shotStrength, false, this, 0);
            heldBall = null;
            ballOwnerIndicator.SetActive(false);
        }

        public void ThrowBall(Vector3 direction, bool _isShot)
        {
            if (heldBall.IsNull())
            {
                return;
            }

            if (!isSetupThrowBall)
            {
                return;
            }

            
            var _ballThrowSpeed = _isShot ? shotStrength : passStrength;
            var _attachedStat = _isShot
                ? characterClassManager.GetRandomShootStat()
                : characterClassManager.GetRandomPassingStat();
            
            heldBall.ThrowBall(direction, _ballThrowSpeed, true, this, _attachedStat);
            heldBall = null;
            isSetupThrowBall = false;
            ballOwnerIndicator.SetActive(false);
            if (!ballThrowIndicator.IsNull())
            {
                ballThrowIndicator.gameObject.SetActive(false);
            }
            UseActionPoint();
        }

        #endregion

        bool IReactor.isPerformingReaction
        {
            get => isPerformingReaction1;
            set => isPerformingReaction1 = value;
        }
    }
}