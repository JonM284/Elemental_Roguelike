using System;
using System.Collections.Generic;
using Runtime.Status;
using UnityEngine;

namespace Data.Elements
{
    [Serializable]
    [CreateAssetMenu(menuName = "Custom Data/Element Type")]
    public class ElementTyping: ScriptableObject
    {
        #region Public Fields
        
        [Header("----- ELEMENT DATA -----")]

        public string elementName = "Default Element Name";

        public string elementGUID = "";
        
        [Header("----- WEAKNESSES & STRENGTHS -----")]
        
        public List<ElementTyping> weaknesses = new List<ElementTyping>();

        [Tooltip("Do not enter it's own element type into resistances, this is automatic")]
        public List<ElementTyping> resistances = new List<ElementTyping>();

        [Header("----- IMMUNITIES -----")]
        public List<ElementTyping> immunities = new List<ElementTyping>();

        public List<Status> statusImmunities = new List<Status>();

        [Header("----- Meeple -----")]
        public Sprite meepleIcon;
        public List<Color> meepleColors = new List<Color>(2);

        [Header("----- Card Display -----")]
        public Sprite elementSprite;
        
        #endregion

        #region Private Fields

        private int damageModifier = 2;

        private float resistanceModifier = 0.5f;

        #endregion

        #region Class Implementation

        public int CalculateDamageOnWeakness(int _incomingDamage, ElementTyping _damagingType)
        {
            if (immunities.Count != 0 && immunities.Contains(_damagingType))
            {
                return 0;
            }
            
            if (weaknesses.Count != 0 && weaknesses.Contains(_damagingType))
            {
                return _incomingDamage * damageModifier;
            }

            if ((resistances.Count != 0 && resistances.Contains(_damagingType)) || _damagingType == this)
            {
                return Mathf.CeilToInt(_incomingDamage * resistanceModifier);
            }

            return _incomingDamage;
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