using Data.Elements;
using Runtime.Status;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Data
{
    [CreateAssetMenu(menuName = "Custom Data/Projectile")]
    public class ProjectileInfo: ScriptableObject
    {

        #region Public Fields
        
        [Tooltip("Positive Num = Damage, Negative Num = HEAL")]
        public int projectileDamage = 1;

        public float projectileDamageRadius = 0.5f;

        public float projectileSpeed = 5f;
        
        public bool isAffectArmor;

        public bool isKnockBack;

        public bool isAffectWhileMoving;

        public ElementTyping projectileType;

        [Tooltip("Chance to apply status when ability hits")]
        [Range(0,100)]
        public int chanceToApplyStatus;
        
        public Status statusEffect;

        public LayerMask projectileCollisionLayers;

        public AssetReference projectilePrefab;
        
        public AnimationCurve projectileArcCurve = AnimationCurve.Constant(0, 1, 0);

        #endregion
        

    }
}