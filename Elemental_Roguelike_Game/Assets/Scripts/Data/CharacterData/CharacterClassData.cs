using System;
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

        [Header("Passive Base")]
        public float overwatchRadius = 1f;
        public float damageReduction = 1f;
        public float knockbackReduction = 1f;

        [Header("Ball Influence Stats")] 
        [Range(1, 10)] [SerializeField] private int BallThrowVelocityChange = 10;
        
        
        [Space(20)]
        [Header("Class Specific")]
        public Color passiveColor;

        public Color darkColor;

        public Color barColor;

        [SerializeField] private string overwatchDescription;

        public string classGUID;

        public Material characterClassGroundMarker;

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

        public int GetBallVelocityChangeAmount()
        {
            return BallThrowVelocityChange;
        }

        public string GetOverwatchDescription()
        {
            return overwatchDescription;
        }

        #endregion
        
    }
}