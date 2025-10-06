using Data.Sides;
using Runtime.Character;

namespace Runtime.Status
{
    public class SilenceStatusEntityBase: StatusEntityBase
    {

        public override void OnApply(CharacterBase characterBase)
        {
            base.OnApply(characterBase);
            
            currentOwner.SetCharacterCanUseAbilities(false);
        }

        public override void OnTick(CharacterSide obj) { }

        public override void OnEnd()
        {
            currentOwner.SetCharacterCanUseAbilities(true);
            
            base.OnEnd();
        }
    }
}