using System.Collections.Generic;
using System.Linq;
using Runtime.Weapons;
using UnityEngine;

namespace Runtime.GameControllers
{
    public class WeaponController: GameControllerBase
    {

        #region Serialized Fields

        [SerializeField] private List<WeaponData> weaponDataList = new List<WeaponData>();

        #endregion

        #region Class Implementation

        public WeaponData GetWeaponByGUID(string _searchGUID)
        {
            var foundWeaponData = weaponDataList.FirstOrDefault(a => a.weaponGUID == _searchGUID);
            if (foundWeaponData == null)
            {
                return default;
            }

            return foundWeaponData;
        }

        public WeaponData GetDefaultWeapon()
        {
            return weaponDataList[0];
        }

        #endregion


    }
}