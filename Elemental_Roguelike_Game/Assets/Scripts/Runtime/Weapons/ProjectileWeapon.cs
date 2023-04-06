using System.Linq;
using Data;
using Data.Elements;
using Project.Scripts.Utils;
using UnityEngine;
using Utils;

namespace Runtime.Weapons
{
    public class ProjectileWeapon: WeaponBase
    {
        #region Serialized Fields

        [SerializeField] private AudioSource audioSource;

        #endregion

        #region Private Fields

        private ProjectileInfo projectileInfoByElement;

        #endregion
        
        #region Accessors

        private ProjectileWeaponData projectileWeaponData => weaponData as ProjectileWeaponData;
        
        #endregion
        
        #region Class Implementation

        public override void Initialize(GameObject _ownerObj, Transform _originTransform, WeaponData _assignedWeaponData, ElementTyping _type)
        {
            base.Initialize(_ownerObj, _originTransform, _assignedWeaponData, _type);
            projectileInfoByElement =
                projectileWeaponData.allPossibleProjectiles.FirstOrDefault(pi =>
                    pi.projectileType == weaponElementType);

            if (weaponData.weaponAudio.Count == 0)
            {
                return;
            }
            
            if (!weaponData.hasMultipleWeaponAudios)
            {
                audioSource.clip = weaponData.weaponAudio[0];
            }
        }

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

            PlayWeaponAudio();
            
            var m_endPos = m_targetTransform != null ? m_targetTransform.position : m_targetPosition;
            var dir = m_endPos.FlattenVector3Y() - m_originTransform.position.FlattenVector3Y();
            projectileInfoByElement.PlayAt(m_originTransform.transform.position, dir, m_endPos);
            base.UseWeapon();
        }

        private void PlayWeaponAudio()
        {
            if (weaponData.weaponAudio.Count == 0)
            {
                return;
            }
            
            //If there are more than one possible sound for the gun, play a random sound
            //otherwise skip this step
            if (weaponData.hasMultipleWeaponAudios)
            {
                audioSource.clip = weaponData.weaponAudio[Random.Range(0, weaponData.weaponAudio.Count - 1)];
            }
        }

        #endregion
    }
}