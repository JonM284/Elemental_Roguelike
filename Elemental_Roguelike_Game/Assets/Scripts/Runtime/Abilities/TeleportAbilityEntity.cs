using Cysharp.Threading.Tasks;
using Runtime.Character;

namespace Runtime.Abilities
{
    public class TeleportAbilityEntity: AbilityEntityBase
    {
        
        protected override async UniTask PerformAbilityAction()
        {
            currentOwner.TryGetComponent(out CharacterMovement characterMovement);
            if (characterMovement)
            {
                characterMovement.TeleportCharacter(targetPosition);
            } 
        }

        public override void OnAbilityUsed() { }
    }
}