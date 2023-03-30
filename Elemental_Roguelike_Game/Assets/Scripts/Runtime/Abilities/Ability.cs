using System;
using Data.Elements;
using Runtime.Character;
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

        public string abilityGUID;

        //cooldowns are determined by rounds
        public int roundCooldownTimer;
        
        public AbilityTargetType targetType;

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
            currentOwner = _ownerObj;
        }

        //Step 2: Select Where or on who to use Ability
        //Select Location or Target
        public abstract void SelectPosition(Vector3 _inputPosition);

        public abstract void SelectTarget(Transform _inputTransform);
        
        //Step 3: Use Ability
        //Use ability is different for each ability type
        public virtual void UseAbility(Vector3 _ownerUsePos)
        {
            currentOwner = null;
            m_targetTransform = null;
        }

        public void CancelAbilityUse()
        {
            currentOwner = null;
        }

        public void UnlockAbility()
        {
            isUnlocked = true;
        }

        [ContextMenu("Generate GUID")]
        private void GenerateID()
        {
            abilityGUID = System.Guid.NewGuid().ToString();
        }

        #endregion



    }
}