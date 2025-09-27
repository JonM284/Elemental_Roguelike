using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Data;
using Data.CharacterData;
using Data.EnemyData;
using Data.Sides;
using Project.Scripts.Utils;
using Runtime.Character;
using Runtime.Gameplay;
using Runtime.Managers;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utils;
using Random = UnityEngine.Random;
using TransformUtils = Project.Scripts.Utils.TransformUtils;

namespace Runtime.GameControllers
{
    public class TurnController : GameControllerBase
    {

        #region Nested Classes

        [Serializable]
        public class BattlersBySide
        {
            public Data.Sides.CharacterSide teamSide;
            public List<CharacterBase> teamMembers;
        }

        #endregion

        #region Static

        public static TurnController Instance { get; private set; }

        #endregion

        #region Events

        public static event Action OnBattlePreStart;

        public static event Action<int> OnBattleStarted;

        public static event Action OnResetField;
        
        public static event Action<CharacterSide> OnChangeActiveTeam;
        
        public static event Action<CharacterBase> OnChangeActiveCharacter;

        public static event Action OnBattleEnded;

        public static event Action OnRunEnded;

        #endregion

        #region Public Fields

        public UIWindowData battleUIData;

        #endregion

        #region Serialized Fields

        [SerializeField] private CharacterSide playerSide;

        [SerializeField] private CharacterSide enemySide;

        [SerializeField] private CharacterSide neutralSide;

        [SerializeField] private List<BattlersBySide> battlersBySides = new List<BattlersBySide>();
        
        [SerializeField] private AssetReference ballReference;

        #endregion

        #region Private Fields
        
        private int m_currentTeamTurnIndex = 0;
        
        private Transform m_knockedPlayerPool;

        private List<ArenaTeamManager> teamManagers = new List<ArenaTeamManager>();

        private int activeTeamID = 0;

        private int m_currentSelectedCharacterID;

        private Vector3 ballInitialPosition;

        private List<CharacterBase> m_cachedHiddenCharacters = new List<CharacterBase>();

        private bool m_hasScoredPoint;

        private bool m_isSettingUpMatch;

        private bool m_isEndingTurn;

        #endregion

        #region Accessors

        public CharacterSide playersSide => playerSide;
        
        public bool isInBattle { get; private set; }

        public BallBehavior ball { get; private set; }

        public CharacterBase activeCharacter { get; private set; }

        public bool isPlayerTurn => battlersBySides[activeTeamID].teamSide == playerSide;
        
        public Transform knockedOutPlayerPool =>
            CommonUtils.GetRequiredComponent(ref m_knockedPlayerPool, ()=>
            {
                var poolTransform = TransformUtils.CreatePool(null , false);
                return poolTransform;
            });
        
        #endregion

        #region Unity Events

        private void OnEnable()
        {
            SceneController.OnLevelFinishedLoading += OnLevelFinishedLoading;
            CharacterGameController.CharacterCreated += OnCharacterCreated;
            CharacterBase.CharacterSelected += OnCharacterSelected;
            CharacterBase.CharacterEndedTurn += OnCharacterEndedTurn;
            WinConditionController.PointThresholdReached += EndBattle;
        }

        private void OnDisable()
        {
            SceneController.OnLevelFinishedLoading -= OnLevelFinishedLoading;
            CharacterGameController.CharacterCreated -= OnCharacterCreated;
            CharacterBase.CharacterSelected -= OnCharacterSelected;
            CharacterBase.CharacterEndedTurn -= OnCharacterEndedTurn;
            WinConditionController.PointThresholdReached -= EndBattle;
        }

        #endregion
        
        #region GameControllerBase Inherited Methods

        public override void Initialize()
        {
            if (!Instance.IsNull())
            {
                return;
            }
            Instance = this;
            base.Initialize();
        }

        #endregion

        #region Class Implementation

        public List<CharacterBase> GetActiveTeam()
        {
            return battlersBySides[activeTeamID].teamMembers;
        }

        public CharacterSide GetActiveTeamSide()
        {
            return battlersBySides[activeTeamID].teamSide;
        }

        public ArenaTeamManager GetTeamManager(CharacterSide _side)
        {
            return teamManagers.FirstOrDefault(atm => atm.characterSide.sideGUID == _side.sideGUID);
        }

        public ArenaTeamManager GetPlayerManager()
        {
            return teamManagers.FirstOrDefault(atm => atm.characterSide.sideGUID == playerSide.sideGUID);
        }

