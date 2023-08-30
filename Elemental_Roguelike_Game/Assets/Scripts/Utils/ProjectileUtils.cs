using System;
using Data;
using Runtime.Character;
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

        public static void ReturnToPool(this ZoneBase zone)
        {
            projectileController.ReturnToPool(zone);
        }

        public static void PlayAt(this ProjectileInfo projectile, Transform user ,Vector3 position, Vector3 rotation, Vector3 endPos)
        {
            projectileController.GetProjectileAt(projectile, user ,position, rotation, endPos);
        }

        public static void PlayAt(this ZoneInfo zoneInfo, Vector3 _position, Transform _user)
        {
            projectileController.GetZoneAt(zoneInfo, _position, _user);
        }

        #endregion

    }
}