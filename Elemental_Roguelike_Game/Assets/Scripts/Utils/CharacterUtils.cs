using System.Collections;
using System.Collections.Generic;
using Data;
using Project.Scripts.Data;
using Project.Scripts.Utils;
using Runtime.Character;
using Runtime.GameControllers;
using UnityEngine;

namespace Utils
{
    public static class CharacterUtils
    {

        #region Private Fields

        private static EnemyController m_enemyController;

        private static MeepleController m_meepleController;

        #endregion

        #region Accessors

        private static EnemyController enemyController => GameControllerUtils.GetGameController(ref m_enemyController);
        
        private static MeepleController meepleController => GameControllerUtils.GetGameController(ref m_meepleController);

        #endregion

        #region Class Implementation
        
        public static void SetActiveCharacter(CharacterBase _activeCharacter)
        {
            if (_activeCharacter == null)
            {
                return;
            }
            
            //ToDo: Add a way to have meeple be able to move by default
        }

        public static List<CharacterBase> SortCharacterTurnOrder(this List<CharacterBase> characters)
        {
            bool isSorted = false;
            int low = 0;
            int high = characters.Count-1;
            
            QuickSort(characters, low, high);
            
            characters.Reverse();
            
            return characters;
        }

        private static void Swap(List<CharacterBase> characters, int i, int j)
        {
            (characters[i], characters[j]) = (characters[j], characters[i]);
        }

        private static void QuickSort(List<CharacterBase> characters, int low, int high)
        {
            if (low < high)
            {
                int pi = Partition(characters, low, high);
                
                QuickSort(characters, low, pi - 1);
                QuickSort(characters, pi + 1, high);
            }
        }

        private static int Partition(List<CharacterBase> characters, int low, int high)
        {
            var pivot = characters[high];
            var i = low - 1;
            for (int j = low; j <= high - 1; j++)
            {
                if (characters[j].initiativeNum < pivot.initiativeNum)
                {
                    i++;
                    Swap(characters, i, j);
                }else if (characters[j].initiativeNum == pivot.initiativeNum)
                {
                    if (characters[j].baseSpeed < pivot.baseSpeed)
                    {
                        i++;
                        Swap(characters, i, j);
                    }
                }
            }
            Swap(characters, i+1, high);
            return (i + 1);
        }

        public static IEnumerator AddEnemy(this CharacterStatsBase _enemyStats, Vector3 _spawnPos)
        {
            var waiting = enemyController.C_AddEnemy(_enemyStats, _spawnPos);
            yield return waiting;
        }

        public static void CacheEnemy(this CharacterBase _enemy)
        {
            enemyController.CacheEnemy(_enemy);
        }

        public static CharacterStatsData GetMeepleFromGUID(this string _UID)
        {
            return meepleController.GetMeepleFromUID(_UID);
        }

        public static void CachePlayerMeeple(CharacterBase _playableMeeple)
        {
            meepleController.CacheMeepleGameObject(_playableMeeple.gameObject);
        }

        public static Dictionary<string, CharacterStatsData> GetAllMeeples()
        {
            return meepleController.GetAllMeeples();
        }

        public static void CreateNewMeeple()
        {
            meepleController.CreateNewCharacter();
        }

        public static void DeletePlayerMeeple(string _uid)
        {
            meepleController.DeletePlayerMeeple(_uid);
        }

        #endregion
        
        
    }
}