        private List<CharacterBase> GetAvailableTeamMembers()
        {
            var activeTeam = CommonUtils.ToNewList(GetActiveTeam());
            return activeTeam.FindAll(cb => cb.characterActionPoints != 0 && cb.isAlive);
        }

        public void AddGoalieToTeam(CharacterBase _character)
        {
            var correctTeam = battlersBySides.FirstOrDefault(bbs => bbs.teamSide == _character.side);
            if(!correctTeam.teamMembers.Contains(_character)){
                correctTeam.teamMembers.Add(_character);
            }
        }

        public void SelectAvailablePlayer(bool _isNext)
        {
            if (!isPlayerTurn)
            {
                return;
            }

            if (!activeCharacter.IsNull())
            {
                return;
            }

            var availableTeamMembers = GetAvailableTeamMembers();
            
            if(availableTeamMembers.Count == 0)
            {
                //None = turn end
                return;
            }

            int nextIndex = 0;
            
            if(availableTeamMembers.Count > 1)
            {
                var currentCharacterIndex = !activeCharacter.IsNull() ? availableTeamMembers.IndexOf(activeCharacter) : 0;
                var _amountChange = _isNext ? 1 : -1;
                nextIndex = currentCharacterIndex + _amountChange;
                if (nextIndex < 0)
                {
                    nextIndex = availableTeamMembers.Count - 1;
                }else if (nextIndex >= availableTeamMembers.Count)
                {
                    nextIndex = 0;
                }
            }
            

            if (availableTeamMembers[nextIndex].IsNull())
            {
                Debug.Log("Next Team Member is NULL");
                return;
            }

            if (!activeCharacter.IsNull())
            {
                activeCharacter.StopAllActions();
            }
            
            activeCharacter = availableTeamMembers[nextIndex];

            CameraUtils.SetCameraTrackPos(activeCharacter.transform.position, false);
            CameraUtils.SetCameraZoom(0.7f);
            
            OnChangeActiveCharacter?.Invoke(activeCharacter);

        }
        
        private void OnLevelFinishedLoading(SceneName _sceneName, bool _isMatchScene)
        {
            if (!_isMatchScene)
            {
                return;
            }

            if (!is_Initialized)
            {
                return;
            }
            
            SetupMatch();
        }

        [ContextMenu("Start Match")]
        public void SetupMatch()
        {
            if(m_isSettingUpMatch)
            {
                return;
            }
            
            T_MatchSetup();
        }
        
        //Setup Match
        //Spawn all characters in correct position
        //flip coin to decided starting team
        private async UniTask T_MatchSetup()
        {
            m_isSettingUpMatch = true;
            
            UIUtils.OpenUI(battleUIData);

            await UniTask.WaitForSeconds(0.3f);
            
            //Find both team arena managers

            if (teamManagers.Count > 0)
            {
                teamManagers.Clear();
            }

            var allTeamSides = GameObject.FindObjectsOfType<ArenaTeamManager>();

            foreach (var manager in allTeamSides)
            {
                teamManagers.Add(manager);
            }
            
            //Find ball
            var ballRef = GameObject.FindObjectOfType<BallBehavior>();

            if (ballRef.IsNull())
            {
                //spawn ball
                Debug.LogError("Ball Null");
            }
            else
            {
                Debug.Log("Ball Found");
            }

            ball = ballRef;

            //Spawn Enemy Team
            await T_LoadEnemyTeam();
            
            //Spawn Player Team
            await T_LoadPlayerTeam();
            
            StartBattle();
        }

        private async UniTask T_LoadPlayerTeam()
        {
            
            Debug.Log("<color=#00FF00>Loading Team</color>");

            var playerTeam = battlersBySides.FirstOrDefault(bbs => bbs.teamSide == playerSide);

            if (playerTeam.IsNull())
            {
                BattlersBySide _batBySide = new BattlersBySide();
                _batBySide.teamSide = playerSide;
                battlersBySides.Add(_batBySide);
            }

            var teamMembers = TeamController.Instance.GetTeam();
            
            Debug.Log($"Team Members = {teamMembers.Count}");
            
            var correctSide = teamManagers.FirstOrDefault(atm => atm.characterSide.sideGUID == playerSide.sideGUID);

            if (correctSide.IsNull())
            {
                Debug.LogError("Team Side null");
            }

            Debug.Log(correctSide.startPositions.Count);
            
            for (int i = 0; i < teamMembers.Count; i++)
            {
                await CharacterGameController.Instance.C_CreateCharacter(teamMembers[i].m_characterStatsBase,
                    correctSide.startPositions[i].position, correctSide.startPositions[i].localEulerAngles, true);
            }


            await correctSide.T_SpawnGoalie();
        }
        
