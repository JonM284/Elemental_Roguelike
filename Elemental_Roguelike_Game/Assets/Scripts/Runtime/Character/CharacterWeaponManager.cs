using System;
using Project.Scripts.Utils;
using Runtime.Environment;
using Runtime.Weapons;
using UnityEngine;

namespace Runtime.Character
{
    public class CharacterWeaponManager : MonoBehaviour
    {

        #region Events

        private Action OnWeaponUsed;

        #endregion
        
        #region Serialized Fields

        [SerializeField] private Transform weaponHeldPos;

        [SerializeField] private LayerMask weaponCheckLayers;

        #endregion

        #region Private Fields

        private CharacterRotation m_characterRotation;

        #endregion

        #region Accessors

        public WeaponBase currentOwnedWeapon { get; private set; }

        public bool isUsingWeapon { get; private set; }
        
        private CharacterRotation characterRotation => CommonUtils.GetRequiredComponent(ref m_characterRotation, () =>
        {
            var cr = this.GetComponent<CharacterRotation>();
            return cr;
        });

        #endregion
        
        #region Unity Events

        private void OnEnable()
        {
            CharacterBase.CharacterSelected += OnCharacterSelected;
        }

        private void OnDisable()
        {
            CharacterBase.CharacterSelected -= OnCharacterSelected;
        }

        #endregion


        #region Class Implementation
        
        public void InitializeCharacterWeapon(WeaponBase _ownedWeapon)
        {
            currentOwnedWeapon = _ownedWeapon;
            SpawnWeapon();
        }

        private void OnCharacterSelected(CharacterBase _selectedCharacter)
        {
            if (!isUsingWeapon)
            {
                return;
            }
            
            SelectWeaponTarget(_selectedCharacter.transform);
        }

        private void SpawnWeapon()
        {
            //spawn prefab at weapon held pos
        }
        
        public void UseWeapon(Action abilityUseCallback = null)
        {
            if (currentOwnedWeapon == null)
            {
                Debug.Log($"No Weapon assigned to {this.gameObject.name}", this.gameObject);
                return;
            }

            isUsingWeapon = true;
            
            if (abilityUseCallback != null)
            {
                OnWeaponUsed = abilityUseCallback;
            }
        }
        
        
        public void SelectWeaponTarget(Transform _targetTransform)
        {
            if (currentOwnedWeapon == null)
            {
                return;
            }

            var chanceToHit = CheckAccuracy(_targetTransform.position);

            if (chanceToHit == 0)
            {
                //hit cover instead of character
            }
            
            currentOwnedWeapon.SelectTarget(_targetTransform);
            characterRotation.SetRotationTarget(_targetTransform.position);
            OnWeaponUsed?.Invoke();
            isUsingWeapon = false;
        }
        
        
        private float CheckAccuracy(Vector3 _checkPos)
        {
            var dir = transform.position - _checkPos;
            var dirMagnitude = dir.magnitude;
            var dirNormalized = dir.normalized;
            Debug.DrawRay(_checkPos, dirNormalized, Color.red, 10f);
            if (Physics.Raycast(_checkPos, dirNormalized, out RaycastHit hit, dirMagnitude, weaponCheckLayers))
            {
                var _obstacle = hit.transform.GetComponent<CoverObstacles>();
                if (_obstacle != null && _obstacle.type == ObstacleType.FULL)
                {
                    return 0;
                }
            }

            return 1;
        }

        #endregion
        
    }
}