using Data.Sides;
using Runtime.Character;

namespace Runtime.Status
{
    public class AbilityRangeBuffDebuffStatusEntityBase: StatusEntityBase
    {
        //NOTE: DEF USE
        //ToDo: need to implement

        public override void OnApply(CharacterBase characterBase)
        {
            base.OnApply(characterBase);
        }

        public override void OnTick(CharacterSide obj) { }

        public override void OnEnd()
        {
            base.OnEnd();
        }
    }
}