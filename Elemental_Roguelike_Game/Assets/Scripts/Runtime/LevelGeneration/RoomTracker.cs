using System.Collections;
using System.Collections.Generic;
using Data.CharacterData;
using Project.Scripts.Data;
using Runtime.Character;
using UnityEngine;
using UnityEngine.AI;
using Utils;

namespace Project.Scripts.Runtime.LevelGeneration
{
    [DisallowMultipleComponent]
    public class RoomTracker: MonoBehaviour
    {

        #region Public Fields

        public int level;

        public RoomType roomType;

        public List<DoorChecker> modifiableDoorCheckers = new List<DoorChecker>();

        #endregion

        #region Serialized Fields

        [SerializeField]
        private List<DoorChecker> doorCheckers = new List<DoorChecker>();

        [SerializeField] private NavMeshSurface navMeshSurface;

        [SerializeField] private Transform decorationHolder;

        #endregion

        #region Private Fields

        [SerializeField]
        private List<CharacterBase> m_enemies = new List<CharacterBase>();

        [SerializeField]
        private List<CharacterStatsBase> m_cachedEnemyStats = new List<CharacterStatsBase>();

        [SerializeField]
        private List<Transform> m_enemySpawnTransforms = new List<Transform>();

        #endregion

        #region Accessor
        
        public bool hasBuiltNavmesh { get; private set; }

        public Transform decorationTransform => decorationHolder;
        
        public bool hasBattle { get; private set; }

        public List<CharacterBase> roomEnemies => m_enemies;

        #endregion

        #region Class Implementation

        public void ResetRoom()
        {
            level = 0;
            roomType = RoomType.FOUR_DOOR;
            doorCheckers.ForEach(dc =>
            {
                dc.ResetWalls();
                if (!modifiableDoorCheckers.Contains(dc))
                {
                    modifiableDoorCheckers.Add(dc);
                }
            });
            
            m_enemies.Clear();
        }

        public void UpdateRoomNavMesh()
        {
            navMeshSurface.BuildNavMesh();
            hasBuiltNavmesh = true;
        }

        public void AddEnemyToRoom(CharacterBase _enemy)
        {
            Debug.Log("Enemy Added");
            m_enemies.Add(_enemy);
        }

        public void AssignBattle(List<CharacterStatsBase> enemies)
        {
            hasBattle = true;
            m_cachedEnemyStats = enemies;
        }

        public void AddEnemySpawnPos(Transform _newTransform)
        {
            m_enemySpawnTransforms.Add(_newTransform);
        }

        public IEnumerator SetupBattle()
        {
            foreach (var enemy in m_cachedEnemyStats)
            {
                var randomInt = Random.Range(0, m_enemySpawnTransforms.Count);
                //StartCoroutine(enemy.AddEnemy(m_enemySpawnTransforms[randomInt].transform.position));
                m_enemySpawnTransforms.Remove(m_enemySpawnTransforms[randomInt]);
                yield return new WaitForSeconds(0.75f);
            }
        }

        public void CompleteBattle()
        {
            hasBattle = false;
            foreach (var enemy in m_enemies)
            {
                enemy.CacheEnemy();
            }
        }

        #endregion

    }
}