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

        [Header("Proximity")]
        [Tooltip("Radius around creation to perform action. AKA, something moves into range, perform action.")]
        [SerializeField] private float detonationDetectionRadius;

        [Header("Hidden Proximity")]
        [Tooltip("Is this creation hidden from the other team until walking into discovery range?")]
        [SerializeField] private bool isHidden;

        [Header("Zone")]
        [Tooltip("Zone to create on detonation")] 
        [SerializeField] private ProjectileInfo projectileInfo;

        [SerializeField] private LayerMask hiddenLayer;

        #endregion
        
        #region Proximity Perameter Getters

        public float GetRadius()
        {
            return detonationDetectionRadius;
        }

        public int GetNumOfShots()
        {
            return numberOfShots;
        }

        public bool GetIsHidden()
        {
            return isHidden;
        }

        public ProjectileInfo GetProjectile()
        {
            return projectileInfo;
        }

        public LayerMask GetHiddenLayer()
        {
            return hiddenLayer;
        }

        #endregion
    }
}