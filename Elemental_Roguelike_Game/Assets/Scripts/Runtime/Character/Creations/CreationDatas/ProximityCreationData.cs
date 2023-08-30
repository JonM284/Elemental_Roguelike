using Data;
using UnityEngine;

namespace Runtime.Character.Creations.CreationDatas
{
    [CreateAssetMenu(menuName = "Creation/Proximity Zone")]
    public class ProximityCreationData: CreationData
    {

        #region Serialized Fields

        [Header("Zone")]
        [Tooltip("Zone to create on detonation")] 
        [SerializeField] private ZoneInfo zoneInfo;
        
        #endregion

        #region Proximity Perameter Getters

        public ZoneInfo GetZoneInfo()
        {
            return zoneInfo;
        }

        #endregion


    }
}