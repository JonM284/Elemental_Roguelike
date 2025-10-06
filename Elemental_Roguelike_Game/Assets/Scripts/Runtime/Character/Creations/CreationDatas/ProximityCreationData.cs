using Data;
using Data.AbilityDatas;
using UnityEngine;
using UnityEngine.Serialization;

namespace Runtime.Character.Creations.CreationDatas
{
    [CreateAssetMenu(menuName = "Creation/Proximity Zone")]
    public class ProximityCreationData: CreationData
    {

        #region Serialized Fields

        [FormerlySerializedAs("zoneInfo")]
        [Header("Zone")]
        [Tooltip("Zone to create on detonation")] 
        [SerializeField] private AoeZoneData aoeZoneData;
        
        #endregion

        #region Proximity Perameter Getters

        public AoeZoneData GetZoneInfo()
        {
            return aoeZoneData;
        }

        #endregion


    }
}