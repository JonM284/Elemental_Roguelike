using System;
using Data;
using Data.Elements;
using Project.Scripts.Data;
using Project.Scripts.Utils;
using Runtime.Damage;
using Runtime.GameControllers;
using Runtime.Selection;
using Unity.VisualScripting;
using UnityEngine;

namespace Runtime.Character
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterAbilityManager))]
    [RequireComponent(typeof(CharacterLifeManager))]
    [RequireComponent(typeof(CharacterMovement))]
    [RequireComponent(typeof(CharacterRotation))]
    [RequireComponent(typeof(CharacterVisuals))]
    public abstract class CharacterBase: MonoBehaviour, ISelectable,IDamageable
    {

        #region Events

        public static event Action<CharacterBase> CharacterEndedTurn; 

        #endregion

        #region Serialized Fields

        [SerializeField] private CharacterSide characterSide;

        #endregion

        #region Private Fields

        protected int m_characterActionPoints = 2;

        protected bool m_isActive;
        
        protected bool m_finishedTurn;

        protected CharacterAbilityManager m_characterAbilityManager;
        
        protected CharacterLifeManager m_characterLifeManager;
        
        protected CharacterMovement m_characterMovement;
        
        protected CharacterRotation m_characterRotation;
        
        protected CharacterVisuals m_characterVisuals;
        
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

        public bool isAlive => characterLifeManager.isAlive;

        public bool finishedTurn => m_finishedTurn;

        public bool isInBattle => characterMovement.isInBattle;

        public int characterActionPoints => m_characterActionPoints;

        public int initiativeNum => GetInitiativeNumber();

        public float baseSpeed => GetBaseSpeed();

        public CharacterSide side => characterSide;

        public bool isBusy => characterMovement.isMoving;
        
        
        public CharacterStatsBase m_characterStatsBase { get; private set; }

        
        #endregion

        #region Class Implementation

        public abstract void InitializeCharacter();

        public abstract int GetInitiativeNumber();

        public abstract float GetBaseSpeed();

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

        protected virtual void OnAttackActionPressed()
        {
            
        }


        public void InitializeCharacterBattle(bool _isInBattle)
        {
            characterMovement.SetCharacterBattleStatus(_isInBattle);
        }

        /// <summary>
        /// Whenever it is this characters turn, ability cooldown's go down by 1
        /// </summary>
        public void CheckAbilityCooldown()
        {
            
        }

        public void UseCharacterAbility(int _abilityIndex)
        {
            if (m_characterActionPoints <= 0)
            {
                return;
            }
            
            if (characterAbilityManager.characterAbilities[_abilityIndex] == null)
            {
                return;
            }
            
            characterAbilityManager.UseAssignedAbility(_abilityIndex);
            UseActionPoint();
        }

        public void UseCharacterWeapon()
        {
            if (m_characterActionPoints <= 0)
            {
                return;
            }
            
            Debug.Log("<color=yellow>Shoot</color>");
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
        
    }
}