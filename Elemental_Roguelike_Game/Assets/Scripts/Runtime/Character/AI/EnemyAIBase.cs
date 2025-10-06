using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Project.Scripts.Utils;
using Runtime.Abilities;
using Runtime.Character.AI.EnemyAI.BehaviourTrees;
using Runtime.GameControllers;
using Runtime.Gameplay;
using Runtime.Status;
using UnityEngine;
using Random = UnityEngine.Random;
using Sequence = Runtime.Character.AI.EnemyAI.BehaviourTrees.Sequence;

namespace Runtime.Character.AI
{
    [RequireComponent(typeof(CharacterBase))]
    public class EnemyAIBase: MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private LayerMask characterCheckMask;

        [SerializeField] private LayerMask obstacleCheckMask;

        [SerializeField] private int m_getBallPriority = 4;
        
        [SerializeField] private int m_attackForBallPriority = 5;

        [SerializeField] private int m_scoreGoalPriority = 1;
        
        [SerializeField] private int m_runBallInPriority = 2;

        [SerializeField] private int m_passBallPriority = 3;

        [SerializeField] private int m_repositionPriority = 6;

        [SerializeField] private int m_considerAbilityPriority = 7;

        [SerializeField] private int m_randomAttackPriority = 8;
        

        #endregion
        
        #region Private Fields

        private CharacterBase m_characterBase;

        protected CharacterBase m_targetCharacter;

        protected float m_standardWaitTime = 0.3f;

        protected float m_abilityWaitTime = 0.2f;

        protected bool m_isPerformingAction;

        protected BehaviourTree m_tree;

        private BallBehavior ballRef;

        #endregion

        #region Protected Fields

        protected bool m_isPerformingAbility;
        
        #endregion

        #region Accessors
        
        public CharacterBase characterBase => CommonUtils.GetRequiredComponent(ref m_characterBase, GetComponent<CharacterBase>);

        public bool isMeepleEnemy => characterBase is EnemyCharacterMeeple;

        public float enemyMovementRange => characterBase.characterMovement.currentMoveDistance - 0.05f;
        
        protected Transform playerTeamGoal => TurnController.Instance.GetPlayerManager().goalPosition;

        protected Transform enemyTeamGoal => TurnController.Instance.GetTeamManager(characterBase.side).goalPosition;

        protected BallBehavior ballReference => CommonUtils.GetRequiredComponent(ref ballRef, () =>
            TurnController.Instance.ball);

        protected bool canPerformNextAction =>
            !m_isPerformingAction && !m_isPerformingAbility && characterBase.characterActionPoints > 0;
        
        #endregion

        #region Unity Events
        
        protected void OnEnable()
        {
            TurnController.OnChangeActiveCharacter += OnChangeCharacterTurn;
        }

        protected void OnDisable()
        {
            TurnController.OnChangeActiveCharacter -= OnChangeCharacterTurn;
        }

        #endregion

        #region Class Implementation

        protected void OnChangeCharacterTurn(CharacterBase _characterBase)
        {
            if (characterBase.IsNull())
            {
                return;
            }
            
            if (_characterBase != characterBase)
            {
                return;
            }

            if (!characterBase.isAlive)
            {
                return;
            }

            StartCoroutine(C_Turn());
        }
        
        protected IEnumerator C_Turn()
        {
            Debug.Log($"{this.gameObject.name} start turn Before while", this);

            yield return new WaitForSeconds(m_standardWaitTime);

            while (characterBase.characterActionPoints > 0)
            {
                if (characterBase.characterActionPoints <= 0 || !characterBase.isAlive)
                {
                    break;
                }

                m_tree.Process();

                yield return null;
            }


            if (!characterBase.isAlive)
            {
                //Another script will handle death
                yield break;
            }

            if (!characterBase.finishedTurn)
            {
                characterBase.EndTurn();
            }
            
        }
        
