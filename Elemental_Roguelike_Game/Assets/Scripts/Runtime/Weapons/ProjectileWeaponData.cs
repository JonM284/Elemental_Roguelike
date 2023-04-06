using System.Collections.Generic;
using Data;
using UnityEngine;

namespace Runtime.Weapons
{
    [CreateAssetMenu(menuName = "Custom Data/Projectile Weapon")]
    public class ProjectileWeaponData: WeaponData
    {

        #region Public Fields
        
        public List<ProjectileInfo> allPossibleProjectiles = new List<ProjectileInfo>();

        #endregion

    }
}