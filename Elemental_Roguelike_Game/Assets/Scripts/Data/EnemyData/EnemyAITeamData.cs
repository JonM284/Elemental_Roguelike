using System.Collections.Generic;
using Data.CharacterData;
using UnityEngine;

namespace Data.EnemyData
{
    [CreateAssetMenu(menuName = "Custom Data/Enemy/Enemy Team Data")]
    public class EnemyAITeamData: ScriptableObject
    {
        public string tournamentGuid;
        public List<CharacterStatsBase> enemyCharacters = new List<CharacterStatsBase>(5);

        [ContextMenu("Generate GUID")]
        private void GenerateID()
        {
            tournamentGuid = System.Guid.NewGuid().ToString();
        }
    }
}