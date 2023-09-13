using System.Collections;
using Project.Scripts.Utils;
using Runtime.Character;
using UnityEngine;
using Utils;

namespace Runtime.Abilities
{
    [CreateAssetMenu(menuName = "Ability/Teleport Ability")]
    public class TeleportAbility: Ability
    {
        
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
        }

        public override void UseAbility(Vector3 _ownerUsePos)
        {
            currentOwner.TryGetComponent(out CharacterMovement characterMovement);
            if (characterMovement)
            {
                characterMovement.TeleportCharacter(m_targetPosition);
            }  
            base.UseAbility(_ownerUsePos);
        }
    }
}