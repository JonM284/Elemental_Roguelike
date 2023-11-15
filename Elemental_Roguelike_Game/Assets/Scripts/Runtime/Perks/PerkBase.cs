using Runtime.Character;
using UnityEngine;

namespace Runtime.Perks
{
    public abstract class PerkBase: ScriptableObject
    {
        
        [Header("Status Common")]
        
        public string perkName;

        public string perkDescription;

        public string perkGUID;

        [Header("Icons")]
        
        public Sprite perkIcon;
        
        public abstract void TriggerPerkEffect(CharacterBase _character);
        
        [ContextMenu("Generate GUID")]
        private void GenerateID()
        {
            perkGUID = System.Guid.NewGuid().ToString();
        }
        
    }
}