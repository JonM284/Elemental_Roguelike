using Project.Scripts.Utils;
using Runtime.Character;
using UnityEngine;

namespace Runtime.Perks
{
    [CreateAssetMenu(menuName = "Perks/Damage Boost Perk")]
    public class DamageBoostPerk: PerkBase
    {

        #region Public Fields

        

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