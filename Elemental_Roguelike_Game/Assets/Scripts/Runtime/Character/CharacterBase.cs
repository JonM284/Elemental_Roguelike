using System;
using Data.Elements;
using Data.Sides;
using Project.Scripts.Data;
using Project.Scripts.Utils;
using Runtime.Damage;
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
    public abstract class CharacterBase: MonoBehaviour, ISelectable, IDamageable, IEffectable
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

        #endregion

        #region Serialized Fields

        [SerializeField] protected CharacterStatsBase m_characterStatsBase;

        [SerializeField] private CharacterSide characterSide;

        [SerializeField] private VFXPlayer deathVFX;

        [SerializeField] private VFXPlayer damageVFX;

        [SerializeField] private GameObject activeCharacterIndicator;

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

        public bool finishedTurn => m_finishedTurn;

        public bool isInBattle => characterMovement.isInBattle;

        public int characterActionPoints => m_characterActionPoints;

        public int initiativeNum => GetInitiativeNumber();

        public float baseSpeed => GetBaseSpeed();

        public CharacterSide side => characterSide;

        public bool isBusy => characterMovement.isMoving;

        public AppliedStatus appliedStatus { get; private set; }

        public CharacterStatsBase characterStatsBase => m_characterStatsBase;
        
        
        #endregion

        #region Unity Events

        private void OnEnable()
        {
            CharacterLifeManager.OnCharacterDied += OnCharacterDied;
        }

        private void OnDisable()
        {
            CharacterLifeManager.OnCharacterDied -= OnCharacterDied;
        }

        #endregion
        
        #region Class Implementation

        public abstract void InitializeCharacter();

        public abstract int GetInitiativeNumber();

        public abstract float GetBaseSpeed();
        
        protected abstract void CharacterDeath();

        protected virtual void OnWalkActionPressed()
        {
            
        }

        protected virtual void OnBeginWalkAction()
        {
            UseActionPoint();
        }

        protected virtual void OnWalkActionEnded()
        {
            
        }

        private void OnCharacterDied(CharacterBase _character)
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
            
            characterWeaponManager.SetupWeaponAction(OnAttackUsed);
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

            OnWalkActionPressed();
            characterMovement.SetCharacterMovable(OnBeginWalkAction, OnWalkActionEnded);
        }

        public void UseActionPoint()
        {
            m_characterActionPoints--;
            Debug.Log($"<color=yellow>Used action point // Action Points:{characterActionPoints} left</color>");
        }

        [ContextMenu("Reset Actions")]
        public void ResetCharacterActions()
        {
            if (!isAlive)
            {
                return;
            }
            
            m_finishedTurn = false;
            m_characterActionPoints = 2;
            characterAbilityManager.CheckAbilityCooldown();
            CheckStatus();
            SetActiveVisual(true);
        }

        private void SetActiveVisual(bool isActive)
        {
            if (activeCharacterIndicator == null)
            {
                return;
            }
            
            activeCharacterIndicator.SetActive(isActive);
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
            Debug.Log($"{this} has ended turn");
            m_finishedTurn = true;
            SetActiveVisual(false);
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
        
    }
}