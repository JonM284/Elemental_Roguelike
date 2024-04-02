﻿using System;
using System.Collections.Generic;
using Runtime.Character;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Data.CharacterData
{

    #region Nested Classes

    [Serializable]
    public class IconByColorblindOption
    {
        public ColorblindOptions _option;
        public List<Sprite> _icons = new List<Sprite>();
    }

    #endregion
    
    [CreateAssetMenu(menuName = "Custom Data/Character Class Data")]
    public class CharacterClassData : ScriptableObject
    {

        [Header("Enum")] public CharacterClass classType;

        [Header("Passive Base")] public float radius;

        [Header("Stats for Roguelike")] 
        [Range(30, 100)] [SerializeField] private int HealthMin = 30;
        [Range(30, 200)] [SerializeField] private int HealthMax = 100;
        
        [Range(30, 100)] [SerializeField] private int ShieldMin = 30;
        [Range(30, 200)] [SerializeField] private int ShieldMax = 100;

        [Header("Agility Stats")]
        [Range(0, 10)] [SerializeField] private int AgilityStatMin = 1;
        [Range(0, 10)] [SerializeField] private int AgilityStatMax = 10;

        [Header("Shooting Stats")]
        [Range(0, 10)] [SerializeField] private int ShootingStatMin = 1;
        [Range(0, 10)] [SerializeField] private int ShootingStatMax = 10;
        
        [Header("Passing Stats")]
        [Range(0, 10)] [SerializeField] private int PassingStatMin = 1;
        [Range(0, 10)] [SerializeField] private int PassingStatMax = 10;

        [Header("Damage Stats")]
        [Range(0, 10)] [SerializeField] private int DamageStatMin = 1;
        [Range(0, 10)] [SerializeField] private int DamageStatMax = 10;
        
        [Space(20)]
        [Header("Class Specific")]
        public Color passiveColor;

        public Color darkColor;

        public Color barColor;

        [SerializeField] private float moveDistance;

        [SerializeField] private int tackleDamageAmount;

        public string classGUID;

        public List<IconByColorblindOption> _iconsByOption = new List<IconByColorblindOption>();
        
        #region Class Implementation

        [ContextMenu("Generate GUID")]
        private void GenerateID()
        {
            if (classGUID != string.Empty)
            {
                return;
            }
            
            classGUID = System.Guid.NewGuid().ToString();
        }

        public float GetMoveDistance()
        {
            return moveDistance;
        }

        public int GetTackleDamage()
        {
            return tackleDamageAmount;
        }

        public int GetRandomHealth()
        {
            return Random.Range(HealthMin, HealthMax);
        }

        public int GetRandomShield()
        {
            return Random.Range(ShieldMin, ShieldMax);
        }

        public int GetRandomAgilityScore()
        {
            return Random.Range(AgilityStatMin, AgilityStatMax);
        }

        public int GetRandomShootingScore()
        {
            return Random.Range(ShootingStatMin, ShootingStatMax);
        }
        
        public int GetRandomPassingScore()
        {
            return Random.Range(PassingStatMin, PassingStatMax);
        }
        
        public int GetRandomDamageScore()
        {
            return Random.Range(DamageStatMin, DamageStatMax);
        }

        #endregion
        
    }
}