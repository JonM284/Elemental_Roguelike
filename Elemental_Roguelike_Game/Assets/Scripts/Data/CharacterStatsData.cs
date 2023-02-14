﻿using System.Collections.Generic;
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
        
        //Max health of character
        public int baseHealth;

        //Max shield of character
        public int baseShields;
        
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

        public ElementTyping type;

        public List<Ability> abilityReferences;

        //Change later to actual weapon
        public int weapon;


        #endregion

        #region Constructor

        /// <summary>
        /// Add Element type when creating this, otherwise they will have no element
        /// Add Abilities when creating this, otherwise they will have no abilities
        /// </summary>
        public CharacterStatsData()
        {
            this.baseSpeed = 10;
            this.baseHealth = 10;
            this.baseShields = 10;
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
            this.abilityReferences = new List<Ability>(2);
            //Abilities must also be added when creating a new meeple
            this.weapon = 0;
        }

        #endregion
    }
}