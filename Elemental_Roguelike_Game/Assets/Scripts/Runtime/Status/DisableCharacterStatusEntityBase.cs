using Data.Sides;
using Runtime.Character;

namespace Runtime.Status
{
    public class DisableCharacterStatusEntityBase: StatusEntityBase
    {
        
        public override void OnApply(CharacterBase characterBase)
        {
            base.OnApply(characterBase);
            
            currentOwner.SetCharacterUsable(false);
            currentOwner.characterClassManager.SetAbleToReact(false);

            if (!currentOwner.characterBallManager.hasBall)
            {
                currentOwner.characterBallManager.KnockBallAway();
            }
        }

        public override void OnTick(CharacterSide obj) { }

        public override void OnEnd()
        {
            currentOwner.SetCharacterUsable(true);
            currentOwner.characterClassManager.SetAbleToReact(true);
            
            base.OnEnd();
        }
    }
}