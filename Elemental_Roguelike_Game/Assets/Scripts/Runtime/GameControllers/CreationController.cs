using System.Collections.Generic;
using System.Linq;
using Data.Sides;
using Project.Scripts.Utils;
using Runtime.Character.Creations;
using Runtime.Character.Creations.CreationDatas;
using Runtime.Weapons;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Runtime.GameControllers
{
    public class CreationController: GameControllerBase
    {

        #region Statics

        public static CreationController Instance { get; private set; }

        #endregion
        
        #region Serialized Fields

        
        
        #endregion

        #region Private Fields
        
        private List<CreationBase> m_cachedCreations = new List<CreationBase>();

        private Transform m_disabledCreationPool;

        #endregion

        #region Accessors

        public Transform disabledCreationPool => CommonUtils.GetRequiredComponent(ref m_disabledCreationPool, () =>
        {
            var t = TransformUtils.CreatePool(transform, false);
            t.RenameTransform("Disabled Creation Pool");
            return t;
        });

        #endregion

        #region GameControllerBase Inherited Methods

        public override void Initialize()
        {
            Instance = this;
            base.Initialize();
        }
        
        public override void Cleanup()
        {
            m_cachedCreations.ForEach(c => Destroy(c.gameObject));
            m_cachedCreations.Clear();
            
            for (var n = 0; n < disabledCreationPool.childCount; ++n)
            {
                Transform temp = disabledCreationPool.GetChild(n);
                GameObject.Destroy(temp.gameObject);
            }
            base.Cleanup();
        }

        #endregion

        #region Class Implementation

        public void ReturnToPool(CreationBase _creation)
        {
            if (_creation.IsNull())
            {
                return;
            }

            m_cachedCreations.Add(_creation);
            _creation.transform.ResetTransform(disabledCreationPool);
        }
        
        
        public void GetCreationAt(CreationData _creationInfo ,Vector3 startPos, Vector3 startRotation, Transform _user, CharacterSide _side)
        {
            if (_creationInfo == null)
            {
                Debug.LogError("Creation Info Null");
                return;
            }
            
            var foundCreation = m_cachedCreations.FirstOrDefault(c => c.creationData == _creationInfo);

            //projectile not found in cachedProjectiles
            if (foundCreation == null)
            {
                //instantiate gameobject
                var handle = Addressables.LoadAssetAsync<GameObject>(_creationInfo.creationRef);
                handle.Completed += operation =>
                {
                    if (operation.Status == AsyncOperationStatus.Succeeded)
                    {
                        var _newCreationObj = Instantiate(handle.Result, startPos, Quaternion.Euler(startRotation));
                        _newCreationObj.TryGetComponent(out CreationBase _creation);
                        if (!_creation.IsNull())
                        {
                            foundCreation = _creation;
                            _creation.Initialize(_creationInfo, _user, _side);
                        }
                    }
                };
                return;
            }

            if (m_cachedCreations.Contains(foundCreation))
            {
                m_cachedCreations.Remove(foundCreation);
            }

            foundCreation.transform.parent = null;
            foundCreation.transform.position = startPos;
            foundCreation.transform.forward = startRotation;
            
            foundCreation.Initialize(_creationInfo, _user, _side);
        }

        #endregion
        
        

    }
}