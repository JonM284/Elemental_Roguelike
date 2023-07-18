using Runtime.Character;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(menuName = "Custom Data/Character Class Data")]
    public class CharacterClassData : ScriptableObject
    {

        [Header("Enum")] public CharacterClass classType;

        [Header("Passive Base")] public float radius;

        [Header("Agility Stats")]
        [Range(1, 100)] public int AgilityStatMin = 1;
        [Range(1, 100)] public int AgilityStatMax = 100;

        [Header("Throwing Stats")]
        [Range(1, 100)] public int ShootingStatMin = 1;
        [Range(1, 100)] public int ShootingStatMax = 100;

        [Header("Damage Stats")]
        [Range(1, 100)] public int DamageStatMin = 1;
        [Range(1, 100)] public int DamageStatMax = 100;
        
        [Space(20)]
        [Header("Class Specific")]
        public Color passiveColor;

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