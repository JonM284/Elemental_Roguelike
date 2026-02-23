using Data.Sides;
using Runtime.Character;

namespace Runtime.Status
{
    public class CleanseStatusEntityBase: StatusEntityBase
    {
        public override void OnApply(CharacterBase characterBase)
        {
            base.OnApply(characterBase);
            
            characterBase.RemoveAllStatuses();
        }

        public override void OnTick(CharacterSide characterSide) { }
    }
}