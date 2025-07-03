using Project.Scripts.Utils;
using Runtime.Character;
using UnityEngine;

namespace Runtime.Status
{
    [CreateAssetMenu(menuName = "Status/Stun Status")]
    public class StunStatus: Status
    {
        public override void TriggerStatusEffect(CharacterBase _character)
        {
            if (_character.IsNull())
            {
                return;
            }
            
            _character.SetCharacterUsable(false);
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
            
            _character.SetCharacterUsable(true);
            _character.characterClassManager.SetAbleToReact(true);
        }
    }
}