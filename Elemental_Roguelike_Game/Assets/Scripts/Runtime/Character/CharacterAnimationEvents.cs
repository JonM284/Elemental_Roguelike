using Project.Scripts.Utils;
using UnityEngine;

namespace Runtime.Character
{
    public class CharacterAnimationEvents: MonoBehaviour
    {

        #region Private Fields

        private CharacterAnimations m_characterAnimations;

        private CharacterAbilityManager m_characterAbilityManager;

        private CharacterWeaponManager m_characterWeaponManager;

        #endregion
        
        #region Accessors
        
        private CharacterAnimations characterAnimations => CommonUtils.GetRequiredComponent(ref m_characterAnimations, () =>
        {
            var ca = GetComponent<CharacterAnimations>();
            return ca;
        });
        
        private CharacterAbilityManager characterAbilityManager => CommonUtils.GetRequiredComponent(ref m_characterAbilityManager, () =>
        {
            var cam = GetComponentInParent<CharacterAbilityManager>();
            return cam;
        });

        private CharacterWeaponManager characterWeaponManager => CommonUtils.GetRequiredComponent(
            ref m_characterWeaponManager,
            () =>
            {
                var cwm = GetComponentInParent<CharacterWeaponManager>();
                return cwm;
            });

        #endregion


        #region Class Implementation

        public void UseAbility()
        {
            characterAbilityManager.UseActiveAbility();
        }

        public void OnAbilityEnded()
        {
            characterAnimations.AbilityAnim(false);
        }

        public void OnDeathAnimationEnded()
        {
            
        }

        public void OnDamageEnded()
        {
            characterAnimations.DamageAnim(false);
        }

        public void OnAttackEnded()
        {
            characterAnimations.AttackAnim(false);
        }

        public void Attack()
        {
            characterWeaponManager.UseWeapon();
        }

        #endregion
        
        
    }
}