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

        #endregion

        #region Class Implementation

        public ElementTyping GetElementByName(string _name)
        {
            var foundElement = allElementTypes.FirstOrDefault(et => et.elementName == _name);
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

        #endregion
        
        
    }
}