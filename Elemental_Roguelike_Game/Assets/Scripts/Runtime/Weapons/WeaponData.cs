using System.Collections;
using System.Collections.Generic;
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

        public float weaponAttackRange = 1;

        public WeaponTargetType targetType;
        
        public List<AudioClip> weaponAudio = new List<AudioClip>();

        public AssetReference weaponPrefab;
        
        #endregion

        #region Accessors

        public bool hasMultipleWeaponAudios => weaponAudio.Count > 1;

        #endregion

        #region Class Implementation


        [ContextMenu("Generate GUID")]
        private void GenerateID()
        {
            if (weaponGUID != string.Empty)
            {
                return;
            }
            
            weaponGUID = System.Guid.NewGuid().ToString();
        }

        #endregion
        
        
    }
}