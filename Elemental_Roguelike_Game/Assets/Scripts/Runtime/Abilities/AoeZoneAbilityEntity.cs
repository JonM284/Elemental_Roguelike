using Cysharp.Threading.Tasks;
using Data;
using Data.AbilityDatas;
using Project.Scripts.Utils;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace Runtime.Abilities
{
    public class AoeZoneAbilityEntity: AbilityEntityBase
    {

        #region Public Fields

        public AoeZoneData ZoneData;

        #endregion

        protected override UniTask PerformAbilityAction()
        {
            //Get zone projectile
            
            //Move Projectile
            
            //Wait for projectile end
            
            //Activate ZONE at end position
            var m_endPos = !targetTransform.IsNull() ? targetTransform.position.FlattenVector3Y() : targetPosition;
            //mAoeZoneData.PlayAt(m_endPos, currentOwner.transform);
            
            return base.PerformAbilityAction();
        }

        public override void OnAbilityUsed() { }
    }
}