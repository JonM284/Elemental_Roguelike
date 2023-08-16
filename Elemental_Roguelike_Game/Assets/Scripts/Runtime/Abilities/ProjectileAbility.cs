using Data;
using Project.Scripts.Utils;
using Runtime.Weapons;
using UnityEngine;
using Utils;

namespace Runtime.Abilities
{
    [CreateAssetMenu(menuName = "Ability/Projectile Ability")]
    public class ProjectileAbility : Ability
    {

        #region Public Fields

        public ProjectileInfo projectileInfo;

        #endregion

        #region Ability Inherited Methods

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
            if (_inputTransform == null)
            {
                return; 
            }
            
            m_targetTransform = _inputTransform;
        }

        public override void UseAbility(Vector3 _ownerUsePos)
        {
            if (currentOwner == null)
            {
                return;
            }

            var m_endPos = m_targetTransform != null ? m_targetTransform.position : m_targetPosition;
            projectileInfo.PlayAt(currentOwner.transform ,_ownerUsePos, currentOwner.transform.forward, m_endPos);
            base.UseAbility(_ownerUsePos);
        }

        #endregion
        
    }
}