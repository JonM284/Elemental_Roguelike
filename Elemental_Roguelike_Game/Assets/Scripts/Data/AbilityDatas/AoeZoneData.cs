using System;
using Runtime.Status;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

namespace Data.AbilityDatas
{
    [CreateAssetMenu(menuName = "Maulball/AOE/Zone Data")]
    [Obsolete("Use AoeZoneAbilityInstead")]
    public class AoeZoneData: ScriptableObject
    {
        public int roundStayAmount = 1;
        
        public int zoneDamage = 1;

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
        
        [FormerlySerializedAs("statusBaseEffect")] public StatusEntityBase statusEntityBaseEffect;
        
        public AssetReference zonePrefab;
    }
}