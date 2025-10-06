using Project.Scripts.Utils;
using Runtime.Character;
using UnityEngine;

namespace Runtime.Perks
{
    
    [CreateAssetMenu(menuName = "Perks/Stat Boost Perk")]
    public class StatBoostPerk: PerkBase
    {

        #region Public Fields

        public CharacterStatsEnum targetStat = CharacterStatsEnum.AGILITY;

        [Tooltip("Can be + or -, + = BUFF, - = DEBUFF")]
        public int amountToChangeBy; 
        
        [Tooltip("Does the stat continuously go up or down while the status is active?")]
        public bool isChangeByPercent;

        #endregion

        #region PerkBase Inherited Methods

        public override void TriggerPerkEffect(CharacterBase _character)
        {
            if (_character.IsNull())
            {
                return;
            }

            var newAmount = 0;
            if (isChangeByPercent)
            {
                switch (targetStat)
                {
                    case CharacterStatsEnum.AGILITY:
                        newAmount = (_character.characterClassManager.agilityScore * amountToChangeBy) + _character.characterClassManager.agilityScore;
                        break;
                    case CharacterStatsEnum.THROW:
                        newAmount = (_character.characterClassManager.throwingScore * amountToChangeBy) + _character.characterClassManager.throwingScore;
                        break;
                    case CharacterStatsEnum.TACKLE:
                        newAmount = (_character.characterClassManager.tacklingScore * amountToChangeBy) + _character.characterClassManager.tacklingScore;
                        break;
                }
            }
            else
            {
                switch (targetStat)
                {
                    case CharacterStatsEnum.AGILITY:
                        newAmount = _character.characterClassManager.agilityScore + amountToChangeBy;
                        break;
                    case CharacterStatsEnum.THROW:
                        newAmount = _character.characterClassManager.throwingScore + amountToChangeBy;
                        break;
                    case CharacterStatsEnum.TACKLE:
                        newAmount = _character.characterClassManager.tacklingScore + amountToChangeBy;
                        break;
                }
            }
            
            
            _character.characterClassManager.ChangeMaxScore(targetStat, newAmount);
        }

        #endregion

        
    }
}