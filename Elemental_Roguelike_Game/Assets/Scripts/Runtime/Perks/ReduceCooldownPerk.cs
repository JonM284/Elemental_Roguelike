using Project.Scripts.Utils;
using Runtime.Character;
using UnityEngine;

namespace Runtime.Perks
{
    
    [CreateAssetMenu(menuName = "Perks/Reduce Cooldown Perk")]
    public class ReduceCooldownPerk: PerkBase
    {
        
        #region Public Fields

        [Range(0f,1f)]
        public float percentDecrease;

        #endregion

        #region PerkBase Inherited Methods

        public override void TriggerPerkEffect(CharacterBase _character)
        {
            if (_character.IsNull())
            {
                return;
            }

            _character.characterAbilityManager.ChangeAbilityCooldown(percentDecrease);
        }

        #endregion
        
    }
}