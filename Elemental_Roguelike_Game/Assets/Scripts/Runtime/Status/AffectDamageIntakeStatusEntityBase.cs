using Data.Sides;
using Data.StatusDatas;
using Runtime.Character;

namespace Runtime.Status
{
    public class AffectDamageIntakeStatusEntityBase: StatusEntityBase
    {
        
        #region Accessors

        private DamageIntakeChangeStatusData damageIntakeChangeStatusData => statusData as DamageIntakeChangeStatusData;

        #endregion

        #region Status Inherited Methods
        
        public override void OnApply(CharacterBase characterBase)
        {
            base.OnApply(characterBase);
            currentOwner.characterLifeManager.SetDamageIntakeModifier(damageIntakeChangeStatusData.damageIntakeModifier);
        }

        public override void OnTick(CharacterSide obj) { }

        public override void OnEnd()
        {
            currentOwner.characterLifeManager.SetDamageIntakeModifier(1);
            base.OnEnd();
        }

        #endregion

        
    }
}