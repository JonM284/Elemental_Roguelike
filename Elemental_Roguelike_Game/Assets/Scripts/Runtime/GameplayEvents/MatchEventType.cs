using Data;
using UnityEngine;

namespace Runtime.GameplayEvents
{
    
    [CreateAssetMenu(menuName = "Custom Data/Match Event Type")]
    public class MatchEventType : GameplayEventType
    {

        public int arenaID = 0;

        public SceneName sceneName;

    }
}