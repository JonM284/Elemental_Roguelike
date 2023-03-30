using System.Collections;
using Data.Elements;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Runtime.Weapons
{
    public abstract class WeaponData: ScriptableObject
    {

        #region Public Fields

        public string weaponName = "weapon";

        public string weaponGUID;

        public ElementTyping type;

        public AssetReference weaponModelRef;

        #endregion

        #region Class Implementation


        [ContextMenu("Generate GUID")]
        private void GenerateID()
        {
            weaponGUID = System.Guid.NewGuid().ToString();
        }

        #endregion
        
        
    }
}