        public void SetupBehaviorTrees()
        {
            m_tree = new BehaviourTree("Tree");

            PrioritySelector _availableActions = new PrioritySelector("Test Agent");

            //Go grab ball, if ball dropped
            Sequence _getBall = new Sequence("Get dropped ball", m_getBallPriority);
            _getBall.AddChild(new Leaf("CanGrabBall?", new Condition(() => canPerformNextAction && !TurnController.Instance.ball.isControlled && IsBallInMovementRange())));
            _getBall.AddChild(new Leaf("GetBall", new MovementStrategy(characterBase, enemyMovementRange, TurnController.Instance.ball.transform.position, FinishAction)));

            _availableActions.AddChild(_getBall);

            //-----------------
            //Go for ball carrier, if other team has ball 
            //-> random between ability and tackle if ability is damaging
            RandomSelector _attackForBall = new RandomSelector("Attack For Ball", m_attackForBallPriority);
            
            //--- First Option
            Sequence _tackle = new Sequence("PerformTackle");
            _tackle.AddChild(new Leaf("CanAttackCarrier?", new Condition(() => canPerformNextAction && TurnController.Instance.ball.isControlled 
                && TurnController.Instance.ball.controlledCharacterSide.sideGUID != this.characterBase.side.sideGUID 
                && IsBallInMovementRange())));
            _tackle.AddChild(new Leaf("TackleBallCarrier", new MovementStrategy(characterBase, enemyMovementRange, TurnController.Instance.ball.transform.position, FinishAction)));
            
            _attackForBall.AddChild(_tackle);

            //--- Second Option
            Sequence _attackCarrierwAbility = new Sequence("UseAbilityForBall");
            _attackCarrierwAbility.AddChild(new Leaf("HasGoodAbility?", new Condition(() => canPerformNextAction && !GetBestAbility().IsNull())));
            _attackCarrierwAbility.AddChild(new Leaf("AttackWithAbility", new AwaitingAction(AbilityConsiderationInBetween)));
            
            _attackForBall.AddChild(_attackCarrierwAbility);

            _availableActions.AddChild(_attackForBall);
            
            //--------------
            //Go for goal, if this team has ball
            PrioritySelector _scoreGoal = new PrioritySelector("Score Goal", m_scoreGoalPriority);

            Sequence _shootBall = new Sequence("ShootBall", 600);
            _shootBall.AddChild(new Leaf("CanShootBall?", new Condition(() => canPerformNextAction && characterBase.characterBallManager.hasBall && IsInShootRange())));
            _shootBall.AddChild(new Leaf("ShootBall", new AwaitingAction(ShootBallInBetween)));

            _scoreGoal.AddChild(_shootBall);
            
            Sequence _runBallIn = new Sequence("RunBall", m_runBallInPriority);
            _runBallIn.AddChild(new Leaf("CanRunBallIn?", new Condition(() => canPerformNextAction && characterBase.characterBallManager.hasBall 
                && !HasPlayerBlockingRoute() && !IsNearBruiserCharacter(enemyMovementRange))));
            _runBallIn.AddChild(new Leaf("RunBallIn", new MovementStrategy(characterBase, enemyMovementRange, GetRunInBallPosition(), FinishAction)));
            
            _scoreGoal.AddChild(_runBallIn);
            
            Sequence _passBall = new Sequence("PassBall", m_passBallPriority);
            _passBall.AddChild(new Leaf("CanPass?", new Condition(() => canPerformNextAction && characterBase.characterBallManager.hasBall && (HasPlayerBlockingRoute() || IsNearBruiserCharacter(enemyMovementRange)) && HasPassableTeammate())));
            _passBall.AddChild(new Leaf("PassBall", new AwaitingAction(TryPassInBetween)));

            _scoreGoal.AddChild(_passBall);
            
            Sequence _repositionToScore = new Sequence("Reposition", m_repositionPriority);
            _repositionToScore.AddChild(new Leaf("CanReposition?", new Condition(() => canPerformNextAction && characterBase.characterBallManager.hasBall 
                                            && HasPlayerBlockingRoute() && !IsNearBruiserCharacter(enemyMovementRange) && !HasPassableTeammate())));
            _repositionToScore.AddChild(new Leaf("RepositionToScore",
                new MovementStrategy(characterBase, enemyMovementRange, GetPositionToScore(), FinishAction)));
            
            _scoreGoal.AddChild(_repositionToScore);
            
            _availableActions.AddChild(_scoreGoal);

            //--------------
            //If AI has healing or buffing ability, use?
            Sequence _considerHealingOrBuffing = new Sequence("ConsiderAbility", m_considerAbilityPriority);
            _considerHealingOrBuffing.AddChild(new Leaf("CanHealSomething?", new Condition(() => canPerformNextAction && HasUsableHealingAbility())));
            _considerHealingOrBuffing.AddChild(new Leaf("HealSomething", new AwaitingAction(AbilityConsiderationInBetween)));

            _availableActions.AddChild(_considerHealingOrBuffing);

            
            //--------------
            //If can not go for ball, and Enemy is in range, go for enemy
            Sequence _attackRandomCloseEnemy = new Sequence("RandomAttack", m_randomAttackPriority);
            _attackRandomCloseEnemy.AddChild(new Leaf("HasEnemyNearby?", new Condition(() => canPerformNextAction 
                && HasUsableDamagingAbility() && !GetBestAbility().IsNull() && IsNearPlayerMember(GetBestAbility().currentRange))));
            _attackRandomCloseEnemy.AddChild(new Leaf("AttackRandom", new AwaitingAction(AbilityConsiderationInBetween)));
            
            _availableActions.AddChild(_attackRandomCloseEnemy);
            
            //------------
            //Stay near ball carrier
            Sequence _protectBallCarrier = new Sequence("ProtectBallCarrier");
            _protectBallCarrier.AddChild(new Leaf("CanProtect?", new Condition(() => canPerformNextAction &&
                TurnController.Instance.ball.isControlled && TurnController.Instance.ball.controlledCharacterSide.sideGUID == characterBase.side.sideGUID && IsTypeToHelpCarrier())));

            _protectBallCarrier.AddChild(new Leaf("GoToProtect", new MovementStrategy(characterBase, enemyMovementRange, GetProtectBallCarrierPosition(), FinishAction)));

            _availableActions.AddChild(_protectBallCarrier);

            //-------------
            //Stay at halfway point
            
            Sequence _stayHalfWay = new Sequence("StayBack");
            _stayHalfWay.AddChild(new Leaf("CanProtect?", new Condition(() => canPerformNextAction && 
                TurnController.Instance.ball.isControlled && TurnController.Instance.ball.controlledCharacterSide.sideGUID == characterBase.side.sideGUID && IsTypeToStayBack())));

            _stayHalfWay.AddChild(new Leaf("GoToProtect", new MovementStrategy(characterBase, enemyMovementRange, GetStayBackPosition(), FinishAction)));
            
            _availableActions.AddChild(_stayHalfWay);
            
            //--------------
            //Other wise skip turn, this will happen if we have nothing else we can do
            Sequence _skipTurn = new Sequence("Skip_Turn");
            
            _skipTurn.AddChild(new Leaf("Has nothing they can do", new Condition(() => canPerformNextAction && !CanOverwatch() 
                || characterBase.characterMovement.isRooted)));
            
            _skipTurn.AddChild(new Leaf("SkipTurn", new TreeAction(() =>
            {
                Debug.Log($"Skipping TURN", this);
                characterBase.EndTurn();
            })));
            
            _availableActions.AddChild(_skipTurn);
            
            //---------------
            Sequence _overwatch = new Sequence("Overwatch");
            
            _overwatch.AddChild(new Leaf("IsGoodToOverwatch", new Condition(() => canPerformNextAction && CanOverwatch())));
            _overwatch.AddChild(new Leaf("DoOverwatch", new TreeAction(() => characterBase.SetOverwatch())));
            
            _availableActions.AddChild(_overwatch);
            
            //-------------
            
            m_tree.AddChild(_availableActions);

        }

