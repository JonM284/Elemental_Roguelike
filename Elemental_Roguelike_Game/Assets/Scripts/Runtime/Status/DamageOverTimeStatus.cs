using Runtime.Character;
using UnityEngine;

namespace Runtime.Status
{
    [CreateAssetMenu(menuName = "Status/Damage Over Time Status")]
    public class DamageOverTimeStatus: Status
    {

        #region Public Fields
        [Space(20)]
        [Header("Damage Stats")]

        public int damageOverTime;

        public bool isArmorPiercing;

        #endregion

        #region Status Inherited Methods

        public override void TriggerStatusEffect(CharacterBase affectedCharacter)
        {
            if (affectedCharacter == null)
            {
                return;
            }
            
            affectedCharacter.OnDealDamage(affectedCharacter.transform, damageOverTime, isArmorPiercing, abilityElement, affectedCharacter.transform ,false);
        }

        public override void ResetStatusEffect(CharacterBase _character)
        {
            
        }

        #endregion
        
        
    }
}