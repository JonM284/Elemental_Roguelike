﻿using System;
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
    public abstract class CharacterBase: MonoBehaviour, ISelectable, IDamageable, IEffectable, IBallInteractable
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

        #endregion

        #region Serialized Fields

        [SerializeField] protected CharacterStatsBase m_characterStatsBase;

        [SerializeField] private CharacterSide characterSide;

        [SerializeField] private VFXPlayer deathVFX;

        [SerializeField] private VFXPlayer damageVFX;

        [SerializeField] private VFXPlayer healVFX;

        [SerializeField] protected GameObject ballOwnerIndicator;

        [SerializeField] protected GameObject activePlayerIndicator;

        [SerializeField] protected LineRenderer ballThrowIndicator;
 
        #endregion

        #region Unity Public Events

        public UnityEvent onHighlight;

        #endregion
        
        #region Protected Fields

        protected int m_characterActionPoints = 2;
        
        protected bool m_finishedTurn;
        
        protected CharacterAbilityManager m_characterAbilityManager;
        
        protected CharacterLifeManager m_characterLifeManager;
        
        protected CharacterMovement m_characterMovement;
        
        protected CharacterVisuals m_characterVisuals;

        protected CharacterAnimations m_characterAnimations;

        protected CharacterWeaponManager m_characterWeaponManager;
        
        protected CharacterClassManager m_characterClassManager;

        protected CharacterRotation m_characterRotation;
        
        private Transform m_statusEffectTransform;
        
        private float shootSpeed = 8f;

        private bool m_canPickupBall = true;

        private bool m_canUseAbilities = true;

        private CharacterSide damageableSide1;
        
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

        public bool isAlive => characterLifeManager.isAlive;

        public bool isActiveCharacter { get; private set; }

        public bool finishedTurn => m_finishedTurn;

        public bool isInBattle => characterMovement.isInBattle;

        public int characterActionPoints => m_characterActionPoints;
        
        public float baseSpeed => GetBaseSpeed();

        public CharacterSide side => characterSide;

        public bool isBusy => characterMovement.isMoving;

        public bool isDoingAction =>
            characterAbilityManager.isUsingAbilityAction || characterWeaponManager.isUsingWeapon || characterMovement.isUsingMoveAction 
            || isSetupThrowBall;

        public AppliedStatus appliedStatus { get; private set; }

        public CharacterStatsBase characterStatsBase => m_characterStatsBase;

        public BallBehavior heldBall { get; private set; }

        public bool isSetupThrowBall { get; private set; }

        public bool isTargetable { get; private set; } = true;

        public bool canPickupBall => m_canPickupBall && !characterMovement.isKnockedBack;

        public float shotStrength => shootSpeed;
        
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

        public abstract void InitializeCharacter();
        
        public abstract float GetBaseSpeed();


        protected abstract void OnBattleEnded();
        
        protected virtual void OnWalkActionPressed()
        {
            
        }

        protected void CharacterDeath()
        {
            TurnController.Instance.HideCharacter(this);
        }
        
        private void OnOtherCharacterSelected(CharacterBase _character)
        {
            if (_character == null)
            {
                return;
            }
            
            if (_character == this)
            {
                return;
            }

            if (_character.side != this.side)
            {
                if (characterMovement.isUsingMoveAction)
                {
                    var _dir = _character.transform.position.FlattenVector3Y() - transform.position.FlattenVector3Y();
                    
                    if (_dir.magnitude > characterMovement.battleMoveDistance)
                    {
                        return;
                    }
                    
                    characterMovement.MoveCharacter(_character.transform.position, true);
                    return;
                }   
            }

            if (_character.side == this.side && isSetupThrowBall)
            {
                ThrowBall(_character.transform.position - transform.position);
            }
            

        }

        protected virtual void OnBeginWalkAction()
        {
            
        }

        protected virtual void OnWalkActionEnded()
        {
            UseActionPoint();
            if (characterActionPoints == 0)
            {
                EndTurn();
            }
            else
            {
                SetCharacterWalkAction();
            }
        }

        public void CheckDeath()
        {
            if (isAlive)
            {
                return;
            }
            
            StartCoroutine(DoDeathAction());
        }

        private IEnumerator DoDeathAction()
        {
            characterVisuals.SetNewLayer(displayLayerVal);
            yield return StartCoroutine(JuiceController.Instance.C_DoDeathAnimation(this.characterClassManager.reactionCameraPoint));
            
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

            m_characterActionPoints = 0;
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
                var dirToTarget = _selectedPosition - transform.position;

                if (dirToTarget.magnitude > characterMovement.battleMoveDistance)
                {
                    Debug.Log($"<color=orange>12</color>");
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
                ThrowBall(_selectedPosition - transform.position);
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
            if (m_characterActionPoints <= 0)
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
                isSetupThrowBall = false;
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
            UseActionPoint();
        }

        public void UseCharacterWeapon()
        {
            if (m_characterActionPoints <= 0)
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
            if (m_characterActionPoints <= 0)
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
                isSetupThrowBall = false;
            }

            if (characterAbilityManager.isUsingAbilityAction)
            {
                characterAbilityManager.CancelAbilityUse();
            }

            OnWalkActionPressed();
            characterMovement.SetCharacterMovable(true, OnBeginWalkAction, OnWalkActionEnded);
        }

        public void SetCharacterThrowAction()
        {
            if (m_characterActionPoints <= 0)
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

        public void UseActionPoint()
        {
            m_characterActionPoints--;
            Debug.Log($"<color=yellow>Used action point // Action Points:{characterActionPoints} left</color>");
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
            m_characterActionPoints = 2;
            characterAbilityManager.CheckAbilityCooldown();
            CheckStatus();
        }

        public void ResetCharacter(Vector3 _position)
        {
            characterLifeManager.FullReviveCharacter();
            RemoveEffect();
            characterMovement.ResetCharacter(_position);
            CharacterReset?.Invoke(this);
        }

        public void SetCharacterUsable(bool _isUseable)
        {
            m_characterActionPoints = _isUseable ? 2 : 0;
        }

        public void SetTargetable(bool _isTargetable)
        {
            isTargetable = _isTargetable;

            if (!isTargetable)
            {
                SetCanPickupBall(isTargetable);
                
                if (!heldBall.IsNull())
                {
                    ThrowBall(Vector3.zero);
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
            if (appliedStatus == null)
            {
                return;
            }

            if (appliedStatus.status.chanceToRemove > 0)
            {
                var randomKnockoffChance = Random.Range(0 , 100);
                if (randomKnockoffChance <= appliedStatus.status.chanceToRemove)
                {
                    RemoveEffect();
                    return;
                }    
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
            m_finishedTurn = true;
            m_characterActionPoints = 0;
            CharacterEndedTurn?.Invoke(this);
        }

        private void MarkThrowBall(Vector3 _position)
        {
            if (ballThrowIndicator.IsNull())
            {
                return;
            }
            
            var dir = transform.InverseTransformDirection(_position - transform.position);
            var furthestPoint = (dir.normalized * 2);
            ballThrowIndicator.SetPosition(1, new Vector3(furthestPoint.x, furthestPoint.z, 0));
        }

        #endregion

        #region ISelectable Inherited Methods

        public void OnSelect()
        {
            CharacterSelected?.Invoke(this);
        }

        public void OnUnselected()
        {
            
        }

        public void OnHover()
        {
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
                Debug.Log("Has Knockback");
                var _direction = transform.position - _knockbackAttacker.position;
                characterMovement.ApplyKnockback(10, _direction.FlattenVector3Y(), 0.5f);
                if (!heldBall.IsNull())
                {
                    KnockBallAway(_attacker);
                }
            }
            
            if (characterAnimations != null)
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

        public void KnockBallAway(Transform attacker)
        {
            if (heldBall.IsNull())
            {
                return;
            }

            var randomPos = Random.insideUnitCircle;
            var direction = (transform.position + new Vector3(randomPos.x, 0 , randomPos.y) - transform.position);
            heldBall.ThrowBall(direction, shootSpeed, false, side, 0);
            heldBall = null;
            ballOwnerIndicator.SetActive(false);
        }

        public void ThrowBall(Vector3 direction)
        {
            if (heldBall.IsNull())
            {
                return;
            }

            if (!isSetupThrowBall)
            {
                return;
            }

            /*var stat = Random.Range(characterClassManager.assignedClass.ShootingStatMin,
                characterClassManager.assignedClass.ShootingStatMax);*/
            
            heldBall.ThrowBall(direction, shootSpeed, true, side, characterClassManager.GetRandomShootStat());
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
    }
}