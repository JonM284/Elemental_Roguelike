using Project.Scripts.Utils;
using Runtime.Weapons;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utils;

namespace Runtime.Abilities
{
    [CreateAssetMenu(menuName = "Ability/ Projectile Ability")]
    public class ProjectileAbility : Ability
    {

        #region Public Fields

        public ProjectileBase projectilePrefab;

        #endregion

        #region Ability Inherited Methods

        public override void SelectPosition(Vector3 _inputPosition)
        {
            if (_inputPosition.IsNan())
            {
               return; 
            }
            
            m_targetPosition = _inputPosition;
            UseAbility();
        }

        public override void SelectTarget(Transform _inputTransform)
        {
            if (_inputTransform == null)
            {
                return; 
            }
            
            m_targetTransform = _inputTransform;
            UseAbility();
        }

        public override void UseAbility()
        {
            if (currentOwner == null)
            {
                return;
            }

            var m_endPos = m_targetTransform != null ? m_targetTransform.position : m_targetPosition;
            ProjectileUtils.PlayAt(projectilePrefab, currentOwner.transform.position, currentOwner.transform.forward, m_endPos);
            
        }

        #endregion
        
    }
}