        #endregion

        #region Performable Actions - Functions

        private void FinishAction()
        {
            m_isPerformingAction = false;
        }

        #endregion
        
        #region Performable Actions - Coroutines


        private async void ShootBallInBetween(Action _callback)
        {
            await T_ShootBall(_callback);
        }

        protected async UniTask T_ShootBall(Action _callback)
        {
            m_isPerformingAction = true;
            
            characterBase.SetCharacterThrowAction();
            
            characterBase.CheckAllAction(playerTeamGoal.position , false);

            await UniTask.WaitUntil(() => !ballReference.isThrown);
            
            _callback?.Invoke();
            
            m_isPerformingAction = false;
        }

        private async void TryPassInBetween(Action _callback)
        {
            await T_TryPass(_callback);
        }

        protected async UniTask T_TryPass(Action _callback)
        {
            var allAlliesInRange = GetAllTargets(false, characterBase.characterBallManager.passStrength);
            List<CharacterBase> passableAllies = new List<CharacterBase>();

            foreach (var ally in allAlliesInRange)
            {
                var dir = playerTeamGoal.position - transform.position;
                var dirToAlly = ally.transform.position - transform.position;
                var angle = Vector3.Angle(dir, dirToAlly);
                if (angle is < 150f and > 25f)
                {
                    passableAllies.Add(ally);
                }
            }

            if (passableAllies.Count == 0)
            {
                _callback?.Invoke();
                FinishAction();
                characterBase.UseActionPoint();
                return;
            }

            CharacterBase bestPossiblePass = null;

            foreach (var availableAlly in passableAllies)
            {
                if (!bestPossiblePass.IsNull())
                {
                    continue;
                }
                
                var dirToAlly = availableAlly.transform.position - transform.position;
                if (!IsPlayerInDirection(dirToAlly))
                {
                    bestPossiblePass = availableAlly;
                }
            }

            if (bestPossiblePass.IsNull())
            {
                _callback?.Invoke();
                FinishAction();
                characterBase.UseActionPoint();
                return;
            }
            
            Debug.Log("THROW BALL");
            
            characterBase.SetCharacterThrowAction();
            
            characterBase.CheckAllAction(bestPossiblePass.transform.position , false);

            await UniTask.WaitUntil(() => characterBase.isDoingAction == false);
            
            _callback?.Invoke();
            FinishAction();
        }

        private async void AbilityConsiderationInBetween(Action _callback)
        {
            await T_ConsiderAbility(_callback);
        }
        
