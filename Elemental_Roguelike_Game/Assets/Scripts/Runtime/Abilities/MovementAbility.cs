using Runtime.Character;
using UnityEngine;

namespace Runtime.Abilities
{
    [CreateAssetMenu(menuName = "Ability/Movement Upgrade Ability")]
    public class MovementAbility: Ability
    {
        public override void SelectPosition(Vector3 _inputPosition)
        {
            throw new System.NotImplementedException();
        }

        public override void SelectTarget(Transform _inputTransform)
        {
            throw new System.NotImplementedException();
        }

        public override void UseAbility(Vector3 _ownerUsePos)
        {
            currentOwner.TryGetComponent(out CharacterMovement characterMovement);
            if (characterMovement)
            {
                characterMovement.ChangeMovementRange(characterMovement.battleMoveDistance + range);
            }
            base.UseAbility(_ownerUsePos);
        }
    }
}