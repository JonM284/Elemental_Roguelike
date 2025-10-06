using Runtime.Abilities;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Data.AbilityDatas
{
    [CreateAssetMenu(menuName = "Maulball/Ability/Projectile Ability Data")]
    public class ProjectileAbilityData: AbilityData
    {
        [Header("Projectile Specific")] 
        public float projectileLifetime = 1f;
        public float passiveRadius = 1;

        [Header("On Projectile End")] 
        public ProjectileEndType projectileEndType;
        
        [Header("Booleans")]
        //ToDo: change this to enum flags
        public bool isPassThroughObjects, isAffectArmor, isAffectWhileMoving, isStopReaction;
        
        [Header("Multi-shot options")]
        [Tooltip("Amount of projectiles to be fired")]
        public int projectileAmount = 1;
        public float timeBetweenShots = 0.25f;

        public AnimationCurve animationCurve;

        public LayerMask projectileCollisionLayers;
        
        [Space]
        public AssetReference projectilePrefab;

        [Header("End Creatable")] 
        public GameObject endCreatable;

    }
}