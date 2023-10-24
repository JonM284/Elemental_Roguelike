using Data;
using Project.Scripts.Utils;
using Runtime.Character;
using UnityEngine;
using Utils;

namespace Runtime.Abilities
{
    [CreateAssetMenu(menuName = "Ability/Dash Ability")]
    public class DashAbility: Ability
    {
            //Dash is essentially a teleport with a projectile mixed    
            
            #region Public Fields

            public ProjectileInfo projectileInfo;

            #endregion

            #region Ability Inherited Methods

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
                if (_inputTransform.IsNull())
                {
                    return;
                }
            }
            
            
            public override void UseAbility(Vector3 _ownerUsePos)
            {
                currentOwner.TryGetComponent(out CharacterMovement characterMovement);
                
                if (characterMovement)
                {
                    characterMovement.TeleportCharacter(m_targetPosition);
                }

                projectileInfo.PlayAt(currentOwner.transform ,_ownerUsePos, currentOwner.transform.forward, m_targetPosition);
                
                base.UseAbility(_ownerUsePos);
            }

            #endregion

            
            
            
    }
}