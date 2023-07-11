using Project.Scripts.Utils;
using Runtime.Damage;
using UnityEngine;

namespace Runtime.Weapons
{
    public class MeleeWeapon: WeaponBase
    {

        #region Accessors

        private MeleeWeaponData meleeWeaponData => weaponData as MeleeWeaponData;

        #endregion
        
        public override void SelectPosition(Vector3 _inputPosition)
        {
            if (_inputPosition.IsNan())
            {
                return;
            }

            m_targetPosition = _inputPosition;
        }
        
        
        public override void SelectTarget(Transform _inputTransform)
        {
            if (_inputTransform == null)
            {
                return; 
            }
            
            m_targetTransform = _inputTransform;
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
                    damageable?.OnDealDamage(this.transform, meleeWeaponData.meleeDamage, meleeWeaponData.armorPiercing, weaponElementType, meleeWeaponData.hasKnockback);
                }
            }

            base.UseWeapon();
        }
    }
}