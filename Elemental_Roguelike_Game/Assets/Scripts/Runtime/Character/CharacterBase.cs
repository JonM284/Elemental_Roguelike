using System;
using Data.Elements;
using Project.Scripts.Data;
using Project.Scripts.Utils;
using Runtime.Damage;
using Runtime.Selection;
using Runtime.Status;
using Runtime.VFX;
using UnityEngine;
using Utils;

namespace Runtime.Character
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterAbilityManager))]
    [RequireComponent(typeof(CharacterLifeManager))]
    [RequireComponent(typeof(CharacterMovement))]
    [RequireComponent(typeof(CharacterRotation))]
    [RequireComponent(typeof(CharacterVisuals))]
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

        [SerializeField] private CharacterSide characterSide;

        #endregion

        #region Protected Fields

        protected int m_characterActionPoints = 2;

        protected bool m_isActive;
        
        protected bool m_finishedTurn;
        
        protected CharacterAbilityManager m_characterAbilityManager;
        
        protected CharacterLifeManager m_characterLifeManager;
        
        protected CharacterMovement m_characterMovement;
        
        protected CharacterRotation m_characterRotation;
        
        protected CharacterVisuals m_characterVisuals;

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
        
        public CharacterRotation characterRotation => CommonUtils.GetRequiredComponent(ref m_characterRotation,
            () =>
            {
                var cr = GetComponent<CharacterRotation>();
                return cr;
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

        public bool isAlive => characterLifeManager.isAlive;

        public bool finishedTurn => m_finishedTurn;

        public bool isInBattle => characterMovement.isInBattle;

        public int characterActionPoints => m_characterActionPoints;

        public int initiativeNum => GetInitiativeNumber();

        public float baseSpeed => GetBaseSpeed();

        public CharacterSide side => characterSide;

        public bool isBusy => characterMovement.isMoving;

        public AppliedStatus appliedStatus { get; private set; }
        
        
        public CharacterStatsBase m_characterStatsBase { get; private set; }

        
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
            
            Debug.Log($"{this} has died");
            RemoveEffect();
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

            characterAbilityManager.UseAssignedAbility(_abilityIndex, UseActionPoint);
        }

        public void UseCharacterWeapon()
        {
            if (m_characterActionPoints <= 0)
            {
                return;
            }
            
            Debug.Log("<color=yellow>Shoot</color>");
            //ToDo: Add Weapons
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

        public void ResetCharacterActions()
        {
            if (!isAlive)
            {
                return;
            }
            
            m_finishedTurn = false;
            m_characterActionPoints = 2;
            m_isActive = true;
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
            Debug.Log($"{this} has ended turn");
            m_finishedTurn = true;
            CharacterEndedTurn?.Invoke(this);
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