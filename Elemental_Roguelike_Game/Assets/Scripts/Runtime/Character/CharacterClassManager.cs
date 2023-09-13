using System;
using System.Collections;
using Data;
using Data.CharacterData;
using Data.Sides;
using Project.Scripts.Utils;
using Runtime.GameControllers;
using Runtime.Gameplay;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Runtime.Character
{
    
    [RequireComponent(typeof(CharacterBase))]
    [DisallowMultipleComponent]
    public class CharacterClassManager: MonoBehaviour
    {

        #region Read-Only

        private static readonly int m_colorVarName = Shader.PropertyToID("_EmisColor");

        #endregion

        #region Serialized Fields

        [SerializeField] private GameObject m_passiveIndicator;

        [SerializeField] private Transform m_reactionCameraPoint;

        #endregion

        #region Private Fields

        private CharacterBase m_characterBase;

        private bool isCheckingPassive;

        private CharacterClassData m_assignedClass;

        private float passiveRadius;

        private BallBehavior m_ballRef;

        private bool m_isPerformingReaction;

        private bool m_hasPerformedReaction;

        private CharacterBase m_inRangeCharacter;

        private bool m_canPerformReaction = true;

        #endregion
        
        #region Accessors

        public CharacterBase characterBase => CommonUtils.GetRequiredComponent(ref m_characterBase, () =>
        {
            var cb = GetComponent<CharacterBase>();
            return cb;
        });

        public CharacterClassData assignedClass => m_assignedClass;
        
        public BallBehavior ball => CommonUtils.GetRequiredComponent(ref m_ballRef, () =>
        {
            var bb = TurnController.Instance.ball;
            return bb;
        });

        public int agilityScore { get; private set; }

        public int shootingScore { get; private set; }
        
        public int tacklingScore { get; private set; }
        
        public int currentMaxAgilityScore { get; private set; }
        
        public int currentMaxShootingScore { get; private set; }
        
        public int currentMaxTacklingScore { get; private set; }

        public Transform reactionCameraPoint => m_reactionCameraPoint;

        private LayerMask displayLayerVal => LayerMask.NameToLayer("DISPLAY");
        
        private LayerMask displayEnemyLayerVal => LayerMask.NameToLayer("DISPLAY_ENEMY");

        private LayerMask charLayerVal => LayerMask.NameToLayer("CHARACTER");

        #endregion

        #region Unity Events

        private void OnEnable()
        {
            TurnController.OnChangeActiveTeam += SetCharacterPassive;
        }

        private void OnDisable()
        {
            TurnController.OnChangeActiveTeam -= SetCharacterPassive;
        }

        private void Update()
        {
            if (!isCheckingPassive)
            {
                return;
            }

            if (m_isPerformingReaction || m_hasPerformedReaction)
            {
                return;
            }
            
            Check();
            
        }

        #endregion

        #region Class Implementation

        public void InitializedCharacterPassive(CharacterClassData _data, int agilityScore, int shootingScore, int tacklingScore)
        {
            if (_data.IsNull())
            {
                Debug.LogError("Data null");
                return;
            }
            
            m_assignedClass = _data;
            passiveRadius = _data.radius;
            
            this.agilityScore = agilityScore;
            this.shootingScore = shootingScore;
            this.tacklingScore = tacklingScore;

            currentMaxAgilityScore = this.agilityScore;
            currentMaxShootingScore = this.shootingScore;
            currentMaxTacklingScore = this.tacklingScore;
            
            if (m_passiveIndicator.IsNull())
            {
                return;
            }

            //Radius is 1/2 scale, scale is diameter
            var doubleRadiusSize = passiveRadius * 2;
            m_passiveIndicator.transform.localScale =
                new Vector3(doubleRadiusSize, doubleRadiusSize, doubleRadiusSize);
            
            var meshRend = m_passiveIndicator.GetComponent<MeshRenderer>();
            
            var indicatorMat = meshRend.materials[0];
            
            indicatorMat.SetColor(m_colorVarName, _data.passiveColor);
            
            m_passiveIndicator.SetActive(false);
        }

        public void SetCharacterPassive(CharacterSide _side)
        {
            var _isActiveTeam = characterBase.side == _side;
            if (assignedClass.classType != CharacterClass.STRIKER)
            {
                isCheckingPassive = !_isActiveTeam;
                if (!_isActiveTeam)
                {
                    m_isPerformingReaction = false;
                    m_hasPerformedReaction = false;
                }
                if (!m_passiveIndicator.IsNull())
                {
                    m_passiveIndicator.SetActive(!_isActiveTeam);
                }    
            }
            else
            {
                if (_isActiveTeam)
                {
                    m_isPerformingReaction = false;
                    m_hasPerformedReaction = false;   
                }
                if (!m_passiveIndicator.IsNull())
                {
                    m_passiveIndicator.SetActive(_isActiveTeam);
                }    
            }
            
        }

        private void Check()
        {
            if (m_isPerformingReaction || m_hasPerformedReaction)
            {
                return;
            }
            
            switch (m_assignedClass.classType)
            {
                case CharacterClass.DEFENDER:
                    if (IsBallThrownInRange())
                    {
                        m_isPerformingReaction = true;
                        StartCoroutine(C_AttemptGrabBall());
                    }
                    break;
                case CharacterClass.BRUISER:
                    if (IsEnemyWalkInRange())
                    {
                        m_isPerformingReaction = true;
                        StartCoroutine(C_AttemptAttackPlayer());
                    }
                    break;
            }
        }


        private IEnumerator C_AttemptGrabBall()
        {
            ball.SetBallPause(true);
            
            var rollToGrab = GetRandomAgilityStat();

            bool isPlayer = characterBase.side == TurnController.Instance.playersSide;
            
            var _layer = isPlayer ? displayLayerVal : displayEnemyLayerVal;
            var oppositeLayer = isPlayer ? displayEnemyLayerVal : displayLayerVal;
           
            ChangeToVisualLayer(_layer);
            ball.SetVisualsToLayer(oppositeLayer);

            var _LCam = isPlayer ? reactionCameraPoint : ball.ballCamPoint;
            
            var _RCam = isPlayer ? ball.ballCamPoint : reactionCameraPoint;

            int _LValue = isPlayer ? rollToGrab : (int)ball.thrownBallStat;

            int _RValue = isPlayer ? (int)ball.thrownBallStat : rollToGrab;

            yield return StartCoroutine(JuiceController.Instance.DoReactionAnimation(_LCam, _RCam ,_LValue, _RValue));

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
        }

        private IEnumerator C_AttemptAttackPlayer()
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

            yield return StartCoroutine(JuiceController.Instance.DoReactionAnimation(_LCam, _RCam, _LValue, _RValue));
            
            //Missed Attack
            if (enemyAttackRoll > rollToAttack)
            {
                Debug.Log($"<color=orange>BIG MISS ON ATTACK /// Other: {enemyAttackRoll} // Self: {rollToAttack}</color>", this);
                m_inRangeCharacter.characterMovement.PauseMovement(false);

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
            if (m_inRangeCharacter.characterClassManager.assignedClass.classType == CharacterClass.STRIKER)
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

                    yield return StartCoroutine(JuiceController.Instance.DoReactionAnimation(_LCam, _RCam,_LValue, _RValue));

                    //Missed On Reroll
                    if (enemyAgilityRoll > rollToAttack)
                    {
                        Debug.Log("<color=orange>Striker Rerolled and WON!</color>", this);
                        m_inRangeCharacter.characterMovement.PauseMovement(false);
                        
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
            
            characterBase.characterMovement.SetCharacterMovable(true, null, HasPerformedReaction);
            characterBase.CheckAllAction(m_inRangeCharacter.transform.position, true);
            
            m_inRangeCharacter = null;
            
            yield return new WaitUntil(() => characterBase.characterMovement.isUsingMoveAction == false);

        }

        private bool IsBallThrownInRange()
        {
            var directionToBall = ball.transform.position - transform.position;
            directionToBall.FlattenVector3Y();

            if (directionToBall.magnitude < passiveRadius)
            {
                if (ball.isThrown)
                {
                    return true;
                }
            }
            
            return false;
        }

        private bool IsEnemyWalkInRange()
        {
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
                    
                    if (character != this.characterBase)
                    {
                        m_inRangeCharacter = character;
                        return true;
                    }
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

        private void OnMissedReaction()
        {
            m_inRangeCharacter = null;
        }

        private void HasPerformedReaction()
        {
            m_hasPerformedReaction = true;
            m_isPerformingReaction = false;
            m_passiveIndicator.SetActive(false);
        }

        public int GetRandomAgilityStat()
        {
            return Random.Range(1, currentMaxAgilityScore);
        }
        
        public int GetRandomShootStat()
        {
            return Random.Range(1, currentMaxShootingScore);
        }
        
        public int GetRandomDamageStat()
        {
            return Random.Range(1, currentMaxTacklingScore);
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
                case CharacterStatsEnum.SHOOTING:
                    currentMaxShootingScore = newAmount;
                    break;
                case CharacterStatsEnum.TACKLE:
                    currentMaxTacklingScore = newAmount;
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
            currentMaxShootingScore = shootingScore;
            currentMaxTacklingScore = tacklingScore;
        }
        #endregion

    }
}