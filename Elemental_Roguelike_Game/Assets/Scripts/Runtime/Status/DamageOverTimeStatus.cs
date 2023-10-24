using Project.Scripts.Utils;
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

        public override void TriggerStatusEffect(CharacterBase _character)
        {
            if (_character.IsNull())
            {
                return;
            }
            
            _character.OnDealDamage(_character.transform, damageOverTime, isArmorPiercing, abilityElement, _character.transform ,false);
            
            if (!_character.heldBall.IsNull())
            {
                _character.KnockBallAway();
            }
        }

        public override void ResetStatusEffect(CharacterBase _character)
        {
            
        }

        #endregion
        
        
    }
}