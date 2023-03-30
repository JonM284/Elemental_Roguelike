using System;
using Project.Scripts.Utils;
using UnityEngine;

namespace Runtime.Character
{
    public class CharacterAnimations: MonoBehaviour
    {
        #region Read-Only

        //Animator Params
        private readonly string isMovingParam = "isWalking";
        
        private readonly string isAttackingParam = "isAttacking";
        
        private readonly string damagedParam = "Damaged";
        
        private readonly string deathParam = "OnDeath";
        
        private readonly string useAbilityParam = "OnAbilityOneUse";

        #endregion

        #region Private Fields

        private CharacterMovement m_characterMovement;

        private Animator m_animator;

        #endregion

        #region Accessor

        private CharacterMovement characterMovement => CommonUtils.GetRequiredComponent(ref m_characterMovement, () =>
        {
            var cm = GetComponentInParent<CharacterMovement>();
            return cm;
        });

        private Animator animator => CommonUtils.GetRequiredComponent(ref m_animator, () =>
        {
            var a = GetComponent<Animator>();
            return a;
        });

        private bool isWalking => characterMovement != null && characterMovement.isMoving;

        #endregion

        #region Unity Events

        private void LateUpdate()
        {
            HandleAnimator();
        }

        #endregion

        #region Class Implementation

        public void AbilityAnim(bool _usingAbility)
        {
            animator.SetBool(useAbilityParam, _usingAbility);
        }

        public void AttackAnim(bool _isAttacking)
        {
            animator.SetBool(isAttackingParam, _isAttacking);
        }

        public void DamageAnim(bool _takingDamage)
        {
            animator.SetBool(damagedParam, _takingDamage);
        }

        private void HandleAnimator()
        {
            animator.SetBool(isMovingParam, isWalking);
        }

        #endregion


    }
}