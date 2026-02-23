using Data.Sides;
using Data.StatusDatas;
using Project.Scripts.Utils;
using Runtime.Character;
using UnityEngine;

namespace Runtime.Status
{
    [CreateAssetMenu(menuName = "Status/Slow Status")]
    public class MoveDistChangeStatusEntityBase: StatusEntityBase
    {

        #region Accessors

        protected SpeedChangeStatusData speedChangeStatusData => statusData as SpeedChangeStatusData;

        #endregion
        
        public override void OnApply(CharacterBase characterBase)
        {
            base.OnApply(characterBase);

            var amountToChange = characterBase.characterMovement.currentMoveDistance * speedChangeStatusData.moveDistChangeModifier;
            characterBase.characterMovement.ChangeMovementRange(amountToChange);
        }

        public override void OnTick(CharacterSide characterSide) { }

        public override void OnEnd()
        {
            currentOwner.characterMovement.ResetOriginalMoveDistance();
        }
    }
}