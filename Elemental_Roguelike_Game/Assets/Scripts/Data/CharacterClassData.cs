using UnityEditor;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(menuName = "Custom Data/Character Class Data")]
    public class CharacterClassData : ScriptableObject
    {
        [Header("Agility Stats")]
        [Range(1, 100)] public int AgilityStatMin = 1;
        [Range(1, 100)] public int AgilityStatMax = 100;

        [Header("Shooting Stats")]
        [Range(1, 100)] public int ShootingStatMin = 1;
        [Range(1, 100)] public int ShootingStatMax = 100;
        
        [Header("Passing Stats")]
        [Range(1, 100)] public int PassingStatMin = 1;
        [Range(1, 100)] public int PassingStatMax = 100;
        
        [Header("Damage Stats")]
        [Range(1, 100)] public int DamageStatMin = 1;
        [Range(1, 100)] public int DamageStatMax = 100;

    }
}