using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Scripts.Utils;
using Runtime.VFX;
using UnityEngine;
using Utils;

namespace Runtime.Character
{
    public class CharacterAnimationEvents: MonoBehaviour
    {

        #region Private Fields

        private CharacterAnimations m_characterAnimations;

        private CharacterAbilityManager m_characterAbilityManager;

        private CharacterWeaponManager m_characterWeaponManager;

        private CharacterBase m_characterBase;
 
        private VFXPlayer m_savedPlayer;

        #endregion

        #region Serialized Fields

        [SerializeField] private List<Transform> vfxPositions = new List<Transform>();

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

        private CharacterBase characterBase => CommonUtils.GetRequiredComponent(ref m_characterBase, () =>
        {
            var cb = GetComponentInParent<CharacterBase>();
            return cb;
        });

        #endregion


        #region Class Implementation

        public void UseAbility()
        {
            characterAbilityManager.UseActiveAbility().Forget();
        }

        public void OnAbilityEnded()
        {
            if (!m_savedPlayer.IsNull())
            {
                Debug.Log("No saved player");
                m_savedPlayer = null;
            }

            characterAnimations.AbilityAnim(characterAbilityManager.GetPreviousActiveAbilityIndex(),false);
            characterBase.UseActionPoint();
        }

        public void OnDamageEnded()
        {
            characterAnimations.DamageAnim(false);
            characterBase.CheckDeath();
        }

        public void OnAttackEnded()
        {
            characterAnimations.AttackAnim(false);
        }

        public void Attack()
        {
            characterWeaponManager.UseWeapon();
        }

        public void PlayVFX()
        {
            var ability = characterAbilityManager.GetActiveAbility();

            if (ability.IsNull())
            {
                return;
            }
            
            var abilityVFX = ability.abilityData.abilityVFX;
            
            if (abilityVFX.IsNull())
            {
                return;
            }
            abilityVFX.PlayAt(transform.position, Quaternion.identity);
        }

        public void PlayVFXAtTransform(int _locIndex)
        {
            var ability = characterAbilityManager.GetActiveAbility();

            if (ability.IsNull() && m_savedPlayer.IsNull())
            {
                return;
            }

            if (!ability.IsNull())
            {
                if (!ability.abilityData.playVFXAtTransform)
                {
                    return;
                }    
            }
            

            VFXPlayer abilityVFX = null;
            if (!m_savedPlayer.IsNull())
            {
                abilityVFX = m_savedPlayer;
            }
            else
            {
                abilityVFX = ability.abilityData.abilityVFX;
                m_savedPlayer = abilityVFX;
            }
            
            if (abilityVFX.IsNull() || vfxPositions[_locIndex].IsNull())
            {
                return;
            }
            
            abilityVFX.PlayAt(vfxPositions[_locIndex].position, Quaternion.identity);
        }

        #endregion
        
        
    }
}