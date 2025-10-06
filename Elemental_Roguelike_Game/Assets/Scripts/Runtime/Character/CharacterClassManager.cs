using System.Collections;
using Cysharp.Threading.Tasks;
using Data.CharacterData;
using Data.Sides;
using Project.Scripts.Utils;
using Runtime.GameControllers;
using Runtime.Gameplay;
using Runtime.Managers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Runtime.Character
{

    [RequireComponent(typeof(CharacterBase))]
    [DisallowMultipleComponent]
    public class CharacterClassManager : MonoBehaviour, IReactor
    {

        #region Read-Only

        private static readonly int m_colorVarName = Shader.PropertyToID("_EmisColor");

        #endregion

        #region Serialized Fields

        [SerializeField] private GameObject m_passiveIndicator;

        [SerializeField] private Transform m_reactionCameraPoint;

        [SerializeField] private LayerMask characterLayers;

        [SerializeField] private float passiveBaseDistance = 1f, maxPassiveBonusDistance = 10f;

        [SerializeField] private float gravityBaseAmount = 1f, maxGravity = 10f;

        #endregion

        #region Private Fields

        private CharacterBase m_characterBase;
        
        private CharacterClassData assignedClassRef;

        private CharacterStatsBase assignedCharacterStatsRef;
        
        private BallBehavior m_ballRef;

        private bool m_isPerformingReaction;

        private bool m_hasPerformedReaction;

        private CharacterBase m_inRangeCharacter;

        private bool m_canPerformReaction = true, m_isWaitingForReactionQueue;

        private bool hasAddedToBallThrowVelocity, ballHasEnteredPassiveRange;

        private ArenaTeamManager m_teamManager;

        private int failedReactionCounter;

        private int successfulMeleeReactionCounter = 1;

        private float bottomRangeMod = 0.2f;

        private int m_maxOverwatchCooldown = 1;

        private Collider[] charactersInRange = new Collider[10];
        private int characterInRangeAmount;

        private bool ballHasExitedRange;
        private int ballChangeVelocityAmount;
        private float ballInfluenceIntensity;
        
        #endregion

        #region Accessors

        public CharacterBase characterBase => CommonUtils.GetRequiredComponent(ref m_characterBase,
            GetComponent<CharacterBase>);

        public CharacterClassData assignedClass => assignedClassRef;

        public CharacterStatsBase assignedCharacterStats => assignedCharacterStatsRef;

        public BallBehavior ball => CommonUtils.GetRequiredComponent(ref m_ballRef, () =>
            TurnController.Instance.ball);

        private ArenaTeamManager teamManager => CommonUtils.GetRequiredComponent(ref m_teamManager, () =>
            TurnController.Instance.GetTeamManager(characterBase.side));

        bool IReactor.isPerformingReaction
        {
            get => m_isPerformingReaction;
            set => m_isPerformingReaction = value;
        }

        public int agilityScore { get; private set; }

        public int throwingScore { get; private set; }

        public int tacklingScore { get; private set; }
        
        public int currentMaxAgilityScore { get; private set; }
        
        public int currentMaxThrowingScore { get; private set; }
        
        public int currentMaxTacklingScore { get; private set; }

        public float passiveRadius { get; private set; }

        public float overwatchRadius { get; private set; }

        public bool isOverwatch { get; private set; }

        public int overwatchCoolDown { get; private set; }

        public float overwatchCooldownPrct => overwatchCoolDown / (float)m_maxOverwatchCooldown;

        public int bottomRange => assignedClass.classType == CharacterClass.ALL ? 100 : 1;

        private bool isPlayer => characterBase.side.sideGUID == TurnController.Instance.playersSide.sideGUID;

        public bool hasPerformedReaction => m_hasPerformedReaction;

        public Transform reactionCameraPoint => m_reactionCameraPoint;

        public bool isAbleToReact => m_canPerformReaction && !m_hasPerformedReaction && !m_isPerformingReaction;

        private LayerMask displayLayerVal => LayerMask.NameToLayer("DISPLAY");
        
        private LayerMask displayEnemyLayerVal => LayerMask.NameToLayer("DISPLAY_ENEMY");

        private LayerMask charLayerVal => LayerMask.NameToLayer("CHARACTER");

        #endregion

        #region Unity Events

        private void OnEnable()
        {
            TurnController.OnChangeActiveTeam += OnSwapSides;
        }

        private void OnDisable()
        {
            TurnController.OnChangeActiveTeam -= OnSwapSides;
        }

        public void InfluenceUpdate()
        {
            if (isOverwatch)
            {
                return;
            }

            CheckInfluence();
        }
        
        public void OverwatchUpdate()
        {
            if (!isOverwatch)
            {
                return;
            }

            if (m_isPerformingReaction || m_hasPerformedReaction)
            {
                return;
            }
            
            CheckOverwatch();
            
        }

        #endregion

        #region Class Implementation

        public async UniTask InitializedCharacterPassive(CharacterStatsBase characterStatsData, CharacterClassData classData)
        {
            if (classData.IsNull() || characterStatsData.IsNull())
            {
                Debug.LogError($"Character Stats NULL: {characterStatsData.IsNull()} //// Class Data: {classData.IsNull()}");
                return;
            }
            
            assignedClassRef = classData;
            assignedCharacterStatsRef = characterStatsData;

            //Formula = Base + (Influence Range Score [0-100] / 100)[Percentage] * Max Range for Passive
            passiveRadius = passiveBaseDistance + 
                            ((assignedCharacterStats.influenceRange/100f) * maxPassiveBonusDistance);
            
            overwatchRadius = assignedClass.overwatchRadius;
            
            this.agilityScore = assignedCharacterStatsRef.agilityScore;
            this.throwingScore = assignedCharacterStatsRef.throwScore;
            this.tacklingScore = assignedCharacterStatsRef.tackleScore;

            currentMaxAgilityScore = this.agilityScore;
            currentMaxThrowingScore = this.throwingScore;
            currentMaxTacklingScore = this.tacklingScore;
            
            ballChangeVelocityAmount = assignedClass.GetBallVelocityChangeAmount();
            ballInfluenceIntensity = gravityBaseAmount + 
                                     ((assignedCharacterStats.gravityScore/100f) * maxGravity);
            
            if (m_passiveIndicator.IsNull())
            {
                return;
            }

            //ToDo: overwatch and passive
            //Radius is 1/2 scale, scale is diameter
            var doubleRadiusSize = passiveRadius * 2;
            Debug.Log($"Passive Size: {passiveRadius} ::: Double:{doubleRadiusSize}", this.gameObject);
            m_passiveIndicator.transform.localScale = Vector3.one * doubleRadiusSize;
            
            //ToDo: make an overwatch range indicator
            var meshRend = m_passiveIndicator.GetComponent<MeshRenderer>();
            var indicatorMat = meshRend.materials[0];
            indicatorMat.SetColor(m_colorVarName, classData.passiveColor);
            
            DisplayIndicator(false);
            
            await UniTask.WaitForEndOfFrame();
            
            Debug.Log("Class Initialized");
        }

        public void UpdateCharacterPassiveRadius(float newAmount)
        {
            passiveRadius = newAmount;
            //Radius is 1/2 scale, scale is diameter
            var doubleRadiusSize = passiveRadius * 2;
            m_passiveIndicator.transform.localScale =
                new Vector3(doubleRadiusSize, doubleRadiusSize, doubleRadiusSize);
        }
        
        
        //Overwatch is only active on other teams turn
        /// <summary>
        /// Automatically deactivate character passives when it gets back to the character's team's turn
        /// </summary>
        /// <param name="_side"></param>
        private void OnSwapSides(CharacterSide _side)
        {
            if (!m_canPerformReaction)
            {
                DisplayIndicator(false);
                return;
            }
            
            //Enemy turn
            if (characterBase.side.sideGUID != _side.sideGUID)
            {
                return;
            }

            //Team turn
            m_isPerformingReaction = false;
            m_hasPerformedReaction = false;
            isOverwatch = false;

            if (!m_passiveIndicator.IsNull())
            {
                DisplayIndicator(false);
            }

            if (overwatchCoolDown > 0)
            {
                overwatchCoolDown--;
            }
        }
        
        public void ActivateCharacterOverwatch()
        {
            if (overwatchCoolDown > 0)
            {
                return;
            }
            
            if (!m_canPerformReaction)
            {
                DisplayIndicator(false);
                return;
            }

            m_isPerformingReaction = false;
            m_hasPerformedReaction = false;
            isOverwatch = true;

            if (!m_passiveIndicator.IsNull())
            {
                DisplayIndicator(true);
            }

            overwatchCoolDown = m_maxOverwatchCooldown;
        }

        private void CheckOverwatch()
        {
            if (m_isPerformingReaction || m_hasPerformedReaction)
            {
                return;
            }

            if (m_isWaitingForReactionQueue)
            {
                return;
            }

            if (characterBase.characterMovement.isKnockedBack)
            {
                return;
            }

            if (!m_canPerformReaction)
            {
                return;
            }
            
            switch (assignedClass.classType)
            {
                case CharacterClass.DEFENDER:
                    if (IsBallInRange(overwatchRadius))
                    {
                        m_isWaitingForReactionQueue = true;
                        ReactionQueueController.Instance.QueueReaction(this, PerformDefenderAction);
                    }
                    break;
                case CharacterClass.BRUISER:
                    if (IsEnemyWalkInRange())
                    {
                        m_isWaitingForReactionQueue = true;
                        ReactionQueueController.Instance.QueueReaction(this, PerformBruiserAction);
                    }
                    break;
                case CharacterClass.TANK:
                    if (IsEnemyWalkInRange())
                    {
                        m_isWaitingForReactionQueue = true;
                        ReactionQueueController.Instance.QueueReaction(this, PerformTankAction);
                    }
                    break;
                case CharacterClass.ALL:
                    if (IsEnemyWalkInRange())
                    {
                        m_isWaitingForReactionQueue = true;
                        ReactionQueueController.Instance.QueueReaction(this, PerformTankAction);
                    }else if (IsBallInRangeWithThrowCondition(overwatchRadius,true))
                    {
                        m_isWaitingForReactionQueue = true;
                        ReactionQueueController.Instance.QueueReaction(this, PerformDefenderAction);
                    }else if (IsBallInRangeWithThrowCondition(overwatchRadius,false))
                    {
                        m_isWaitingForReactionQueue = true;
                        ReactionQueueController.Instance.QueueReaction(this, PerformPlaymakerAction);
                    }
                    break;
            }
        }

        public void SetAbleToReact(bool _isAble)
        {
            m_canPerformReaction = _isAble;
            DisplayIndicator(_isAble);
        }

        private void PerformDefenderAction()
        {
            m_isPerformingReaction = true;
            m_isWaitingForReactionQueue = false;

            if (!IsBallInRangeWithThrowCondition(overwatchRadius, true))
            {
                m_isPerformingReaction = false;
                return;
            }
            
            StartCoroutine(C_AttemptGrabBall());
        }
        
        private void PerformPlaymakerAction()
        {
            m_isPerformingReaction = true;
            m_isWaitingForReactionQueue = false;

            if (!IsBallInRangeWithThrowCondition(overwatchRadius,false))
            {
                m_isPerformingReaction = false;
                return;
            }
            
            StartCoroutine(C_AttemptGrabBall());
        }
        
        private void PerformBruiserAction()
        {
            m_isPerformingReaction = true;
            m_isWaitingForReactionQueue = false;

            if (!IsEnemyWalkInRange())
            {
                m_isPerformingReaction = false;
                return;
            }
            
            StartCoroutine(C_AttemptAttackDamagePlayer());
        }
        
        private void PerformTankAction()
        {
            m_isPerformingReaction = true;
            m_isWaitingForReactionQueue = false;

            if (!IsEnemyWalkInRange())
            {
                m_isPerformingReaction = false;
                return;
            }
            
            StartCoroutine(C_AttemptAttackStopPlayer());
        }

        public bool CheckStealBall(CharacterBase attackedCharacter)
        {
            if (attackedCharacter.IsNull())
            {
                return false;
            }

            if (this.assignedClass.classType != CharacterClass.PLAYMAKER)
            {
                return false;
            }

            if (m_hasPerformedReaction)
            {
                return false;
            }

            var playmakerStat = GetRandomAgilityStat();

            var defendingCharStat = attackedCharacter.characterClassManager.GetRandomAgilityStat();

            if (playmakerStat >= defendingCharStat)
            {
                OnPassedReaction();
                HasPerformedReaction();
                return true;
            }
            else
            {
                attackedCharacter.characterClassManager.OnPassedReaction();
                OnFailedReaction();
            }

            HasPerformedReaction();
            return false;
        }

        private IEnumerator C_AttemptGrabBall()
        {
            ball.SetBallPause(true);

            var originalPosition = transform.position.FlattenVector3Y();
            
            var rollToGrab = GetRandomAgilityStat();

            bool isPlayer = characterBase.side.sideGUID == TurnController.Instance.playersSide.sideGUID;
            
            var _layer = isPlayer ? displayLayerVal : displayEnemyLayerVal;
            var oppositeLayer = isPlayer ? displayEnemyLayerVal : displayLayerVal;
           
            ChangeToVisualLayer(_layer);
            ball.SetVisualsToLayer(oppositeLayer);

            var _LCam = isPlayer ? reactionCameraPoint : ball.ballCamPoint;
            
            var _RCam = isPlayer ? ball.ballCamPoint : reactionCameraPoint;

            int _LValue = isPlayer ? rollToGrab : (int)ball.thrownBallStat;

            int _RValue = isPlayer ? (int)ball.thrownBallStat : rollToGrab;

            yield return StartCoroutine(JuiceController.Instance.C_DoReactionAnimation(_LCam, _RCam ,_LValue, _RValue, assignedClass.classType, isPlayer));

            if (ball.thrownBallStat > rollToGrab)
            {
                //Didn't intercept ball
                Debug.Log($"<color=orange>BIG MISS ON PASS INTERCEPT /// Ball: {ball.thrownBallStat} // Self: {rollToGrab}</color>", this);
                
                ChangeToVisualLayer(charLayerVal);
                ball.SetVisualsToLayer(charLayerVal);
                
                ball.SetBallPause(false);
                HasPerformedReaction();
                yield break;
            }

            ChangeToVisualLayer(charLayerVal);
            ball.SetVisualsToLayer(charLayerVal);
            
            //Grab ball
            Debug.Log($"<color=orange>HAS HIT PASS INTERCEPT REACTION /// Ball: {ball.thrownBallStat} // Self: {rollToGrab}</color>", this);
            characterBase.characterMovement.SetCharacterMovable(true, null, HasPerformedReaction);
            characterBase.CheckAllAction(ball.transform.position, true);
            
            yield return new WaitUntil(() => characterBase.characterMovement.isUsingMoveAction == false);

            ball.SetBallPause(false);
            
            if (characterBase.isGoalieCharacter)
            {
                yield return new WaitForSeconds(0.3f);
                
                characterBase.characterMovement.SetCharacterMovable(true, null, null);
                characterBase.characterMovement.MoveCharacter(originalPosition, true);

                yield return new WaitUntil(() => characterBase.characterMovement.isUsingMoveAction == false);
            }
        }

        private IEnumerator C_AttemptAttackDamagePlayer()
        {
            var rollToAttack = GetRandomDamageStat();
            
            Debug.Log($"{m_inRangeCharacter.name} /// Class:{m_inRangeCharacter.characterClassManager.assignedClass.classType.ToString()}", m_inRangeCharacter);
            
            m_inRangeCharacter.characterMovement.PauseMovement(true);
            
            var enemyAttackRoll = m_inRangeCharacter.characterClassManager.GetRandomDamageStat();
            
            bool isPlayer = characterBase.side == TurnController.Instance.playersSide;

            var _layer = isPlayer ? displayLayerVal : displayEnemyLayerVal;
            var oppositeLayer = isPlayer ? displayEnemyLayerVal : displayLayerVal;
            
            ChangeToVisualLayer(_layer);
            m_inRangeCharacter.characterClassManager.ChangeToVisualLayer(oppositeLayer);

            var _LCam = isPlayer ? reactionCameraPoint : m_inRangeCharacter.characterClassManager.reactionCameraPoint;
            
            var _RCam = isPlayer ? m_inRangeCharacter.characterClassManager.reactionCameraPoint : reactionCameraPoint;
            
            int _LValue = isPlayer ? rollToAttack : enemyAttackRoll;

            int _RValue = isPlayer ? enemyAttackRoll : rollToAttack;

            yield return StartCoroutine(JuiceController.Instance.C_DoReactionAnimation(_LCam, _RCam, _LValue, _RValue, assignedClass.classType, isPlayer));
            
            //Missed Attack
            if (enemyAttackRoll > rollToAttack)
            {
                Debug.Log($"<color=orange>BIG MISS ON ATTACK /// Other: {enemyAttackRoll} // Self: {rollToAttack}</color>", this);
                m_inRangeCharacter.characterMovement.PauseMovement(false);
                m_inRangeCharacter.characterClassManager.OnPassedReaction();

                ChangeToVisualLayer(charLayerVal);
                m_inRangeCharacter.characterClassManager.ChangeToVisualLayer(charLayerVal);

                OnMissedReaction();
                HasPerformedReaction();
                yield break;
            }
            
            Debug.Log("<color=orange>HAS HIT ATTACK REACTION</color>", this);
            
            ChangeToVisualLayer(charLayerVal);
            m_inRangeCharacter.characterClassManager.ChangeToVisualLayer(charLayerVal);

            yield return new WaitUntil(() => !m_inRangeCharacter.characterMovement.isMoving);

            //Reroll if striker
            if (m_inRangeCharacter.characterClassManager.assignedClass.classType == CharacterClass.STRIKER && !m_inRangeCharacter.characterClassManager.hasPerformedReaction)
            {
                //re-roll
                Debug.Log("<color=cyan>DOING REROLL</color>");
                
                var enemyAgilityRoll = m_inRangeCharacter.characterClassManager.GetReroll();

                //Character has not yet used reroll
                if (enemyAgilityRoll != 0)
                {
                    _LValue = isPlayer ? rollToAttack : enemyAgilityRoll;

                    _RValue = isPlayer ? enemyAgilityRoll : rollToAttack;
                    
                    ChangeToVisualLayer(_layer);
                    m_inRangeCharacter.characterClassManager.ChangeToVisualLayer(oppositeLayer);

                    yield return StartCoroutine(JuiceController.Instance.C_DoReactionAnimation(_LCam, _RCam,_LValue, _RValue,  assignedClass.classType, isPlayer));

                    //Missed On Reroll
                    if (enemyAgilityRoll > rollToAttack)
                    {
                        Debug.Log("<color=orange>Striker Rerolled and WON!</color>", this);
                        m_inRangeCharacter.characterMovement.PauseMovement(false);
                        m_inRangeCharacter.characterClassManager.OnPassedReaction();

                        ChangeToVisualLayer(charLayerVal);
                        m_inRangeCharacter.characterClassManager.ChangeToVisualLayer(charLayerVal);

                        OnMissedReaction();
                        HasPerformedReaction();
                        yield break;
                    }
                }
                
            }
            
            ChangeToVisualLayer(charLayerVal);
            m_inRangeCharacter.characterClassManager.ChangeToVisualLayer(charLayerVal);

            yield return new WaitForSeconds(0.5f);
            
            m_inRangeCharacter.OnDealDamage(this.transform, assignedCharacterStats.GetTackleDamage(), false, null, null, false);
            
            m_inRangeCharacter.characterMovement.PauseMovement(false);

            m_inRangeCharacter.characterClassManager.OnFailedReaction();

            DisplayIndicator(false);

            m_inRangeCharacter = null;

            OnSuccessfulMeleeReaction();
        }

        private IEnumerator C_AttemptAttackStopPlayer()
        {
            var rollToAttack = GetRandomDamageStat();

            var originalPosition = transform.position.FlattenVector3Y();

            Debug.Log($"{m_inRangeCharacter.name} /// Class:{m_inRangeCharacter.characterClassManager.assignedClass.classType.ToString()}", m_inRangeCharacter);
            
            m_inRangeCharacter.characterMovement.PauseMovement(true);
            
            var enemyAttackRoll = m_inRangeCharacter.characterClassManager.GetRandomDamageStat();
            
            bool isPlayer = characterBase.side == TurnController.Instance.playersSide;

            var _layer = isPlayer ? displayLayerVal : displayEnemyLayerVal;
            var oppositeLayer = isPlayer ? displayEnemyLayerVal : displayLayerVal;
            
            ChangeToVisualLayer(_layer);
            m_inRangeCharacter.characterClassManager.ChangeToVisualLayer(oppositeLayer);

            var _LCam = isPlayer ? reactionCameraPoint : m_inRangeCharacter.characterClassManager.reactionCameraPoint;
            
            var _RCam = isPlayer ? m_inRangeCharacter.characterClassManager.reactionCameraPoint : reactionCameraPoint;
            
            int _LValue = isPlayer ? rollToAttack : enemyAttackRoll;

            int _RValue = isPlayer ? enemyAttackRoll : rollToAttack;

            yield return StartCoroutine(JuiceController.Instance.C_DoReactionAnimation(_LCam, _RCam, _LValue, _RValue, assignedClass.classType, isPlayer));
            
            //Missed Attack
            if (enemyAttackRoll > rollToAttack)
            {
                Debug.Log($"<color=orange>BIG MISS ON ATTACK /// Other: {enemyAttackRoll} // " +
                          $"Self: {rollToAttack}</color>", this);
                m_inRangeCharacter.characterMovement.PauseMovement(false);
                m_inRangeCharacter.characterClassManager.OnPassedReaction();

                ChangeToVisualLayer(charLayerVal);
                m_inRangeCharacter.characterClassManager.ChangeToVisualLayer(charLayerVal);

                OnMissedReaction();
                HasPerformedReaction();
                yield break;
            }
            
            Debug.Log("<color=orange>HAS HIT ATTACK REACTION</color>", this);
            
            ChangeToVisualLayer(charLayerVal);
            m_inRangeCharacter.characterClassManager.ChangeToVisualLayer(charLayerVal);

            yield return new WaitUntil(() => !m_inRangeCharacter.characterMovement.isMoving);

            //Reroll if striker
            if (m_inRangeCharacter.characterClassManager.assignedClass.classType == CharacterClass.STRIKER && !m_inRangeCharacter.characterClassManager.hasPerformedReaction)
            {
                //re-roll
                
                Debug.Log("<color=cyan>DOING REROLL</color>");
                
                var enemyAgilityRoll = m_inRangeCharacter.characterClassManager.GetReroll();

                //Character has not yet used reroll
                if (enemyAgilityRoll != 0)
                {
                    _LValue = isPlayer ? rollToAttack : enemyAgilityRoll;

                    _RValue = isPlayer ? enemyAgilityRoll : rollToAttack;
                    
                    ChangeToVisualLayer(_layer);
                    m_inRangeCharacter.characterClassManager.ChangeToVisualLayer(oppositeLayer);

                    yield return StartCoroutine(JuiceController.Instance.C_DoReactionAnimation(_LCam, _RCam,_LValue, _RValue,  assignedClass.classType, isPlayer));

                    //Missed On Reroll
                    if (enemyAgilityRoll > rollToAttack)
                    {
                        Debug.Log("<color=orange>Striker Rerolled and WON!</color>", this);
                        m_inRangeCharacter.characterMovement.PauseMovement(false);
                        m_inRangeCharacter.characterClassManager.OnPassedReaction();

                        ChangeToVisualLayer(charLayerVal);
                        m_inRangeCharacter.characterClassManager.ChangeToVisualLayer(charLayerVal);

                        OnMissedReaction();
                        HasPerformedReaction();
                        yield break;
                    }
                }
                
            }
            
            ChangeToVisualLayer(charLayerVal);
            m_inRangeCharacter.characterClassManager.ChangeToVisualLayer(charLayerVal);

            yield return new WaitForSeconds(0.5f);
            
            m_inRangeCharacter.characterMovement.ForceStopMovement(true);

            m_inRangeCharacter.characterClassManager.OnFailedReaction();

            DisplayIndicator(false);
            
            characterBase.characterMovement.SetCharacterMovable(true, null, HasPerformedReaction);
            characterBase.characterMovement.MoveCharacter(m_inRangeCharacter.transform.position, true);
            
            m_inRangeCharacter = null;
            
            yield return new WaitUntil(() => characterBase.characterMovement.isUsingMoveAction == false);

            OnSuccessfulMeleeReaction();

            if (!characterBase.isGoalieCharacter)
            {
                yield break;
            }
            
            yield return new WaitForSeconds(0.3f);

            characterBase.characterMovement.SetCharacterMovable(true, null, null);
            characterBase.characterMovement.MoveCharacter(originalPosition, true);

            yield return new WaitUntil(() => characterBase.characterMovement.isUsingMoveAction == false);

        }

        public void ResetInfluence()
        {
            hasAddedToBallThrowVelocity = false;
            ballHasExitedRange = false;
            DisplayIndicator(true);
        }

        private void CheckInfluence()
        {
            if (characterBase.characterMovement.isKnockedBack)
            {
                return;
            }

            if (!IsBallInRange(passiveRadius))
            {
                if (!ballHasEnteredPassiveRange || ballHasExitedRange) return;
                ball.RemoveInfluence(characterBase);
                ballHasExitedRange = true;
                ballHasEnteredPassiveRange = false;
                return;
            }

            if (!hasAddedToBallThrowVelocity)
            {
                ChangeBallVelocity();
            }

            var directionOffset = ball.lastThrownCharacter.side.sideGUID == characterBase.side.sideGUID ? -1 : 1;
            var directionToBall = (ball.transform.position - transform.position);
            var influenceVector = 
                directionToBall.normalized / directionToBall.sqrMagnitude * (ballInfluenceIntensity * directionOffset); 
            ball.AddInfluence(characterBase, influenceVector);
        }

        private void ChangeBallVelocity()
        {
            ball.ChangeBallIntensity(characterBase.side, ballChangeVelocityAmount).Forget();
            hasAddedToBallThrowVelocity = true;
            ballHasEnteredPassiveRange = true;
        }

        private bool IsBallInRange(float checkRange)
        {
            if (characterBase.isBusy)
            {
                return false;
            }
            
            if (!ball.canBeCaught)
            {
                return false;
            }

            if (!ball.isMoving)
            {
                return false;
            }

            if (ball.lastHeldCharacter == this.characterBase)
            {
                return false;
            }

            if (!ball.currentOwner.IsNull())
            {
                return false;
            }
            
            var directionToBall = ball.transform.position - transform.position;
            directionToBall.FlattenVector3Y();

            return directionToBall.magnitude <= checkRange;
        }
        
        private bool IsBallInRangeWithThrowCondition(float checkRange, bool ballThrownCondition)
        {
            return IsBallInRange(checkRange) && ball.isThrown == ballThrownCondition;
        }

        private bool IsEnemyWalkInRange()
        {
            if (characterBase.isBusy)
            {
                return false;
            }
            
            characterInRangeAmount = Physics.OverlapSphereNonAlloc(transform.position, overwatchRadius, charactersInRange, characterLayers);

            if (characterInRangeAmount <= 0)
            {
                return false;
            }
            
            foreach (var col in charactersInRange)
            {
                col.TryGetComponent(out CharacterBase character);
                
                if (!character)
                {
                    continue;
                }

                if (character.side == this.characterBase.side)
                {
                    continue;
                }
                
                if (!character.characterMovement.isMoving)
                {
                    continue;
                }

                if (character == this.characterBase)
                {
                    continue;
                }
                
                m_inRangeCharacter = character;
                return true;
            }

            return false;
        }
        
        private bool IsEnemyWalkInRangeLimited()
        {
            if (characterBase.isBusy)
            {
                return false;
            }
            
            Collider[] colliders = Physics.OverlapSphere(transform.position, passiveRadius);

            if (colliders.Length > 0)
            {
                foreach (var col in colliders)
                {
                    col.TryGetComponent(out CharacterBase character);
                    if (!character)
                    {
                        continue;
                    }

                    if (!character.characterMovement.isMoving)
                    {
                        continue;
                    }

                    if (character.side == this.characterBase.side)
                    {
                        continue;
                    }
                    
                    if (character == this.characterBase)
                    {
                        continue;
                    }

                    var dirToCharacter = character.transform.position - transform.position;
                    var dot = Vector3.Dot(transform.forward.normalized, dirToCharacter.normalized);

                    //if the distance is super close, instant return true
                    if (dirToCharacter.magnitude <= 0.5f)
                    {
                        m_inRangeCharacter = character;
                        return true;
                    }
                    
                    //if the character is behind this character, ignore it. Unless it's close
                    if (dot < 0)
                    {
                        continue;
                    }
                    
                    m_inRangeCharacter = character;
                    return true;
                }
            }
            
            return false;
        }

        private void GetClosestTarget()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, passiveRadius);

            if (colliders.Length > 0)
            {
                foreach (var col in colliders)
                {
                    var character = col.GetComponent<CharacterBase>();
                    if (!character.IsNull() && character.characterMovement.isMoving && character.side != this.characterBase.side &&
                        character != this.characterBase)
                    {
                        m_inRangeCharacter =  character;
                    }
                }
            }
        }

        private void OnFailedReaction()
        {
            failedReactionCounter++;
        }

        private void OnPassedReaction()
        {
            failedReactionCounter = 0;
        }

        private void OnMissedReaction()
        {
            m_inRangeCharacter = null;
            failedReactionCounter++;

            successfulMeleeReactionCounter = 1;
        }

        private void OnSuccessfulMeleeReaction()
        {
            successfulMeleeReactionCounter++;
        }

        private void HasPerformedReaction()
        {
            m_hasPerformedReaction = true;
            m_isPerformingReaction = false;
            DisplayIndicator(false);
        }

        private void DisplayIndicator(bool _display)
        {
            m_passiveIndicator.SetActive(_display);
        }

        private void SuccessfulMeleeReaction()
        {
            m_hasPerformedReaction = false;
            m_isPerformingReaction = false;
            DisplayIndicator(true);
        }

        public int GetRandomAgilityStat()
        {
            if (assignedClass.classType == CharacterClass.ALL)
            {
                return bottomRange;
            }

            if (isPlayer)
            {
                if (failedReactionCounter >= 2)
                {
                    return currentMaxAgilityScore;
                }

                var adjustedBottomRange = Mathf.RoundToInt(currentMaxAgilityScore * bottomRangeMod);
                return Random.Range(adjustedBottomRange, currentMaxAgilityScore);
            }
            
            return Random.Range(bottomRange, currentMaxAgilityScore);
        }
        
        public int GetRandomShootStat()
        {
            if (assignedClass.classType == CharacterClass.ALL)
            {
                return bottomRange;
            }
            
            if (isPlayer)
            {
                if (failedReactionCounter >= 2)
                {
                    return currentMaxThrowingScore;
                }

                var adjustedBottomRange = Mathf.RoundToInt(currentMaxThrowingScore * bottomRangeMod);
                return Random.Range(adjustedBottomRange, currentMaxThrowingScore);
            }

            return Random.Range(bottomRange, currentMaxThrowingScore);
        }
        
        public int GetRandomDamageStat()
        {
            if (assignedClass.classType == CharacterClass.ALL)
            {
                return bottomRange;
            }
            
            if (isPlayer)
            {
                if (failedReactionCounter >= 2)
                {
                    return currentMaxTacklingScore;
                }
                
                var adjustedBottomRange = Mathf.RoundToInt(currentMaxTacklingScore * bottomRangeMod);
                return Random.Range(adjustedBottomRange, currentMaxTacklingScore);
            }
            
            return Random.Range(bottomRange, (currentMaxTacklingScore/successfulMeleeReactionCounter));
        }

        public int GetRandomPassingStat()
        {
            if (assignedClass.classType == CharacterClass.ALL)
            {
                return bottomRange;
            }
            
            if (isPlayer)
            {
                if (failedReactionCounter >= 2)
                {
                    return currentMaxThrowingScore;
                }

                var adjustedBottomRange = Mathf.RoundToInt(currentMaxThrowingScore * bottomRangeMod);
                return Random.Range(adjustedBottomRange, currentMaxThrowingScore);
            }
            
            return Random.Range(bottomRange, currentMaxThrowingScore);
        }

        public int GetMaxDamageStat()
        {
            return currentMaxTacklingScore;
        }

        public int GetReroll()
        {
            if (m_hasPerformedReaction || this.assignedClass.classType != CharacterClass.STRIKER)
            {
                Debug.LogError($"Something Wrong with class,{assignedClass.classType}");
                return 0;
            }
            
            HasPerformedReaction();

            return GetRandomAgilityStat();
        }

        public void ChangeMaxScore(CharacterStatsEnum _desiredStat, int newAmount)
        {
            Mathf.Clamp(newAmount,0, 100);
            switch (_desiredStat)
            {
                case CharacterStatsEnum.AGILITY:
                    currentMaxAgilityScore = newAmount;
                    break;
                case CharacterStatsEnum.THROW:
                    currentMaxThrowingScore = newAmount;
                    break;
                case CharacterStatsEnum.TACKLE:
                    currentMaxTacklingScore = newAmount;
                    break;
                case CharacterStatsEnum.GRAVITY:
                    //currentMaxPassingScore = newAmount;
                    break;
            }
        }

        public void ChangeToVisualLayer(LayerMask _mask)
        {
            characterBase.characterVisuals.SetNewLayer(_mask);
        }

        public void ChangeToDisplayLayer()
        {
            bool isPlayer = characterBase.side == TurnController.Instance.playersSide;
            var _layer = isPlayer ? displayLayerVal : displayEnemyLayerVal;
            characterBase.characterVisuals.SetNewLayer(_layer);
        }

        public void ChangeToNormalLayer()
        {
            characterBase.characterVisuals.SetNewLayer(charLayerVal);
        }

        public void ResetMaxScores()
        {
            currentMaxAgilityScore = agilityScore;
            currentMaxThrowingScore = throwingScore;
            currentMaxTacklingScore = tacklingScore;
        }
        #endregion

    }
}