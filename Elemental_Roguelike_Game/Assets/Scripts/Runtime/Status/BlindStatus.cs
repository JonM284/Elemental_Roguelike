using Project.Scripts.Utils;
using Runtime.Abilities;
using Runtime.Character;
using UnityEngine;

namespace Runtime.Status
{
    [CreateAssetMenu(menuName = "Status/Blind Status")]
    public class BlindStatus: Status
    {
        public override void TriggerStatusEffect(CharacterBase _character)
        {
            if (_character.IsNull())
            {
                return;
            }

            _character.characterAbilityManager.SetAbilityTypeAllowed(AbilityTargetType.SELF);
        }

        public override void ResetStatusEffect(CharacterBase _character)
        {
            if (_character.IsNull())
            {
                return;
            }

            _character.characterAbilityManager.AllowAllAbilityActive();
        }
    }
}