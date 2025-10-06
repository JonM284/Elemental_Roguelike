using Data.Sides;
using Data.StatusDatas;
using Project.Scripts.Utils;
using Runtime.Character;
using UnityEngine;
using Utils;

namespace Runtime.Status
{
    public class LifeStealStatusEntityBase: StatusEntityBase
    {

        #region Accessors

        private LifestealStatusData lifeStealStatusData => statusData as LifestealStatusData;

        #endregion
        
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

        private void CharacterLifeManagerOnCharacterTookDamage(CharacterBase attacker, int _damageAsHealth)
        {
            if (attacker.IsNull() || currentOwner.IsNull() || attacker != currentOwner)
            {
                return;
            }

            if (attacker.ContainsStatus(this.statusData))
            {
                return;
            }
            
            attacker.OnHeal(Mathf.RoundToInt(
                Mathf.Abs(_damageAsHealth) * lifeStealStatusData.lifeStealPercentage),
                false);

            if (!lifeStealStatusData.statusOneTimeVFX.IsNull())
            {
                lifeStealStatusData.statusOneTimeVFX.PlayAt(currentOwner.transform.position, Quaternion.identity);
            }
        }

        #endregion

        #region Status Inherited Methods
        public override void OnTick(CharacterSide obj) { }

        #endregion


    }
}