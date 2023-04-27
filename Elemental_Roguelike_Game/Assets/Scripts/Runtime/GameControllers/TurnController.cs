using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Sides;
using Project.Scripts.Runtime.LevelGeneration;
using Runtime.Character;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Utils;

namespace Runtime.GameControllers
{
    public class TurnController: GameControllerBase
    {

        #region Nested Classes
        
        [Serializable]
        public class BattlersBySide
        {
            public Data.Sides.CharacterSide teamSide;
            public List<CharacterBase> teamMembers;
        }

        #endregion

        #region Events

        public static event Action OnBattlePreStart;

        public static event Action OnBattleStarted;

        public static event Action<List<CharacterBase>> OnTurnOrderChanged;

        public static event Action<CharacterBase> OnChangeCharacterTurn;

        public static event Action OnBattleEnded;

        public static event Action OnRunEnded;

        #endregion
        
        #region Public Fields

        public UIWindowData battleUIData;

        #endregion

        #region Serialized Fields
        
        [SerializeField] private CharacterSide playerSide;
        
        [SerializeField] private List<BattlersBySide> battlersBySides = new List<BattlersBySide>();

        #endregion
        
        #region Private Fields

        private CharacterBase m_cachedMovementMeeple;

        private int m_currentTeamTurnIndex = 0;

        [SerializeField]
        private List<CharacterBase> m_allBattlers = new List<CharacterBase>();
        
        #endregion

        #region Accessors
        
        public bool isInBattle { get; private set; }
        
        public CharacterBase activeCharacter { get; private set; }

        public Team playerTeam => TeamUtils.GetCurrentTeam();
        
        #endregion

        #region Unity Events

        private void OnEnable()
        {
            MeepleController.PlayerMeepleCreated += OnPlayerMeepleCreated;
            LevelController.OnRoomChanged += OnRoomChange;
            CharacterBase.CharacterEndedTurn += OnCharacterEndedTurn;
            CharacterLifeManager.OnCharacterDied += OnCharacterDied;
        }

        private void OnDisable()
        {
            MeepleController.PlayerMeepleCreated -= OnPlayerMeepleCreated;
            LevelController.OnRoomChanged -= OnRoomChange;
            CharacterBase.CharacterEndedTurn -= OnCharacterEndedTurn;
            CharacterLifeManager.OnCharacterDied -= OnCharacterDied;
        }

        #endregion

        #region Class Implementation
        
        private void OnCharacterEndedTurn(CharacterBase _character)
        {
            StartCoroutine(C_EndCharTurn(_character));
        }

        private IEnumerator C_EndCharTurn(CharacterBase _character)
        {
            //Return to end of List
            m_allBattlers.Add(_character);

            yield return new WaitForSeconds(0.05f);
            
            SetNextCharacterActive();
        }
        
        private void OnPlayerMeepleCreated(CharacterBase _playerMeeple, CharacterStatsData _meepleData)
        {
            if (m_cachedMovementMeeple != null)
            {
                return;
            }

            m_cachedMovementMeeple = _playerMeeple;
            activeCharacter = m_cachedMovementMeeple;
        }

        private void OnRoomChange(RoomTracker _roomTracker)
        {
            StartCoroutine(C_BattleSetup(_roomTracker));
        }
        
        private IEnumerator C_BattleSetup(RoomTracker _roomTracker)
        {
            if (_roomTracker == null || !_roomTracker.hasBattle)
            {
                UIUtils.FadeBlack(false);
                yield break;
            }

            m_allBattlers.Clear();
            
            Debug.Log("<color=yellow>Before await</color>");
            yield return StartCoroutine(_roomTracker.SetupBattle());
            Debug.Log("<color=red>After await</color>");

            if (playerTeam.teamMembers.Count == 0)
            {
                Debug.LogError("Player doesn't have team");
                yield break;
            }
            
            foreach (var teamMember in playerTeam.teamMembers)
            {
                AddCharacter(teamMember);
            }
            
            if (_roomTracker.roomEnemies.Count == 0)
            {
                Debug.LogError("Room does not contain enemies");
                yield break;
            }

            foreach (var enemy in _roomTracker.roomEnemies)
            {
                AddCharacter(enemy);
            }

            foreach (var battler in m_allBattlers)
            {
                var correctTeam = battlersBySides.FirstOrDefault(bbs => bbs.teamSide == battler.side);
                correctTeam.teamMembers.Add(battler);
            }
            
            StartBattle();
        }

