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

        [Header("Stats")]
        [Tooltip("tackle score and damage")]
        public int tackleScore = 1;
        public int agilityScore = 1;
        public int shootingScore = 1;

        [Tooltip("Character associated color")]
        public Color characterColor = Color.white;

        [Tooltip("Character Element type")] 
        public ElementTyping typing;

        public List<Ability> abilities;

        public CharacterClassData classTyping;

        public AssetReference characterAssetRef;


        #endregion


    }   
}