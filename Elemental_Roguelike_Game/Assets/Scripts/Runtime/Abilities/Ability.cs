using Project.Scripts.Data;
using UnityEngine;

namespace Runtime.Abilities
{
    public abstract class Ability: MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private CharacterAbilityData characterAbilityData;

        #endregion

        #region Private Fields

        private bool m_canUseAbility = true;

        private AbilityState m_abilityCurrentState = AbilityState.READY_TO_USE;

        #endregion

        #region Protected Fields

        protected Transform _targetTransform;

        protected Vector3 _targetPosition;

        #endregion

        #region Accessors

        public int abilityCooldownTimer => characterAbilityData.roundCooldownTimer;

        public float abilityDamage => characterAbilityData.baseDamage;

        public float abilityCritMultiplier => characterAbilityData.criticalMultiplier;

        public bool canUseAbility => m_canUseAbility;

        public bool abilityHasPrerequisites => characterAbilityData.hasPrerequisites;

        #endregion

        #region Class Implementation

        public virtual void Initialize()
        {
            m_abilityCurrentState = AbilityState.READY_TO_USE;
            m_canUseAbility = true;
        }

        public virtual void SelectPosition(Vector3 _inputPosition)
        {
            m_abilityCurrentState = AbilityState.SELECTING;
        }

        public virtual void SelectTarget(Transform _inputTransform)
        {
            m_abilityCurrentState = AbilityState.SELECTING;
        }

        public virtual void UseAbility()
        {
            m_abilityCurrentState = AbilityState.IN_USE;
            m_canUseAbility = false;
        }

        public virtual void AbilityUsed()
        {
            m_abilityCurrentState = AbilityState.COOLDOWN;
        }

        public virtual void AbilityReset()
        {
            m_abilityCurrentState = AbilityState.READY_TO_USE;
            m_canUseAbility = true;
        }

        #endregion



    }
}