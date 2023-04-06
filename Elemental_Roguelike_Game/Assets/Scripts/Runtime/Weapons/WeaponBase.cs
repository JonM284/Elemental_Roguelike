using System.Collections.Generic;
using Data.Elements;
using UnityEngine;

namespace Runtime.Weapons
{
    public abstract class WeaponBase: MonoBehaviour
    {

        #region Serialized Fields
        
        [SerializeField] private List<Transform> weaponMuzzlePos = new List<Transform>();

        #endregion

        #region Protected Fields

        protected GameObject currentOwner;

        protected Transform m_originTransform;

        protected Transform m_targetTransform;

        protected Vector3 m_targetPosition;

        #endregion

        #region Accessors

        public WeaponData weaponData { get; private set; }

        public ElementTyping weaponElementType { get; private set; }

        public int weaponMuzzleNum => weaponMuzzlePos.Count;

        #endregion

        #region Class Implementation

        public virtual void Initialize(GameObject _ownerObj, Transform _originTransform ,WeaponData _assignedWeaponData, ElementTyping _type)
        {
            currentOwner = _ownerObj;
            m_originTransform = _originTransform;
            weaponData = _assignedWeaponData;
            weaponElementType = _type;
        }

        public abstract void SelectPosition(Vector3 _inputPosition);
        
        public abstract void SelectTarget(Transform _inputTransform);
        
        public virtual void UseWeapon()
        {
            m_targetTransform = null;
        }

        public void CancelWeaponUse()
        {
            
        }

        public Transform GetMuzzleTransform(int _index)
        {
            return weaponMuzzlePos[_index];
        }

        #endregion

    }
}