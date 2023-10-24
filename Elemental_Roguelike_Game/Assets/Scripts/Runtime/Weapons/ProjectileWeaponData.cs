using System.Collections.Generic;
using Data;
using UnityEngine;

namespace Runtime.Weapons
{
    [CreateAssetMenu(menuName = "Custom Data/Weapon/Projectile Weapon")]
    public class ProjectileWeaponData: WeaponData
    {

        #region Public Fields
        
        [Header("Shotgun")]
        public bool isBurstWeapon;
        public float coneSize;
        
        [Header("General")]
        public int amountOfShots = 1;
        public float fireRate;
        public float shotMissDistance = 1f;
        public List<ProjectileInfo> allPossibleProjectiles = new List<ProjectileInfo>();

        #endregion

        #region Accessors

        public bool hasMultipleBullets => amountOfShots > 1;

        #endregion

    }
}