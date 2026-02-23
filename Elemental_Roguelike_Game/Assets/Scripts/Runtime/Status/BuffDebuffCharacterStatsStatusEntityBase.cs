using Data.Sides;
using Data.StatusDatas;
using Project.Scripts.Utils;
using Runtime.Character;
using UnityEngine;

namespace Runtime.Status
{
    [CreateAssetMenu(menuName = "Status/Buff : Debuff Character Stats Status")]
    public class BuffDebuffCharacterStatsStatusEntityBase: StatusEntityBase
    {
        #region Accessor

        private AffectCharacterStatsData affectCharacterStatsData => statusData as AffectCharacterStatsData;

        #endregion

        #region Status Inherited Methods

        public override void OnApply(CharacterBase characterBase)
        {
            base.OnApply(characterBase);
            
            ChangeStat(affectCharacterStatsData.amountToChangeBy, affectCharacterStatsData.isChangeOverTime);
        }

        public override void OnTick(CharacterSide characterSide)
        {
            if (!affectCharacterStatsData.isChangeOverTime)
            {
                return;
            }
            
            ChangeStat(affectCharacterStatsData.amountToChangeBy, affectCharacterStatsData.isChangeOverTime);
        }

        public override void OnEnd()
        {
            currentOwner.characterClassManager.ResetMaxScores();
            
            base.OnEnd();
        }
        
        #endregion

        #region Class Implementation

        private void ChangeStat(int amountToChangeBy, bool isChangeOverTime)
        {
            var newAmount = 0;
            
            switch (affectCharacterStatsData.targetStat)
            {
                case CharacterStatsEnum.AGILITY:
                    newAmount = isChangeOverTime ? 
                        currentOwner.characterClassManager.currentMaxAgilityScore 
                        : currentOwner.characterClassManager.agilityScore + amountToChangeBy;
                    break;
                case CharacterStatsEnum.THROW:
                    newAmount = isChangeOverTime ?
                        currentOwner.characterClassManager.currentMaxThrowingScore 
                        :currentOwner.characterClassManager.throwingScore  + amountToChangeBy;
                    break;
                case CharacterStatsEnum.TACKLE:
                    newAmount = isChangeOverTime ?
                        currentOwner.characterClassManager.currentMaxTacklingScore
                        : currentOwner.characterClassManager.tacklingScore + amountToChangeBy;
                    break;
            }
            
            
            currentOwner.characterClassManager.ChangeMaxScore(affectCharacterStatsData.targetStat , newAmount);
        }

        #endregion
    }
}