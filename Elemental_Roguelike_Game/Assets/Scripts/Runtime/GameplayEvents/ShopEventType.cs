using System.Collections.Generic;
using Data;
using Project.Scripts.Utils;
using UnityEngine;

namespace Runtime.GameplayEvents
{
    [CreateAssetMenu(menuName = "Custom Data/Shop Event Type")]
    public class ShopEventType: GameplayEventType
    {
        [SerializeField] private int amountOfItems = 3;
        
        [SerializeField] private List<GameplayItemData> allPossibleItems = new List<GameplayItemData>();

        private List<GameplayItemData> currentPossibleItems = new List<GameplayItemData>();

        public List<GameplayItemData> GetRandomItems()
        {
            if (currentPossibleItems.Count > 0)
            {
                currentPossibleItems.Clear();
            }
            
            currentPossibleItems = allPossibleItems.ToList();

            var returnedItems = new List<GameplayItemData>();

            for (int i = 0; i < amountOfItems; i++)
            {
                var randomIndex = Random.Range(0, currentPossibleItems.Count);
                var randomItem = currentPossibleItems[randomIndex];
                returnedItems.Add(randomItem);
                currentPossibleItems.Remove(randomItem);
            }
            
            return returnedItems;
        }
    }
}