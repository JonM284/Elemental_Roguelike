using System;
using System.Collections;
using System.Collections.Generic;
using Data.Elements;
using Project.Scripts.Utils;
using Runtime.Environment;
using Runtime.Selection;
using Runtime.Weapons;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Runtime.Character
{
    public class CharacterWeaponManager : MonoBehaviour
    {

        #region Events

        private Action OnWeaponUsed;

        #endregion

        #region Read-Only

        private static readonly int WeaponColorB = Shader.PropertyToID("_WeaponColorB");
        
        private static readonly int WeaponColorG = Shader.PropertyToID("_WeaponColorG");

        #endregion
        
        #region Serialized Fields

        [SerializeField] private Transform weaponHeldPos;

        [SerializeField] private LayerMask weaponCheckLayers;

        #endregion

        #region Private Fields

        private Material m_weaponClonedMat;

        private CharacterRotation m_characterRotation;

        private List<ParticleSystem> weaponMuzzleVFX = new List<ParticleSystem>();

        #endregion

        #region Accessors

        public WeaponBase currentOwnedWeapon { get; private set; }
        
        public bool isUsingWeapon { get; private set; }

        public ElementTyping weaponElementType { get; private set; }

        private CharacterRotation characterRotation => CommonUtils.GetRequiredComponent(ref m_characterRotation, () =>
        {
            var cr = this.GetComponent<CharacterRotation>();
            return cr;
        });

        public Transform handPos => weaponHeldPos;

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
        
        //Get the data from saved Character Data
        public void InitializeCharacterWeapon(WeaponData _ownedWeapon, ElementTyping _type)
        {
            if (_ownedWeapon == null || _type == null)
            {
                return;
            }

            weaponElementType = _type;
            
            StartCoroutine(C_SpawnWeapon(_ownedWeapon));
        }

        private void OnCharacterSelected(CharacterBase _selectedCharacter)
        {
            if (!isUsingWeapon)
            {
                return;
            }
            
            SelectWeaponTarget(_selectedCharacter.transform);
        }

        //Spawn prefab of weapon
        //Assign saved data to weapon
        //Change weapon color
        private IEnumerator C_SpawnWeapon(WeaponData _data)
        {
            //spawn prefab at weapon held pos
            //instantiate gameobject
            var handle = Addressables.LoadAssetAsync<GameObject>(_data.weaponPrefab);
            handle.Completed += operation =>
            {
                if (operation.Status == AsyncOperationStatus.Succeeded)
                {
                    var newWeaponObj = Instantiate(handle.Result, weaponHeldPos);
                    var newWeapon = newWeaponObj.GetComponent<WeaponBase>();
                    if (newWeapon != null)
                    {
                        currentOwnedWeapon = newWeapon;
                        currentOwnedWeapon.Initialize(this.gameObject, currentOwnedWeapon.GetMuzzleTransform(0) ,_data, weaponElementType);
                    }
                }
            };

            yield return new WaitUntil(() => handle.IsDone);

            if (handle.Status == AsyncOperationStatus.Failed)
            {
                Debug.Log("Loading Weapon Failed");
                yield break;
            }
            
            //During this time change the color to what the element should be on the weapon (decided by user)

            var weaponMeshRend = currentOwnedWeapon.gameObject.GetComponentInChildren<MeshRenderer>();
            if (weaponMeshRend == null)
            {
                Debug.LogError($"Could not get Mesh Renderer for {currentOwnedWeapon}", currentOwnedWeapon.gameObject);
                yield break;
            }

            m_weaponClonedMat = new Material(weaponMeshRend.material);
            weaponMeshRend.material = m_weaponClonedMat;
            
            /*
            m_weaponClonedMat.SetColor(WeaponColorG, weaponElementType.weaponColors[0]);
            m_weaponClonedMat.SetColor(WeaponColorB, weaponElementType.weaponColors[1]);

            if (weaponElementType.weaponMuzzleParticles.IsNull())
            {
                Debug.LogError("Weapon has no muzzle vfx attached", currentOwnedWeapon.gameObject);
                yield break;
            }
            
            //Spawn Muzzle VFX
            for (int i = 0; i < currentOwnedWeapon.weaponMuzzleNum; i++)
            {
                var vfxHandle = Addressables.LoadAssetAsync<GameObject>(weaponElementType.weaponMuzzleParticles);
                vfxHandle.Completed += operation =>
                {
                    if (operation.Status == AsyncOperationStatus.Succeeded)
                    {
                        var newVFXObj = Instantiate(vfxHandle.Result, currentOwnedWeapon.GetMuzzleTransform(i));
                        var newWeaponParticles = newVFXObj.GetComponent<ParticleSystem>();
                        if (newWeaponParticles != null)
                        {
                            weaponMuzzleVFX.Add(newWeaponParticles);
                        }
                    }
                };
            
            
                yield return new WaitUntil(() => vfxHandle.IsDone);
            }
            */
            currentOwnedWeapon.AssignWeaponMuzzleFX(weaponMuzzleVFX);
            
        }

        public void SetupWeaponAction(bool _isUsingWeapon, Action abilityUseCallback = null)
        {
            if (currentOwnedWeapon == null)
            {
                Debug.Log($"No Weapon assigned to {this.gameObject.name}", this.gameObject);
                return;
            }

            isUsingWeapon = _isUsingWeapon;

            if (abilityUseCallback != null)
            {
                OnWeaponUsed = abilityUseCallback;
            }
        }


        public void SelectWeaponTarget(Transform _targetTransform)
        {
            if (currentOwnedWeapon == null)
            {
                Debug.Log("No Current Owned Weapon");
                return;
            }

            if (currentOwnedWeapon.weaponData.targetType　== WeaponTargetType.LOCATION)
            {
                Debug.Log($"<color=red>Target Type:{currentOwnedWeapon.weaponData.targetType}</color>");
                CancelWeaponUse();
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
            
        }
        
        public void SelectWeaponTarget(Vector3 _targetPos)
        {
            if (currentOwnedWeapon == null)
            {
                Debug.Log("No Current Owned Weapon");
                return;
            }

            if (!isUsingWeapon)
            {
                return;
            }

            if (currentOwnedWeapon.weaponData.targetType　== WeaponTargetType.CHARACTER_TRANSFORM)
            {
                Debug.Log($"<color=red>Target Type:{currentOwnedWeapon.weaponData.targetType}</color>");
                CancelWeaponUse();
                return;
            }
            
            currentOwnedWeapon.SelectPosition(_targetPos);
            characterRotation.SetRotationTarget(_targetPos);
            OnWeaponUsed?.Invoke();
        }

        public void UseWeapon()
        {
            currentOwnedWeapon.UseWeapon();
            isUsingWeapon = false;
        }
        
        public void CancelWeaponUse()
        {
            if (currentOwnedWeapon == null)
            {
                return;
            }
            
            currentOwnedWeapon.CancelWeaponUse();
            OnWeaponUsed = null;
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