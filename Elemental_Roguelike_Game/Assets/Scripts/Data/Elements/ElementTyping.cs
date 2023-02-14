using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data.Elements
{
    [Serializable]
    [CreateAssetMenu(menuName = "Custom Data/Element Type")]
    public class ElementTyping: ScriptableObject
    {
        #region Public Fields

        public string elementName = "Default Change";

        public List<ElementTyping> weaknesses = new List<ElementTyping>();

        #endregion

        #region Private Fields

        private float damageModifier = 1.5f;

        #endregion

        #region Class Implementation

        public float CalculateDamageOnWeakness(float _incomingDamage, ElementTyping _damagingType)
        {
            if (weaknesses.Count == 0 || !weaknesses.Contains(_damagingType))
            {
                return _incomingDamage;
            }

            return _incomingDamage * damageModifier;
        }

        #endregion
        
    }
}