using Data.Sides;
using Data.StatusDatas;
using Runtime.Character;
using UnityEngine;

namespace Runtime.Status
{
    public class GrowStatusEntityBase: StatusEntityBase
    {

        #region Accessors

        private AffectCharacterSizeStatusBase affectCharacterSizeStatusBase => statusData as AffectCharacterSizeStatusBase;

        #endregion

        #region Status Base Inherited Methods

        public override void OnApply(CharacterBase characterBase)
        {
            base.OnApply(characterBase);
            
            currentOwner.transform.localScale = Vector3.one * affectCharacterSizeStatusBase.m_increasePercentage;
        }

        public override void OnTick(CharacterSide characterSide) { }

        public override void OnEnd()
        {
            currentOwner.transform.localScale = Vector3.one;

            base.OnEnd();
        }

        #endregion

       
    }
}