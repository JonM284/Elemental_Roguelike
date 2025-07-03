using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using Data;
using Data.CharacterData;
using Data.Elements;
using Data.Sides;
using Project.Scripts.Utils;
using Runtime.Character.StateMachines;
using Runtime.Damage;
using Runtime.GameControllers;
using Runtime.Gameplay;
using Runtime.Managers;
using Runtime.Selection;
using Runtime.Status;
using Runtime.VFX;
using Unity.VisualScripting;
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
    [RequireComponent(typeof(StateManager))]
    [RequireComponent(typeof(CharacterBallManager))]
    public abstract class CharacterBase: MonoBehaviour, ISelectable, IDamageable, IEffectable, IReactor
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
        
        [SerializeField] protected GameObject activePlayerIndicator;
        
        [SerializeField] private MeshRenderer characterClassMarker;

        [SerializeField] protected Transform m_ballHoldPosition;

        [Header("Camera Shake on Damage")]
        [SerializeField] private float shakeDuration = 0.1f;

        [SerializeField] private float shakeStrength = 0.1f;

        [SerializeField] private int shakeVibrationAmount = 1;

        [Range(0,90)]
        [SerializeField] private float shakeRandomness = 90f;
        
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
        
        protected CharacterBallManager m_characterBallManager;

        protected CharacterRotation m_characterRotation;

        protected StateManager m_stateManager;
        
        protected Transform m_statusEffectTransform;
        
        protected bool m_canUseAbilities = true;

        protected Color m_passColor;

        protected Color m_shotColor;
        
        private bool isPerformingReaction1;
        
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

        public StateManager stateManager => CommonUtils.GetRequiredComponent(ref m_stateManager,
            GetComponent<StateManager>);
        
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
        
        public CharacterBallManager characterBallManager => CommonUtils.GetRequiredComponent(
            ref m_characterBallManager,
            GetComponent<CharacterBallManager>);
    
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

        public bool isDoingAction => !stateManager.currentState.IsNull() && stateManager.GetCurrentStateEnum() != ECharacterStates.Idle ;

        public AppliedStatus appliedStatus { get; private set; }

        public CharacterStatsBase characterStatsBase => m_characterStatsBase;
        
        public bool isTargetable { get; private set; } = true;

        public bool isSilenced => !m_canUseAbilities;

        public bool isInitialized { get; protected set; }

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


        public virtual async UniTask InitializeCharacter(CharacterStatsBase _characterStatsBase)
        {
            side = ScriptableDataController.Instance.GetSideByGuid(characterSideRef);
            m_characterStatsBase = _characterStatsBase;
            
            if (characterVisuals.isMeeple)
            {
                await characterVisuals.InitializeMeepleCharacterVisuals(m_characterStatsBase ,
                    m_characterStatsBase.typing, m_ballHoldPosition.transform);
            }
            else
            {
                await characterVisuals.InitializeCharacterVisuals(m_characterStatsBase, m_ballHoldPosition.transform);
            }
            
            await characterLifeManager.InitializeCharacterHealth(m_characterStatsBase.baseHealth, m_characterStatsBase.baseShields, m_characterStatsBase.baseHealth,
                m_characterStatsBase.baseShields, m_characterStatsBase.typing, m_characterStatsBase.healthBarOffset);
            
            await characterClassManager.InitializedCharacterPassive(m_characterStatsBase.classTyping, m_characterStatsBase.agilityScore,
                m_characterStatsBase.shootingScore, m_characterStatsBase.passingScore ,m_characterStatsBase.tackleScore);

            await characterBallManager.Initialize(m_characterStatsBase, m_ballHoldPosition.transform);

            InitializeCharacterMarker();
            
            await stateManager.InitStateMachine(ECharacterStates.Idle, this);
            
            isInitialized = true;
        }

        public abstract float GetBaseSpeed();

        protected abstract void OnBattleEnded();

        protected void InitializeCharacterMarker()
        {
            if (characterClassMarker.IsNull())
            {
                return;
            }

            var _clonedMat = new Material(characterClassManager.assignedClass.characterClassGroundMarker);

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
                    stateManager.currentState.stateBehavior.SelectTarget(_character.transform.position);
                }   
            } else if (_character.side.sideGUID == this.side.sideGUID)
            {
                if (characterMovement.isUsingMoveAction)
                {
                    stateManager.ChangeState(ECharacterStates.Idle);
                    _character.OnSelect();
                    return;
                }
                
                stateManager.currentState.stateBehavior.SelectTarget(_character.transform.position);
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
            stateManager.ChangeState(ECharacterStates.Idle);
        }

        private IEnumerator C_DoDeathAction()
        {
            characterVisuals.SetNewLayer(displayLayerVal);
            
            yield return StartCoroutine(JuiceController.Instance.C_DoDeathAnimation(this.characterClassManager.reactionCameraPoint));
            
            if (!characterBallManager.hasBall)
            {
                characterBallManager.KnockBallAway();
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

            stateManager.currentState.stateBehavior.MarkHighlight(_selectedPosition);
        }

        public void CheckAllAction(Vector3 _selectedPosition, bool _isReaction)
        {
            if (!isActiveCharacter && !_isReaction)
            {
                Debug.Log("Not active character AND not in reaction");
                return;
            }

            if (stateManager.currentState.IsNull())
            {
                Debug.LogError("NULL STATE");
                return;
            }

            stateManager.currentState.stateBehavior.SelectTarget(_selectedPosition);
            
            CameraUtils.SetCameraTrackPos(transform, true);

        }

        private void PlayDamageVFX()
        {
            if (damageVFX.IsNull())
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

            
            stateManager.ChangeState(ECharacterStates.Ability);
            
            object[] _arg = {_abilityIndex};
            stateManager.currentState.stateBehavior.AssignArgument(_arg);
        }

        public void SetOverwatch()
        {
            characterClassManager.ActivateCharacterOverwatch();
            EndTurn();
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
            if (!characterAnimations.IsNull())
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

            stateManager.ChangeState(ECharacterStates.Movement);
        }

        public void SetCharacterThrowAction()
        {
            if (characterActionPoints <= 0)
            {
                return;
            }

            if (!characterBallManager.hasBall)
            {
                return;
            }

            stateManager.ChangeState(ECharacterStates.ThrowBall);
        }

        public void CancelThrowAction()
        {
           stateManager.ChangeState(ECharacterStates.Idle);
        }

        public void UseActionPoint()
        {
            characterActionPoints--;
            CharacterUsedActionPoint?.Invoke(this, characterActionPoints);
            Debug.Log($"<color=yellow>Used action point // Action Points:{characterActionPoints} left</color>");

            if (characterActionPoints != 0)
            {
                return;
            }
            
            Debug.Log($"character finished turn");
            EndTurn();
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
                characterBallManager.SetCanPickupBall(isTargetable);
                
                if (characterBallManager.hasBall)
                {
                    characterBallManager.ThrowBall(Vector3.zero, true);
                }
            }
        }

        public void DetachBall()
        {
            characterBallManager.DetachBall();
        }

        public void PickUpBall(BallBehavior _ball)
        {
            characterBallManager.PickUpBall(_ball);
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

        public void OnDealDamage(Transform _attacker, int _damageAmount, bool _armorPiercing ,
            ElementTyping _damageElementType, Transform _knockbackAttacker ,bool _hasKnockback)
        {
            characterLifeManager.DealDamage(_attacker, _damageAmount, _armorPiercing, _damageElementType);
            
            if (_damageAmount > 0)
            {
                PlayDamageVFX();
            }

            if (_hasKnockback)
            {
                var _direction = transform.position - _knockbackAttacker.position;
                
                var _knockbackForce = (characterClassManager.currentMaxTacklingScore / 100f) * 10;

                if (isGoalie)
                {
                    _knockbackForce /= 2;
                }
                
                characterMovement.ApplyKnockback(_knockbackForce, _direction.FlattenVector3Y(), 0.5f);
               
                JuiceController.Instance.DoCameraShake(shakeDuration, shakeStrength, shakeVibrationAmount, shakeRandomness);

                _knockbackAttacker.TryGetComponent(out CharacterBase _character);
                
                if (characterBallManager.hasBall)
                {

                    if (!_character.IsNull() && _character != this &&
                        _character.characterClassManager.assignedClass.classType == CharacterClass.PLAYMAKER)
                    {
                        if (_character.characterClassManager.CheckStealBall(this))
                        {
                            //Succeeded
                            characterBallManager.TransferBall(this, _character);
                        }
                        else
                        {
                            //Failed
                            characterBallManager.KnockBallAway(_attacker);
                        }
                    }
                    else
                    {
                        characterBallManager.KnockBallAway(_attacker);
                    }

                }
                else
                {
                    
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

            if (characterAnimations.IsNull())
            {
                return;
            }
            
            if (_damageAmount > 0)
            {
                characterAnimations.DamageAnim(true);
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

        bool IReactor.isPerformingReaction
        {
            get => isPerformingReaction1;
            set => isPerformingReaction1 = value;
        }
    }
}