        protected async UniTask T_ConsiderAbility(Action _callback)
        {
            Debug.Log($"<color=orange>{gameObject.name} is considering abilities</color>");
            
            m_isPerformingAction = true;

            var _ability = GetBestAbility();
            
            if (_ability.IsNull())
            {
                //Not Valid
                FinishAction();
                _callback?.Invoke();
                characterBase.UseActionPoint();
                return;
            }
            
            var _abilityIndex = characterBase.characterAbilityManager.GetAssignedAbilities().IndexOf(_ability);
            Debug.Log($"<color=green>Ability Index:{_abilityIndex}</color>");
            
            //Check what type of ability it is, do thing accordingly
            switch (_ability.abilityData.abilityType)
            {
                case AbilityType.ProjectileDamage: case AbilityType.ProjectileKnockback : case AbilityType.ProjectileStatus: case AbilityType.ApplyStatusEnemy:
                    //Check to see if enemy is in range, if they are: USE ABILITY
                    if (IsNearPlayerMember(_ability.currentRange - 0.1f))
                    {
                        var availableTargets = GetAllTargets(true, _ability.currentRange - 0.1f);
                        
                        if(availableTargets.Count == 0){
                            break;    
                        }

                        CharacterBase bestTarget = null;

                        if (TurnController.Instance.ball.isControlled)
                        {
                            if (TurnController.Instance.ball.controlledCharacterSide.sideGUID != characterBase.side.sideGUID)
                            {
                                //other team has ball
                                bestTarget = availableTargets.TrueForAll(cb => cb.characterBallManager.hasBall)
                                    ? availableTargets.FirstOrDefault()
                                    : availableTargets.FirstOrDefault(cb => cb.characterBallManager.hasBall);
                            }
                            else
                            {
                                bestTarget = availableTargets.FirstOrDefault();
                            }
                        }
                        else
                        {
                            bestTarget = availableTargets.FirstOrDefault();
                        }

                        if (bestTarget.IsNull())
                        {
                            break;
                        }
                        
                        if (_ability.abilityData.targetType == AbilityTargetType.CHARACTER_TRANSFORM)
                        {
                            m_isPerformingAbility = true;
                            characterBase.UseCharacterAbility(_abilityIndex);
                            bestTarget.OnSelect();
                        }
                        else
                        {
                            m_isPerformingAbility = true;
                            characterBase.UseCharacterAbility(_abilityIndex);
                            characterBase.CheckAllAction(bestTarget.transform.position, false);
                        }
                        
                        Debug.Log($"<color=green>Ability Index 2:{_abilityIndex}</color>");
                    }

                    break;
                
                case AbilityType.ZoneHeal: 
                    //Look for allies to help, try to fire as close as possible
                    if (IsNearTeammates(_ability.currentRange - 0.07f))
                    {
                        var availableTargets = GetAllTargets(false, _ability.currentRange);
                        
                        if(availableTargets.Count == 0){
                            break;    
                        }
                        
                        if (!availableTargets.TrueForAll(cb =>
                                cb.characterLifeManager.currentHealthPoints == cb.characterLifeManager.maxHealthPoints))
                        {
                            m_isPerformingAbility = true;
                            characterBase.UseCharacterAbility(_abilityIndex);
                            
                            var _actualTarget = availableTargets.FirstOrDefault(cb =>
                                cb.characterLifeManager.currentHealthPoints < cb.characterLifeManager.maxHealthPoints);
                            if (_ability.abilityData.targetType == AbilityTargetType.CHARACTER_TRANSFORM)
                            {
                                _actualTarget.OnSelect();
                            }
                            else
                            {
                                characterBase.CheckAllAction(_actualTarget.transform.position, false);
                            }
                        }
                    }
                    
                    break;
                case AbilityType.ApplyStatusTarget:
                    if (IsNearTeammates(_ability.currentRange - 0.07f))
                    {
                        var availableTargets = GetAllTargets(false, _ability.currentRange);
                        
                        if(availableTargets.Count == 0){
                            break;    
                        }
                        
                        if (!availableTargets.TrueForAll(cb => cb.HasStatusEffects()))
                        {
                            var _actualTarget = availableTargets.FirstOrDefault(cb =>
                                cb.HasStatusEffectOfType(StatusType.Negative));
                            if (!_actualTarget.IsNull())
                            {
                                m_isPerformingAbility = true;
                                characterBase.UseCharacterAbility(_abilityIndex);
                                
                                if (_ability.abilityData.targetType == AbilityTargetType.CHARACTER_TRANSFORM)
                                {
                                    _actualTarget.OnSelect();
                                }
                                else
                                {
                                    characterBase.CheckAllAction(_actualTarget.transform.position, false);
                                }
                            }
                        }
                    }
                    break;
                case AbilityType.ApplyStatusSelf: case AbilityType.AgilityStatUpgrade: case AbilityType.ThrowStatUpgrade: case AbilityType.DamageStatUpgrade: case AbilityType.MovementUpgrade:
                    //Just apply to self?
                    //Maybe depends?
                    Debug.Log($"<color=orange>Applying Status or Upgrade Self</color>");
                    if (!characterBase.HasStatusEffects())
                    {
                        m_isPerformingAbility = true;
                        characterBase.UseCharacterAbility(_abilityIndex);
                        Debug.Log($"<color=green>Ability Index 2:{_abilityIndex}</color>");
                    }
                    else
                    {
                        if (characterBase.HasStatusEffectOfType(StatusType.Negative))
                        {
                            m_isPerformingAbility = true;
                            characterBase.UseCharacterAbility(_abilityIndex);
                            Debug.Log($"<color=green>Ability Index 3:{_abilityIndex}</color>");
                        }
                    }
                    break;
                case AbilityType.Teleport:
                    if (TurnController.Instance.ball.isControlled && !characterBase.characterBallManager.hasBall)
                    {
                        //If I don't have the ball, ignore use
                        break;
                    }

                    if (IsNearPlayerMember(enemyMovementRange))
                    {
                        var availableTargets = GetAllTargets(false, enemyMovementRange);
                        
                        if(availableTargets.Count == 0){
                            break;    
                        }

                        foreach (var _target in availableTargets)
                        {
                            if (_target.characterClassManager.assignedClass.classType == CharacterClass.BRUISER)
                            {
                                var dirToBruiser = _target.transform.position - transform.position;
                                if (!(dirToBruiser.magnitude <= _target.characterClassManager.assignedClass.overwatchRadius))
                                {
                                    continue;
                                }
                                
                                if (!TurnController.Instance.ball.isControlled)
                                {
                                    var dirToBall = TurnController.Instance.ball.transform.position.FlattenVector3Y() -
                                                    transform.position.FlattenVector3Y();
                                    m_isPerformingAbility = true;
                                    characterBase.UseCharacterAbility(_abilityIndex);
                                    var targetPos = dirToBall.normalized * _ability.currentRange;
                                    characterBase.CheckAllAction(targetPos, false);
                                }
                                else
                                {
                                    if (characterBase.characterBallManager.hasBall)
                                    {
                                        var dirToGoal = TurnController.Instance.GetPlayerManager().goalPosition.position.FlattenVector3Y() -
                                                        transform.position.FlattenVector3Y();
                                        m_isPerformingAbility = true;
                                        characterBase.UseCharacterAbility(_abilityIndex);
                                        Vector3 targetPos = TurnController.Instance.GetPlayerManager().goalPosition
                                        .position.FlattenVector3Y();
                                        
                                        if (dirToGoal.magnitude >= _ability.currentRange - 0.1f)
                                        {
                                            targetPos = dirToGoal.normalized * _ability.currentRange;
                                        }
                                            
                                        characterBase.CheckAllAction(targetPos, false);
                                    }
                                        
                                    break;
                                }
                            }
                        }
                    }
                    break;
                case AbilityType.ZoneDamage: case AbilityType.DamageCreation: case AbilityType.TrapCreation: case AbilityType.WallCreation:
                    //Do Damage
                    if (IsNearPlayerMember(_ability.currentRange - 0.1f))
                    {
                        var availableTargets = GetAllTargets(true, _ability.currentRange);
                        
                        if(availableTargets.Count == 0){
                            break;    
                        }
                        
                        m_isPerformingAbility = true;
                        characterBase.UseCharacterAbility(_abilityIndex);
                        
                        var targetPos = (availableTargets.FirstOrDefault().transform.position +
                                         Random.insideUnitSphere).FlattenVector3Y();
                        characterBase.CheckAllAction(targetPos, false);
                    }
                    break;
                case AbilityType.ZoneSelf:
                    if (IsNearPlayerMember(_ability.currentRange - 0.1f))
                    {
                        m_isPerformingAbility = true;
                        characterBase.UseCharacterAbility(_abilityIndex);
                        
                        characterBase.CheckAllAction(this.transform.position, false);
                    }
                    break;
                case AbilityType.Pull:
                    if (IsBallInRange(_ability.currentRange - 0.1f))
                    {
                        m_isPerformingAbility = true;
                        characterBase.UseCharacterAbility(_abilityIndex);
                        
                        characterBase.CheckAllAction(ballReference.transform.position, false);
                    }
                    break;
            }
            
            if (m_isPerformingAbility)
            {
                Debug.Log("Is waiting for ability");
                await UniTask.WaitUntil(() => !characterBase.characterAbilityManager.isUsingAbilityAction || characterBase.characterAbilityManager.hasCanceledAbility);

                if (characterBase.characterAbilityManager.hasCanceledAbility)
                {
                    characterBase.UseActionPoint();
                }   
            }
           
            Debug.Log("<color=red>Complete Ability Consideration</color>");
            _callback?.Invoke();
            m_isPerformingAction = false;
            m_isPerformingAbility = false;
        }

