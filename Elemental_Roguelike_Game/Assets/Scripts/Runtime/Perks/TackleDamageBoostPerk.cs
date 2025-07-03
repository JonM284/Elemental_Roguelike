using Project.Scripts.Utils;
using Runtime.Character;
using UnityEngine;

namespace Runtime.Perks
{
    
    [CreateAssetMenu(menuName = "Perks/Tackle Damage Perk")]
    public class TackleDamageBoostPerk: PerkBase
    {
        
        #region Public Fields

        public int amountChange;

        public float percentChange;

        public bool isUsePercent;

        #endregion

        #region PerkBase Inherited Methods

        public override void TriggerPerkEffect(CharacterBase _character)
        {
            if (_character.IsNull())
            {
                return;
            }

            /*if (isUsePercent)
            {
                var _amountChange = Mathf.RoundToInt(_character.characterMovement.tackleDamage * percentChange);
                _character.characterMovement.UpdateTackleDamage(_character.characterMovement.tackleDamage + _amountChange);
                return;
            }
            
            _character.characterMovement.UpdateTackleDamage(_character.characterMovement.tackleDamage + amountChange);*/
            
        }

        #endregion
        
    }
}