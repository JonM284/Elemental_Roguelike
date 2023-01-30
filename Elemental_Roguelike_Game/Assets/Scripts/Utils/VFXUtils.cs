using GameControllers;
using Runtime.VFX;
using UnityEngine;

namespace Utils
{
    public static class VFXUtils
    {

        #region Private Fields

        private static VFXController _vfxController;

        #endregion

        #region Accessors

        public static VFXController vfxController => GameControllerUtils.GetGameController(ref _vfxController);

        #endregion

        #region Class Implementation

        /// <summary>
        /// Check to see if this vfxPlayer is done, if it is, return it to the pool for later
        /// </summary>
        /// <param name="vfxPlayer"></param>
        public static void CheckVFX(this VFXPlayer vfxPlayer)
        {
            if (vfxPlayer == null)
            {
                return;
            }

            if (vfxPlayer.is_playing)
            {
                return;
            }
            
            vfxPlayer.ReturnToPool();
        }

        public static void ReturnToPool(this VFXPlayer vfxPlayer)
        {
            vfxController.ReturnToPool(vfxPlayer);
        }

        public static void PlayAt(this VFXPlayer vfxPlayer, Vector3 position, Quaternion rotation)
        {
            vfxController.PlayAt(vfxPlayer, position, rotation);
        }

        #endregion

    }
}