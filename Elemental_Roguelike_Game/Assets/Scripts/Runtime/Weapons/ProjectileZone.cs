using Data;
using Project.Scripts.Utils;
using UnityEngine;
using Utils;

namespace Runtime.Weapons
{
    public class ProjectileZone: ProjectileBase
    {

        #region Serialized Fields

        [SerializeField] private ZoneInfo zoneInfo;

        #endregion

        #region ProjectileBase Inherited Methods

        protected override void OnEndMovement()
        {
            zoneInfo.PlayAt(transform.position.FlattenVector3Y(), m_user);
            base.OnEndMovement();
        }

        #endregion

    }
}