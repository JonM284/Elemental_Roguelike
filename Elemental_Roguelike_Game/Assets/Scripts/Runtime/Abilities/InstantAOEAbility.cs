using Data;
using Project.Scripts.Utils;
using UnityEngine;
using Utils;

namespace Runtime.Abilities
{
    [CreateAssetMenu(menuName = "Ability/InstantAOEAbility")]
    public class InstantAOEAbility: Ability
    {

        #region Public Fields

        public ZoneInfo m_zoneInfo;

        #endregion
        
        public override void SelectPosition(Vector3 _inputPosition)
        {
            if (_inputPosition.IsNan())
            {
                return;
            }

            m_targetPosition = _inputPosition;

        }

        public override void SelectTarget(Transform _inputTransform)
        {
            if (_inputTransform.IsNull())
            {
                return;
            }

            m_targetTransform = _inputTransform;
        }

        public override void UseAbility(Vector3 _ownerUsePos)
        {
            var m_endPos = m_targetTransform != null ? m_targetTransform.position.FlattenVector3Y() : m_targetPosition;
            m_zoneInfo.PlayAt(m_endPos, currentOwner.transform);
            base.UseAbility(_ownerUsePos);
        }
    }
}