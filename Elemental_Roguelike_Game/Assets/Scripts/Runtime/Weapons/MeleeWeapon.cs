using Runtime.Damage;
using UnityEngine;

namespace Runtime.Weapons
{
    public class MeleeWeapon: WeaponBase
    {

        #region Accessors

        private MeleeWeaponData meleeWeaponData => weaponData as MeleeWeaponData;

        #endregion
        
        
        public override void SelectTarget(Transform _inputTransform)
        {
            if (_inputTransform == null)
            {
                return; 
            }
            
            m_targetTransform = _inputTransform;
            //ToDO: change the placement of this method to be during the swing animation
            UseWeapon();
        }


        public override void UseWeapon()
        {
            if (currentOwner == null)
            {
                return;
            }
            
            var m_endPos = m_targetTransform != null ? m_targetTransform.position : m_targetPosition;
            
            Collider[] colliders = Physics.OverlapSphere(m_endPos, meleeWeaponData.meleeRadius, meleeWeaponData.meleeCollisionLayers);

            if (colliders.Length > 0)
            {
                foreach (var collider in colliders)
                {
                    var damageable = collider.GetComponent<IDamageable>();
                    damageable?.OnDealDamage(meleeWeaponData.meleeDamage, meleeWeaponData.armorPiercing, meleeWeaponData.type);
                }
            }

            base.UseWeapon();
        }
    }
}