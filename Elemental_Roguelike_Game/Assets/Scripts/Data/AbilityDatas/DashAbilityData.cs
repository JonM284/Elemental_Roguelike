using Runtime.Abilities;
using Runtime.Gameplay;
using Runtime.Weapons;
using UnityEngine;

namespace Data.AbilityDatas
{
    [CreateAssetMenu(menuName = "Maulball/Ability/Dash Ability Data")]
    public class DashAbilityData: AbilityData
    {
        [Header("Dash Specific")] 
        public AnimationCurve jumpCurve;
        public float dashSpeed = 1;
        public ProjectileBase projectilePrefab;
    }
}