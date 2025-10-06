using Data;
using Data.AbilityDatas;
using Project.Scripts.Utils;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace Runtime.Weapons
{
    public class ProjectileZone: ProjectileBase
    {

        #region Serialized Fields

        [FormerlySerializedAs("zoneInfo")] [SerializeField] private AoeZoneData aoeZoneData;

        #endregion

        #region ProjectileBase Inherited Methods

        protected override void OnEndMovement()
        {
            //aoeZoneData.PlayAt(transform.position.FlattenVector3Y(), m_user);
            base.OnEndMovement();
        }

        #endregion

    }
}