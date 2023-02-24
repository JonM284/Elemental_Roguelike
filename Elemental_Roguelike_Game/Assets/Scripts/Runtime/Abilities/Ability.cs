using System;
using Data.Elements;
using UnityEngine;

namespace Runtime.Abilities
{
    [Serializable]
    public abstract class Ability: ScriptableObject
    {

        //Types of abilities include {Projectile, Puppet, Cast, Movement}
        //Most ability types can be summarized to this
        
        #region Public Fields

        public string abilityName;

        //cooldowns are determined by rounds
        public int roundCooldownTimer;

        public float baseDamage;

        public ElementTyping abilityElement;

        public float criticalMultiplier;

        public bool hasPrerequisites;

        #endregion

        #region Private Fields

        private bool m_canUseAbility = true;

        private AbilityState m_abilityCurrentState = AbilityState.READY_TO_USE;

        #endregion

        #region Protected Fields

        protected GameObject currentOwner;

        protected Transform m_targetTransform;

        protected Vector3 m_targetPosition;

        #endregion

        #region Accessors

        public bool isUnlocked { get; private set; }

        #endregion

        #region Class Implementation
        
        //Step 1: Select Ability to use
        //Initialization happens when user (player or Ai) want to use this SO
        public void Initialize(GameObject _ownerObj)
        {
            m_abilityCurrentState = AbilityState.READY_TO_USE;
            m_canUseAbility = true;
            currentOwner = _ownerObj;
        }

        //Step 2: Select Where or on who to use Ability
        //Select Location or Target
        public abstract void SelectPosition(Vector3 _inputPosition);

        public abstract void SelectTarget(Transform _inputTransform);
        
        //Step 3: Use Ability
        //Use ability is different for each ability type
        public abstract void UseAbility();

        //Step 4: Put Ability on cooldown
        public virtual void AbilityUsed()
        {
            m_abilityCurrentState = AbilityState.COOLDOWN;
        }

        public virtual void AbilityReset()
        {
            m_abilityCurrentState = AbilityState.READY_TO_USE;
            m_canUseAbility = true;
        }

        public void UnlockAbility()
        {
            isUnlocked = true;
        }

        #endregion



    }
}