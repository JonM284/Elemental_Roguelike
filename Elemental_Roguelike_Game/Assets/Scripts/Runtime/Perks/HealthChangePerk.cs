using Project.Scripts.Utils;
using Runtime.Character;
using UnityEngine;

namespace Runtime.Perks
{
    
    [CreateAssetMenu(menuName = "Perks/Health Boost Perk")]
    public class HealthChangePerk: PerkBase
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
                var percentageAdd = Mathf.CeilToInt(_character.characterLifeManager.maxHealthPoints * percentChange);
                _newAmount = _character.characterLifeManager.maxHealthPoints + percentageAdd;
            }
            else
            {
                _newAmount = _character.characterLifeManager.maxHealthPoints + directAmountChange;
            }
                
            
            _character.characterLifeManager.ChangeMaxShield(_newAmount);
            
        }

        #endregion
        
    }
}