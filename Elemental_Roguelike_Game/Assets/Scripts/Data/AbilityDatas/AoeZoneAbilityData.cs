using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Data.AbilityDatas
{
    [CreateAssetMenu(menuName = "Maulball/Ability/Zone Ability Data")]
    public class AoeZoneAbilityData: AbilityData
    {
        [Header("Zone Specific")] 
        public AreaOfEffectType aoeType;
        
        public int roundStayAmount = 1;
        
        public int zoneDamage = 0;

        public float zoneRadius = 0.5f;
        
        public bool isArmorAffecting;

        [Tooltip("Do other damageables get knocked back?")]
        public bool hasKnockback;

        [Tooltip("Will this ability knock the ball away from the holder?")]
        public bool isRandomKnockawayBall;

        [Tooltip("Will this zone stop character reactions")]
        public bool isStopReaction;

        [Tooltip("Should the user of this ability be ignored during the damage/ heal/ status check?")]
        public bool isIgnoreUser;
        
        public LayerMask zoneCheckLayer;
        
        public GameObject zonePrefab;

        [Header("Traveling Projectile")]
        
        public GameObject displayedProjectile;
        public float projectileTravelTime = 1f;
        public AnimationCurve projectileYCurve;

    }
}