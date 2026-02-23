using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data.Sides;
using Project.Scripts.Utils;
using Runtime.Character;
using Runtime.GameControllers;
using Runtime.Gameplay.Sensors;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Runtime.Gameplay
{
    public class BallBehavior : MonoBehaviour
    {

        #region Serialized Field

        [Header("Ball Variables")] [SerializeField]
        private float m_wallRayLegnth = 0.3f;

        [SerializeField] private LayerMask wallLayers;

        [SerializeField] private float m_fullDecelTime = 1.9f;

        [SerializeField] private AnimationCurve speedCurve;

        [Space(15)] 
        [Header("Player Check")] [SerializeField] private float playerCheckRadius;
        [SerializeField] private PlayerDetectionSensor characterCatchSensor;
        [SerializeField] private LayerMask playerCheckLayer, outsideInfluenceLayers;

        [Space(15)] [Header("Spring")] [SerializeField]
        private float springConstant;

        [SerializeField] private float damping;

        [Space(15)] [Header("Ground Check")] [SerializeField]
        private GameObject groundIndicator;

        [SerializeField] private float groundIndOffset;

        [SerializeField] private Transform m_ballCamPoint;

        [Space(15)] [Header("Visuals")] [SerializeField]
        private List<GameObject> visualGOs = new List<GameObject>();

        [SerializeField] private Canvas ballLevelCanvas;
        [SerializeField] private GameObject ballThrowVisuals;
        [SerializeField] private Image ballThrowIntensityImage;
        [SerializeField] private TMP_Text ballThrowIntensityText;
        [SerializeField] private float countFPS = 30f, textDuration = 0.2f;

        #endregion

        #region Private Fields

        private List<CharacterBase> m_lastContactedCharacters = new List<CharacterBase>();
        
        private Rigidbody m_rb;

        private float m_initialY;

        private readonly float dragModifier = 0.2f;

        private float m_afterThrowThreshold = 0.25f;

        private float m_throwStartTime;

        private float currentBallModifier, startingBallThrowModifier, ballInfluenceStrength = 10f;
        private const float averageBallSpeed = 5f;

        private Vector3 ballThrownDirection, ballOutsideInfluence, ballVelocity;

        private bool m_isBallPaused;

        private Vector3 m_ballStartPosition;
        private List<Vector3> trajectoryWaypoints = new List<Vector3>();
        private Vector3 currentWaypoint;
        private int trajectoryIndex;
        private float distancePreviousFrame;
        private Vector3 direction, lastDirection;
        
        private Collider[] foundPlayers = new Collider[15];
        private int foundPlayersAmount;

        private Dictionary<CharacterBase, Vector3> currentInfluences = new Dictionary<CharacterBase, Vector3>();
        private List<CharacterBase> charactersPassedByDuringThrow = new();
        private CancellationTokenSource ballCts;

        private bool isRequestChargeBall = false;

        #endregion

        #region Accessors
        
        private Rigidbody rb => CommonUtils.GetRequiredComponent(ref m_rb, GetComponent<Rigidbody>);

        public bool isControlled { get; private set; }

        public Transform followerTransform { get; private set; }

        public CharacterBase currentOwner { get; private set; }

        public Transform ballCamPoint => m_ballCamPoint;

        public bool isThrown { get; private set; }
        
        public bool isMoving => currentBallModifier > 0;
        
        public float thrownBallStat { get; private set; }

        public List<CharacterBase> lastContactedCharacters => m_lastContactedCharacters;

        public CharacterBase lastThrownCharacter { get; private set; }

        public CharacterBase lastHeldCharacter { get; private set; }

        public CharacterSide controlledCharacterSide { get; private set; }

        #endregion

        #region Unity Events

        private void Awake()
        {
            characterCatchSensor.SetColliderRadius(playerCheckRadius);
            m_initialY = transform.position.y;
            m_ballStartPosition = transform.position;
            ballLevelCanvas.worldCamera = CameraUtils.GetMainCamera();
            ballCts = new CancellationTokenSource();
            BallIdle(ballCts.Token).Forget();
        }

        private void OnEnable()
        {
            characterCatchSensor.OnCharacterEnter += OnCharacterCatchBall;
        }

        private void OnDisable()
        {
            characterCatchSensor.OnCharacterEnter -= OnCharacterCatchBall;
        }

        #endregion
        
        #region Class Implementation

        private void EnableCatchSensor(bool isEnable)
        {
            characterCatchSensor.gameObject.SetActive(isEnable);
        }
        
        /// <summary>
        /// UnPlanned throw, influence detection needed
        /// </summary>
        /// <param name="direction">Initial Direction</param>
        /// <param name="throwForce">Initial Force</param>
        /// <param name="_isThrown">Thrown or KnockedAway</param>
        /// <param name="_character">Throwing Character</param>
        /// <param name="_thrownBallStat">Stats</param>
        public void ThrowBall(Vector3 direction, float throwForce,
            bool _isThrown, CharacterBase _character, int _thrownBallStat)
        {
            ballCts?.Cancel();
            trajectoryWaypoints.Clear();
            currentInfluences.Clear();
            
            thrownBallStat = _thrownBallStat;
            m_throwStartTime = Time.time;
            isThrown = _isThrown;
            
            if (isThrown && !_character.IsNull())
            {
                lastThrownCharacter = _character;
            }

            lastHeldCharacter = _character;
            isControlled = false;
            controlledCharacterSide = null;
            followerTransform = null;

            currentOwner = null;
            ballThrownDirection = direction;
            currentBallModifier = Mathf.Clamp01(throwForce);
            startingBallThrowModifier = currentBallModifier;
            
            CameraUtils.SetCameraTrackPos(transform, true);
            ballThrowVisuals.SetActive(true);
            
            ballCts = new CancellationTokenSource();
            BallThrown(ballCts.Token).Forget();
        }

        /// <summary>
        /// Pre-planned throw
        /// </summary>
        /// <param name="waypoints"></param>
        /// <param name="throwForce"></param>
        /// <param name="_character"></param>
        public void ThrowBall(Vector3[] waypoints, float throwForce,
            CharacterBase _character)
        {
            ballCts?.Cancel();

            m_throwStartTime = Time.time;
            isThrown = true;
            
            if (isThrown && !_character.IsNull())
            {
                lastThrownCharacter = _character;
            }

            lastHeldCharacter = _character;
            isControlled = false;
            controlledCharacterSide = null;
            followerTransform = null;

            currentOwner = null;

            trajectoryIndex = 0;
            trajectoryWaypoints.Clear();
            trajectoryWaypoints = waypoints.ToList();
            currentWaypoint = trajectoryWaypoints[1];
            
            currentBallModifier = Mathf.Clamp01(throwForce);
            startingBallThrowModifier = currentBallModifier;
            
            CameraUtils.SetCameraTrackPos(transform, true);
            ballThrowVisuals.SetActive(true);
            
            ballCts = new CancellationTokenSource();
            BallThrown(ballCts.Token).Forget();
        }

        public float GetInfluence() => ballInfluenceStrength;
        
        public float GetSpeed() => averageBallSpeed;

        /// <summary>
        /// Ball is in Idle state, being held
        /// </summary>
        /// <param name="token"></param>
        private async UniTask FollowTransform(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            while (isControlled)
            {
                transform.position = followerTransform.position;
                await UniTask.Yield(PlayerLoopTiming.PreLateUpdate, token);
            }
        }

        /// <summary>
        /// Ball in Idle State, not being held
        /// </summary>
        private async UniTask BallIdle(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            while (!isControlled)
            {
                BouncyFloat();
                MarkGround();
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken:token);
            }
        }

        private async UniTask BallThrown(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (trajectoryWaypoints.Count > 0)
            {
                await PlannedTrajectoryMovement(token);
            }
            else
            {
                await UnplannedTrajectoryMovement(token);
            }
            
            isThrown = false;
            thrownBallStat = 0;
            lastThrownCharacter = null;
            ballThrowVisuals.SetActive(false);
            currentInfluences.Clear();
            charactersPassedByDuringThrow.Clear();
            trajectoryWaypoints.Clear();
            BallIdle(token).Forget();
        }
        
        
        private void BouncyFloat()
        {
            var displacement = m_initialY - transform.position.y;
            var springForce = springConstant * displacement;
            var dampingForce = -damping * rb.linearVelocity.y;
            var totalForce = springForce + dampingForce;
        
            rb.AddForce(Vector3.up * totalForce, ForceMode.Force);
        }

        private void MarkGround()
        {
            if (isControlled)
            {
                return;
            }
            
            groundIndicator.SetActive(!isControlled);

            groundIndicator.transform.position =
                new Vector3(transform.position.x, groundIndOffset, transform.position.z);
        }

        private async UniTask UnplannedTrajectoryMovement(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            
            TurnController.Instance.RequestPauseChange(true);

            while (currentBallModifier > 0 && !isControlled)
            {
                if (Physics.Raycast(transform.position, ballThrownDirection, out RaycastHit hit, m_wallRayLegnth, wallLayers))
                {
                    ballThrownDirection = Vector3.Reflect(ballThrownDirection, hit.normal);
                }
                
                BouncyFloat();
                MarkGround();
                UpdateThrowIntensityVisuals();
                
                if (CanBeCaught() && !characterCatchSensor.isActiveAndEnabled)
                {
                    EnableCatchSensor(true);
                }

                if (isRequestChargeBall)
                {
                    await ChangeBallIntensity(token);
                }
                
                if (!m_isBallPaused)
                {
                    var ballSpeed = averageBallSpeed * speedCurve.Evaluate(currentBallModifier);
                    var ballVelocity = GetCurrentInfluenceDirection().normalized * ((ballSpeed) * Time.deltaTime);
                    rb.MovePosition(rb.position + ballVelocity);
                    currentBallModifier -= dragModifier * Time.deltaTime;
                    ballThrownDirection = Vector3.ClampMagnitude(ballVelocity.normalized * ballInfluenceStrength, ballInfluenceStrength);
                }
                
                if (currentBallModifier <= 0.01f)
                {
                    Debug.Log("[Ball Behaviour] Current Ball modifier has fallen below 0.1f");
                    break;
                }
                
                Debug.Log($"[Ball Behaviour] {currentBallModifier}");
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
            }
            
            TurnController.Instance.RequestPauseChange(false);
        }
        
        private async UniTask PlannedTrajectoryMovement(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            Debug.Log("[BallBehavior][Movement] Start Planned Movement");
            TurnController.Instance.RequestPauseChange(true);

            while (currentBallModifier > 0 && trajectoryIndex < trajectoryWaypoints.Count -1 && !isControlled)
            {
                if (!m_isBallPaused)
                {
                    var diff = currentWaypoint - transform.position.FlattenVector3Y();
                    if (diff.magnitude > 0.8f)
                    {
                        var ballSpeed = averageBallSpeed * speedCurve.Evaluate(currentBallModifier);
                        ballVelocity = diff.FlattenVector3Y().normalized * (ballSpeed * Time.deltaTime);
                        distancePreviousFrame = diff.magnitude;
                    } else if(diff.magnitude <= 0.8f || (distancePreviousFrame > 0 && diff.magnitude > distancePreviousFrame)) 
                        //Current Distance is greater than previous frame distance
                    {
                        trajectoryIndex++;
                        Debug.Log("Next Point");
                        if (trajectoryIndex < trajectoryWaypoints.Count - 1)
                        {
                            currentWaypoint = trajectoryWaypoints[trajectoryIndex];
                            distancePreviousFrame = 0;
                        }
                        else
                        {
                            //finished planned movement, move onto unplanned movement
                            ballThrownDirection = diff;
                            break;
                        }
                    }
                }
                
                BouncyFloat();
                MarkGround();
                UpdateThrowIntensityVisuals();

                if (CanBeCaught() && !characterCatchSensor.isActiveAndEnabled)
                {
                    EnableCatchSensor(true);
                }

                if (isRequestChargeBall)
                {
                    await ChangeBallIntensity(token);
                }
                
                var dragAmplifier = trajectoryIndex < trajectoryWaypoints.Count - 2 ? 1f : 5f;
                rb.MovePosition(rb.position + ballVelocity);
                currentBallModifier -= (dragModifier * dragAmplifier) * Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
            }

            if (currentBallModifier > 0 && !ballThrownDirection.IsNan())
            {
                await UnplannedTrajectoryMovement(token);
            }
            else
            {
                TurnController.Instance.RequestPauseChange(false);
            }
        }
        
        public bool CanBeCaught()
        {
            return Time.time - m_throwStartTime >= m_afterThrowThreshold && !m_isBallPaused;
        }

        private void OnCharacterCatchBall(CharacterBase catchingCharacter)
        {
            Debug.Log("Catch called");
            if (isThrown && !CanBeCaught())
            {
                Debug.Log("return 1");
                return;
            }

            if (isControlled)
            {
                Debug.Log("return 2");
                return;
            }
            
            if (catchingCharacter.IsNull())
            {
                Debug.Log("return 3");
                return;
            }
            
            if (!catchingCharacter.characterBallManager.canPickupBall)
            {
                Debug.Log("return 4");
                return;
            }

            if (!lastThrownCharacter.IsNull() && catchingCharacter == lastThrownCharacter)
            {
                Debug.Log("return 5");
                return;
            }
                    
            isControlled = true;
            currentBallModifier = 0;
                    
            if (isThrown)
            {
                isThrown = false;
                lastThrownCharacter = null;
            }
                    
            controlledCharacterSide = catchingCharacter.side;
                    
            catchingCharacter.PickUpBall(this);
                    
            groundIndicator.SetActive(!isControlled);

            EnableCatchSensor(false);
        }
        
        private async UniTask ChangeBallIntensity(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            
            Debug.Log("<color=cyan>Starting Recharge</color>");
            
            SetBallPause(true);
            await CountToNumberAsync(startingBallThrowModifier, token);

            Debug.Log("<color=cyan>End Recharge</color>");
            
            SetBallPause(false);
            isRequestChargeBall = false;
        }
        
        private async UniTask CountToNumberAsync(float _newValue, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            float _previousValue = currentBallModifier;
            float _stepAmount;
            float _waitTime = 1 / countFPS;

            _stepAmount = (_newValue - _previousValue) / (countFPS * textDuration);

            _stepAmount = Mathf.Abs(_stepAmount);
            
            //Going up
            if (_previousValue < _newValue)
            {
                while (_previousValue < _newValue)
                {
                    _previousValue += _stepAmount;
                    if (_previousValue > _newValue)
                    {
                        _previousValue = _newValue;
                    }
                    
                    ballThrowIntensityText.text = $"{_previousValue * 100f:N0}%";;
                    ballThrowIntensityImage.fillAmount = _previousValue;
                    Debug.Log($"Wait time: {_waitTime} Step:{_stepAmount} ___ Prev:{_previousValue} ___ New:{_newValue}");
                    await UniTask.WaitForSeconds(_waitTime, cancellationToken:token);
                }
            }
            else //Going down
            {
                while (_previousValue > _newValue)
                {
                    _previousValue -= _stepAmount;
                    if (_previousValue < _newValue)
                    {
                        _previousValue = _newValue;
                    }
                    
                    ballThrowIntensityText.text = $"{_previousValue * 100f:N0}%";;
                    ballThrowIntensityImage.fillAmount = _previousValue;
                    await UniTask.WaitForSeconds(_waitTime, cancellationToken:token);
                }
            }

            currentBallModifier = _newValue;
        }
        
        public void AddInfluence(CharacterBase influencingCharacter)
        {
            if (!isThrown)
            {
                return;
            }

            if (influencingCharacter == lastThrownCharacter)
            {
                return;
            }

            var dirToCharacter = influencingCharacter.transform.position - transform.position;
            
            var influenceDirection = (dirToCharacter.normalized / Mathf.Clamp(dirToCharacter.sqrMagnitude,
                                SettingsController.GravDistClampMin, SettingsController.GravDistClampMax))
                            * (influencingCharacter.characterClassManager.ballInfluenceIntensity);

            if (currentInfluences.TryAdd(influencingCharacter, influenceDirection))
            {
                AddPassedCharacter(influencingCharacter);
                return;
            }
            
            currentInfluences[influencingCharacter] = influenceDirection;
        }
        
        public void RemoveInfluence(CharacterBase removingCharacter)
        {
            if (!currentInfluences.ContainsKey(removingCharacter))
            {
                return;
            }
            
            Debug.Log("<color=orange>Removing Influence</color>");
            currentInfluences.Remove(removingCharacter);
        }

        private void AddPassedCharacter(CharacterBase newCharacter)
        {
            if (charactersPassedByDuringThrow.Contains(newCharacter))
            {
                return;
            }
            
            charactersPassedByDuringThrow.Add(newCharacter);
            
            if (newCharacter.side.sideGUID != lastThrownCharacter.side.sideGUID)
            {
                return;
            }
            
            isRequestChargeBall = true;
        }

        private void UpdateThrowIntensityVisuals()
        {
            ballThrowIntensityImage.fillAmount = currentBallModifier;
            ballThrowIntensityText.text = $"{currentBallModifier * 100f:N0}%";
        }
        
        private Vector3 GetCurrentInfluenceDirection()
        {
            if (currentInfluences.Count == 0)
            {
                return ballThrownDirection;
            }
            
            return new Vector3(ballThrownDirection.x + currentInfluences.Values.Sum(vec => vec.x),
                0,
                ballThrownDirection.z + currentInfluences.Values.Sum(vec => vec.z));
        }

        /// <summary>
        /// AKA, pickup ball
        /// </summary>
        public void SetFollowTransform(Transform _follower, CharacterBase _ownerCharacter)
        {
            followerTransform = _follower.transform;
            currentOwner = _ownerCharacter;
            
            ballThrowVisuals.SetActive(false);
            currentInfluences.Clear();
            
            if (m_lastContactedCharacters.Count >= 5)
            {
                m_lastContactedCharacters.RemoveAt(0);
            }
            m_lastContactedCharacters.Add(currentOwner);

            ballCts?.Cancel();
            ballCts = new CancellationTokenSource();
            FollowTransform(ballCts.Token).Forget();
        }


        public void SetBallPause(bool _isPaused)
        {
            m_isBallPaused = _isPaused;
            rb.linearVelocity = Vector3.zero;
        }

        public void ForceStopBall()
        {
            currentBallModifier = 0;
            rb.linearVelocity = Vector3.zero;
        }

        public void ReduceForce(int _reductionAmount)
        {
            currentBallModifier -= _reductionAmount;
        }

        public void ResetBall()
        {
            ForceStopBall();
            if (isControlled)
            {
                currentOwner.DetachBall();
                m_lastContactedCharacters.Clear();
                currentOwner = null;
            }
            isControlled = false;
            followerTransform = null;
            transform.position = m_ballStartPosition;
            currentInfluences.Clear();
        }

        public void SetVisualsToLayer(LayerMask _layer)
        {
            visualGOs.ForEach(g => g.layer = _layer);
        }


        #endregion
        
        
    }
}