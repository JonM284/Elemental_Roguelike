using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Data.AbilityDatas;
using Project.Scripts.Utils;
using Runtime.Abilities;
using Runtime.Environment;
using Runtime.GameControllers;
using UnityEngine;

namespace Runtime.Character
{
    public class CharacterAbilityManager: MonoBehaviour
    {
        
        #region Events

        private Action OnAbilityUsed;
        
        public static event Action<CharacterBase> ActionUsed; 
        
        #endregion

        #region Serialize Fields

        [SerializeField] private LayerMask abilityUsageMask;

        [SerializeField] private Transform abilityUseTransform;
        
        [SerializeField] private Transform abilityParent;

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

        private List<AbilityEntityBase> assignedAbilities = new List<AbilityEntityBase>();
        
        #endregion

        #region Accessors
        
        public bool isUsingAbilityAction => m_activeAbilityIndex != -1;

        public bool hasCanceledAbility { get; private set; }

        public bool hasAvailableAbility => assignedAbilities.Any(a => a.canUseAbility);

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

            if (!_selectedCharacter.isTargetable)
            {
                UIController.Instance.CreateFloatingTextAtCursor("Can't Target", Color.red);
                return;
            }
            
            SelectAbilityTarget(_selectedCharacter.transform);
        }

        public List<AbilityEntityBase> GetAssignedAbilities()
        {
            return assignedAbilities;
        }
        
        public List<AbilityEntityBase> GetAssignedAbilitiesNewList()
        {
            return assignedAbilities.ToList();
        }

        public void ChangeAbilityCooldown(float _percentReduce)
        {
            if (assignedAbilities.Count == 0)
            {
                return;
            }

            foreach (var _assignedAbility in assignedAbilities)
            {
                var _reduceAmount = Mathf.CeilToInt(_assignedAbility.abilityCooldownMax * _percentReduce);
                _assignedAbility.abilityCooldownMax -= _reduceAmount;
            }
        }

        public AbilityEntityBase GetAbilityAtIndex(int _index)
        {
            _index %= assignedAbilities.Count;

            return assignedAbilities[_index];

        }

        public AbilityEntityBase GetActiveAbility()
        {
            return m_activeAbilityIndex == m_defaultInactiveAbilityIndex ? default : assignedAbilities[m_activeAbilityIndex];
        }

        public int GetActiveAbilityIndex()
        {
            return m_activeAbilityIndex;
        }

        public int GetPreviousActiveAbilityIndex()
        {
            return m_previousActiveAbilityIndex;
        }
        
