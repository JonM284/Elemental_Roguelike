using System.Collections.Generic;
using Runtime.Abilities;
using UnityEngine;

namespace Runtime.Character
{
    public class CharacterAbilityManager: MonoBehaviour
    {

        #region Serialize Fields

        [SerializeField] private List<Ability> assignedAbilities = new List<Ability>();

        #endregion

        #region Private Fields

        private List<int> assignedAbilitiesCooldown = new List<int>();

        #endregion

        #region Accessors

        public List<Ability> characterAbilities => assignedAbilities;

        #endregion

        #region Class Implementation

        /// <summary>
        /// Use abilities usable by this character. [ONLY TWO]
        /// </summary>
        /// <param name="_abilityIndex">First ability = 0, Second Ability = 1</param>
        public void UseAssignedAbility(int _abilityIndex)
        {
            //Initialize ability with owner [this gameobject]
            assignedAbilities[_abilityIndex].Initialize(this.gameObject);
        }

        #endregion


    }
}