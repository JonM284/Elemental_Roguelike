using Project.Scripts.Utils;
using Runtime.Character;
using UnityEngine;

namespace Runtime.Status
{
    [CreateAssetMenu(menuName = "Status/Increase Speed Over Time Status")]
    public class IncreaseSpeedOverTime: Status
    {

        #region Public Fields

        public float amountToAdd = 0.5f;

        #endregion

        #region Status Inherited Methods

        public override void TriggerStatusEffect(CharacterBase _character)
        {
            if (_character.IsNull())
            {
                return;
            }

            var newRange = _character.characterMovement.currentMoveDistance + amountToAdd;
            _character.characterMovement.ChangeMovementRange(newRange);
        }

        public override void ResetStatusEffect(CharacterBase _character)
        {
            _character.characterMovement.ResetOriginalMoveDistance();
        }

        #endregion
        
        
    }
}