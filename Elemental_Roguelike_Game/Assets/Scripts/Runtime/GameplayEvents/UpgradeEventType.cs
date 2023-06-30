using System.Collections.Generic;
using UnityEngine;

namespace Runtime.GameplayEvents
{
    
    [CreateAssetMenu(menuName = "Custom Data/Match Event Type")]
    public class UpgradeEventType: GameplayEventType
    {

        //ToDo: set data for this
        public List<int> upgradeChoices = new List<int>();


    }
}