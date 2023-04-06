using System;
using System.Collections.Generic;
using System.Linq;
using Data.Elements;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Runtime.GameControllers
{
    public class ElementController: GameControllerBase
    {

        #region Serialized Fields

        [SerializeField] private List<ElementTyping> allElementTypes;

        [SerializeField] private ElementTyping defaultElement;

        #endregion

        #region Class Implementation

        public ElementTyping GetElementByGUID(string guid)
        {
            var foundElement = allElementTypes.FirstOrDefault(et => et.elementGUID == guid);
            if (foundElement == null)
            {
                return default;
            }

            return foundElement;
        }
        
        public ElementTyping GetRandomElementTyping()
        {
            var randomElementIndex = Random.Range(0, allElementTypes.Count);
            return allElementTypes[randomElementIndex];
        }

        public ElementTyping GetDefaultElement()
        {
            return defaultElement;
        }

        #endregion
        
        
    }
}