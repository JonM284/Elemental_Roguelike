using Data.Sides;
using Project.Scripts.Utils;
using Runtime.Abilities;
using Runtime.Character;

namespace Runtime.Status
{
   public class BlindStatusEntity: StatusEntityBase
    {
        
        public override void OnApply(CharacterBase characterBase)
        {
            base.OnApply(characterBase);
            
            if (characterBase.IsNull())
            {
                return;
            }

            characterBase.characterAbilityManager.SetAbilityTypeAllowed(AbilityTargetType.SELF);
            characterBase.characterClassManager.SetAbleToReact(false);
        }

        public override void OnTick(CharacterSide characterSide) { }

        public override void OnEnd()
        {
            if (currentOwner.IsNull())
            {
                return;
            }

            currentOwner.characterAbilityManager.AllowAllAbilityActive();
            currentOwner.characterClassManager.SetAbleToReact(true);
            
            base.OnEnd();
        }
    }
}