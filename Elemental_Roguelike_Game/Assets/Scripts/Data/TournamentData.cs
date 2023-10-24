using System.Collections.Generic;
using Data.EnemyData;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(menuName = "Tournament/Tournament Data")]
    public class TournamentData: ScriptableObject
    {
        public bool isUnlocked;
        public bool isCompleted;
        public string tournamentName;
        public List<SceneName> possibleArenaPools = new List<SceneName>();
        public List<EnemyAITeamData> tournamentTeams = new List<EnemyAITeamData>();
    }
}