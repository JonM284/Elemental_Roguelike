using UnityEngine;
using Utils;

namespace Runtime.Weapons
{
    public class ProjectileWeapon: WeaponBase
    {
        #region Accessors

        private ProjectileWeaponData projectileWeaponData => weaponData as ProjectileWeaponData;

        #endregion
        
        #region Class Implementation

        public override void SelectTarget(Transform _inputTransform)
        {
            if (_inputTransform == null)
            {
                return; 
            }
            
            m_targetTransform = _inputTransform;
            //ToDO: change the placement of this method to be during the shoot animation
            UseWeapon();
        }

        public override void UseWeapon()
        {
            if (currentOwner == null)
            {
                return;
            }

            var m_endPos = m_targetTransform != null ? m_targetTransform.position : m_targetPosition;
            ProjectileUtils.PlayAt(projectileWeaponData.projectilePrefab, currentOwner.transform.position, currentOwner.transform.forward, m_endPos);
            base.UseWeapon();
        }

        #endregion
    }
}