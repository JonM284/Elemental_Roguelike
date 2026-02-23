using Data.Sides;
using Runtime.Character;

namespace Runtime.Status
{
    public class RootStatusEntityBase: StatusEntityBase
    {
        
        public override void OnApply(CharacterBase characterBase)
        {
            base.OnApply(characterBase);
            
            currentOwner.characterMovement.ChangeMovementRange(0);
            currentOwner.characterClassManager.SetAbleToReact(false);
            
            if (!currentOwner.characterBallManager.hasBall)
            {
                currentOwner.characterBallManager.KnockBallAway();
            }
        }

        public override void OnTick(CharacterSide characterSide) { }

        public override void OnEnd()
        {
            currentOwner.characterMovement.ResetOriginalMoveDistance();
            currentOwner.characterClassManager.SetAbleToReact(true);
            
            base.OnEnd();
        }
    }
}