using System.Collections.Generic;
using Data.Elements;
using Runtime.Abilities;
using Runtime.Weapons;
using UnityEngine;
using UnityEngine.AddressableAssets;


namespace Project.Scripts.Data
{
    [CreateAssetMenu(fileName = "CharacterData", menuName = "Custom Data/CharacterData")]
    public class CharacterStatsBase : ScriptableObject
    {

        #region Public Fields

        [Header("Always used stats")] 
        public int initiativeNumber = 0;

        [Tooltip("Speed character will normally move at")]
        public float baseSpeed = 1;

        [Header("In Battle Stats")] 
        [Tooltip("Max health of character")]
        public int baseHealth = 10;
        
        [Tooltip("Max shield of character")]
        public int baseShields = 0;
        
        [Tooltip("Distance from start position, character is able to move")]
        public float movementDistance = 1f;

        [Header("To be decided")]
        [Tooltip("Damage character does? NOTE: MAY BE CHANGED")]
        public int baseDamage = 1;

        [Tooltip("Character associated color")]
        public Color characterColor = Color.white;

        [Tooltip("Character Element type")] 
        public ElementTyping typing;

        public List<Ability> abilities;

        public AssetReference characterVisualReference;

        public WeaponData weaponData;
        
        public ElementTyping weaponTyping;


        #endregion


    }   
}
