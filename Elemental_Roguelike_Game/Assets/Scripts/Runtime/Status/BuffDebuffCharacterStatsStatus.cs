using Project.Scripts.Utils;
using Runtime.Character;
using UnityEngine;

namespace Runtime.Status
{
    [CreateAssetMenu(menuName = "Status/Buff : Debuff Character Stats Status")]
    public class BuffDebuffCharacterStatsStatus: Status
    {
        #region Public Fields

        public CharacterStatsEnum targetStat = CharacterStatsEnum.AGILITY;

        [Tooltip("Can be + or -, + = BUFF, - = DEBUFF")]
        public int amountToChangeBy; 
        
        [Tooltip("Does the stat continuously go up or down while the status is active?")]
        public bool isChangeOverTime;

        #endregion

        #region Status Inherited Methods

        public override void TriggerStatusEffect(CharacterBase _character)
        {
            if (_character.IsNull())
            {
                return;
            }

            var newAmount = 0;
            if (isChangeOverTime)
            {
                switch (targetStat)
                {
                    case CharacterStatsEnum.AGILITY:
                        newAmount = _character.characterClassManager.currentMaxAgilityScore + amountToChangeBy;
                        break;
                    case CharacterStatsEnum.SHOOTING:
                        newAmount = _character.characterClassManager.currentMaxShootingScore + amountToChangeBy;
                        break;
                    case CharacterStatsEnum.TACKLE:
                        newAmount = _character.characterClassManager.currentMaxTacklingScore + amountToChangeBy;
                        _character.characterMovement.SetElementTackle(true);
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
                    case CharacterStatsEnum.SHOOTING:
                        newAmount = _character.characterClassManager.shootingScore + amountToChangeBy;
                        break;
                    case CharacterStatsEnum.TACKLE:
                        newAmount = _character.characterClassManager.tacklingScore + amountToChangeBy;
                        _character.characterMovement.SetElementTackle(true);
                        break;
                }
            }
            
            
            _character.characterClassManager.ChangeMaxScore(targetStat, newAmount);
        }

        public override void ResetStatusEffect(CharacterBase _character)
        {
            if (_character.IsNull())
            {
                return;
            }
            
            _character.characterClassManager.ResetMaxScores();
            _character.characterMovement.SetElementTackle(false);
        }
        
        #endregion
        
    }
}