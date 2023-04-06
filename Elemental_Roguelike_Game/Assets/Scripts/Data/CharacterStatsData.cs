using System.Collections.Generic;
using Data.Elements;
using Runtime.Abilities;
using UnityEngine;
using Utils;

namespace Data
{
    [System.Serializable]
    public class CharacterStatsData
    {
        #region Public Fields

        public string id;
        
        //Speed character will normally move at
        public float baseSpeed;

        //Higher initiative goes first
        public int initiativeNumber;
        
        //Max health of character
        public int baseHealth;

        public int currentHealth;

        //Max shield of character
        public int baseShields;

        public int currentShield;

        //Distance from start position, character is able to move
        public float movementDistance;
    
        //Damage character does? NOTE: MAY BE CHANGED ****
        public float baseDamage;
        
        //Cosmetic items
        public int cosmeticHat;

        public int cosmeticHead;
        
        public int cosmeticBody;
        
        public int cosmeticHands;
        
        public int cosmeticLegs;
        
        public int cosmeticFeet;


        //Character associated color
        public Color characterColor;

        public string meepleElementTypeRef;

        public List<string> abilityReferences;

        public string weaponReference;

        public string weaponElementTypeRef;


        #endregion

        #region Constructor

        /// <summary>
        /// Add Element type when creating this, otherwise they will have no element
        /// Add Abilities when creating this, otherwise they will have no abilities
        /// </summary>
        public CharacterStatsData()
        {
            this.baseSpeed = 10;
            this.initiativeNumber = 1;
            this.baseHealth = 10;
            this.currentHealth = this.baseHealth;
            this.baseShields = 10;
            this.currentShield = this.baseShields;
            this.movementDistance = 5f;
            this.baseDamage = 1;
            this.cosmeticHat = 0;
            this.cosmeticHead = 0;
            this.cosmeticBody = 0;
            this.cosmeticHands = 0;
            this.cosmeticLegs = 0;
            this.cosmeticFeet = 0;
            this.characterColor = Color.white;
            //Type must be added whenever creating a new meeple
            this.abilityReferences = new List<string>();
            //Abilities must also be added when creating a new meeple
            this.weaponReference = "";
            this.weaponElementTypeRef = "";
            //Weapon must be added when creating new meeple
            //Weapon type will default to normal
            //Add default weapon, hands?
        }

        #endregion
    }
}