using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Project.Scripts.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Runtime.GameControllers
{
    public class ObjectPoolController: GameControllerBase
    {
        
        #region Instance

        public static ObjectPoolController Instance { get; private set; }

        #endregion

        #region Nested Classes
        //ToDo: change to addressables
        [Serializable]
        public class ObjectPool
        {
            public GameObject clonedObject;
            public List<GameObject> pooledObjects = new List<GameObject>();
            
            public ObjectPool(GameObject _newObject, GameObject _firstObject)
            {
                clonedObject = _newObject;
                pooledObjects.Add(_firstObject);
            }

            public void ForceCreateNewItem(Transform _parent)
            {
                Instantiate(clonedObject, _parent);
            }

            public GameObject GetItem()
            {
                if (pooledObjects.Count == 0)
                {
                    return Instantiate(clonedObject);
                }

                var _item = pooledObjects[0];
                pooledObjects.Remove(_item);
                return _item;
            }

            public void ReturnItem(GameObject _returnedItem)
            {
                pooledObjects.Add(_returnedItem);
            }
        }

        #endregion

        #region Private Fields
        
        private Dictionary<string, Dictionary<string, ObjectPool>> m_cachedPools =
            new Dictionary<string, Dictionary<string, ObjectPool>>(); 

        private Transform m_cachedObjectPoolParent;

        private CancellationTokenSource cts = new CancellationTokenSource();
        
        #endregion

        #region Read-Only Pool Names

        public static readonly string StatusPoolName = "Statuses@Pool";
        
        public static readonly string ProjectilePoolName = "Projectiles@Pool";

        public static readonly string ZonePoolName = "Zones@Pool";

        public static readonly string CreationPoolName = "Creations@Pool";
        
        #endregion

        #region Accessors

        public Transform cachedObjectPoolParent =>
            CommonUtils.GetRequiredComponent(ref m_cachedObjectPoolParent, 
                ()=> TransformUtils.CreatePool(this.transform, false));

        #endregion

        #region GameControllerBase Inherited Methods

        public override void Initialize()
        {
            if (!Instance.IsNull())
            {
                return;
            }
            
            Instance = this;
            base.Initialize();
        }

        #endregion

        #region Class Implementation

        public async UniTask<GameObject> CreateObjectAsync(string poolName, string poolKey, 
            AssetReference reference, Vector3 position, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            GameObject _returnedObject = null;
            if (m_cachedPools.Count == 0 || !ContainsPool(poolName, poolKey))
            {
                Debug.Log($"{this.name} Creating Pool {poolName} with Key:{poolKey}");
                var newPool = await CreateNewPoolAsync(poolName, poolKey, reference, token);
                _returnedObject = newPool[poolKey].GetItem();
                _returnedObject.transform.position = position;
                _returnedObject.transform.parent = null;
                return _returnedObject;
            }

            _returnedObject = GetCachedObject(poolName, poolKey);
            _returnedObject.transform.position = position;
            _returnedObject.transform.parent = null;
            await UniTask.Yield();
            return _returnedObject;
        }
        
        public async UniTask<GameObject> CreateObjectAsync(string poolName, string poolKey, 
            GameObject reference, Vector3 position, CancellationToken token)
        {
            GameObject _returnedObject = null;
            if (m_cachedPools.Count == 0 || !ContainsPool(poolName, poolKey))
            {
                var newPool = await CreateNewPoolAsync(poolName, poolKey, reference, token);
                _returnedObject = newPool[poolKey].GetItem();
                _returnedObject.transform.position = position;
                _returnedObject.transform.parent = null;
                return _returnedObject;
            }

            _returnedObject = GetPool(poolName, poolKey).GetItem();
            _returnedObject.transform.position = position;
            _returnedObject.transform.parent = null;
            await UniTask.Yield();
            return _returnedObject;
        }

        public async UniTask PreCreateObjectAsync(string poolName, string key, GameObject reference, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (m_cachedPools.Count == 0 || !ContainsPool(poolName, key))
            {
                await CreateNewPoolAsync(poolName, key, reference, token);
            }

            GetPool(poolName, key).ForceCreateNewItem(m_cachedObjectPoolParent);
        }
        
        

        public async UniTask<GameObject> CreateParentedObjectAsync(string poolName, string poolKey, GameObject reference,
            Transform _parent, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            GameObject _returnedObject = null;
            if (m_cachedPools.Count == 0 || !ContainsPool(poolName, poolKey))
            {
                var newPool = await CreateNewPoolAsync(poolName, poolKey ,reference, token);
                _returnedObject = newPool[poolKey].GetItem();
                _returnedObject.transform.parent = _parent;
                return _returnedObject;
            }

            _returnedObject = GetCachedObject(poolName, poolKey);
            _returnedObject.transform.parent = _parent;
            return _returnedObject;
        }

        
        private async UniTask<Dictionary<string, ObjectPool>> CreateNewPoolAsync(string poolName, 
            string referenceGUID, AssetReference _reference, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            
            var _clonedObject = await AddressableController.Instance.T_LoadGameObject(_reference, null, cachedObjectPoolParent);
            var _firstObject = Instantiate(_clonedObject, cachedObjectPoolParent);

            var newPool = new ObjectPool(_clonedObject, _firstObject);
            var newPoolGroup = new Dictionary<string, ObjectPool> { { referenceGUID, newPool } };
            m_cachedPools.Add(poolName, newPoolGroup);
            
            return newPoolGroup;
        }
        
        private async UniTask<Dictionary<string, ObjectPool>> CreateNewPoolAsync(string poolName,
            string referenceGUID, GameObject reference, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            
            var _clonedObject = Instantiate(reference, cachedObjectPoolParent);
            var _firstObject = Instantiate(_clonedObject, cachedObjectPoolParent);

            var newPool = new ObjectPool(_clonedObject, _firstObject);
            var newPoolGroup = new Dictionary<string, ObjectPool> { { referenceGUID, newPool } };
            m_cachedPools.Add(poolName, newPoolGroup);
            
            return newPoolGroup;
        }

        private ObjectPool GetPool(string poolName, string key)
        {
            return m_cachedPools[poolName][key];
        }

        private void DeleteAllPools()
        {
            if (m_cachedPools.Count == 0)
            {
                return;
            }
            
            foreach (var poolDictionaries in m_cachedPools)
            {
                foreach (var poolGroup in poolDictionaries.Value)
                {
                    if (poolGroup.Value.IsNull())
                    {
                        continue;
                    }
                    
                    Destroy(poolGroup.Value.clonedObject);

                    if (poolGroup.Value.pooledObjects.Count == 0)
                    {
                        continue;
                    }

                    foreach (var pooledObject in poolGroup.Value.pooledObjects)
                    {
                        Destroy(pooledObject);
                    }
                }
            }
            
            m_cachedPools.Clear();
        }
        
        private bool ContainsPool(string poolKey, string objectGUID)
        {
            return m_cachedPools.ContainsKey(poolKey) && m_cachedPools[poolKey].ContainsKey(objectGUID);
        }

        private GameObject GetCachedObject(string poolKey, string key)
        {
            return m_cachedPools[poolKey][key].GetItem();
        }

        public async UniTask ReturnToPool(string poolName, string key, GameObject returnedObject, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (!ContainsPool(poolName,key))
            {
                await CreateNewPoolAsync(poolName, key, returnedObject, token);
                return;
            }
            
            m_cachedPools[poolName][key].ReturnItem(returnedObject);
            ReParentObject(returnedObject.transform);
        }

        public async UniTask ReturnToPool(string poolName, string key, GameObject returnedObject)
        {
            if (!ContainsPool(poolName, key))
            {
                await CreateNewPoolAsync(poolName, key, returnedObject, CreateToken());
                return;
            }
            
            m_cachedPools[poolName][key].ReturnItem(returnedObject);
            ReParentObject(returnedObject.transform);
        }

        private void ReParentObject(Transform returnedTransform)
        {
            returnedTransform.transform.parent = cachedObjectPoolParent;
            returnedTransform.transform.localPosition = Vector3.zero;
        }

        private CancellationToken CreateToken()
        {
            if (!cts.IsNull())
            {
                cts.Cancel();
            }

            cts = new CancellationTokenSource();
            return cts.Token;
        }

        #endregion
        
    }
}