using Data.AbilityDatas;
using Runtime.Character;
using Runtime.Damage;
using Runtime.Gameplay;
using UnityEngine;

namespace Runtime.Abilities
{
    public class PullAbilityEntity: AbilityEntityBase
    {
        
        protected RaycastHit[] hits = new RaycastHit[15];
        protected int hitsAmount;
        
        private MeleeAbilityData meleeAbilityData => abilityData as MeleeAbilityData;

        public override void OnAbilityUsed()
        {
            var ownerPos = currentOwner.transform.position;
            var m_selectedPos = targetTransform != null ? targetTransform.position : targetPosition;
            var dirToTarget = m_selectedPos - ownerPos;
            var dirToUser = ownerPos - m_selectedPos;
            var m_endPos = dirToTarget.normalized * currentRange;

            //Dir to user: Target Pos -> User Character
            //Dir to Target: User Character -> Target Pos
            var desiredDirection = meleeAbilityData.isPush ? dirToTarget : dirToUser;

            hitsAmount = Physics.CapsuleCastNonAlloc(ownerPos, m_endPos, 0.2f, dirToTarget, hits, dirToTarget.magnitude);

            if (hitsAmount == 0)
            {
                return;
            }

            for (int i = 0; i < hitsAmount; i++)
            {
                if (currentDamage != 0)
                {
                    hits[i].collider.TryGetComponent(out IDamageable _damageable);

                    if (currentDamage > 0)
                    {
                        _damageable.OnDealDamage(currentOwner.transform, Mathf.CeilToInt(currentDamage),
                            false, null, currentOwner.transform, false);
                    }else
                    {
                        _damageable.OnHeal(Mathf.CeilToInt(currentDamage), false);
                    }
                }
                
                //if they are running into an enemy character, make them stop at that character and perform melee
                if (hits[i].collider.TryGetComponent(out CharacterBase otherCharacter))
                {
                    if (otherCharacter == currentOwner)
                    {
                        continue;
                    }
                    
                    otherCharacter.characterMovement.ApplyKnockback(currentKnockback, desiredDirection, 0.5f);
                }

                if (hits[i].collider.TryGetComponent(out BallBehavior ballBehavior))
                {
                    if (ballBehavior.isControlled)
                    {
                        continue;
                    }
                    
                    ballBehavior.ThrowBall(desiredDirection, currentKnockback, 
                        true, null, currentOwner.characterClassManager.GetRandomPassingStat());
                }
            }
            
        }
    }
}