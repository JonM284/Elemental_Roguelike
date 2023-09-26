using System;
using UnityEngine;

namespace Data.Sides
{
    [Serializable]
    [CreateAssetMenu(menuName = "Custom Data/Character Side")]
    public class CharacterSide: ScriptableObject
    {

        public string sideGUID;
        
        [ContextMenu("Generate GUID")]
        private void GenerateID()
        {
            if (sideGUID != string.Empty)
            {
                return;
            }
            
            sideGUID = System.Guid.NewGuid().ToString();
        }
        
        
    }
}