        protected Vector3 GetRunInBallPosition()
        {
            var dirToGoal = playerTeamGoal.transform.position - transform.position;
            return dirToGoal.magnitude > enemyMovementRange ? transform.position + (dirToGoal.normalized * enemyMovementRange) : playerTeamGoal.transform.position;
        }

        protected Vector3 GetPositionToScore()
        {
            var randomPosition = Random.insideUnitCircle * characterBase.characterBallManager.shotStrength / 2;
            var adjustedRandomPos = new Vector2(Mathf.Abs(randomPosition.x), randomPosition.y);
            var randomPointInShotRange = new Vector3(playerTeamGoal.position.x + adjustedRandomPos.x, playerTeamGoal.position.y, playerTeamGoal.position.z + adjustedRandomPos.y);
            var dirToRandomPoint = randomPointInShotRange - transform.position;

            return dirToRandomPoint.magnitude >= enemyMovementRange
                ? transform.position + (dirToRandomPoint.normalized * enemyMovementRange)
                : randomPointInShotRange;
        }

        protected Vector3 GetProtectBallCarrierPosition()
        {
            var randomPosition = ballReference.transform.position + 
                                 (Random.insideUnitSphere.FlattenVector3Y() * (characterBase.characterMovement.currentMoveDistance/0.5f));
            
            var dirToPosition = randomPosition - transform.position;
            
            return dirToPosition.magnitude > enemyMovementRange ?  transform.position + (dirToPosition.normalized * enemyMovementRange) : randomPosition;
        }

