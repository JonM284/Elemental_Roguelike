using System;
using Project.Scripts.Utils;
using Runtime.Character;
using UnityEngine;
using Utils;

namespace Runtime.Status
{
    [CreateAssetMenu(menuName = "Status/Life Steal Status")]
    public class LifeStealStatus: Status
    {
        #region Unity Events

        private void OnEnable()
        {
            CharacterLifeManager.CharacterTookDamage += CharacterLifeManagerOnCharacterTookDamage;
        }

        private void OnDisable()
        {
            CharacterLifeManager.CharacterTookDamage -= CharacterLifeManagerOnCharacterTookDamage;
        }

        #endregion

        #region Class Implementation

        private void CharacterLifeManagerOnCharacterTookDamage(CharacterBase _Attacker, int _damageAsHealth)
        {
            if (_Attacker.IsNull())
            {
                return;
            }

            if (_Attacker.appliedStatus.status != this)
            {
                return;
            }

            var fixedAmount = _damageAsHealth * -1;
            
            _Attacker.OnHeal(fixedAmount, false);

            if (!statusOneTimeVFX.IsNull())
            {
                statusOneTimeVFX.PlayAt(_Attacker.transform.position, Quaternion.identity);
            }
        }

        #endregion

        #region Status Inherited Methods

        public override void TriggerStatusEffect(CharacterBase _character)
        {
            //Doesn't get triggered at the start of every round
        }

        public override void ResetStatusEffect(CharacterBase _character)
        {
            //Doesn't Do anything, user just stops receiving the health
        }

        #endregion

        
    }
}