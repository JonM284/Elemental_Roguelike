using Project.Scripts.Utils;
using Runtime.Character;
using UnityEngine;

namespace Runtime.Perks
{
    
    [CreateAssetMenu(menuName = "Perks/Passive Radius Perk")]
    public class PassiveRadiusPerk: PerkBase
    {
        
        #region Public Fields

        [Range(0f,1f)]
        public float percentIncrease;

        #endregion

        #region PerkBase Inherited Methods

        public override void TriggerPerkEffect(CharacterBase _character)
        {
            if (_character.IsNull())
            {
                return;
            }

            var newAmount = (_character.characterClassManager.passiveRadius * percentIncrease) +
                            _character.characterClassManager.passiveRadius;
            _character.characterClassManager.UpdateCharacterPassiveRadius(newAmount);

        }

        #endregion

        
        
    }
}