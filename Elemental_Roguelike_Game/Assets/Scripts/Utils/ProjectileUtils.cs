using System;
using Runtime.GameControllers;
using Runtime.Weapons;
using UnityEngine;

namespace Utils
{
    public static class ProjectileUtils
    {

        #region Private Fields

        private static ProjectileController _projectileController;

        #endregion

        #region Accessors

        private static ProjectileController projectileController => GameControllerUtils.GetGameController(ref _projectileController);

        #endregion
        
        #region Class Implementation

        public static void ReturnToPool(this ProjectileBase projectile)
        {
            projectileController.ReturnToPool(projectile);
        }

        public static void PlayAt(this ProjectileBase projectile, Vector3 position, Vector3 rotation, Vector3 endPos)
        {
            projectileController.GetProjectileAt(projectile, position, rotation, endPos);
        }

        #endregion
        
    }
}