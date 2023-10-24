using System.Collections.Generic;
using Data;
using UnityEngine;

namespace Runtime.GameplayEvents
{
    [CreateAssetMenu(menuName = "Custom Data/Event/Vending Event Type")]
    public class VendingEventType: GameplayEventType
    {
        [SerializeField] private List<GameplayItemData> m_items = new List<GameplayItemData>();
        
        public List<GameplayItemData> GetItems()
        {
            return m_items;
        }
    }
}