        private void OnCharacterCreated(CharacterBase _character)
        {
            if (!is_Initialized)
            {
                return;
            }
            
            if (_character.IsNull())
            {
                return;
            }

            if (_character.side == playerSide)
            {
                var _playerTeam = battlersBySides.FirstOrDefault(bbs => bbs.teamSide == playerSide);

                if (_playerTeam.IsNull())
                {
                    Debug.LogError("Player Team NULL");
                    return;
                }
            
                _playerTeam.teamMembers.Add(_character);
            }
            else
            {
                var _enemyTeam = battlersBySides.FirstOrDefault(bbs => bbs.teamSide == enemySide);

                if (_enemyTeam.IsNull())
                {
                    Debug.LogError("Enemy Team NULL");
                    return;
                }
            
                _enemyTeam.teamMembers.Add(_character);
            }
            
        }

        private async UniTask T_LoadEnemyTeam()
        {
            var teamData = TournamentController.Instance.GetCurrentEnemyTeam();
            
            if (teamData.IsNull())
            {
                return;
            }
            
            Debug.Log("<color=#00FF00>Loading ENEMY TEAM</color>");

            var enemyTeam = battlersBySides.FirstOrDefault(bbs => bbs.teamSide == enemySide);

            if (enemyTeam.IsNull())
            {
                BattlersBySide _batBySide = new BattlersBySide();
                _batBySide.teamSide = enemySide;
                battlersBySides.Add(_batBySide);
            }
            
            var teamMembers = teamData.enemyCharacters;
            
            Debug.Log($"Team Members = {teamMembers.Count}");
            
            var correctSide = teamManagers.FirstOrDefault(atm => atm.characterSide.sideGUID == enemySide.sideGUID);
            
            for (int i = 0; i < teamMembers.Count; i++)
            {
                await CharacterGameController.Instance.C_CreateCharacter(teamMembers[i],
                    correctSide.startPositions[i].position, correctSide.startPositions[i].localEulerAngles, false);
            }

            await correctSide.T_SpawnGoalie();
        }

        public void StartBattle()
        {
            StartCoroutine(C_StartBattle());
        }

        private IEnumerator C_StartBattle()
        {
            isInBattle = true;
            
            m_hasScoredPoint = false;
            
            OnBattlePreStart?.Invoke();

            yield return new WaitForSeconds(1f);
            
            activeTeamID = Random.Range(0,2);

            yield return new WaitForSeconds(1f);
            
            UIUtils.FadeBlack(false);
            
            yield return new WaitForSeconds(1f);
            
            SetTeamActive();

            m_isSettingUpMatch = false;

            int teamAmount = 0;
            
            battlersBySides.ForEach(bbs =>
            {
                if (bbs.teamMembers.Count > 0)
                {
                    teamAmount++;
                }
            });
            
            OnBattleStarted?.Invoke(teamAmount);
            
        }
        
        private void OnCharacterSelected(CharacterBase _character)
        {
            if (!is_Initialized)
            {
                Debug.Log("NOT INITIALIZED");
                return;
            }
            
            if (battlersBySides[activeTeamID].teamSide.sideGUID != playerSide.sideGUID)
            {
                Debug.Log("NOT PLAYERS TURN");
                return;
            }

            //If the character isn't on the active team, Ignore them
            //Case Scenario: Healing Ally - it will then go past this check
            if (!battlersBySides[activeTeamID].teamMembers.Contains(_character))
            {
                Debug.Log("Doesn't contain player");
                return;
            }

            //If this character doesn't have points left, ignore them
            if (_character.characterActionPoints <= 0)
            {
                UIController.Instance.CreateFloatingTextAtCursor("No Action Points", Color.red);
                return;
            }

            //If the already active character is doing an action, then the character selected is 
            // the receiver of the action.
            //Case Scenario: Healing Ally
            if (!activeCharacter.IsNull())
            {
                if (activeCharacter.isDoingAction)
                {
                    return;
                }
                activeCharacter.StopAllActions();
            }
            
            activeCharacter = _character;

            CameraUtils.SetCameraTrackPos(activeCharacter.transform.position, false);
            CameraUtils.SetCameraZoom(0.7f);
            
            OnChangeActiveCharacter?.Invoke(activeCharacter);

        }
        
