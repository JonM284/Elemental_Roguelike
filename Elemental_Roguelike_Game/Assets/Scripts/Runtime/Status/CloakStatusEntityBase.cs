using Data.Sides;
using Runtime.Character;

namespace Runtime.Status
{
    public class CloakStatusEntityBase: StatusEntityBase
    {
        public override void OnApply(CharacterBase characterBase)
        {
            base.OnApply(characterBase);
            
            characterBase.SetTargetable(false);
        }

        public override void OnTick(CharacterSide obj) { }

        public override void OnEnd()
        {
            currentOwner.SetTargetable(true);
            
            base.OnEnd();
        }
        
    }
}