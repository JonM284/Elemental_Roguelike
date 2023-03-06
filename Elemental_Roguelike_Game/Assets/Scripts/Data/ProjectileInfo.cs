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

        public int projectileDamage = 1;

        public float projectileDamageRadius = 0.5f;

        public float projectileSpeed = 1f;

        public float projectileLifetime = 1f;

        public bool isArmorPiercing;

        public ElementTyping projectileType;

        public Status statusEffect;

        public LayerMask projectileCollisionLayers;

        public AssetReference projectileAsset;

        public AnimationCurve projectileArcCurve = AnimationCurve.Constant(0, 1, 0);

        #endregion
        

    }
}