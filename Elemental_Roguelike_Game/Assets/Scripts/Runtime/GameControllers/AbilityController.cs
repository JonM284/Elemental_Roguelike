using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Data.AbilityDatas;
using Data.CharacterData;
using Data.Elements;
using NUnit.Framework;
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
            public List<AbilityData> abilitiesOfElementClass = new List<AbilityData>(1);
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
        
        [FormerlySerializedAs("abilitiesByType")] [SerializeField] private List<AbilitiesByElement> abilitiesByElement = new List<AbilitiesByElement>();

        [SerializeField] private List<AbilityData> allAbilities = new List<AbilityData>();
        
        #endregion

        #region Private Fields
        
        private List<AbilityData> m_unlockedAbilities = new List<AbilityData>();

        #endregion


        #region Controller Inherited Methods

        public override void Initialize()
        {
            if (!Instance.IsNull())
            {
                return;
            }
            
            Instance = this;
            base.Initialize();
        }

        #endregion

        #region Class Implementation

        public AbilityData GetAbilityData(string searchGUID)
        {
            return allAbilities.FirstOrDefault(ad => ad.abilityGUID == searchGUID);
        }

        public AbilityData GetAbilityData(ElementTyping _element, CharacterClassData classData, string _abilitySearchGUID)
        {
            var foundElement = abilitiesByElement.FirstOrDefault(abe => abe.type == _element);
           
            if (CommonUtils.IsNull(foundElement))
            {
                Debug.Log("Element Doesn't Exist in LIST", this);
                return default;
            }
            
            var foundType =
                foundElement.abilitiesOfClassType.FirstOrDefault(abc =>
                    abc.assignedClass == classData);
            
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

        public AbilityData GetAbilityData(string _ElementSearchGUID, string _classSearchGUID, string _abilitySearchGUID)
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

        public List<AbilityData> GetAbilitiesByTypeAndClass(ElementTyping _type, CharacterClassData _class)
        {
            var foundType = abilitiesByElement.FirstOrDefault(abt => abt.type == _type);
            var foundClass = foundType.abilitiesOfClassType.FirstOrDefault(abc => abc.assignedClass == _class);
            return Enumerable.ToList(foundClass.abilitiesOfElementClass);
        }
        
        public AbilityData GetRandomAbilityByTypeAndClass(ElementTyping _type, CharacterClassData _class)
        {
            var foundType = abilitiesByElement.FirstOrDefault(abt => abt.type == _type);
            var foundClass = foundType.abilitiesOfClassType.FirstOrDefault(abc => abc.assignedClass == _class);
            var foundAbilities = foundClass.abilitiesOfElementClass;
            var randomAbilityIndex = Random.Range(0, foundAbilities.Count);
            return foundAbilities[randomAbilityIndex];
        }
        
        public AbilityData GetRandomAbilityByTypeAndClass(string guid, CharacterClassData _class)
        {
            var foundElement = abilitiesByElement.FirstOrDefault(abt => abt.type.elementGUID == guid);
            var foundClass = foundElement.abilitiesOfClassType.FirstOrDefault(abc => abc.assignedClass == _class);
            var randomAbilityIndex = Random.Range(0, foundClass.abilitiesOfElementClass.Count);
            return foundClass.abilitiesOfElementClass[randomAbilityIndex];
        }
        
        public AbilityData GetRandomAbilityByTypeAndClass(string _elementGUID, string _classGUID)
        {
            var foundElement = abilitiesByElement.FirstOrDefault(abt => abt.type.elementGUID == _elementGUID);
            var foundClass = foundElement.abilitiesOfClassType.FirstOrDefault(abc => abc.assignedClass.classGUID == _classGUID);
            var randomAbilityIndex = Random.Range(0, foundClass.abilitiesOfElementClass.Count);
            return foundClass.abilitiesOfElementClass[randomAbilityIndex];
        }
        
        public AbilityData GetRandomAbilityByTypeAndClass(ElementTyping _type, CharacterClassData _class ,AbilityData _excludingAbility)
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