using Data;
using UnityEngine;
using Utils;

namespace Runtime.Weapons
{
    public class ZoneCreator: MonoBehaviour
    {
        
        #region Serialized Fields

        [SerializeField] private ZoneInfo zoneToCreate;

        #endregion

        #region Class Implementation

        public void CreateZone()
        {
            zoneToCreate.PlayAt(transform.position, transform);
        }
        

        #endregion
        
        
    }
}