using System;
using System.Collections.Generic;
using Data.CharacterData;
using Data.Elements;
using Project.Scripts.Utils;
using Runtime.Abilities;
using Runtime.Environment;
using Runtime.GameControllers;
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
            public float roundCooldownPercentage;
            public int roundCooldown;
            public int maxRoundCooldown;
            public bool canUse;
            public float abilityUseRange;
            public float abilitySize;
            public Ability ability;
        }
        
        
        #endregion

        #region Events

        private Action OnAbilityUsed;
        
        public static event Action<CharacterBase> ActionUsed; 
        
        #endregion

        #region Serialize Fields

        [SerializeField] private LayerMask abilityUsageMask;

        [SerializeField] private Transform abilityUseTransform;
        
        [SerializeField] private List<AssignedAbilities> m_assignedAbilities = new List<AssignedAbilities>();

        [SerializeField] private GameObject abilityRangeIndicator;

        [SerializeField] private GameObject abilityPositionIndicator;

        [SerializeField] private LineRenderer abilityDirectionIndicator;

        #endregion

        #region Private Fields
        
        private int m_activeAbilityIndex = -1;

        private int m_previousActiveAbilityIndex;

        private int m_defaultInactiveAbilityIndex = -1;

        private CharacterMovement m_characterMovement;

        private CharacterRotation m_characterRotation;

        private CharacterBase m_characterBase;

        private float m_floorOffset = 0.1f;

        private bool m_isAbilityDisallowed;

        private AbilityTargetType m_allowedType;
        
        #endregion

        #region Accessors
        
        public bool isUsingAbilityAction => m_activeAbilityIndex != -1;

        public bool hasCanceledAbility { get; private set; }

        private Vector3 abilityPos => abilityUseTransform != null ? abilityUseTransform.position : transform.position;

        private CharacterMovement characterMovement =>
            CommonUtils.GetRequiredComponent(ref m_characterMovement, GetComponent<CharacterMovement>);
        
        private CharacterRotation characterRotation => CommonUtils.GetRequiredComponent(ref m_characterRotation, GetComponent<CharacterRotation>);
        
        private CharacterBase characterBase => CommonUtils.GetRequiredComponent(ref m_characterBase, GetComponent<CharacterBase>);
        
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

            if (!_selectedCharacter.isTargetable)
            {
                UIController.Instance.CreateFloatingTextAtCursor("Can't Target", Color.red);
                return;
            }
            
            SelectAbilityTarget(_selectedCharacter.transform);
        }

        public List<AssignedAbilities> GetAssignedAbilities()
        {
            return m_assignedAbilities;
        }

        public void ChangeAbilityCooldown(float _percentReduce)
        {
            if (m_assignedAbilities.Count == 0)
            {
                return;
            }

            foreach (var _assignedAbility in m_assignedAbilities)
            {
                _assignedAbility.maxRoundCooldown -= Mathf.CeilToInt(_assignedAbility.ability.roundCooldownTimer * _percentReduce);
            }
        }

        public Ability GetAbilityAtIndex(int _index)
        {
            Mathf.Clamp(_index, 0, 1);

            return m_assignedAbilities[_index].ability;

        }

        public Ability GetActiveAbility()
        {
            if (m_activeAbilityIndex == m_defaultInactiveAbilityIndex)
            {
                return default;
            }
            return m_assignedAbilities[m_activeAbilityIndex].ability;
        }

        public int GetActiveAbilityIndex()
        {
            return m_activeAbilityIndex;
        }

        public int GetPreviousActiveAbilityIndex()
        {
            return m_previousActiveAbilityIndex;
        }
        
        public void InitializeCharacterAbilityList(List<string> _abilities, ElementTyping _elementTyping, CharacterClassData _characterClass)
        {
            _abilities.ForEach(a =>
            {
                var locatedAbility = AbilityController.Instance.GetAbility(_elementTyping, _characterClass, a);
                var _newAssign = new AssignedAbilities
                {
                    ability = locatedAbility,
                    canUse = true,
                    roundCooldown = locatedAbility.roundCooldownTimer,
                    maxRoundCooldown = locatedAbility.roundCooldownTimer,
                    roundCooldownPercentage = 1f
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
                    roundCooldown = a.roundCooldownTimer,
                    maxRoundCooldown = a.roundCooldownTimer,
                    abilityUseRange = a.range,
                    abilitySize = a.abilitySize,
                    roundCooldownPercentage = 1f
                };

                m_assignedAbilities.Add(_newAssign);
            });
        }

        public void SetAbilityTypeAllowed(AbilityTargetType _targetType)
        {
            m_isAbilityDisallowed = true;
            m_allowedType = _targetType;
        }

        public void AllowAllAbilityActive()
        {
            m_isAbilityDisallowed = false;
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

            if (m_isAbilityDisallowed && m_assignedAbilities[_abilityIndex].ability.targetType != m_allowedType)
            {
                Debug.Log("This ability type isn't allowed");
                UIController.Instance.CreateFloatingTextAtCursor("Ability can't be used", Color.red);
                return;
            }

            if (!m_assignedAbilities[_abilityIndex].canUse)
            {
                Debug.LogError("Ability on cooldown");
                UIController.Instance.CreateFloatingTextAtCursor("Ability on Cooldown", Color.red);
                return;
            }

            hasCanceledAbility = false;
            m_assignedAbilities[_abilityIndex].ability.Initialize(this.gameObject);
            m_activeAbilityIndex = _abilityIndex;
            SetIndicators(true, m_assignedAbilities[_abilityIndex].ability.targetType == AbilityTargetType.DIRECTIONAL);
            abilityRangeIndicator.transform.localScale = Vector3.one * (m_assignedAbilities[_abilityIndex].abilityUseRange * 2);

            if (abilityUseCallback != null)
            {
                OnAbilityUsed = abilityUseCallback;
            }

            if (m_assignedAbilities[m_activeAbilityIndex].ability.targetType == AbilityTargetType.SELF)
            {
                SelectAbilityTarget(this.transform);
            }
            
        }

        public void MarkAbility(Vector3 _position)
        {
            if (m_activeAbilityIndex == -1)
            {
                return;
            }
            
            if (m_assignedAbilities[m_activeAbilityIndex].IsNull())
            {
                return;
            }

            switch (m_assignedAbilities[m_activeAbilityIndex].ability.targetType)
            {
                case AbilityTargetType.CHARACTER_TRANSFORM:
                    if (abilityPositionIndicator.activeInHierarchy)
                    {
                        abilityPositionIndicator.SetActive(false);
                    }
                    break;
                case AbilityTargetType.LOCATION: case AbilityTargetType.FREE:
                    var direction = _position - transform.position;
                    if (direction.magnitude > m_assignedAbilities[m_activeAbilityIndex].abilityUseRange)
                    {
                        return;
                    }
                    abilityPositionIndicator.transform.position = new Vector3(_position.x, m_floorOffset, _position.z);
                    break;
                case AbilityTargetType.DIRECTIONAL:
                    var localDirection = transform.InverseTransformDirection(_position - transform.position);
                    var finalPoint = (localDirection.normalized * m_assignedAbilities[m_activeAbilityIndex].abilityUseRange);
                    
                    abilityDirectionIndicator.SetPosition(1, new Vector3(finalPoint.x, finalPoint.z, 0));
                    break;
                case AbilityTargetType.SELF:
                    abilityPositionIndicator.transform.position = new Vector3(_position.x, m_floorOffset, _position.z);
                    break;
            }
            
        }

        public void CancelAbilityUse()
        {
            if (m_assignedAbilities[m_activeAbilityIndex] == null)
            {
                return;
            }

            SetIndicators(false,false);
            
            m_assignedAbilities[m_activeAbilityIndex].ability.CancelAbilityUse();
            m_activeAbilityIndex = m_defaultInactiveAbilityIndex;
            hasCanceledAbility = true;
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
                UIController.Instance.CreateFloatingTextAtCursor("Select Location", Color.red);
                return;
            }
            
            if (!InLineOfSight(_targetTransform.position))
            {
                Debug.Log("<color=red>Can't hit target</color>");
                UIController.Instance.CreateFloatingTextAtCursor("Not in Line of Sight", Color.red);
                return;
            }

            if (!IsInRange(_targetTransform.position))
            {
                Debug.Log($"<color=red>Target OUT OF RANGE</color>");
                UIController.Instance.CreateFloatingTextAtCursor("Out of Range", Color.red);
                CancelAbilityUse();
                return;
            }
            
            m_assignedAbilities[m_activeAbilityIndex].ability.SelectTarget(_targetTransform);
            if (_targetTransform != this.transform)
            {
                characterRotation.SetRotationTarget(_targetTransform.position);
            }
            
            SetIndicators(false, false);
            OnAbilityUsed?.Invoke();
        }

        public void UseActiveAbility()
        {
            m_assignedAbilities[m_activeAbilityIndex].ability.UseAbility(abilityPos);
            m_assignedAbilities[m_activeAbilityIndex].canUse = false;
            m_previousActiveAbilityIndex = m_activeAbilityIndex;
            m_activeAbilityIndex = m_defaultInactiveAbilityIndex;
            ActionUsed?.Invoke(characterBase);
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
                UIController.Instance.CreateFloatingTextAtCursor("Select Character", Color.red);

                CancelAbilityUse();
                return;
            }

            if (!IsInRange(_targetPos))
            {
                Debug.Log($"<color=red>Target OUT OF RANGE</color>");
                UIController.Instance.CreateFloatingTextAtCursor("Out of Range", Color.red);
                CancelAbilityUse();
                return;
            }
            
            m_assignedAbilities[m_activeAbilityIndex].ability.SelectPosition(_targetPos);
            characterRotation.SetRotationTarget(_targetPos);
            SetIndicators(false,false);
            OnAbilityUsed?.Invoke();
        }

        private void SetIndicators(bool _active, bool isDir)
        {
            if (abilityDirectionIndicator.IsNull() || abilityPositionIndicator.IsNull() || abilityRangeIndicator.IsNull())
            {
                return;
            }

            if (_active)
            {
                if (isDir)
                {
                    abilityDirectionIndicator.gameObject.SetActive(_active);
                }
                else
                {
                    abilityPositionIndicator.SetActive(_active);
                }
            }
            else
            {
                abilityDirectionIndicator.gameObject.SetActive(_active);
                abilityPositionIndicator.SetActive(_active);
            }
            
            abilityRangeIndicator.SetActive(_active);
        }

        public void CheckAbilityCooldown()
        {
            m_assignedAbilities.ForEach(aa =>
            {
                if (!aa.canUse)
                {
                    aa.roundCooldown--;
                    aa.roundCooldownPercentage = (float)aa.roundCooldown / aa.maxRoundCooldown;
                    if (aa.roundCooldown <= 0)
                    {
                        aa.canUse = true;
                        aa.roundCooldown = aa.maxRoundCooldown;
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

        private bool IsInRange(Vector3 _checkPos)
        {
            if (m_assignedAbilities[m_activeAbilityIndex].IsNull())
            {
                return false;
            }
            
            var dir = _checkPos - transform.position;
            var magnitude = dir.magnitude;

            if (magnitude <= m_assignedAbilities[m_activeAbilityIndex].abilityUseRange)
            {
                return true;
            }

            return false;
        }

        #endregion


    }
}