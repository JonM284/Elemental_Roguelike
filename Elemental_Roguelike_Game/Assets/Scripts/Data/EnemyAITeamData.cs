using System.Collections.Generic;
using Project.Scripts.Data;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Data
{
    [CreateAssetMenu(menuName = "Custom Data/ Enemy Team Data")]
    public class EnemyAITeamData: ScriptableObject
    {
        public List<CharacterStatsBase> enemyCharacters = new List<CharacterStatsBase>(5);
    }
}