        public async UniTask InitializeCharacterAbilityList(List<AbilityData> abilityDatas)
        {
            if (abilityDatas.Count == 0)
            {
                return;
            }

            foreach (var abilityData in abilityDatas)
            {
                if (abilityData.IsNull())
                {
                    continue;
                }
                
                var _currentAbilityPrefab = Instantiate(abilityData.abilityGameObject, abilityParent);
                _currentAbilityPrefab.TryGetComponent(out AbilityEntityBase abilityComponent);
                assignedAbilities.Add(abilityComponent);
                abilityComponent.InitializeAbility(this.characterBase, abilityData).Forget();
            }
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
        public void ActivateAssignedAbilityAtIndex(int abilityIndex, Action abilityUseCallback)
        {
            if (abilityIndex >= assignedAbilities.Count || assignedAbilities[abilityIndex].IsNull())
            {
                Debug.Log($"Ability doesn't exist {this.gameObject.name} /// Ability:{abilityIndex}", this.gameObject);
                return;
            }

            if (m_isAbilityDisallowed && assignedAbilities[abilityIndex].abilityData.targetType != m_allowedType)
            {
                Debug.Log("This ability type isn't allowed");
                UIController.Instance.CreateFloatingTextAtCursor("Ability can't be used", Color.red);
                return;
            }

            if (!assignedAbilities[abilityIndex].canUseAbility)
            {
                Debug.LogError("Ability on cooldown");
                UIController.Instance.CreateFloatingTextAtCursor("Ability on Cooldown", Color.red);
                return;
            }

            hasCanceledAbility = false;
            assignedAbilities[abilityIndex].ShowAttackIndicator(true);
            m_activeAbilityIndex = abilityIndex;
            //SetIndicators(true, m_assignedAbilities[_abilityIndex].ability.targetType == AbilityTargetType.DIRECTIONAL);
            //abilityRangeIndicator.transform.localScale = Vector3.one * (assignedAbilities[abilityIndex].currentRange * 2);

            if (abilityUseCallback != null)
            {
                OnAbilityUsed = abilityUseCallback;
            }

            if (assignedAbilities[m_activeAbilityIndex].abilityData.targetType == AbilityTargetType.SELF)
            {
                SelectAbilityTarget(this.transform);
            }
            
        }

        public void MarkAbility(Vector3 position)
        {
            if (m_activeAbilityIndex == -1)
            {
                return;
            }
            
            if (assignedAbilities[m_activeAbilityIndex].IsNull())
            {
                return;
            }

            assignedAbilities[m_activeAbilityIndex].MarkHighlight(position);
        }

        public void CancelAbilityUse()
        {
            if (m_activeAbilityIndex == -1 || 
                assignedAbilities.Count == 0 ||
                m_activeAbilityIndex >= assignedAbilities.Count ||
                assignedAbilities[m_activeAbilityIndex].IsNull())
            {
                return;
            }

            //SetIndicators(false,false);
            
            assignedAbilities[m_activeAbilityIndex].ShowAttackIndicator(false);
            m_activeAbilityIndex = m_defaultInactiveAbilityIndex;
            hasCanceledAbility = true;
            OnAbilityUsed = null;
        }
        
        public void SelectAbilityTarget(Transform _targetTransform)
        {
            if (assignedAbilities[m_activeAbilityIndex] == null)
            {
                return;
            }

            if (assignedAbilities[m_activeAbilityIndex].abilityData.targetType　== AbilityTargetType.LOCATION)
            {
                Debug.Log($"<color=red>Target Type:{assignedAbilities[m_activeAbilityIndex].abilityData.targetType}</color>");
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
            
            assignedAbilities[m_activeAbilityIndex].SelectTarget(_targetTransform);
            
            if (_targetTransform != this.transform)
            {
                characterRotation.SetRotationTarget(_targetTransform.position);
            }
            
            assignedAbilities[m_activeAbilityIndex].ShowAttackIndicator(false);
            
            //SetIndicators(false, false);
            OnAbilityUsed?.Invoke();
        }

        public async UniTask UseActiveAbility()
        {
            if (m_activeAbilityIndex >= assignedAbilities.Count && m_activeAbilityIndex < 0)
            {
                return;
            }
            
            await assignedAbilities[m_activeAbilityIndex].UseAbility();
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
            
            if (assignedAbilities[m_activeAbilityIndex] == null)
            {
                return;
            }

            if (!IsInRange(_targetPos))
            {
                Debug.Log($"<color=red>Target OUT OF RANGE</color>");
                UIController.Instance.CreateFloatingTextAtCursor("Out of Range", Color.red);
                CancelAbilityUse();
                return;
            }
            
            assignedAbilities[m_activeAbilityIndex].SelectPosition(_targetPos);
            characterRotation.SetRotationTarget(_targetPos);
            //SetIndicators(false,false);
            OnAbilityUsed?.Invoke();
        }

        public void CheckAbilityCooldown()
        {
            assignedAbilities.ForEach(aa =>
            {
                if (aa.canUseAbility)
                {
                    return;
                }
                aa.abilityCooldownCurrent++;
                //aa.roundCooldownPercentage = (float)aa.roundCooldown / aa.maxRoundCooldown;
                
                if (aa.abilityCooldownCurrent < aa.abilityCooldownMax)
                {
                    return;
                }
                
                aa.abilityCooldownCurrent = aa.abilityCooldownMax;
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
            if (!Physics.Raycast(_checkPos, dirNormalized, out RaycastHit hit, dirMagnitude, abilityUsageMask))
                return true;
            var _obstacle = hit.transform.GetComponent<CoverObstacles>();
            return _obstacle == null || _obstacle.type != ObstacleType.FULL;
        }

        private bool IsInRange(Vector3 _checkPos)
        {
            if (assignedAbilities[m_activeAbilityIndex].IsNull())
            {
                return false;
            }
            
            var dir = _checkPos - transform.position;

            return dir.magnitude <= assignedAbilities[m_activeAbilityIndex].currentRange - 0.08f;
        }

        #endregion


    }
}