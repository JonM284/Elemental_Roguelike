using System;
using System.Collections.Generic;
using Project.Scripts.Utils;
using Runtime.Abilities;
using Runtime.Environment;
using Runtime.Selection;
using UnityEngine;
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

        [SerializeField] private Transform abilityUseTransform;
        
        [SerializeField] private List<AssignedAbilities> m_assignedAbilities = new List<AssignedAbilities>();

        [SerializeField] private GameObject abilityRangeIndicator;

        #endregion

        #region Private Fields
        
        private int m_activeAbilityIndex = -1;

        private int m_defaultInactiveAbilityIndex = -1;

        private CharacterMovement m_characterMovement;

        private CharacterRotation m_characterRotation;

        private CharacterBase m_characterBase;
        #endregion

        #region Accessors
        
        public bool isUsingAbilityAction => m_activeAbilityIndex != -1;
        
        private Vector3 abilityPos => abilityUseTransform != null ? abilityUseTransform.position : transform.position;
        
        private CharacterMovement characterMovement => CommonUtils.GetRequiredComponent(ref m_characterMovement, () =>
        {
            var cm = this.GetComponent<CharacterMovement>();
            return cm;
        });
        
        private CharacterRotation characterRotation => CommonUtils.GetRequiredComponent(ref m_characterRotation, () =>
        {
            var cr = this.GetComponent<CharacterRotation>();
            return cr;
        });
        
        private CharacterBase characterBase => CommonUtils.GetRequiredComponent(ref m_characterBase, () =>
        {
            var cr = this.GetComponent<CharacterBase>();
            return cr;
        });
        
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

            if (_selectedCharacter == characterBase)
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
            abilityRangeIndicator.SetActive(true);
            abilityRangeIndicator.transform.localScale = Vector3.one * (m_assignedAbilities[_abilityIndex].ability.range * 2);

            
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

            abilityRangeIndicator.SetActive(false);
            
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
            characterRotation.SetRotationTarget(_targetTransform.position);
            abilityRangeIndicator.SetActive(false);
            OnAbilityUsed?.Invoke();
        }

        public void UseActiveAbility()
        {
            m_assignedAbilities[m_activeAbilityIndex].ability.UseAbility(abilityPos);
            m_assignedAbilities[m_activeAbilityIndex].canUse = false;
            m_activeAbilityIndex = m_defaultInactiveAbilityIndex;
        }

        public void SelectAbilityTarget(Vector3 _targetPos)
        {
            if (!isUsingAbilityAction)
            {
                return;
            }
            
            if (m_assignedAbilities[m_activeAbilityIndex] == null)
            {
                return;
            }

            if (m_assignedAbilities[m_activeAbilityIndex].ability.targetType　== AbilityTargetType.CHARACTER_TRANSFORM)
            {
                Debug.Log($"<color=red>Target Type:{m_assignedAbilities[m_activeAbilityIndex].ability.targetType}</color>");
                CancelAbilityUse();
                return;
            }
            
            m_assignedAbilities[m_activeAbilityIndex].ability.SelectPosition(_targetPos);
            characterRotation.SetRotationTarget(_targetPos);
            abilityRangeIndicator.SetActive(false);
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
        /// <param name="_checkPos">Target Transform Position</param>
        /// <returns>If enemy can be hit</returns>
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