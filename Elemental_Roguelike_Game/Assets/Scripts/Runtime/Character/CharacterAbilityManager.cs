﻿using System;
using System.Collections.Generic;
using System.Linq;
using Runtime.Abilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Runtime.Character
{
    public class CharacterAbilityManager: MonoBehaviour
    {

        #region Nested Classes

        [Serializable]
        public class AssignedAbilities
        {
            public int roundCooldown;
            public bool canUse;
            public Ability ability;
        }
        
        
        #endregion

        #region Events

        private Action OnAbilityUsed;

        #endregion

        #region Serialize Fields

        [SerializeField] private List<AssignedAbilities> m_assignedAbilities = new List<AssignedAbilities>();

        #endregion

        #region Private Fields
        
        private int m_activeAbilityIndex = -1;

        private int m_defaultInactiveAbilityIndex = -1;
        
        #endregion

        #region Accessors
        
        public bool isUsingAbilityAction => m_activeAbilityIndex != -1;

        #endregion

        #region Unity Events

        private void OnEnable()
        {
            CharacterBase.CharacterSelected += OnCharacterSelected;
        }

        private void OnDisable()
        {
            CharacterBase.CharacterSelected -= OnCharacterSelected;
        }

        #endregion

        #region Class Implementation

        private void OnCharacterSelected(CharacterBase _selectedCharacter)
        {
            if (!isUsingAbilityAction)
            {
                return;
            }
            
            SelectAbilityTarget(_selectedCharacter.transform);
        }
        
        public void InitializeCharacterAbilityList(List<Ability> _abilities)
        {
            _abilities.ForEach(a =>
            {
                var _newAssign = new AssignedAbilities();
                _newAssign.ability = a;
                _newAssign.canUse = true;
                _newAssign.roundCooldown = a.roundCooldownTimer;
                
                m_assignedAbilities.Add(_newAssign);
            });
        }

        /// <summary>
        /// Use abilities usable by this character. [ONLY TWO]
        /// </summary>
        /// <param name="_abilityIndex">First ability = 0, Second Ability = 1</param>
        public void UseAssignedAbility(int _abilityIndex, Action abilityUseCallback)
        {
            if (m_assignedAbilities[_abilityIndex] == null)
            {
                Debug.Log($"Ability doesn't exist {this.gameObject.name} /// Ability:{_abilityIndex}", this.gameObject);
                return;
            }

            if (!m_assignedAbilities[_abilityIndex].canUse)
            {
                Debug.LogError("Ability on cooldown");
                return;
            }
            
            
            m_assignedAbilities[_abilityIndex].ability.Initialize(this.gameObject);
            m_activeAbilityIndex = _abilityIndex;
            
            if (abilityUseCallback != null)
            {
                OnAbilityUsed = abilityUseCallback;
            }
        }

        public void CancelAbilityUse()
        {
            if (m_assignedAbilities[m_activeAbilityIndex] == null)
            {
                return;
            }
            
            m_assignedAbilities[m_activeAbilityIndex].ability.CancelAbilityUse();
            m_activeAbilityIndex = m_defaultInactiveAbilityIndex;
            OnAbilityUsed = null;
        }
        
        public void SelectAbilityTarget(Transform _targetTransform)
        {
            if (m_assignedAbilities[m_activeAbilityIndex] == null)
            {
                return;
            }

            if (m_assignedAbilities[m_activeAbilityIndex].ability.targetType　== AbilityTargetType.LOCATION)
            {
                Debug.Log($"<color=red>Target Type:{m_assignedAbilities[m_activeAbilityIndex].ability.targetType}</color>");
                return;
            }
            
            m_assignedAbilities[m_activeAbilityIndex].ability.SelectTarget(_targetTransform);
            m_assignedAbilities[m_activeAbilityIndex].canUse = false;
            m_activeAbilityIndex = m_defaultInactiveAbilityIndex;
            OnAbilityUsed?.Invoke();
        }

        public void SelectAbilityTarget(Vector3 _targetPos)
        {
            if (m_assignedAbilities[m_activeAbilityIndex] == null)
            {
                return;
            }

            if (m_assignedAbilities[m_activeAbilityIndex].ability.targetType　== AbilityTargetType.CHARACTER_TRANSFORM)
            {
                Debug.Log($"<color=red>Target Type:{m_assignedAbilities[m_activeAbilityIndex].ability.targetType}</color>");
                return;
            }
            
            m_assignedAbilities[m_activeAbilityIndex].ability.SelectPosition(_targetPos);
            m_assignedAbilities[m_activeAbilityIndex].canUse = false;
            m_activeAbilityIndex = m_defaultInactiveAbilityIndex;
            OnAbilityUsed?.Invoke();
        }

        public void CheckAbilityCooldown()
        {
            m_assignedAbilities.ForEach(aa =>
            {
                if (!aa.canUse)
                {
                    aa.roundCooldown--;
                    if (aa.roundCooldown <= 0)
                    {
                        aa.canUse = true;
                        aa.roundCooldown = aa.ability.roundCooldownTimer;
                    }
                }
            });
        }

        #endregion


    }
}