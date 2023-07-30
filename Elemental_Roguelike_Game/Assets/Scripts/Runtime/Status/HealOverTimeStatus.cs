using Runtime.Character;
using UnityEngine;

namespace Runtime.Status
{
    [CreateAssetMenu(menuName = "Status/Heal Over Time Status")]
    public class HealOverTimeStatus: Status
    {

        #region Public Fields

        public bool isHealArmor;

        public int healOverTime;
        
        #endregion
        
        public override void TriggerStatusEffect(CharacterBase _character)
        {
            if (_character == null)
            {
                return;
            }
            
            _character.OnHeal(healOverTime, isHealArmor);
        }

        public override void ResetStatusEffect(CharacterBase _character)
        {
            
        }
    }
}