        //Use this for AI only
        private void OnCharacterEndedTurn(CharacterBase _character)
        {
            if (!is_Initialized)
            {
                return;
            }
            
            if (_character == activeCharacter)
            {
                activeCharacter = null;
            }

            if (!battlersBySides[activeTeamID].teamMembers.FindAll(cb => cb.isAlive).TrueForAll(cb => cb.characterActionPoints == 0))
            {
                if (_character.side == playerSide)
                {
                    StartCoroutine(C_WaitToSwap(_character));
                    return;
                }
                return;
            }

            Debug.Log("Team Turn Ended");
            if (m_isEndingTurn)
            {
                return;
            }
            
            StartCoroutine(C_EndTeamTurn());
        }

        public IEnumerator C_WaitToSwap(CharacterBase _character)
        {
            yield return null;

            yield return new WaitForSeconds(0.3f);

            if (_character.isDoingAction)
            {
                yield return new WaitUntil(() => !_character.isDoingAction);
            }

            if (ball.isThrown)
            {
                yield return new WaitUntil(() => !ball.isThrown);
            }

            if (!activeCharacter.IsNull())
            {
                yield break;
            }
            
            SelectAvailablePlayer(true);

        }

        public void EndTeamTurn()
        {
            if (m_isEndingTurn)
            {
                return;
            }
            
            StartCoroutine(C_EndTeamTurn());
        }

        private IEnumerator C_EndTeamTurn()
        {
            m_isEndingTurn = true;
            yield return new WaitForSeconds(0.5f);

            if (ball.isMoving)
            {
                yield return new WaitUntil(() => !ball.isMoving);
            }

            if (m_hasScoredPoint)
            {
                yield break;
            }

            activeTeamID++;
            if (activeTeamID >= battlersBySides.Count)
            {
                activeTeamID = 0;
            }

            if (battlersBySides[activeTeamID].teamMembers.Count == 0)
            {
                yield return null;
                StartCoroutine(C_EndTeamTurn());
                yield break;
            }

            SetTeamActive();

        }


        private void SetTeamActive()
        {
            var isPlayerSide = battlersBySides[activeTeamID].teamSide == playerSide;
            if (isPlayerSide)
            {
                Debug.Log("<color=#007700>IS PLAYER TEAM START</color>");
            }
            else
            {
                Debug.Log("<color=red>IS ENEMY TEAM START</color>");
                StartCoroutine(C_EnemyAITurn());
            }
            
            CameraUtils.SetCameraZoom(1f);
            List<Transform> _characterTransforms = new List<Transform>();
            battlersBySides[activeTeamID].teamMembers.ForEach(c =>
            {
                if (c.isAlive)
                {
                    _characterTransforms.Add(c.transform);
                }
            });
            
            CameraUtils.SetCameraTrackPosCentral(_characterTransforms, false);
            
            JuiceController.Instance.ChangeSide(isPlayerSide);
            
            m_isEndingTurn = false;

            OnChangeActiveTeam?.Invoke(battlersBySides[activeTeamID].teamSide);
        }

        private IEnumerator C_EnemyAITurn()
        {
            yield return new WaitForSeconds(1f);

            var enemySide = battlersBySides.FirstOrDefault(bbs => bbs.teamSide == this.enemySide);
            
            if (enemySide.teamMembers.Count == 0)
            {
                Debug.Log("ENEMY TEAM EMPTY");
                EndTeamTurn();
                yield break;
            }

            if (enemySide.teamMembers.TrueForAll(cb => !cb.isAlive))
            {
                EndTeamTurn();
            }

            foreach (var currentMember in enemySide.teamMembers)
            {
                if (!currentMember.isAlive)
                {
                    continue;
                }

                if (m_hasScoredPoint)
                {
                    yield break;
                }
                
                activeCharacter = currentMember;
                
                OnChangeActiveCharacter?.Invoke(currentMember);

                CameraUtils.SetCameraTrackPos(activeCharacter.transform, true);
                CameraUtils.SetCameraZoom(0.5f);
                
                yield return null;
                
                Debug.Log("BEFORE ENEMY CHARACTER TURN");
                yield return new WaitUntil(() => currentMember.finishedTurn);
                Debug.Log("AFTER ENEMY CHARACTER TURN");

                yield return new WaitForSeconds(0.5f);

                CameraUtils.SetCameraZoom(1f);

            }


        }

