using Project.Scripts.Utils;
using Runtime.Character;
using Runtime.Damage;
using Runtime.Gameplay;
using UnityEngine;

namespace Runtime.Abilities
{
    [CreateAssetMenu(menuName = "Ability/Pull Ability")]
    public class PullAbility: Ability
    {

        #region Public Fields

        public bool isPush;

        public int pullForceCharacter;

        public int pullForceBall;

        public int damage;

        #endregion
        
        public override void SelectPosition(Vector3 _inputPosition)
        {
            if (_inputPosition.IsNan())
            {
                return; 
            }
            
            m_targetPosition = _inputPosition;
        }

        public override void SelectTarget(Transform _inputTransform)
        {
            if (_inputTransform == null)
            {
                return; 
            }
            
            m_targetTransform = _inputTransform;
        }

        public override void UseAbility(Vector3 _ownerUsePos)
        {
            if (currentOwner == null)
            {
                return;
            }

            currentOwner.TryGetComponent(out CharacterBase _ownerCharacter);

            var m_selectedPos = m_targetTransform != null ? m_targetTransform.position : m_targetPosition;
            var dirToTarget = m_selectedPos - _ownerUsePos;
            var dirToUser = _ownerUsePos - m_selectedPos;
            var m_endPos = dirToTarget.normalized * range;

            //Dir to user: Target Pos -> User Character
            //Dir to Target: User Character -> Target Pos
            var desiredDirection = isPush ? dirToTarget : dirToUser;

            RaycastHit[] hits = Physics.CapsuleCastAll(_ownerUsePos, m_endPos, 0.2f, dirToTarget, dirToTarget.magnitude);
            
            foreach (RaycastHit hit in hits)
            {
                
                if (damage != 0)
                {
                    hit.collider.TryGetComponent(out IDamageable _damageable);

                    if (damage > 0)
                    {
                        _damageable.OnDealDamage(currentOwner.transform, damage, false, null, currentOwner.transform, false);
                    }else if (damage < 0)
                    {
                        _damageable.OnHeal(damage, false);
                    }
                }
                
                //if they are running into an enemy character, make them stop at that character and perform melee
                if (hit.collider.TryGetComponent(out CharacterBase otherCharacter))
                {
                    if (otherCharacter == _ownerCharacter)
                    {
                        continue;
                    }
                    
                    otherCharacter.characterMovement.ApplyKnockback(pullForceCharacter, desiredDirection, 0.5f);
                }

                if (hit.collider.TryGetComponent(out BallBehavior ballBehavior))
                {
                    if (ballBehavior.isControlled)
                    {
                        continue;
                    }
                    
                    ballBehavior.ThrowBall(desiredDirection, pullForceBall, true, null, _ownerCharacter.characterClassManager.GetRandomPassingStat());
                }

            }
        }

    }
}