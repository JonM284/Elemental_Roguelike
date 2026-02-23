using Data.Sides;
using Data.StatusDatas;
using Project.Scripts.Utils;
using Runtime.Character;

namespace Runtime.Status
{
    public class AffectHealthStatusEntityBase: StatusEntityBase
    {

        #region Accessors
        
        private AffectHealthStatusData affectHealthOverTimeStatusData => statusData as AffectHealthStatusData;
        
        #endregion

        #region Status Inherited Methods

        public override void OnApply(CharacterBase characterBase)
        {
            base.OnApply(characterBase);

            if (!affectHealthOverTimeStatusData.isOnApply)
            {
                return;
            }
            
            if (affectHealthOverTimeStatusData.amountChange > 0)
            {
                DealDamage();
            }
            else
            {
                HealCharacter();
            }        }

        public override void OnTick(CharacterSide characterSide)
        {
            if (currentOwner.IsNull() || !affectHealthOverTimeStatusData.isOverTime)
            {
                return;
            }

            if (affectHealthOverTimeStatusData.amountChange > 0)
            {
                DealDamage();
            }
            else
            {
                HealCharacter();
            }
        }

        public override void OnEnd()
        {
            base.OnEnd();
        }
        
        #endregion
        
        #region Class Implementation


        private void DealDamage()
        {
            currentOwner.OnDealDamage(currentOwner.transform, affectHealthOverTimeStatusData.amountChange,
                affectHealthOverTimeStatusData.isArmorPiercing, null,
                currentOwner.transform ,false);
            
            if (!currentOwner.characterBallManager.hasBall)
            {
                currentOwner.characterBallManager.KnockBallAway();
            }
        }

        private void HealCharacter()
        {
            currentOwner.OnHeal(affectHealthOverTimeStatusData.amountChange, affectHealthOverTimeStatusData.isArmorPiercing);
        }

        #endregion
        
    }
}