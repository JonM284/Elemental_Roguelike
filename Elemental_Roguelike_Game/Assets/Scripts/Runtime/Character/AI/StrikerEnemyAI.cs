using System.Collections;
using Runtime.GameControllers;

namespace Runtime.Character.AI
{
    public class StrikerEnemyAI: EnemyAIBase
    {

        #region EnemyAIBase Inherited Methods

        public override IEnumerator C_PerformEnemyAction()
        {
            //Other team Controlling ball
            if (TurnController.Instance.ball.controlledCharacterSide != this.characterBase.side)
            {
                if (IsBallInMovementRange())
                {
                    yield return C_GoForBallCarrier();
                }
                else
                {
                    if (IsNearEnemyMember())
                    {
                        yield return C_StayNearClosestEnemy();     
                    }
                    else
                    {
                        yield return C_ConsiderAbilities();
                    }

                }
            }//This team Controlling ball
            else
            {
                yield return C_PositionToScore();
            }
        }

        private IEnumerator C_StayNearClosestEnemy()
        {

            var allTargetsAround = GetAllTargets(true);

            if (allTargetsAround.Count == 0)
            {
                
                yield break;
            }
            
            
            yield return null;
            
            characterBase.UseActionPoint();

        }

        private IEnumerator C_ConsiderAbilities()
        {
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