        protected Vector3 GetStayBackPosition()
        {
            var enemyFieldSideMidpoint = (Vector3.zero - TurnController.Instance.GetTeamManager(characterBase.side).goalPosition.position) / 2;
            var randomPosition = enemyFieldSideMidpoint + 
                                 (Random.insideUnitSphere.FlattenVector3Y() * (characterBase.characterMovement.currentMoveDistance/0.5f));           
            var dirToPosition = randomPosition - transform.position;
            return dirToPosition.magnitude > enemyMovementRange ?  transform.position + (dirToPosition.normalized * enemyMovementRange) : randomPosition;
        }

        #endregion

        #region Conditions
        
        protected bool IsBallInMovementRange()
        {
            var directionToBall = ballReference.transform.position - transform.position;
            var distanceToBall = directionToBall.magnitude;
            var enemyMovementThreshold = enemyMovementRange * characterBase.characterActionPoints;
            return enemyMovementThreshold >= distanceToBall;
        }
        
        protected bool IsBallInRange(float _range)
        {
            var directionToBall = ballReference.transform.position - transform.position;
            var distanceToBall = directionToBall.magnitude;
            return _range >= distanceToBall;
        }

        protected bool IsNearPlayerMember(float _range)
        {
            return GetAllTargets(true, _range).Count > 0;
        }

        protected bool IsNearTeammates(float _range)
        {
            return GetAllTargets(false, _range).Count > 0;
        }
        
        protected bool HasPlayerBlockingRoute()
        {
            var dirToGoal = playerTeamGoal.position - transform.position;
            RaycastHit[] hits = Physics.CapsuleCastAll(transform.position, playerTeamGoal.position, 0.2f, dirToGoal, dirToGoal.magnitude);
            
            foreach (RaycastHit hit in hits)
            {
                //if they are running into an enemy character, make them stop at that character and perform melee
                if (!hit.collider.TryGetComponent(out CharacterBase otherCharacter))
                {
                    continue;
                }
                
                if (otherCharacter.side.sideGUID != characterBase.side.sideGUID && otherCharacter != characterBase)
                {
                    return true;
                }
            }

            return false;
        }

        protected bool HasPassableTeammate()
        {
            return GetAllTargets(false, characterBase.characterBallManager.shotStrength).Count > 0;
        }
        
        protected bool IsPlayerInDirection(Vector3 direction)
        {
            if (!Physics.Raycast(transform.position, direction.normalized, out RaycastHit hit, direction.magnitude, characterCheckMask))
            {
                return false;
            }
            
            hit.transform.TryGetComponent(out PlayableCharacter player);
            return !player.IsNull();
        }

        protected bool IsNearBruiserCharacter(float range)
        {
            var adjustedRange = (range/2) - 0.07f;
            Collider[] colliders = Physics.OverlapSphere(transform.position, adjustedRange, characterCheckMask);

            if (colliders.Length <= 0)
            {
                return false;
            }

            foreach (var _collider in colliders)
            {
                _collider.TryGetComponent(out CharacterBase _characterBase);
                if (!_characterBase)
                {
                    continue;
                }

                if (!_characterBase.characterClassManager.isAbleToReact)
                {
                    continue;
                }

                if (_characterBase.characterClassManager.assignedClass.classType != CharacterClass.BRUISER)
                {
                    continue;
                }

                return true;
            }

            return false;
        }
        
