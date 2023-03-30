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

        public List<Color> weaponColors = new List<Color>(2);

        public List<Color> meepleColors = new List<Color>(2);

        #endregion

        #region Private Fields

        private int damageModifier = 2;

        #endregion

        #region Class Implementation

        public int CalculateDamageOnWeakness(int _incomingDamage, ElementTyping _damagingType)
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