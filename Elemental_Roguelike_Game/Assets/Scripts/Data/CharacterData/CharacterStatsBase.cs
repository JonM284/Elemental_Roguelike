using System.Collections.Generic;
using Data.Elements;
using Runtime.Abilities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace Data.CharacterData
{
    [CreateAssetMenu(fileName = "CharacterData", menuName = "Custom Data/CharacterData")]
    public class CharacterStatsBase : ScriptableObject
    {

        #region Public Fields

        [Header("Always used stats")] 
        public bool isCaptain;

        public string characterName;

        public float healthBarOffset = 1f;

        [Tooltip("Speed character will normally move at")]
        public float baseSpeed = 1;

        [Header("In Battle Stats")] 
        [Tooltip("Max health of character")]
        public int baseHealth = 10;
        
        [Tooltip("Max shield of character")]
        public int baseShields = 0;

        [Tooltip("Damage on regular tackle")] 
        public int tackleDamageAmount = 15;
        
        [Header("Stats")]
        [Tooltip("tackle score and damage")]
        [Range(0,100)] public int tackleScore = 30;
        [Tooltip("movement and dodge tackle reaction")]
        [Range(0,100)] public int agilityScore = 30;
        [Tooltip("throw ball")]
        [Range(0, 100)] public int throwScore = 30;
        [Tooltip("passive influence range")]
        [Range(0, 100)] public int influenceRange = 30;
        [Tooltip("gravity pull")]
        [Range(0, 100)] public int gravityScore = 30;
        
        [Tooltip("Character associated color")]
        public Color characterColor = Color.white;
        
        public Color characterColorDark = Color.white;

        [Tooltip("Character Element type")] 
        public ElementTyping typing;

        public List<Ability> abilities;

        public CharacterClassData classTyping;
        
        public GameObject characterModelAssetRef;

        public string characterGUID;

        public Sprite characterImage;
        
        #endregion

        #region Private Read-Only

        private readonly int maxDamage = 100;

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

        public int GetTackleDamage()
        {
            return Mathf.CeilToInt(maxDamage * (tackleScore / 100f));
        }
        
        #endregion


    }   
}
