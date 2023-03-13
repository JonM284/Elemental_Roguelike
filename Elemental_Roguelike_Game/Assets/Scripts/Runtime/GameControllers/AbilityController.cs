using System;
using System.Collections.Generic;
using System.Linq;
using Data.Elements;
using Project.Scripts.Data;
using Runtime.Abilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Runtime.GameControllers
{
    public class AbilityController: GameControllerBase
    {

        #region Nested Classes
        
        [Serializable]
        public class AbilitiesByType
        {
            public ElementTyping type;
            public List<Ability> abilitiesOfType = new List<Ability>();
        }

        #endregion

        #region Serialized Fields

        [SerializeField] private List<Ability> allAbilities = new List<Ability>();

        [SerializeField] private List<AbilitiesByType> abilitiesByType = new List<AbilitiesByType>();

        #endregion

        #region Private Fields
        
        private List<Ability> m_unlockedAbilities = new List<Ability>();

        #endregion


        #region Controller Inherited Methods

        public Ability GetAbilityByName(string _searchName)
        {
            var foundAbility = allAbilities.FirstOrDefault(a => a.abilityName == _searchName);
            if (foundAbility == null)
            {
                return default;
            }
            return foundAbility;
        }

        public Ability GetAbilityByGUID(string _searchGUID)
        {
            var foundAbility = allAbilities.FirstOrDefault(a => a.abilityGUID == _searchGUID);
            if (foundAbility == null)
            {
                return default;
            }

            return foundAbility;
        }

        public List<Ability> GetAbilitiesByType(ElementTyping _type)
        {
            var foundAbilities = abilitiesByType.FirstOrDefault(abt => abt.type == _type);
            return foundAbilities.abilitiesOfType.ToList();
        }

        public Ability GetRandomAbility()
        {
            var randomInt = Random.Range(0, allAbilities.Count);
            return allAbilities[randomInt];
        }

        public Ability GetRandomAbilityByType(ElementTyping _type)
        {
            var foundAbilities = abilitiesByType.FirstOrDefault(abt => abt.type == _type);
            var randomAbilityIndex = Random.Range(0, foundAbilities.abilitiesOfType.Count);
            return foundAbilities.abilitiesOfType[randomAbilityIndex];
        }
        
        public Ability GetRandomAbilityByType(ElementTyping _type, Ability _excludingAbility)
        {
            var foundAbilities = abilitiesByType.FirstOrDefault(abt => abt.type == _type);
            var abilitiesExclusive = foundAbilities.abilitiesOfType.ToList();
            abilitiesExclusive.Remove(_excludingAbility);
            var randomAbilityIndex = Random.Range(0, abilitiesExclusive.Count);
            return abilitiesExclusive[randomAbilityIndex];
        }

        #endregion

    }
}