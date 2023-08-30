using System.Collections;
using Runtime.GameControllers;
using UnityEngine;

namespace Runtime.Character.AI
{
    public class StrikerEnemyAI: EnemyAIBase
    {

        #region EnemyAIBase Inherited Methods

        public override IEnumerator C_PerformEnemyAction()
        {
            Debug.Log("<color=orange>Performing Enemy Action</color>");
            //Other team Controlling ball
            if (TurnController.Instance.ball.controlledCharacterSide != this.characterBase.side)
            {
                if (IsBallInMovementRange())
                {
                    yield return StartCoroutine(C_GoForBallCarrier());
                }
                else
                {
                    if (IsNearEnemyMember())
                    {
                        yield return StartCoroutine(C_StayNearClosestEnemy());     
                    }
                    else
                    {
                        yield return StartCoroutine(C_ConsiderAbilities());
                    }

                }
            }//This team Controlling ball
            else
            {
                yield return StartCoroutine(C_PositionToScore());
            }
        }

        private IEnumerator C_StayNearClosestEnemy()
        {
            Debug.Log($"<color=orange>{gameObject.name} is staying close to enemy</color>");
            var allTargetsAround = GetAllTargets(true);

            if (allTargetsAround.Count == 0)
            {
                characterBase.UseActionPoint();
                yield break;
            }

            yield return null;
            
            characterBase.UseActionPoint();

        }

        private IEnumerator C_ConsiderAbilities()
        {
            Debug.Log($"<color=orange>{gameObject.name} is considering abilities</color>");

            
            var abilities = characterBase.characterAbilityManager.GetAssignedAbilities();

            CharacterAbilityManager.AssignedAbilities usableAbility = null;
            
            
            foreach (var ability in abilities)
            {
                if (!ability.canUse)
                {
                    continue;
                }

                
                
                
            }
            
            
            yield return null;
            
            characterBase.UseActionPoint();

        }

        #endregion
        
        
        
    }
}