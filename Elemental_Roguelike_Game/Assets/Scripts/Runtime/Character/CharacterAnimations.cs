using System.Collections.Generic;
using Project.Scripts.Utils;
using Runtime.Abilities;
using Runtime.GameControllers;
using UnityEngine;
using UnityEngine.Rendering;

namespace Runtime.Character
{
    public class CharacterAnimations: MonoBehaviour
    {
        #region Read-Only

        //Animator Params
        private readonly string isMovingParam = "isWalking";
        
        private readonly string isAttackingParam = "isAttacking";
        
        private readonly string damagedParam = "Damaged";
        
        private readonly string useFirstAbilityParam = "OnAbilityOneUse";
        
        private readonly string useSecondAbilityParam = "OnAbilityTwoUse";
        
        private readonly string abilityOneClipName = "DefaultAbility1";
        
        private readonly string abilityTwoClipName = "DefaultAbility2";
        
        private readonly string DefaultAttack = "DefaultAttack";
        
        private readonly string DefaultDamaged = "DefaultDamaged";

        private readonly string DefaultDeath = "DefaultDeath";

        private readonly string DefaultIdle = "DefaultIdle";

        private readonly string DefaultWalk = "DefaultWalk";
        
        #endregion

        #region Serialized Fields

        [SerializeField]
        private bool m_isEnemy;

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
        
        private AnimatorOverrideController originalAnimOverrideController => animator.runtimeAnimatorController as AnimatorOverrideController;

        private AnimatorOverrideController currentOverrideController => animator.runtimeAnimatorController as AnimatorOverrideController;

        private bool isWalking => characterMovement != null && characterMovement.isMoving && !characterMovement.isPaused;

        #endregion

        #region Unity Events

        private void LateUpdate()
        {
            HandleAnimator();
        }

        #endregion

        #region Class Implementation

        public void InitializeAnimations(List<string> _abilities, string _classType, string _elementType)
        {
            if (_abilities.Count == 0)
            {
                return;
            }

            List<Ability> abilities = new List<Ability>();
            
            _abilities.ForEach(a =>
            {
                var locatedAbility = AbilityController.Instance.GetAbility(_elementType, _classType, a);
                
                abilities.Add(locatedAbility);
                
            });

            if (abilities.Count == 0 || m_isEnemy)
            {
                return;
            }
            
            //Create new overrides for abilities
            var newAnimator = new AnimatorOverrideController(animator.runtimeAnimatorController);

            var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>(newAnimator.overridesCount);
            
            originalAnimOverrideController.GetOverrides(overrides);

            for(int i = 0; i < overrides.Count; i++)
            {
                if (overrides[i].Key.name == abilityOneClipName)
                {
                    if (abilities[0].abilityAnimationOverride.IsNull())
                    {
                        return;
                    }
                    var newValuePair = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, abilities[0].abilityAnimationOverride);
                    overrides[i] = newValuePair;

                }else if (overrides[i].Key.name == abilityTwoClipName)
                {
                    if (abilities[1].abilityAnimationOverride.IsNull())
                    {
                        return;
                    }
                    var newValuePair = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, abilities[1].abilityAnimationOverride);
                    overrides[i] = newValuePair;
                }
            }

            newAnimator.ApplyOverrides(overrides);
            newAnimator.name = "NewOverride";
            
            animator.runtimeAnimatorController = newAnimator;
            
            animator.Play("Idle");

        }

        /// <summary>
        /// Set Correct ability to play animation.
        /// </summary>
        /// <param name="_abilityIndex">Index of ability, 0 or 1</param>
        /// <param name="_usingAbility">Start / Stop animation</param>
        public void AbilityAnim(int _abilityIndex, bool _usingAbility)
        {
            var abilityUsedParam = _abilityIndex == 0 ? useFirstAbilityParam : useSecondAbilityParam;
            animator.SetBool(abilityUsedParam, _usingAbility);
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