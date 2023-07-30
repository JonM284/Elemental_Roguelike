using System.Collections.Generic;
using Data.CharacterData;
using UnityEngine;

namespace Data.EnemyData
{
    [CreateAssetMenu(menuName = "Custom Data/ Enemy Team Data")]
    public class EnemyAITeamData: ScriptableObject
    {
        public List<CharacterStatsBase> enemyCharacters = new List<CharacterStatsBase>(5);
    }
}