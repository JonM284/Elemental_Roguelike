using Project.Scripts.Utils;
using Runtime.Character;
using UnityEngine;

namespace Runtime.Perks
{
    
    [CreateAssetMenu(menuName = "Perks/Shield Perk")]
    public class ShieldPerk: PerkBase
    {
        
        #region Public Fields

        public int directAmountChange;

        public float percentChange;

        public bool isPercentChange;

        #endregion

        #region PerkBase Inherited Methods

        public override void TriggerPerkEffect(CharacterBase _character)
        {
            if (_character.IsNull())
            {
                return;
            }

            int _newAmount = 0;

            if (isPercentChange)
            {
                var percentageAdd = Mathf.CeilToInt(_character.characterLifeManager.maxShieldPoints * percentChange);
                _newAmount = _character.characterLifeManager.maxShieldPoints + percentageAdd;
            }
            else
            {
                _newAmount = _character.characterLifeManager.maxShieldPoints + directAmountChange;
            }
                
            
            _character.characterLifeManager.ChangeMaxShield(_newAmount);
            
        }

        #endregion

        
        
    }
}