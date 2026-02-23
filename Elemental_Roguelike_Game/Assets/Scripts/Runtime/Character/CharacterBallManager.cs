using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Data.CharacterData;
using NUnit.Framework;
using Project.Scripts.Utils;
using Runtime.GameControllers;
using Runtime.Gameplay;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Runtime.Character
{
    public class CharacterBallManager: MonoBehaviour, IBallInteractable
    {

        #region Read-Only

        private readonly string handPosTransformName = "BallHoldPos@Transform";

        #endregion
        
        #region Actions

        public static event Action<CharacterBase> BallPickedUp;

        public static event Action<CharacterBase> BallThrown;

        #endregion
        
        #region Serialized Fields

        [SerializeField] private float m_shotDistanceBase = 1f;

        [SerializeField] private float m_maxShotDistance = 6f;

        [SerializeField] private Transform m_handPos;
        
        [SerializeField] protected GameObject ballOwnerIndicator;

        [SerializeField] protected LineRenderer ballThrowIndicator;

        [SerializeField] private LayerMask goalLayer, wallLayer, playerCheckLayer;

        [SerializeField] private float throwIndicatorYOffset = -0.49f;

        #endregion

        #region Private Fields

        private CharacterBase m_characterBase;

        private int shootingScore, passingScore;
        private int shootingScoreOriginal, passingScoreOriginal;

        private int highestPossibleScore = 100;
        private BallBehavior m_heldBall;
        private bool m_canPickupBall = true;
        private Vector3 previousMarkPosition = Vector3.zero;
        private List<CharacterBase> characterRefs = new List<CharacterBase>();
        private float influenceAdditionalOffset = 1.25f;
        private int amountOfCalculatedPoints = 40, amountOfShownPoints = 20;
        
        private Collider[] foundPlayers = new Collider[15];
        private int foundPlayersAmount;
        private float distanceBetweenPoints = 1f;

        #endregion

        #region Accessors
        
        public float shotStrength => m_shotDistanceBase + (shootingScore/100f) * m_maxShotDistance;

        public float passStrength => m_shotDistanceBase + (passingScore / 100f) * m_maxShotDistance;
        
        public CharacterBase characterBase => CommonUtils.GetRequiredComponent(
            ref m_characterBase, GetComponent<CharacterBase>);

        public List<CharacterBase> allCharacter
        {
            get => characterRefs;
            set => characterRefs = value;
        }

        public bool hasBall => !m_heldBall.IsNull();

        public bool canPickupBall => m_canPickupBall;
        
        #endregion

        #region Unity Events

        private void OnEnable()
        {
            TurnController.OnBattlePreStart += GetAllCharactersRef;
        }

        private void OnDisable()
        {
            TurnController.OnBattlePreStart -= GetAllCharactersRef;
        }

        #endregion

        #region IBallInteractalbe Inherited Methods

        BallBehavior IBallInteractable.heldBall
        {
            get => m_heldBall;
            set => m_heldBall = value;
        }

        bool IBallInteractable.canPickupBall
        {
            get => m_canPickupBall;
            set => m_canPickupBall = value;
        }

        public void PickUpBall(BallBehavior ball)
        {
            if (!m_canPickupBall)
            {
                return;
            }
            
            m_heldBall = ball;
            m_heldBall.SetFollowTransform(m_handPos, characterBase);
            ballOwnerIndicator.SetActive(true);
            BallPickedUp?.Invoke(characterBase);
        }

        public void DetachBall()
        {
            m_heldBall = null;
            ballOwnerIndicator.SetActive(false);
        }

        public void KnockBallAway(Transform attacker = null)
        {
            if (m_heldBall.IsNull())
            {
                return;
            }

            var randomPos = Random.insideUnitCircle;
            
            var direction = attacker.IsNull() ? 
                (transform.position + new Vector3(randomPos.x, 0 , randomPos.y) 
                - transform.position) : attacker.position - transform.position;
            
            Debug.Log($"[Ball Throw] Amount: {(shootingScore/100f)}");
            
            m_heldBall.ThrowBall(direction, (shootingScore/100f), false, this.characterBase, 0);
            
            m_heldBall = null;
            
            ballOwnerIndicator.SetActive(false);
        }

        public void ThrowBall(Vector3 _position)
        {
            if (m_heldBall.IsNull())
            {
                return;
            }
            
            m_heldBall.ThrowBall(CalculatePoints(m_heldBall.transform.position,
                (_position - transform.position).normalized, amountOfCalculatedPoints,
                distanceBetweenPoints, false)
                , (shootingScore/100f), characterBase);
            
            m_heldBall = null;
            ballOwnerIndicator.SetActive(false);
            
            if (!ballThrowIndicator.IsNull())
            {
                ballThrowIndicator.gameObject.SetActive(false);
            }
            
            BallThrown?.Invoke(characterBase);
        }

        public void ThrowBall(Vector3[] waypoints)
        {
            if (waypoints.IsNull() || waypoints.Length == 0)
            {
                return;
            }
            
            if (m_heldBall.IsNull())
            {
                return;
            }
            
            m_heldBall.ThrowBall(waypoints, (shootingScore/100f), characterBase);
            m_heldBall = null;
            
            ballOwnerIndicator.SetActive(false);
            if (!ballThrowIndicator.IsNull())
            {
                ballThrowIndicator.gameObject.SetActive(false);
            }
            
            BallThrown?.Invoke(characterBase);
        }

        #endregion

        #region Class Implementation


        public async UniTask Initialize(CharacterStatsBase _characterStats, Transform ballHandPos)
        {
            if (_characterStats.IsNull())
            { 
                return;   
            }

            shootingScoreOriginal = _characterStats.throwScore;
            shootingScore = shootingScoreOriginal;
            passingScoreOriginal = _characterStats.throwScore;
            passingScore = passingScoreOriginal;
            
            m_handPos = ballHandPos;

            m_canPickupBall = true;
            
            await UniTask.WaitForEndOfFrame();
        }

        private void GetAllCharactersRef()
        {
            characterRefs = TurnController.Instance.GetAllCharacters();
        }
        
        public void MarkThrowBall(Vector3 _position)
        {
            if (ballThrowIndicator.IsNull())
            {
                return;
            }

            if (VectorUtils.IsFastApproximate(_position.FlattenVector3Y(), previousMarkPosition.FlattenVector3Y(), 0.1f))
            {
                return;
            }

            previousMarkPosition = _position;
            
            ballThrowIndicator.SetPositions(CalculatePoints(m_heldBall.transform.position,
                (_position - transform.position).normalized, amountOfShownPoints, distanceBetweenPoints));
        }

        private Vector3[] CalculatePoints(Vector3 startPoint, Vector3 startDir, 
            int amountOfPoints, float distanceBetweenPoints, bool isStopAtEnemy = true)
        {
            var currentPoint = startPoint.FlattenVector3Y(transform.position.y);
            var currentDirection = startDir;

            List<Vector3> points = new() { currentPoint };

            for (int i = 0; i < amountOfPoints; i++)
            {
                var (influenceDir, continueIteration) = GetNextDirection(currentPoint, 
                    currentDirection);
                
                Debug.DrawRay(currentPoint, influenceDir, Color.magenta, 10f );
                
                if (!continueIteration && isStopAtEnemy)
                {
                    points.Add(currentPoint.FlattenVector3Y(transform.position.y));
                    continue; //aiming direction has passed through enemy zone
                }
                
                currentDirection = influenceDir.normalized * distanceBetweenPoints;

                var (hasHitWall, raycastHit) = IsWallInThrowDir(currentPoint ,currentDirection, distanceBetweenPoints);

                if (hasHitWall)
                {
                    currentPoint = raycastHit.point.FlattenVector3Y(transform.position.y);
                    points.Add(currentPoint);
                    currentDirection = Vector3.Reflect(currentDirection, raycastHit.normal);
                    continue;
                }
                
                currentPoint += currentDirection;
                currentPoint.FlattenVector3Y(transform.position.y);
                points.Add(currentPoint);
            }

            return points.ToArray();
        }

        private (Vector3, bool) GetNextDirection(Vector3 currentPoint, 
            Vector3 currentDirection)
        {
            if (!HasInfluencesAroundPoint(currentPoint, out var passableCharacters))
            {
                return (currentDirection,true);
            }

            var isContinueIteration = true;
            
            List<Vector3> influences = new List<Vector3>();
            
            foreach (var character in passableCharacters)
            {
                if (character.side.sideGUID != characterBase.side.sideGUID)
                {
                    isContinueIteration = false;
                    break; //stop iterating if the ball will pass through an enemy zone
                }
                
                var dirToCharacter = character.transform.position.FlattenVector3Y(transform.position.y) - currentPoint.FlattenVector3Y(transform.position.y);
                var influenceVector = 
                    (dirToCharacter.normalized / Mathf.Clamp(dirToCharacter.sqrMagnitude, 
                        SettingsController.GravDistClampMin, SettingsController.GravDistClampMax))
                    * (character.characterClassManager.ballInfluenceIntensity * influenceAdditionalOffset);
                Debug.DrawRay(currentPoint, influenceVector, Color.red, 10f);
                influences.Add(influenceVector);
            }
            
            return (new Vector3(currentDirection.x + influences.Sum(v => v.x), 0,
                currentDirection.z + influences.Sum(v => v.z)), isContinueIteration);
        }

        private (bool, RaycastHit) IsWallInThrowDir(Vector3 currentPosition, Vector3 dir, float wallRayLegnth)
        {
            var hasHit = Physics.Raycast(currentPosition, dir, out RaycastHit hit, wallRayLegnth, wallLayer);
            return (hasHit, hit);
        }
        
        public void TransferBall(CharacterBase _ballHolder, CharacterBase _ballStealer)
        {
            var _ball = TurnController.Instance.ball;
            _ballHolder.DetachBall();
            _ballStealer.PickUpBall(_ball);
        }

        public bool IsShot(Vector3 _endPos)
        {
            var _dir = _endPos - transform.position;
            var strongerStrength = shotStrength > passStrength ? shotStrength : passStrength;
            var _furthestPoint = transform.position + (_dir.normalized * strongerStrength);
            var _mag = _furthestPoint - transform.position;
            RaycastHit[] hits = Physics.RaycastAll(transform.position, _dir, _mag.magnitude, goalLayer, QueryTriggerInteraction.Collide);

            if (hits.Length == 0)
            {
                Debug.DrawLine(transform.position, _furthestPoint, Color.red);
                return false;
            }
            
            Debug.DrawLine(transform.position, _furthestPoint, Color.green);
            return true;
            
        }

        public void SetCanPickupBall(bool _canPickupBall)
        {
            m_canPickupBall = _canPickupBall;
        }

        public void DisplayThrowIndicator(bool _isOn)
        {
            ballThrowIndicator.gameObject.SetActive(_isOn);
        }

        public bool HasInfluencesAroundPoint(Vector3 point, out List<CharacterBase> influenceCharacters)
        {
            influenceCharacters = new List<CharacterBase>();
            
            foundPlayersAmount = Physics.OverlapSphereNonAlloc(point, 
                SettingsController.PassiveDistMax, foundPlayers ,playerCheckLayer);

            if (foundPlayersAmount == 0)
            {
                return false;
            }
            
            foreach (var collider in foundPlayers)
            {
                if (collider.IsNull())
                {
                    continue;
                }
                
                collider.TryGetComponent(out CharacterBase character);
                    
                if (character.IsNull())
                {
                    continue;
                }

                if (character == characterBase)
                {
                    continue;
                }

                if ((character.transform.position - point).sqrMagnitude > 
                    character.characterClassManager.passiveRadiusSqr)
                {
                    continue;
                }
                
                influenceCharacters.Add(character);
            }

            return influenceCharacters.Any();
        }
        
        private int GetRandomShootStat()
        {
            return Random.Range(shootingScore, highestPossibleScore);
        }

        private int GetRandomPassStat()
        {
            return Random.Range(passingScore, highestPossibleScore);
        }

        #endregion
    }
}