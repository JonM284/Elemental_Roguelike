using System.Collections.Generic;
using Data;
using Data.CharacterData;
using UnityEngine;

namespace Runtime.GameplayEvents
{
    
    [CreateAssetMenu(menuName = "Custom Data/Match Event Type")]
    public class MemberEventType : GameplayEventType
    {
        
        public List<CharacterStatsData> randomMemberAdditions = new List<CharacterStatsData>();

    }
}