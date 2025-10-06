using System;
using Data.Elements;
using Runtime.Status;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

namespace Data
{
    [CreateAssetMenu(menuName = "Custom Data/Projectile")]
    [Obsolete]
    public class ProjectileInfo: ScriptableObject
    {

        #region Public Fields
        
        [Tooltip("Positive Num = Damage, Negative Num = HEAL")]
        public int projectileDamage = 1;

        public float projectileDamageRadius = 0.5f;

        public float projectileSpeed = 5f;
        
        public bool isAffectArmor;

        public bool isKnockBack;

        public bool isRandomKnockBallAway;

        public bool isStopReaction;

        public bool isAffectWhileMoving;

        public ElementTyping projectileType;

        [FormerlySerializedAs("statusBaseEffect")] [FormerlySerializedAs("statusEffect")] public StatusEntityBase statusEntityBaseEffect;

        public LayerMask projectileCollisionLayers;

        public AssetReference projectilePrefab;
        
        public AnimationCurve projectileArcCurve = AnimationCurve.Constant(0, 1, 0);

        #endregion
        

    }
}