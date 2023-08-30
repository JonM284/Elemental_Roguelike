using Data;
using UnityEngine;

namespace Runtime.Character.Creations.CreationDatas
{
    
    [CreateAssetMenu(menuName = "Creation/Turret")]
    public class TurretCreationData: CreationData
    {
        #region Serialized Fields

        [Header("Shots")]
        [Tooltip("Number of times this turret will fire when allowed")]
        [SerializeField] private int numberOfShots;

        [Header("Zone")]
        [Tooltip("Zone to create on detonation")] 
        [SerializeField] private ProjectileInfo projectileInfo;
        
        #endregion
        
        #region Proximity Perameter Getters

        public int GetNumOfShots()
        {
            return numberOfShots;
        }

        public ProjectileInfo GetProjectile()
        {
            return projectileInfo;
        }
        

        #endregion
    }
}