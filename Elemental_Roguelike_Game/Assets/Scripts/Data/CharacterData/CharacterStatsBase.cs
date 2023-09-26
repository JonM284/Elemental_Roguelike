using System.Collections.Generic;
using Data.Elements;
using Runtime.Abilities;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Data.CharacterData
{
    [CreateAssetMenu(fileName = "CharacterData", menuName = "Custom Data/CharacterData")]
    public class CharacterStatsBase : ScriptableObject
    {

        #region Public Fields

        [Header("Always used stats")] 
        public bool isCaptain;

        [Tooltip("Speed character will normally move at")]
        public float baseSpeed = 1;

        [Header("In Battle Stats")] 
        [Tooltip("Max health of character")]
        public int baseHealth = 10;
        
        [Tooltip("Max shield of character")]
        public int baseShields = 0;
        
        [Tooltip("Distance from start position, character is able to move")]
        public float movementDistance = 1f;

        [Tooltip("Damage on regular tackle")] 
        public int tackleDamageAmount = 15;
        
        [Header("Stats")]
        [Tooltip("tackle score and damage")]
        [Range(1,100)]
        public int tackleScore = 1;
        [Tooltip("movement and dodge tackle reaction")]
        [Range(1,100)]
        public int agilityScore = 1;
        [Tooltip("throw ball for shot distance")]
        [Range(1,100)]
        public int shootingScore = 1;
        [Tooltip("throw ball for pass distance")]
        [Range(1,100)]
        public int passingScore = 1;

        [Tooltip("Character associated color")]
        public Color characterColor = Color.white;

        [Tooltip("Character Element type")] 
        public ElementTyping typing;

        public List<Ability> abilities;

        public CharacterClassData classTyping;

        public AssetReference characterAssetRef;

        public string characterGUID;
        
        #endregion
        
        
        #region Class Implementation

        [ContextMenu("Generate GUID")]
        private void GenerateID()
        {
            if (characterGUID != string.Empty)
            {
                return;
            }
            
            characterGUID = System.Guid.NewGuid().ToString();
        }
        
        #endregion


    }   
}
