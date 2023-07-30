using System;
using System.Collections.Generic;
using System.Linq;
using Data.CharacterData;
using Project.Scripts.Data;
using Runtime.Battle;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Data
{
    [CreateAssetMenu(menuName = "Custom Data/ Battle Generation Rules")]
    public class BattleGenerationRulesData: ScriptableObject
    {
        #region Nested Classes

        [Serializable]
        public class EnemyChanceByWeight
        {
            [Tooltip("higher weight = higher chance of encounter")]
            public int weight;
            public CharacterStatsBase enemy;
        }
        
        [Serializable]
        public class BattlesByDifficulty
        {
            public BattleDifficulty difficulty;
            public List<EnemyChanceByWeight> enemies = new List<EnemyChanceByWeight>();
        }

        #endregion

        #region Public Fields

        public List<BattlesByDifficulty> battlesByDifficulties = new List<BattlesByDifficulty>();

        #endregion

        #region Class Implementation

        public List<CharacterStatsBase> GetBattleEnemies()
        {
            var randomInt = Random.Range(0, battlesByDifficulties.Count);

            var randomBattle = battlesByDifficulties[randomInt];

            var listOfEnemies = new List<CharacterStatsBase>();

            for (int i = 0; i < 3; i++)
            {
                var newEnemy = enemyByWeightChance(randomBattle.enemies);
                listOfEnemies.Add(newEnemy);
            }

            return listOfEnemies;
        }

        private CharacterStatsBase enemyByWeightChance(List<EnemyChanceByWeight> _enemyEncounterChances)
        {
            //default value
            var endValue = _enemyEncounterChances.FirstOrDefault().enemy;

            //get random weight value from room weight
            var totalWeight = 0;
            foreach (var enemyChance in _enemyEncounterChances)
            {
                totalWeight += enemyChance.weight;
            }
            
            //+1 because max value is exclusive
            var randomValue = Random.Range(1, totalWeight + 1);

            var currentWeight = 0;
            foreach (var enemyChance in _enemyEncounterChances)
            {
                currentWeight += enemyChance.weight;
                if (randomValue <= currentWeight)
                {
                    endValue = enemyChance.enemy;
                    break;
                }
            }

            return endValue;
        }

        #endregion


    }
}