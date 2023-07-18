using System;
using System.Collections;
using Data;
using Data.Sides;
using Project.Scripts.Utils;
using Runtime.GameControllers;
using Runtime.Gameplay;
using TMPro;
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

        private int m_agilityScore;
        
        private int m_shootingScore;
        
        private int m_tacklingScore;
        
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

        #endregion

        #region Unity Events

        private void OnEnable()
        {
            TurnController.OnChangeActiveTeam += SetCharacterPassive;
        }

        private void OnDisable()
        {
            TurnController.OnChangeActiveTeam += SetCharacterPassive;
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
            
            m_agilityScore = agilityScore;
            m_shootingScore = shootingScore;
            m_tacklingScore = tacklingScore;
            
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
            switch (m_assignedClass.classType)
            {
                case CharacterClass.DEFENDER:
                    if (IsBallThrownInRange())
                    {
                        Debug.Log("Ball In Range");
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

            yield return new WaitForSeconds(1f);

            if (ball.thrownBallStat >= rollToGrab)
            {
                //Didn't intercept ball
                //ToDo: Miss Animation, restart ball regular movement
                Debug.Log($"<color=orange>BIG MISS ON PASS INTERCEPT /// Ball: {ball.thrownBallStat} // Self: {rollToGrab}</color>", this);
                ball.SetBallPause(false);
                HasPerformedReaction();
                yield break;
            }

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

            while (m_inRangeCharacter.IsNull())
            {
                GetClosestTarget();
            }

            yield return new WaitUntil(() => !m_inRangeCharacter.IsNull());
            
            m_inRangeCharacter.characterMovement.PauseMovement(true);
            
            var enemyAttackRoll = m_inRangeCharacter.characterClassManager.GetRandomDamageStat();
            
            if (enemyAttackRoll >= rollToAttack)
            {
                //ToDo: Miss attack on moving character, trip don't fall?
                Debug.Log($"<color=orange>BIG MISS ON ATTACK /// Other: {enemyAttackRoll} // Self: {rollToAttack}</color>", this);
                m_inRangeCharacter.characterMovement.PauseMovement(false);
                OnMissedReaction();
                HasPerformedReaction();
                yield break;
            }
            
            //ToDo: Attack moving character
            Debug.Log("<color=orange>HAS HIT ATTACK REACTION</color>", this);

            m_inRangeCharacter.characterMovement.ForceStopMovement();

            yield return new WaitUntil(() => !m_inRangeCharacter.characterMovement.isMoving);

            if (m_inRangeCharacter.assignedClass == CharacterClass.STRIKER)
            {
                //re-roll
                var enemyAgilityRoll = m_inRangeCharacter.characterClassManager.GetReroll();
                if (enemyAgilityRoll >= rollToAttack)
                {
                    Debug.Log("<color=orange>Striker Rerolled and WON!</color>", this);
                    OnMissedReaction();
                    HasPerformedReaction();
                    yield break;
                }
            }

            yield return new WaitForSeconds(0.5f);
            
            characterBase.characterMovement.SetCharacterMovable(true, null, HasPerformedReaction);
            characterBase.CheckAllAction(m_inRangeCharacter.transform.position, true);
            
            yield return new WaitUntil(() => characterBase.characterMovement.isUsingMoveAction == false);

            m_inRangeCharacter = null;
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
                    var character = col.GetComponent<CharacterBase>();
                    if (!character.IsNull() && character.characterMovement.isMoving && character.side != this.characterBase.side && 
                        character != this.characterBase)
                    {
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
            
        }

        private void HasPerformedReaction()
        {
            m_hasPerformedReaction = true;
            m_isPerformingReaction = false;
            m_passiveIndicator.SetActive(false);
        }

        public int GetRandomAgilityStat()
        {
            return Random.Range(1, m_agilityScore);
        }
        
        public int GetRandomShootStat()
        {
            return Random.Range(1, m_shootingScore);
        }
        
        public int GetRandomDamageStat()
        {
            return Random.Range(1, m_tacklingScore);
        }

        public int GetReroll()
        {
            if (m_hasPerformedReaction)
            {
                return 0;
            }
            
            HasPerformedReaction();

            return GetRandomAgilityStat();
        }
        #endregion

    }
}