        public void StartBattle()
        {
            StartCoroutine(C_StartBattle());
        }

        private IEnumerator C_StartBattle()
        {
            isInBattle = true;
            SetAllCharactersBattleStatus(isInBattle);
            SortCharacterOrder();
            UIUtils.OpenUI(battleUIData);

            OnBattlePreStart?.Invoke();

            yield return new WaitForSeconds(1f);
            
            UIUtils.FadeBlack(false);
            
            yield return new WaitForSeconds(1f);
            OnBattleStarted?.Invoke();
            SetNextCharacterActive();
            
        }

        private void SortCharacterOrder()
        {
            if (m_allBattlers.Count == 0)
            {
                return;
            }

            m_allBattlers.SortCharacterTurnOrder();
        }

        private void SetAllCharactersBattleStatus(bool _inBattle)
        {
            if (m_allBattlers.Count == 0)
            {
                return;
            }
            
            m_allBattlers.ForEach(t => t.InitializeCharacterBattle(_inBattle));
        }

        public void AddCharacter(CharacterBase _newCharacter)
        {
            if (_newCharacter == null)
            {
                return;
            }
            
            m_allBattlers.Add(_newCharacter);
            if (isInBattle)
            {
                OnTurnOrderChanged?.Invoke(m_allBattlers);   
            }
        }

        private void OnCharacterDied(CharacterBase _character)
        {
            if (!m_allBattlers.Contains(_character))
            {
                return;
            }
            
            StartCoroutine(C_CharacterDeath(_character));
        }

        private IEnumerator C_CharacterDeath(CharacterBase _deadCharacter)
        {

            var tempList = new List<CharacterBase>();
            
            m_allBattlers.ForEach(c => tempList.Add(c));
            
            m_allBattlers.ForEach(c =>
            {
                if (c == _deadCharacter)
                {
                    tempList.Remove(c);
                }
            });

            m_allBattlers = tempList;

            var teamSide = battlersBySides.FirstOrDefault(b => b.teamSide == _deadCharacter.side);
            teamSide.teamMembers.Remove(_deadCharacter);

            var teamSideCount = teamSide.teamMembers.Count;

            yield return new WaitForSeconds(0.25f);

            if (teamSideCount == 0)
            {
                if (teamSide.teamSide == playerSide)
                {
                    //all players eliminated 
                    RunEnded();
                }
                else
                {
                    //all enemies eliminated
                    EndBattle();
                }
                yield break;
            }

            yield return new WaitForSeconds(0.25f);

            if (_deadCharacter == activeCharacter)
            {
                SetNextCharacterActive();    
            }
        }

        private void RemoveAllCharacters()
        {
            m_allBattlers.Clear();
            battlersBySides.ForEach(bbs => bbs.teamMembers.Clear());
        }

        private void RunEnded()
        {
            isInBattle = false;
            SetAllCharactersBattleStatus(isInBattle);
            RemoveAllCharacters();
            Debug.Log("RUN ENDED PLAYER DIED");
            OnRunEnded?.Invoke();
        }

        private void EndBattle()
        {
            isInBattle = false;
            SetAllCharactersBattleStatus(isInBattle);
            RemoveAllCharacters();
            //NOTE: must set active player to the movement meeple, aka the first meeple in the team that is alive, if all meeples dead, player loses
            activeCharacter = m_cachedMovementMeeple;
            Debug.Log("Battle Ended");
            OnBattleEnded?.Invoke();
        }

        public void SetNextCharacterActive()
        {
            if (!isInBattle)
            {
                return;
            }
            
            //take next character out of queue
            var nextInTurn = m_allBattlers[0];

            //set to active character
            activeCharacter = nextInTurn;
            
            Debug.Log($"<color=green>{activeCharacter} is active</color>", activeCharacter);
            
            OnChangeCharacterTurn?.Invoke(activeCharacter);

            m_allBattlers.Remove(nextInTurn);

            OnTurnOrderChanged?.Invoke(m_allBattlers);
        }

        #endregion
        
        
    }
}