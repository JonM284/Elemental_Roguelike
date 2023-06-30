using System;
using Data.Elements;
using Data.Sides;
using Project.Scripts.Data;
using Project.Scripts.Utils;
using Runtime.Damage;
using Runtime.GameControllers;
using Runtime.Gameplay;
using Runtime.Selection;
using Runtime.Status;
using Runtime.VFX;
using UnityEngine;
using UnityEngine.Networking;
using Utils;

namespace Runtime.Character
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterAbilityManager))]
    [RequireComponent(typeof(CharacterLifeManager))]
    [RequireComponent(typeof(CharacterMovement))]
    [RequireComponent(typeof(CharacterRotation))]
    [RequireComponent(typeof(CharacterVisuals))]
    [RequireComponent(typeof(CharacterWeaponManager))]
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

        public static event Action<CharacterBase> BallPickedUp;

        #endregion

        #region Serialized Fields

        [SerializeField] protected CharacterStatsBase m_characterStatsBase;

        [SerializeField] private CharacterSide characterSide;

        [SerializeField] private VFXPlayer deathVFX;

        [SerializeField] private VFXPlayer damageVFX;

        [SerializeField] protected GameObject passivePassInterceptIndicator;

        [SerializeField] protected GameObject passiveMeleeIndicator;
        
        [SerializeField] protected GameObject ballOwnerIndicator;

        [SerializeField] protected GameObject activePlayerIndicator;

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

        private Transform m_statusEffectTransform;
        
        private float shootSpeed = 8f;

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

        public bool isAlive => characterLifeManager.isAlive;

        public bool isActiveCharacter { get; private set; }

        public bool finishedTurn => m_finishedTurn;

        public bool isInBattle => characterMovement.isInBattle;

        public int characterActionPoints => m_characterActionPoints;

        public int initiativeNum => GetInitiativeNumber();

        public float baseSpeed => GetBaseSpeed();

        public CharacterSide side => characterSide;

        public bool isBusy => characterMovement.isMoving;

        public bool isDoingAction =>
            characterAbilityManager.isUsingAbilityAction || characterWeaponManager.isUsingWeapon;

        public AppliedStatus appliedStatus { get; private set; }

        public CharacterStatsBase characterStatsBase => m_characterStatsBase;

        public BallBehavior heldBall { get; private set; }

        public bool isSetupThrowBall { get; private set; }

        #endregion

        #region Unity Events

        private void OnEnable()
        {
            TurnController.OnBattleEnded += OnBattleEnded;
            CharacterLifeManager.OnCharacterDied += OnCharacterDied;
            NavigationSelectable.SelectPosition += CheckAllAction;
            TurnController.OnChangeActiveCharacter += OnChangeActiveCharacter;
            TurnController.OnChangeActiveTeam += OnChangeTeam;
            CharacterSelected += OnOtherCharacterSelected;
        }

        private void OnDisable()
        {
            TurnController.OnBattleEnded -= OnBattleEnded;
            CharacterLifeManager.OnCharacterDied -= OnCharacterDied;
            NavigationSelectable.SelectPosition -= CheckAllAction;
            TurnController.OnChangeActiveCharacter -= OnChangeActiveCharacter;
            TurnController.OnChangeActiveTeam -= OnChangeTeam;
        }

        #endregion
        
        #region Class Implementation

        public abstract void InitializeCharacter();

        public abstract int GetInitiativeNumber();

        public abstract float GetBaseSpeed();
        
        protected abstract void CharacterDeath();

        protected abstract void OnBattleEnded();
        
        protected virtual void OnWalkActionPressed()
        {
            
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
                    //Do Melee Action
                    

                    return;
                }   
            }

            if (_character.side == this.side && isSetupThrowBall)
            {
                ThrowBall(_character.transform.position - transform.position);
                return;
            }
            

        }

        protected virtual void OnBeginWalkAction()
        {
            UseActionPoint();
        }

        protected virtual void OnWalkActionEnded()
        {
            if (characterActionPoints == 0)
            {
                EndTurn();
            }
        }

        protected void OnCharacterDied(CharacterBase _character)
        {
            if (_character != this)
            {
                return;
            }

            PlayDeathEffect();
            RemoveEffect();
            CharacterDeath();
            Debug.Log($"<color=red>{this} has died</color>");
        }

        protected void CheckAllAction(Vector3 _selectedPosition)
        {
            if (!isActiveCharacter)
            {
                return;
            }

            if (characterMovement.isUsingMoveAction)
            {
                characterMovement.MoveCharacter(_selectedPosition);
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
            

        }

        private void PlayDamageVFX()
        {
            if (damageVFX == null)
            {
                return;
            }

            damageVFX.PlayAt(transform.position, Quaternion.identity);
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

            characterAbilityManager.UseAssignedAbility(_abilityIndex, OnAbilityUsed);
        }

        private void OnAbilityUsed()
        {
            characterAnimations.AbilityAnim(true);
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
                return;
            }
            
            Debug.Log("Player start");
            ActivePlayer();
        }

        private void ActivePlayer()
        {
            if (!isAlive)
            {
                OnCharacterDied(this);
                return;
            }

            isActiveCharacter = true;
            activePlayerIndicator.SetActive(isActiveCharacter);
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
                OnCharacterDied(this);
                return;
            }
            
            m_finishedTurn = false;
            m_characterActionPoints = 2;
            characterAbilityManager.CheckAbilityCooldown();
            CheckStatus();
        }

        private void CheckStatus()
        {
            if (appliedStatus == null)
            {
                return;
            }

            if (appliedStatus.roundTimer <= 0)
            {
                RemoveEffect();
                return;
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

        [ContextMenu("Damage Trial")]
        public void TakeDamage()
        {
            characterAnimations.DamageAnim(true);
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
            
        }

        public void OnUnHover()
        {
            
        }

        #endregion

        #region IDamageable Inherited Methods

        public void OnDealDamage(int _damageAmount, bool _armorPiercing ,ElementTyping _damageElementType)
        {
            characterLifeManager.DealDamage(_damageAmount, _armorPiercing, _damageElementType);
            PlayDamageVFX();
            if (characterAnimations != null)
            {
                characterAnimations.DamageAnim(true);
            }
        }

        #endregion

        #region IEffectable Inherited Methods

        public void ApplyEffect(Status.Status _newStatus)
        {
            //if the status only does something when applied, 
            //do the effect to this player but don't save it
            if (_newStatus.isImpactOnlyStatus)
            {
                _newStatus.TriggerStatusEffect(this);
                return;
            }
            
            appliedStatus = new AppliedStatus
            {
                status = _newStatus,
                roundTimer = _newStatus.roundCooldownTimer
            };

            if (_newStatus.statusVFX != null)
            {
                _newStatus.statusVFX.PlayAt(transform.position, Quaternion.identity, statusEffectTransform);    
            }
            
        }

        public void RemoveEffect()
        {
            var vfx = statusEffectTransform.GetComponentInChildren<VFXPlayer>();
            if (vfx != null)
            {
                vfx.Stop();
            }
            
            appliedStatus = null;
        }

        #endregion

        #region IBallInteractalbe Inherited Methods

        public void PickUpBall(BallBehavior ball)
        {
            heldBall = ball;
            heldBall.SetFollowTransform(characterWeaponManager.handPos);
            ballOwnerIndicator.SetActive(true);
            BallPickedUp?.Invoke(this);
        }

        public void KnockBallAway(Transform attacker)
        {
            if (heldBall.IsNull())
            {
                return;
            }

            var direction = transform.position - attacker.position;
            heldBall.ThrowBall(direction, shootSpeed, false);
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
            
            heldBall.ThrowBall(direction, shootSpeed, true);
            heldBall = null;
            isSetupThrowBall = false;
            ballOwnerIndicator.SetActive(false);
            UseActionPoint();
        }

        #endregion
    }
}