        protected List<CharacterBase> GetAllTargets(bool isPlayerTeam, float _checkRange)
        {
            var adjustedRange = _checkRange - 0.05f;
            Collider[] colliders = Physics.OverlapSphere(transform.position, adjustedRange, characterCheckMask);
            List<CharacterBase> _targetTransforms = new List<CharacterBase>();
            if (colliders.Length > 0)
            {
                foreach (var col in colliders)
                {
                    col.TryGetComponent(out CharacterBase _characterBase);

                    if (!_characterBase)
                    {
                        continue;
                    }

                    if (_characterBase == characterBase)
                    {
                        continue;
                    }

                    //Don't do anything to dead or dying characters
                    if (!_characterBase.isAlive)
                    {
                        continue;
                    }
                    
                    if (isPlayerTeam) //Get Player Team Members only
                    {
                        //Is On enemy team
                        if (_characterBase.side.sideGUID == characterBase.side.sideGUID)
                        {
                            continue;
                        }
                        
                        if (!_characterBase.isGoalieCharacter && _characterBase.isTargetable)
                        {
                            _targetTransforms.Add(_characterBase);   
                        }
                    }
                    else // Get AI team Members only
                    {
                        //Not on AI team
                        if (_characterBase.side.sideGUID != characterBase.side.sideGUID)
                        {
                            continue;
                        }
                        
                        if (_characterBase != this.characterBase)
                        {
                            _targetTransforms.Add(_characterBase);
                        }
                    }
                    
                }
            }

            return _targetTransforms;
        }
        
        protected CharacterBase GetHealthiestTarget(List<CharacterBase> _possibleTargets)
        {
            if (_possibleTargets.Count == 0)
            {
                return default;
            }
            
            return _possibleTargets.OrderByDescending(cb => cb.characterLifeManager.currentOverallHealth).LastOrDefault();
        }

        protected CharacterBase GetWeakestTarget(List<CharacterBase> _possibleTargets)
        {
            if (_possibleTargets.Count == 0)
            {
                return default;
            }

            return _possibleTargets.OrderByDescending(cb => cb.characterLifeManager.currentOverallHealth).FirstOrDefault();
        }

        protected bool IsInShootRange()
        {
            var directionToGoal = playerTeamGoal.position - transform.position;
            var distanceToGoal = directionToGoal.magnitude;
            var enemyMovementThreshold = enemyMovementRange * characterBase.characterActionPoints;
            return enemyMovementThreshold >= distanceToGoal || characterBase.characterBallManager.shotStrength >= distanceToGoal;
        }
        
        protected AbilityEntityBase GetBestAbility()
        {
            return characterBase.isSilenced ? null : characterBase.characterAbilityManager.GetAssignedAbilities().FirstOrDefault(aa => AbilityIsCurrentlyUsable(aa));
        }

        protected bool HasUsableHealingAbility()
        {
            return !characterBase.isSilenced && characterBase.characterAbilityManager.GetAssignedAbilities().Any(aa =>
                aa.abilityData.abilityType is AbilityType.ZoneHeal or AbilityType.ApplyStatusSelf
                    or AbilityType.ApplyStatusTarget);
        }

        protected bool HasUsableDamagingAbility()
        {
            return !characterBase.isSilenced && characterBase.characterAbilityManager.GetAssignedAbilities().Any(aa =>
                aa.abilityData.abilityType is AbilityType.ZoneDamage or AbilityType.DamageCreation
                    or AbilityType.ProjectileDamage or AbilityType.ProjectileKnockback or AbilityType.TrapCreation);
        }

