using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Elements;
using Project.Scripts.Data;
using Project.Scripts.Utils;
using Runtime.Abilities;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Runtime.GameControllers
{
    public class AbilityController: GameControllerBase
    {

        #region Nested Classes

        [Serializable]
        public class AbilitiesByClass
        {
            public CharacterClassData assignedClass;
            public List<Ability> abilitiesOfElementClass = new List<Ability>(1);
        }
        
        [Serializable]
        public class AbilitiesByElement
        {
            public ElementTyping type;
            [FormerlySerializedAs("abilitiesOfType")] public List<AbilitiesByClass> abilitiesOfClassType = new List<AbilitiesByClass>(3);
        }

        #endregion
        
        #region Static

        public static AbilityController Instance { get; private set; }

        #endregion

        #region Serialized Fields

        [SerializeField] private List<Ability> allAbilities = new List<Ability>();

        [FormerlySerializedAs("abilitiesByType")] [SerializeField] private List<AbilitiesByElement> abilitiesByElement = new List<AbilitiesByElement>();

        #endregion

        #region Private Fields
        
        private List<Ability> m_unlockedAbilities = new List<Ability>();

        #endregion


        #region Controller Inherited Methods

        public override void Initialize()
        {
            Instance = this;
            base.Initialize();
        }

        #endregion

        #region Class Implementation

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

        public Ability GetAbility(string _ElementSearchGUID, string _classSearchGUID, string _abilitySearchGUID)
        {
            var foundElement = abilitiesByElement.FirstOrDefault(abe => abe.type.elementGUID == _ElementSearchGUID);
           
            if (CommonUtils.IsNull(foundElement))
            {
                Debug.Log("Element Doesn't Exist in LIST", this);
                return default;
            }
            
            var foundType =
                foundElement.abilitiesOfClassType.FirstOrDefault(abc =>
                    abc.assignedClass.classGUID == _classSearchGUID);
            
            if (CommonUtils.IsNull(foundType))
            {
                Debug.Log("Class Type Doesn't Exist in LIST", this);
                return default;
            }
            
            var foundAbility =
                foundType.abilitiesOfElementClass.FirstOrDefault(a => a.abilityGUID == _abilitySearchGUID);
            
            if (CommonUtils.IsNull(foundAbility))
            {
                Debug.Log("Ability Doesn't Exist in LIST", this);
                return default;
            }
            
            return foundAbility;
        }

        public List<Ability> GetAbilitiesByTypeAndClass(ElementTyping _type, CharacterClassData _class)
        {
            var foundType = abilitiesByElement.FirstOrDefault(abt => abt.type == _type);
            var foundClass = foundType.abilitiesOfClassType.FirstOrDefault(abc => abc.assignedClass == _class);
            return Enumerable.ToList(foundClass.abilitiesOfElementClass);
        }

        public Ability GetRandomAbility()
        {
            var randomInt = Random.Range(0, allAbilities.Count);
            return allAbilities[randomInt];
        }

        public Ability GetRandomAbilityByTypeAndClass(ElementTyping _type, CharacterClassData _class)
        {
            var foundType = abilitiesByElement.FirstOrDefault(abt => abt.type == _type);
            var foundClass = foundType.abilitiesOfClassType.FirstOrDefault(abc => abc.assignedClass == _class);
            var foundAbilities = foundClass.abilitiesOfElementClass;
            var randomAbilityIndex = Random.Range(0, foundAbilities.Count);
            return foundAbilities[randomAbilityIndex];
        }
        
        public Ability GetRandomAbilityByTypeAndClass(string guid, CharacterClassData _class)
        {
            var foundElement = abilitiesByElement.FirstOrDefault(abt => abt.type.elementGUID == guid);
            var foundClass = foundElement.abilitiesOfClassType.FirstOrDefault(abc => abc.assignedClass == _class);
            var randomAbilityIndex = Random.Range(0, foundClass.abilitiesOfElementClass.Count);
            return foundClass.abilitiesOfElementClass[randomAbilityIndex];
        }
        
        public Ability GetRandomAbilityByTypeAndClass(string _elementGUID, string _classGUID)
        {
            var foundElement = abilitiesByElement.FirstOrDefault(abt => abt.type.elementGUID == _elementGUID);
            var foundClass = foundElement.abilitiesOfClassType.FirstOrDefault(abc => abc.assignedClass.classGUID == _classGUID);
            var randomAbilityIndex = Random.Range(0, foundClass.abilitiesOfElementClass.Count);
            return foundClass.abilitiesOfElementClass[randomAbilityIndex];
        }
        
        public Ability GetRandomAbilityByTypeAndClass(ElementTyping _type, CharacterClassData _class ,Ability _excludingAbility)
        {
            var foundElement = abilitiesByElement.FirstOrDefault(abt => abt.type == _type);
            var foundClass = foundElement.abilitiesOfClassType.FirstOrDefault(abc => abc.assignedClass == _class);
            var abilitiesExclusive = Enumerable.ToList(foundClass.abilitiesOfElementClass);
            abilitiesExclusive.Remove(_excludingAbility);
            var randomAbilityIndex = Random.Range(0, abilitiesExclusive.Count);
            return abilitiesExclusive[randomAbilityIndex];
        }

        #endregion

    }
}