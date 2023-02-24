using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        #region Serialized Fields

        [SerializeField] private AssetReference regularEnemyReference;

        #endregion

        #region Private Fields

        private List<CharacterBase> m_cachedEnemies = new List<CharacterBase>();
        
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

        public CharacterBase GetCachedEnemy(CharacterStatsBase _stats)
        {
            if (_stats == null)
            {
                return default;
            }

            return m_cachedEnemies.FirstOrDefault(cb => cb.m_characterStatsBase == _stats);
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
                foundEnemy.transform.position = new Vector3(_spawnPos.x, foundEnemy.transform.localScale.y / 2, _spawnPos.z);
                if (foundEnemy is EnemyCharacterRegular regularEnemy)
                {
                    regularEnemy.AssignStats(_enemyStats);
                }
                foundEnemy.InitializeCharacter();
                currentRoom.AddEnemyToRoom(foundEnemy);
                yield break;
            }
            
            var handle = Addressables.LoadAssetAsync<GameObject>(regularEnemyReference);
            yield return handle;
            handle.Completed += operation =>
            {
                if (operation.Status == AsyncOperationStatus.Succeeded)
                {
                    var _newEnemyObject = Instantiate(handle.Result, new Vector3(_spawnPos.x, 
                        handle.Result.transform.localScale.y / 2, _spawnPos.z), Quaternion.identity);
                    var _newEnemy = _newEnemyObject.GetComponent<CharacterBase>();
                    if (_newEnemy != null)
                    {
                        if (_newEnemy is EnemyCharacterRegular regularEnemy)
                        {
                            regularEnemy.AssignStats(_enemyStats);
                        }
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