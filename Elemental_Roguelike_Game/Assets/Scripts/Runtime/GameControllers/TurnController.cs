using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Sides;
using Project.Scripts.Data;
using Project.Scripts.Utils;
using Runtime.Character;
using Runtime.Character.AI;
using Runtime.Gameplay;
using Runtime.Managers;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utils;
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

        public static event Action OnBattleStarted;

        public static event Action<CharacterSide> OnChangeActiveTeam;

        public static event Action<CharacterBase> OnChangeCharacterTurn;

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

        [SerializeField] private EnemyAITeamData temporaryEnemyTeamData;

        [SerializeField] private AssetReference ballReference;

        #endregion

        #region Private Fields

        private CharacterBase m_cachedMovementMeeple;

        private int m_currentTeamTurnIndex = 0;

        [SerializeField] private List<CharacterBase> m_allBattlers = new List<CharacterBase>();

        private Transform m_knockedPlayerPool;

        private List<ArenaTeamManager> teamManagers = new List<ArenaTeamManager>();

        private int activeTeamID = 0;

        #endregion

        #region Accessors

        public bool isInBattle { get; private set; }

        public BallBehavior ball { get; private set; }

        public CharacterBase activeCharacter { get; private set; }
        
        public Transform knockedOutPlayerPool =>
            CommonUtils.GetRequiredComponent(ref m_knockedPlayerPool, ()=>
            {
                var poolTransform = TransformUtils.CreatePool(this.transform, false);
                return poolTransform;
            });
        
        #endregion

        #region Unity Events

        private void OnEnable()
        {
            MeepleController.PlayerMeepleCreated += MeepleControllerOnPlayerMeepleCreated;
            CharacterBase.CharacterSelected += OnCharacterSelected;
            CharacterBase.CharacterEndedTurn += OnCharacterEndedTurn;
            CharacterLifeManager.OnCharacterDied += OnCharacterDied;
            EnemyController.EnemyCreated += OnEnemyCreated;
        }

        private void OnDisable()
        {
            MeepleController.PlayerMeepleCreated -= MeepleControllerOnPlayerMeepleCreated;
            CharacterBase.CharacterSelected -= OnCharacterSelected;
            CharacterBase.CharacterEndedTurn -= OnCharacterEndedTurn;
            CharacterLifeManager.OnCharacterDied -= OnCharacterDied;
            EnemyController.EnemyCreated -= OnEnemyCreated;
        }

        #endregion
        
        #region GameControllerBase Inherited Methods

        public override void Initialize()
        {
            Instance = this;
            base.Initialize();
        }

        #endregion

        #region Class Implementation

        public List<CharacterBase> GetActiveTeam()
        {
            return battlersBySides[activeTeamID].teamMembers;
        }

        [ContextMenu("Start Match")]
        public void SetupMatch()
        {
            StartCoroutine(C_MatchSetup());
        }
        
        //Setup Match
        //Spawn all characters in correct position
        //flip coin to decided starting team
        private IEnumerator C_MatchSetup()
        {
            m_allBattlers.Clear();

            //Find both team arena managers

            var allTeamSides = GameObject.FindObjectsOfType<ArenaTeamManager>();

            foreach (var manager in allTeamSides)
            {
                teamManagers.Add(manager);
            }
            
            //Find ball
            var ballRef = GameObject.FindObjectOfType<BallBehavior>();

            if (ballRef.IsNull())
            {
                
            }

            ball = ballRef;
            
            //Spawn Player Team

            yield return StartCoroutine(C_LoadPlayerTeam());

            yield return null;
            
            //Spawn Enemy Team

            yield return StartCoroutine(C_LoadEnemyTeam());
            
            StartBattle();
        }

        private IEnumerator C_LoadPlayerTeam()
        {
            
            Debug.Log("<color=#00FF00>Loading Team</color>");

            var playerTeam = battlersBySides.FirstOrDefault(bbs => bbs.teamSide == playerSide);

            if (playerTeam.IsNull())
            {
                BattlersBySide _batBySide = new BattlersBySide();
                _batBySide.teamSide = playerSide;
                battlersBySides.Add(_batBySide);
            }

            var teamMembers = TeamController.Instance.savedTeamMembers;
            
            Debug.Log($"Team Members = {teamMembers.Count}");
            
            var correctSide = teamManagers.FirstOrDefault(atm => atm.characterSide == playerSide);
            
            for (int i = 0; i < teamMembers.Count; i++)
            {
                yield return StartCoroutine(MeepleController.Instance.InstantiatePremadeMeeple(teamMembers[i],
                    correctSide.startPositions[i].position, correctSide.startPositions[i].localEulerAngles));;

                yield return null;
            }
            
            
            
        }
        
        private void MeepleControllerOnPlayerMeepleCreated(CharacterBase characterBase, CharacterStatsData stats)
        {
            var _playerTeam = battlersBySides.FirstOrDefault(bbs => bbs.teamSide == playerSide);

            if (_playerTeam.IsNull())
            {
                Debug.LogError("Player Team NULL");
                return;
            }
            
            _playerTeam.teamMembers.Add(characterBase);
        }

        private IEnumerator C_LoadEnemyTeam()
        {
            if (temporaryEnemyTeamData.IsNull())
            {
                yield return new WaitForSeconds(1f);
                yield break;
            }
            
            Debug.Log("<color=#00FF00>Loading ENEMY TEAM</color>");

            var enemyTeam = battlersBySides.FirstOrDefault(bbs => bbs.teamSide == enemySide);

            if (enemyTeam.IsNull())
            {
                BattlersBySide _batBySide = new BattlersBySide();
                _batBySide.teamSide = enemySide;
                battlersBySides.Add(_batBySide);
            }
            
            var teamMembers = temporaryEnemyTeamData.enemyCharacters;
            
            Debug.Log($"Team Members = {teamMembers.Count}");
            
            var correctSide = teamManagers.FirstOrDefault(atm => atm.characterSide == enemySide);
            
            for (int i = 0; i < teamMembers.Count; i++)
            {
                yield return StartCoroutine(EnemyController.Instance.C_AddEnemy(teamMembers[i],
                    correctSide.startPositions[i].position, correctSide.startPositions[i].localEulerAngles));;

                yield return null;
            }
            
        }
        
        private void OnEnemyCreated(CharacterBase _character)
        {
            var _enemyTeam = battlersBySides.FirstOrDefault(bbs => bbs.teamSide == enemySide);

            if (_enemyTeam.IsNull())
            {
                Debug.LogError("Enemy Team NULL");
                return;
            }
            
            _enemyTeam.teamMembers.Add(_character);
        }
        
        public void StartBattle()
        {
            StartCoroutine(C_StartBattle());
        }

        private IEnumerator C_StartBattle()
        {
            isInBattle = true;

            UIUtils.OpenUI(battleUIData);

            OnBattlePreStart?.Invoke();

            yield return new WaitForSeconds(1f);
            
            //ToDo: when adding enemies, have the starting side be decided by coin-toss
            activeTeamID = 0;

            yield return new WaitForSeconds(1f);
            
            UIUtils.FadeBlack(false);
            
            yield return new WaitForSeconds(1f);
            
            SetTeamActive();
            
            OnBattleStarted?.Invoke();
            
        }
        
        private void OnCharacterSelected(CharacterBase _character)
        {
            if (battlersBySides[activeTeamID].teamSide != playerSide)
            {
                Debug.Log("NOT PLAYERS TURN");
                return;
            }

            //If the character isn't on the active team, Ignore them
            //Case Scenario: Healing Ally - it will then go past this check
            if (!battlersBySides[activeTeamID].teamMembers.Contains(_character))
            {
                Debug.LogError("NOT IN ACTIVE TEAM");
                return;
            }

            //If this character doesn't have points left, ignore them
            if (_character.characterActionPoints <= 0)
            {
                Debug.LogError("NOT ENOUGH POINTS");
                return;
            }

            //If the already active character is doing an action, then the character selected is 
            // the receiver of the action.
            //Case Scenario: Healing Ally
            if (activeCharacter != null)
            {
                if (activeCharacter.isDoingAction)
                {
                    Debug.LogError("IS DOING ACTION");
                    return;
                }
            }
            
            activeCharacter = _character;
            
            OnChangeActiveCharacter?.Invoke(activeCharacter);

        }
        
        private void OnCharacterEndedTurn(CharacterBase _character)
        {
            if (_character == activeCharacter)
            {
                activeCharacter = null;
            }
            
            if (!battlersBySides[activeTeamID].teamMembers.TrueForAll(cb => cb.characterActionPoints == 0))
            {
                return;
            }
            
            Debug.Log("Team Turn Ended");
            StartCoroutine(C_EndTeamTurn());
        }

        private IEnumerator C_EndTeamTurn()
        {
            

            yield return new WaitForSeconds(0.5f);

            activeTeamID++;
            if (activeTeamID >= battlersBySides.Count)
            {
                activeTeamID = 0;
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
            
            OnChangeActiveTeam?.Invoke(battlersBySides[activeTeamID].teamSide);
        }

        private IEnumerator C_EnemyAITurn()
        {
            yield return new WaitForSeconds(1f);

            var enemySide = battlersBySides.FirstOrDefault(bbs => bbs.teamSide == this.enemySide);
            
            if (enemySide.teamMembers.Count == 0)
            {
                Debug.Log("ENEMY TEAM EMPTY");
                yield break;
            }


            foreach (var currentMember in enemySide.teamMembers)
            {
                Debug.Log("Invoking");
                OnChangeCharacterTurn?.Invoke(currentMember);

                yield return null;
                
                Debug.Log("BEFORE ENEMY CHARACTER TURN");
                yield return new WaitUntil(() => currentMember.characterActionPoints == 0);
                Debug.Log("AFTER ENEMY CHARACTER TURN");

                yield return new WaitForSeconds(0.5f);

            }


        }

        private void OnCharacterDied(CharacterBase _character)
        {
           
        }

        private IEnumerator C_CharacterDeath(CharacterBase _deadCharacter)
        {
            
            yield return new WaitForSeconds(1f);
            
        }

        private void RemoveAllCharacters()
        {
            m_allBattlers.Clear();
            battlersBySides.ForEach(bbs => bbs.teamMembers.Clear());
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
            isInBattle = false;
            RemoveAllCharacters();
            //NOTE: must set active player to the movement meeple, aka the first meeple in the team that is alive, if all meeples dead, player loses
            activeCharacter = m_cachedMovementMeeple;
            Debug.Log("Battle Ended");
            OnBattleEnded?.Invoke();
        }

        #endregion
        
        
    }
}