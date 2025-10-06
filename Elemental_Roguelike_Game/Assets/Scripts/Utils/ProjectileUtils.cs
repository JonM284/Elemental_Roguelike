using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using Data.AbilityDatas;
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

        public static void PlayAt(this ProjectileAbilityData projectileData, Transform user, 
            Vector3 position, Vector3 rotation, Vector3 endPos, Transform target, CancellationToken cancellationToken)
        {
            projectileController.GetProjectileAt(projectileData, user ,position, rotation, endPos, target, cancellationToken).Forget();
        }

        public static void PlayAt(this AoeZoneData aoeZoneData, Vector3 _position, Transform _user, CancellationToken cancellationToken)
        {
            projectileController.GetZoneAt(aoeZoneData, _position, _user, cancellationToken).Forget();
        }

        #endregion

    }
}