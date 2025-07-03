using Project.Scripts.Utils;
using Runtime.Character;
using UnityEngine;

namespace Runtime.Status
{
    [CreateAssetMenu(menuName = "Status/Root Status")]
    public class RootStatus: Status
    {
        public override void TriggerStatusEffect(CharacterBase _character)
        {
            if (_character.IsNull())
            {
                return;
            }
            
            _character.characterMovement.ChangeMovementRange(0);
            _character.characterClassManager.SetAbleToReact(false);
            
            if (!_character.characterBallManager.hasBall)
            {
                _character.characterBallManager.KnockBallAway();
            }
        }

        public override void ResetStatusEffect(CharacterBase _character)
        {
            if (_character.IsNull())
            {
                return;
            }
            
            _character.characterMovement.ResetOriginalMoveDistance();
            _character.characterClassManager.SetAbleToReact(true);
        }
    }
}