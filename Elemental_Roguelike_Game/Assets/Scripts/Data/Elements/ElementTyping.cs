using System;
using System.Collections.Generic;
using Runtime.Weapons;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Data.Elements
{
    [Serializable]
    [CreateAssetMenu(menuName = "Custom Data/Element Type")]
    public class ElementTyping: ScriptableObject
    {
        #region Public Fields

        public string elementName = "Default Change";

        public string elementGUID = "";

        public List<ElementTyping> weaknesses = new List<ElementTyping>();

        [Header("Weapon Related")]
        public List<Color> weaponColors = new List<Color>(2);

        public AssetReference weaponMuzzleParticles = null;
        
        [Header("Meeple")]
        public List<Color> meepleColors = new List<Color>(2);

        public AssetReference defaultMeepleElementHat = null;

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
        
        
        [ContextMenu("Generate GUID")]
        private void GenerateID()
        {
            if (elementGUID != string.Empty)
            {
                return;
            }
            
            elementGUID = System.Guid.NewGuid().ToString();
        }

        #endregion
        
    }
}