        public IEnumerator C_ResetField(CharacterSide _characterSide)
        {
            
            OnResetField?.Invoke();
            
            UIUtils.FadeBlack(true);
            
            yield return new WaitForSeconds(1f);

            //Release all hidden characters
            if (m_cachedHiddenCharacters.Count > 0)
            {
                m_cachedHiddenCharacters.ForEach(cb =>
                {
                    cb.transform.parent = null;
                    cb.characterLifeManager.FullReviveCharacter();
                });
                m_cachedHiddenCharacters.Clear();
            }
            
            if (!ball.IsNull())
            {
                ball.ResetBall();
                ball.gameObject.SetActive(true);
            }

            var playerTeam = battlersBySides.FirstOrDefault(bbs => bbs.teamSide == playerSide);
            var playerManager = GetPlayerManager();
            
            if (playerTeam.teamMembers.Count > 0)
            {
                for (int i = 0; i < playerTeam.teamMembers.Count; i++)
                {
                    var currentMember = playerTeam.teamMembers[i];

                    currentMember.ResetCharacter(playerManager.startPositions[i].position);
                    
                    Vector3 _rotateTarget =
                        new Vector3(ball.transform.position.x, 0, currentMember.transform.position.z);
                    currentMember.characterRotation.SetRotationTarget(_rotateTarget);
                }
            }
            
            var enemyTeam = battlersBySides.FirstOrDefault(bbs => bbs.teamSide == enemySide);
            var enemyManager = GetTeamManager(enemySide);
            
            if (enemyTeam.teamMembers.Count > 0)
            {
                for (int i = 0; i < enemyTeam.teamMembers.Count; i++)
                { 
                    var currentMember = enemyTeam.teamMembers[i];

                    currentMember.ResetCharacter(enemyManager.startPositions[i].transform.position);

                    Vector3 _rotateTarget =
                        new Vector3(ball.transform.position.x, 0, currentMember.transform.position.z);
                    currentMember.characterRotation.SetRotationTarget(_rotateTarget);
                }
            }
            
            var neutralTeam = battlersBySides.FirstOrDefault(bbs => bbs.teamSide == neutralSide);

            if (neutralTeam.teamMembers.Count > 0)
            {
                //ToDo: Reset them somehow, probably just have them do their own reset
            }

            CameraUtils.SetCameraTrackPos(Vector3.zero, false);
            CameraUtils.SetCameraZoom(0.7f);

            m_hasScoredPoint = false;

            yield return new WaitForSeconds(0.5f);
            
            battlersBySides[activeTeamID].teamMembers.FindAll(cb => cb.isAlive).ForEach(cb => cb.EndTurn());

            UIUtils.FadeBlack(false);

            if (battlersBySides[activeTeamID].teamSide == playerSide)
            {
                EndTeamTurn();
            }

        }

        public void ResetField(CharacterSide _characterSide)
        {
            StartCoroutine(C_ResetField(_characterSide));
        }

        public void HideCharacter(CharacterBase _character)
        {
            if (_character.IsNull())
            {
                return;
            }

            m_cachedHiddenCharacters.Add(_character);
            _character.transform.parent = knockedOutPlayerPool;
        }

        private void RemoveAllCharacters()
        {
            battlersBySides.ForEach(bbs =>
            {
                bbs.teamMembers.ForEach(cb => GameObject.Destroy(cb.gameObject));
            });
            
            battlersBySides.ForEach(bbs =>
            {
                bbs.teamMembers.Clear();
            });
            
            teamManagers.Clear();
            m_cachedHiddenCharacters.Clear();
        }

        public void HaltAllPlayers()
        {
            battlersBySides.ForEach(bbs =>
            {
                bbs.teamMembers.ForEach(cb =>
                {
                    cb.StopAllActions();
                    cb.SetCharacterUsable(false);
                });
            });
            m_hasScoredPoint = true;
        }

        private void RunEnded()
        {
            isInBattle = false;
            RemoveAllCharacters();
            Debug.Log("RUN ENDED PLAYER DIED");
            OnRunEnded?.Invoke();
        }

        private void EndBattle()
        {
            UIUtils.FadeBlack(false);
            var playerTeam = battlersBySides.FirstOrDefault(bbs => bbs.teamSide == playerSide);
            
            foreach (var _character in playerTeam.teamMembers)
            {
                _character.characterLifeManager.FullReviveCharacter();
                _character.RemoveEffect();
            }
            
            isInBattle = false;
            RemoveAllCharacters();
            //NOTE: must set active player to the movement meeple, aka the first meeple in the team that is alive, if all meeples dead, player loses
            Debug.Log("Battle Ended");
            OnBattleEnded?.Invoke();
        }

        #endregion
        
        
    }
}