using Project.Scripts.Utils;
using Runtime.Abilities;
using Runtime.Character;
using UnityEngine;

namespace Runtime.Perks
{
    [CreateAssetMenu(menuName = "Perks/Ability Damage Perk")]
    public class AbilityDamagePerk: PerkBase
    {
        #region Public Fields

        public AbilityType abilityType;

        public int damageToAdd;

        #endregion

        #region PerkBase Inherited Methods

        public override void TriggerPerkEffect(CharacterBase _character)
        {
            if (_character.IsNull())
            {
                return;
            }
            
            
        }

        #endregion
    }
}