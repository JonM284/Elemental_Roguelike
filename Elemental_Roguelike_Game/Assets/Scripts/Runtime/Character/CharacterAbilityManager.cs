using System;
using System.Collections.Generic;
using System.Linq;
using Runtime.Abilities;
using Runtime.Environment;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

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

        [SerializeField] private LayerMask abilityUsageMask;
        
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
        
        public void InitializeCharacterAbilityList(List<string> _abilities)
        {
            _abilities.ForEach(a =>
            {
                var locatedAbility = AbilityUtils.GetAbilityByGUID(a);
                var _newAssign = new AssignedAbilities
                {
                    ability = locatedAbility,
                    canUse = true,
                    roundCooldown = locatedAbility.roundCooldownTimer
                };

                m_assignedAbilities.Add(_newAssign);
            });
        }
        
        public void InitializeCharacterAbilityList(List<Ability> _abilities)
        {
            _abilities.ForEach(a =>
            {
                var _newAssign = new AssignedAbilities
                {
                    ability = a,
                    canUse = true,
                    roundCooldown = a.roundCooldownTimer
                };

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

            if (!InLineOfSight(_targetTransform.position))
            {
                Debug.Log("<color=red>Can't hit target</color>");
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

        /// <summary>
        /// Methods returns if obstacle is in direction trying to attack
        /// </summary>
        /// <param name="_checkPos"></param>
        /// <returns></returns>
        private bool InLineOfSight(Vector3 _checkPos)
        {
            var dir = transform.position - _checkPos;
            var dirMagnitude = dir.magnitude;
            var dirNormalized = dir.normalized;
            Debug.DrawRay(_checkPos, dirNormalized, Color.red, 10f);
            if (Physics.Raycast(_checkPos, dirNormalized, out RaycastHit hit, dirMagnitude, abilityUsageMask))
            {
                var _obstacle = hit.transform.GetComponent<CoverObstacles>();
                if (_obstacle != null && _obstacle.type == ObstacleType.FULL)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion


    }
}