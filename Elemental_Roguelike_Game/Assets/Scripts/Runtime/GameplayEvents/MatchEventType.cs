using System.Collections.Generic;
using Data;
using Data.EnemyData;
using UnityEngine;

namespace Runtime.GameplayEvents
{
    
    [CreateAssetMenu(menuName = "Custom Data/Match Event Type")]
    public class MatchEventType : GameplayEventType
    {

        #region Public Fields

        public SceneName sceneName;

        public List<EnemyAITeamData> possibleEnemyTeams = new List<EnemyAITeamData>();

        public bool m_isMeepleTeam;

        #endregion

        #region Class Implementation

        public EnemyAITeamData GetRandomEnemyTeam()
        {
            return possibleEnemyTeams[Random.Range(0, possibleEnemyTeams.Count)];
        }

        #endregion

    }
}