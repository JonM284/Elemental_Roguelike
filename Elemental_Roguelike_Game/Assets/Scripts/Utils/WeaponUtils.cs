using System;
using Runtime.GameControllers;
using Runtime.Weapons;

namespace Utils
{
    public static class WeaponUtils
    {
        
        #region Private Fields

        private static WeaponController _weaponController;

        #endregion

        #region Accessors

        private static WeaponController weaponController => GameControllerUtils.GetGameController(ref _weaponController);

        #endregion

        #region Class Implementation

        public static WeaponData GetDataByRef(string guid)
        {
            if (guid == String.Empty)
            {
                return default;
            }

            return weaponController.GetWeaponByGUID(guid);
        }

        public static WeaponData GetDefaultWeapon()
        {
            return weaponController.GetDefaultWeapon();
        }

        #endregion
        
        
    }
}