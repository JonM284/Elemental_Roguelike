using System.Collections.Generic;
using Data;
using UnityEngine;

namespace Runtime.GameplayEvents
{
    
    [CreateAssetMenu(menuName = "Custom Data/Event/Item Event Type")]
    public class ItemEventType: GameplayEventType
    {

        [SerializeField] private List<GameplayItemData> allPossibleItems = new List<GameplayItemData>();

        public GameplayItemData GetRandomItem()
        {
            return allPossibleItems[Random.Range(0, allPossibleItems.Count)];
        }

    }
}