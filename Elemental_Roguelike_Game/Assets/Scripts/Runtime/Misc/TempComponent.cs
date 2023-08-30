using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Runtime.Misc
{
    public class TempComponent: MonoBehaviour
    {
        
                    #region Nested Classes
            
                    [Serializable]
                    public class ItemsByWeight
                    {
                        public int weight;
                        public TempItem item;
                    }
            
                    #endregion
            
                    #region Public Fields
            
                    [SerializeField] private List<ItemsByWeight> itemsByWeight = new List<ItemsByWeight>();
                    
                    #endregion
            
                    #region Unity Events
            
                    
            
                    #endregion
            
                    #region Class Implementation
            
                    public TempItem GetValueByWeight(List<ItemsByWeight> itemsByWeight)
                    {
                        if (itemsByWeight.Count == 0)
                        {
                            return default;
                        }
                        
                        //default value
                        var endValue = itemsByWeight.FirstOrDefault().item;
            
                        //get random weight value from assigned weight values
                        var totalWeight = 0;
                        foreach (var itemByWeight in itemsByWeight)
                        {
                            totalWeight += itemByWeight.weight;
                        }
                        
                        //+1 because max value is exclusive
                        var randomValue = Random.Range(1, totalWeight + 1);
            
                        var currentWeight = 0;
                        foreach (var itemByWeight in itemsByWeight)
                        {
                            currentWeight += itemByWeight.weight;
                            if (randomValue <= currentWeight)
                            {
                                endValue = itemByWeight.item;
                                break;
                            }
                        }
            
                        return endValue;
                    }
            
                    #endregion
                    
    }
}