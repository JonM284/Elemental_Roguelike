using Project.Scripts.Utils;
using Runtime.Character;
using UnityEngine;

namespace Runtime.Status
{
    [CreateAssetMenu(menuName = "Status/Slow Status")]
    public class SlowStatus: Status
    {

        #region Public Fields

        [Range(0f,1f)]
        public float slowPercentage;

        #endregion
        
        public override void TriggerStatusEffect(CharacterBase _character)
        {
            if (_character.IsNull())
            {
                return;
            }

            var amountToChange = _character.characterMovement.battleMoveDistance * slowPercentage;
            _character.characterMovement.ChangeMovementRange(amountToChange);
            
        }

        public override void ResetStatusEffect(CharacterBase _character)
        {
            _character.characterMovement.ResetOriginalMoveDistance();
        }
    }
}