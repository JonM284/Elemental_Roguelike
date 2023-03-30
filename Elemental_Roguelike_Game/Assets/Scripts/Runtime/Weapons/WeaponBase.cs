using UnityEngine;

namespace Runtime.Weapons
{
    public abstract class WeaponBase: MonoBehaviour
    {

        #region Serialized Fields

        [SerializeField] private WeaponData m_weaponData;

        #endregion

        #region Protected Fields

        protected GameObject currentOwner;

        protected Transform m_targetTransform;

        protected Vector3 m_targetPosition;

        #endregion

        #region Accessors

        public WeaponData weaponData => m_weaponData;

        #endregion

        #region Class Implementation

        public void Initialize(GameObject _ownerObj)
        {
            currentOwner = _ownerObj;
        }
        
        public abstract void SelectTarget(Transform _inputTransform);
        
        public virtual void UseWeapon()
        {
            currentOwner = null;
            m_targetTransform = null;
        }
        
        public void CancelAbilityUse()
        {
            currentOwner = null;
        }

        #endregion

    }
}