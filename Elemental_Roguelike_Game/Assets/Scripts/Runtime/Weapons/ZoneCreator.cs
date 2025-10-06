using Data;
using Data.AbilityDatas;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace Runtime.Weapons
{
    public class ZoneCreator: MonoBehaviour
    {
        
        #region Serialized Fields

        [FormerlySerializedAs("zoneToCreate")] [SerializeField] private AoeZoneData aoeZoneToCreate;

        #endregion

        #region Class Implementation

        public void CreateZone()
        {
            //aoeZoneToCreate.PlayAt(transform.position, transform);
        }
        

        #endregion
        
        
    }
}