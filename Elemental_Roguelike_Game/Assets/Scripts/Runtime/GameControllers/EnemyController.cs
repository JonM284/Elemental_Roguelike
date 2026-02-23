using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data.CharacterData;
using Project.Scripts.Data;
using Project.Scripts.Utils;
using Runtime.Character;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Runtime.GameControllers
{
    [Obsolete]
    public class EnemyController: GameControllerBase
    {
        
        #region Static

        public static EnemyController Instance { get; private set; }

        #endregion

        #region Events

        public static event Action<CharacterBase> EnemyCreated;

        #endregion

        #region Serialized Fields

        [SerializeField] private AssetReference _enemyStats;

        #endregion
        
        #region Private Fields

        private List<CharacterBase> m_cachedEnemies = new List<CharacterBase>();

        private List<CharacterBase> m_cachedLoadedEnemies = new List<CharacterBase>();

        private Transform m_cachedEnemyPoolTransform;
        
        #endregion

        #region Accessors

        public Transform cachedEnemyPool =>
            CommonUtils.GetRequiredComponent(ref m_cachedEnemyPoolTransform, ()=>
            {
                var poolTransform = TransformUtils.CreatePool(this.transform, false);
                return poolTransform;
            });
        
        

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

        private CharacterBase GetCachedEnemy(CharacterStatsBase _stats)
        {
            if (_stats == null)
            {
                return default;
            }

            return m_cachedEnemies.FirstOrDefault(cb => cb.characterStatsBase == _stats);
        }

        private CharacterBase GetLoadedEnemy(CharacterStatsBase _stats)
        {
            if (_stats == null)
            {
                return default;
            }

            return m_cachedLoadedEnemies.FirstOrDefault(cb => cb.characterStatsBase == _stats);
        }

        public IEnumerator C_AddEnemy(CharacterStatsBase _enemyStats, Vector3 _spawnPos, Vector3 spawnRotation)
        {
            if (_enemyStats == null)
            {
                yield break;
            }
            
            var adjustedSpawnLocation = _spawnPos != Vector3.zero ? _spawnPos : Vector3.zero;

            var adjustedSpawnRotation = spawnRotation != Vector3.zero ? spawnRotation : Vector3.zero;

            var foundEnemy = GetCachedEnemy(_enemyStats);

            if (foundEnemy != null)
            {
                m_cachedEnemies.Remove(foundEnemy);
                foundEnemy.transform.parent = null;
                foundEnemy.transform.position = adjustedSpawnLocation;
                foundEnemy.transform.rotation = Quaternion.Euler(adjustedSpawnRotation);
                foundEnemy.InitializeCharacter(_enemyStats);
                EnemyCreated?.Invoke(foundEnemy);
                yield break;
            }

            //Check if enemy was previously loaded
            var foundLoadedEnemy = GetLoadedEnemy(_enemyStats);

            if (foundLoadedEnemy != null)
            {
                var _newEnemyGO = Instantiate(foundLoadedEnemy.gameObject, adjustedSpawnLocation, Quaternion.Euler(adjustedSpawnRotation));
                var _enemyComp = _newEnemyGO.GetComponent<CharacterBase>();
                _enemyComp.InitializeCharacter(_enemyStats);
                EnemyCreated?.Invoke(_enemyComp);
                yield break;
            }
            
            //Load new Enemy
            
            var handle = Addressables.LoadAssetAsync<GameObject>(_enemyStats);
            yield return handle;
            
            if (!handle.IsDone)
            {
                yield return handle;
            }
            
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                var _newEnemyObject = Instantiate(handle.Result, adjustedSpawnLocation, Quaternion.Euler(adjustedSpawnRotation));
                var _newEnemy = _newEnemyObject.GetComponent<CharacterBase>();
                m_cachedLoadedEnemies.Add(handle.Result.GetComponent<CharacterBase>());
                if (_newEnemy != null)
                {
                    _newEnemy.InitializeCharacter(_enemyStats);
                }
                EnemyCreated?.Invoke(_newEnemy);
            }
            else
            {
                Addressables.Release(handle);
            }
            
        }

        public void CacheEnemy(CharacterBase _enemy)
        {
            if (_enemy == null)
            {
                return;
            }

            m_cachedEnemies.Add(_enemy);
            _enemy.transform.ResetTransform(cachedEnemyPool);
        }

        

        #endregion


    }
}