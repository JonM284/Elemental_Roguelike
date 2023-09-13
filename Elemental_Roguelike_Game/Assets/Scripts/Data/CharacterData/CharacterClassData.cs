using Runtime.Character;
using UnityEngine;

namespace Data.CharacterData
{
    [CreateAssetMenu(menuName = "Custom Data/Character Class Data")]
    public class CharacterClassData : ScriptableObject
    {

        [Header("Enum")] public CharacterClass classType;

        [Header("Passive Base")] public float radius;

        [Header("Vitality Stats")] 
        [Range(1, 100)] [SerializeField] private int HealthMin = 1;
        [Range(1, 200)] [SerializeField] private int HealthMax = 100;
        
        [Range(1, 100)] [SerializeField] private int ShieldMin = 1;
        [Range(1, 200)] [SerializeField] private int ShieldMax = 100;

        [Header("Agility Stats")]
        [Range(1, 100)] [SerializeField] private int AgilityStatMin = 1;
        [Range(1, 100)] [SerializeField] private int AgilityStatMax = 100;

        [Header("Throwing Stats")]
        [Range(1, 100)] [SerializeField] private int ShootingStatMin = 1;
        [Range(1, 100)] [SerializeField] private int ShootingStatMax = 100;

        [Header("Damage Stats")]
        [Range(1, 100)] [SerializeField] private int DamageStatMin = 1;
        [Range(1, 100)] [SerializeField] private int DamageStatMax = 100;
        
        [Space(20)]
        [Header("Class Specific")]
        public Color passiveColor;

        [SerializeField] private float moveDistance;

        public string classGUID;

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
        
        public int GetRandomDamageScore()
        {
            return Random.Range(DamageStatMin, DamageStatMax);
        }

        #endregion
        
    }
}