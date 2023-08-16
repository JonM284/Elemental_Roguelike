using Project.Scripts.Utils;
using Runtime.Status;
using UnityEngine;

namespace Runtime.Abilities
{
    [CreateAssetMenu(menuName = "Ability/Apply Status Ability")]
    public class ApplyStatusAbility: Ability
    {
        public Status.Status statusEffect;
        
        public override void SelectPosition(Vector3 _inputPosition)
        {
            
        }

        public override void SelectTarget(Transform _inputTransform)
        {
            m_targetTransform = _inputTransform;
        }

        public override void UseAbility(Vector3 _ownerUsePos)
        {
            m_targetTransform.TryGetComponent(out IEffectable effectable);
            if (!effectable.IsNull())
            {
                effectable.ApplyEffect(statusEffect);
            }
            base.UseAbility(_ownerUsePos);
        }
    }
}