        protected bool AbilityIsCurrentlyUsable(AbilityEntityBase _testAbility)
        {
            switch (_testAbility.abilityData.abilityType)
            {
                //Currently, never really a bad moment to put on a status or upgrade
                case AbilityType.ApplyStatusSelf or AbilityType.MovementUpgrade or AbilityType.AgilityStatUpgrade
                    or AbilityType.DamageStatUpgrade or AbilityType.ThrowStatUpgrade when !characterBase.HasStatusEffects():
                
                    return true;
                
                case AbilityType.ApplyStatusSelf or AbilityType.MovementUpgrade or AbilityType.AgilityStatUpgrade
                    or AbilityType.DamageStatUpgrade or AbilityType.ThrowStatUpgrade:
                    
                    return characterBase.HasStatusEffectOfType(StatusType.Negative);
                
                case AbilityType.ZoneHeal:
                {
                    if (!IsNearTeammates(_testAbility.currentRange - 0.07f))
                    {
                        return false;
                    }
                    
                    var availableTargets = GetAllTargets(false, _testAbility.currentRange);
                        
                    if(availableTargets.Count == 0){
                        return false;    
                    }
                        
                    return !availableTargets.TrueForAll(cb => cb.characterLifeManager.currentHealthPoints == cb.characterLifeManager.maxHealthPoints);
                }
            }

            //ball is not controlled - relevant abilities: pull, teleport, creations?, 
            if (!TurnController.Instance.ball.isControlled)
            {
                switch (_testAbility.abilityData.abilityType)
                {
                    case AbilityType.Pull:

                        return IsBallInRange(_testAbility.currentRange - 0.07f);
                    
                    case AbilityType.Teleport: case AbilityType.Dash:
                        
                        if (!IsBallInRange(_testAbility.currentRange - 0.07f))
                        {
                            break;
                        }
                        
                        if (IsNearPlayerMember(enemyMovementRange))
                        {
                            var availableTargets = GetAllTargets(false, enemyMovementRange);
                        
                            if(availableTargets.Count == 0){
                                return false;    
                            }

                            foreach (var _target in availableTargets)
                            {
                                if (_target.characterClassManager.assignedClass.classType != CharacterClass.BRUISER)
                                {
                                    continue;
                                }
                                
                                var dirToBruiser = _target.transform.position - transform.position;
                                
                                return dirToBruiser.magnitude <= _target.characterClassManager.assignedClass.overwatchRadius;
                            }
                        }
                        break;
                    
                    case AbilityType.Movement:
                        //never use movement abilities
                        return false;
                    
                    default:
                        return false;
                }
                
                return false;
            }
            
            
            //ball is controlled by other team - relevant abilities: anything that has knockback, knocking ball out of character's hand
            // setting up zones
            if (TurnController.Instance.ball.controlledCharacterSide.sideGUID != characterBase.side.sideGUID)
            {
                switch (_testAbility.abilityData.abilityType)
                {
                    case AbilityType.ProjectileKnockback: case AbilityType.ApplyStatusEnemy: case AbilityType.ProjectileStatus: case AbilityType.ZoneDamage:
                        case AbilityType.ProjectileDamage: case AbilityType.ZoneSelf:
                            
                        //player with ball is in range
                        return IsBallInRange(_testAbility.currentRange - 0.07f);
                    
                    case AbilityType.DamageCreation: case AbilityType.TrapCreation: case AbilityType.WallCreation:
                    
                        return IsNearPlayerMember(_testAbility.currentRange - 0.07f);
                    
                    default:
                        return false;
                }
            }
            
            //ball is controlled by AI TEAM - relevant abilities: dealing damage to anyone around, teleport
            switch (_testAbility.abilityData.abilityType)
            {
                case AbilityType.Teleport: case AbilityType.Dash:
                    if (!characterBase.characterBallManager.hasBall)
                    {
                        break;
                    }
                    
                    if(!IsBallInRange(_testAbility.currentRange - 0.07f))
                    {
                        break;
                    }
                    
                    if (IsNearPlayerMember(enemyMovementRange))
                    {
                        var availableTargets = GetAllTargets(false, enemyMovementRange);
                        
                        if(availableTargets.Count == 0){
                            return false;    
                        }

                        foreach (var _target in availableTargets)
                        {
                            if (_target.characterClassManager.assignedClass.classType != CharacterClass.BRUISER)
                            {
                                continue;
                            }
                            
                            var dirToBruiser = _target.transform.position - transform.position;
                            return dirToBruiser.magnitude <= _target.characterClassManager.assignedClass.overwatchRadius;
                        }
                    }
                    break;
                case AbilityType.ProjectileKnockback: case AbilityType.ApplyStatusEnemy: case AbilityType.ProjectileStatus: case AbilityType.ZoneDamage:
                case AbilityType.ProjectileDamage: case AbilityType.ZoneSelf:
                            
                    //any player nearby
                    return IsNearPlayerMember(_testAbility.currentRange - 0.07f);
                
                default:
                    return false;
            }

            return false;
        }


        private bool CanOverwatch()
        {
            if (characterBase.characterClassManager.assignedClass.classType is CharacterClass.STRIKER or CharacterClass.ALL)
            {
                return false;
            }
            
            return GetAllTargets(true, characterBase.characterClassManager.passiveRadius).Count > 0;
        }


        private bool IsTypeToHelpCarrier()
        {
            return characterBase.characterClassManager.assignedClass.classType is CharacterClass.STRIKER
                or CharacterClass.PLAYMAKER;
        }

        private bool IsTypeToStayBack(){
            return characterBase.characterClassManager.assignedClass.classType is CharacterClass.BRUISER
                or CharacterClass.DEFENDER or CharacterClass.TANK;
        }

        #endregion
        
        
    }
}