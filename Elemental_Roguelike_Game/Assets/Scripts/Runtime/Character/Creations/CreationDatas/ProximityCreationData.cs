using Data;
using UnityEngine;

namespace Runtime.Character.Creations.CreationDatas
{
    [CreateAssetMenu(menuName = "Creation/Proximity Zone")]
    public class ProximityCreationData: CreationData
    {

        #region Serialized Fields

        [Header("Proximity")]
        [Tooltip("Radius around creation to perform action. AKA, something moves into range, perform action.")]
        [SerializeField] private float detonationDetectionRadius;

        [Header("Hidden Proximity")]
        [Tooltip("Is this creation hidden from the other team until walking into discovery range?")]
        [SerializeField] private bool isHidden;
        
        [Header("Zone")]
        [Tooltip("Zone to create on detonation")] 
        [SerializeField] private ZoneInfo zoneInfo;

        [SerializeField] private LayerMask hiddenLayer;

        #endregion

        #region Proximity Perameter Getters

        public float GetRadius()
        {
            return detonationDetectionRadius;
        }

        public bool GetIsHidden()
        {
            return isHidden;
        }

        public ZoneInfo GetZoneInfo()
        {
            return zoneInfo;
        }

        public LayerMask GetHiddenLayer()
        {
            return hiddenLayer;
        }

        #endregion


    }
}