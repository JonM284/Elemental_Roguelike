using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data.Sides;
using Project.Scripts.Data;
using Project.Scripts.Utils;
using Runtime.Character;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utils;

namespace Runtime.GameControllers
{
    public class EnemyController: GameControllerBase
    {
        
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

        public IEnumerator C_AddEnemy(CharacterStatsBase _enemyStats, Vector3 _spawnPos)
        {
            if (_enemyStats == null)
            {
                yield break;
            }

            var foundEnemy = GetCachedEnemy(_enemyStats);
            var currentRoom = LevelUtils.GetCurrentRoom();

            if (foundEnemy != null)
            {
                m_cachedEnemies.Remove(foundEnemy);
                foundEnemy.transform.parent = null;
                foundEnemy.transform.position = new Vector3(_spawnPos.x, foundEnemy.transform.localScale.y / 2, _spawnPos.z);
                foundEnemy.InitializeCharacter();
                currentRoom.AddEnemyToRoom(foundEnemy);
                yield break;
            }

            //Check if enemy was previously loaded
            var foundLoadedEnemy = GetLoadedEnemy(_enemyStats);

            if (foundLoadedEnemy != null)
            {
                var _newEnemyGO = Instantiate(foundLoadedEnemy.gameObject, new Vector3(_spawnPos.x, 
                    foundLoadedEnemy.transform.localScale.y / 2, _spawnPos.z), Quaternion.identity);
                var _enemyComp = _newEnemyGO.GetComponent<CharacterBase>();
                _enemyComp.InitializeCharacter();
                currentRoom.AddEnemyToRoom(_enemyComp);
                yield break;
            }
            
            //Load new Enemy
            
            var handle = Addressables.LoadAssetAsync<GameObject>(_enemyStats.characterAssetRef);
            yield return handle;
            handle.Completed += operation =>
            {
                if (operation.Status == AsyncOperationStatus.Succeeded)
                {
                    var _newEnemyObject = Instantiate(handle.Result, new Vector3(_spawnPos.x, 
                        handle.Result.transform.localScale.y / 2, _spawnPos.z), Quaternion.identity);
                    var _newEnemy = _newEnemyObject.GetComponent<CharacterBase>();
                    m_cachedLoadedEnemies.Add(handle.Result.GetComponent<CharacterBase>());
                    if (_newEnemy != null)
                    {
                        _newEnemy.InitializeCharacter();
                        currentRoom.AddEnemyToRoom(_newEnemy);
                    }
                }
            };
            
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