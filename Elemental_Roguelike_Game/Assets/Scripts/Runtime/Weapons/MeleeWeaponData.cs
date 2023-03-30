using Runtime.Damage;
using Runtime.Status;
using UnityEngine;

namespace Runtime.Weapons
{
    public class MeleeWeaponData: WeaponData
    {

        #region Public Fields

        public float meleeRadius;

        public int meleeDamage;

        public bool armorPiercing;
        
        public LayerMask meleeCollisionLayers;

        #endregion
        
        
    }
}