using Data.Elements;
using Runtime.Status;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Data
{
    [CreateAssetMenu(menuName = "Custom Data/Zone")]
    public class ZoneInfo: ScriptableObject
    {
        public int roundStayAmount = 1;

        public float zoneStaySeconds = 5f;
        
        public int zoneDamage = 1;

        public float zoneRadius = 0.5f;
        
        public bool isArmorAffecting;

        [Tooltip("Do other damageables get knocked back?")]
        public bool hasKnockback;

        [Tooltip("Should the user of this ability be ignored during the damage/ heal/ status check?")]
        public bool isIgnoreUser;
        
        public LayerMask zoneCheckLayer;

        public ElementTyping elementType;

        public Status statusEffect;
        
        public AssetReference zonePrefab;
    }
}