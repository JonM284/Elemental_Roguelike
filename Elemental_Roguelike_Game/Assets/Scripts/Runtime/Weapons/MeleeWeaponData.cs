using Runtime.Damage;
using Runtime.Status;
using UnityEngine;

namespace Runtime.Weapons
{
    [CreateAssetMenu(menuName = "Custom Data/Weapon/Melee Weapon")]
    public class MeleeWeaponData: WeaponData
    {

        #region Public Fields

        public float meleeRadius;

        public int meleeDamage;

        public bool hasKnockback;

        public bool armorPiercing;
        
        public LayerMask meleeCollisionLayers;

        #endregion
        
        
    }
}