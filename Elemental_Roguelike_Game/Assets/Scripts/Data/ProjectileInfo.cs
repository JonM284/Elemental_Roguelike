using UnityEngine;

namespace Data
{
    [CreateAssetMenu(menuName = "Custom Data/Projectile")]
    public class ProjectileInfo: ScriptableObject
    {

        #region Public Fields

        public float projectileDamage = 1f;

        public float projectileSpeed = 1f;

        public float projectileLifetime = 1f;

        #endregion

    }
}