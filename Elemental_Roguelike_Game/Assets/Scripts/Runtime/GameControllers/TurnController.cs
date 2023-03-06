using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using Project.Scripts.Runtime.LevelGeneration;
using Runtime.Character;
using UnityEditor;
using UnityEngine;
using Utils;

namespace Runtime.GameControllers
{
    public class TurnController: GameControllerBase
    {

        #region Events

        public static event Action OnBattlePreStart;

        public static event Action OnBattleStarted;

        public static event Action<List<CharacterBase>> OnTurnOrderChanged;

        public static event Action<CharacterBase> OnChangeCharacterTurn;

        public static event Action OnBattleEnded;

        #endregion
        
        #region Public Fields

        public UIWindowData battleUIData;

        #endregion
        
        #region Private Fields

        private CharacterBase m_cachedMovementMeeple;

        private int m_currentTeamTurnIndex = 0;

        private List<CharacterBase> m_allBattlers = new List<CharacterBase>();

        private Queue<CharacterBase> m_battlersQueue = new Queue<CharacterBase>();
        
        #endregion

        #region Accessors
        
        public bool isInBattle { get; private set; }

        public List<CharacterBase> allBattlers => m_allBattlers;

        public CharacterBase activeCharacter { get; private set; }

        public Team playerTeam => TeamUtils.GetCurrentTeam();
        
        #endregion

        #region Unity Events

        private void OnEnable()
        {
            MeepleController.PlayerMeepleCreated += OnPlayerMeepleCreated;
            LevelController.OnRoomChanged += OnRoomChange;
            CharacterBase.CharacterEndedTurn += OnCharacterEndedTurn;
        }

        private void OnDisable()
        {
            MeepleController.PlayerMeepleCreated -= OnPlayerMeepleCreated;
            LevelController.OnRoomChanged -= OnRoomChange;
            CharacterBase.CharacterEndedTurn -= OnCharacterEndedTurn;
        }

        #endregion

        #region Class Implementation
        
        private void OnCharacterEndedTurn(CharacterBase _character)
        {
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

            allBattlers.Clear();
            
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
            if (allBattlers.Count == 0)
            {
                return;
            }

            m_allBattlers.SortCharacterTurnOrder();
            
            m_battlersQueue.Clear();
            for (int i = 0; i < m_allBattlers.Count; i++)
            {
                m_battlersQueue.Enqueue(m_allBattlers[i]);
            }
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
        }

        public void RemoveCharacter(CharacterBase _removedCharacter)
        {
            if (_removedCharacter == null)
            {
                return;
            }

            m_allBattlers.Remove(_removedCharacter);
        }

        public void RemoveAllCharacters()
        {
            m_allBattlers.Clear();
        }

        public void EndBattle()
        {
            isInBattle = false;
            SetAllCharactersBattleStatus(isInBattle);
            RemoveAllCharacters();
            OnBattleEnded?.Invoke();
        }

        public void SetNextCharacterActive()
        {
            //take next character out of queue
            var nextInTurn = m_battlersQueue.Dequeue();

            //set to active character
            activeCharacter = nextInTurn;
            Debug.Log($"{activeCharacter} is active", activeCharacter);
            OnChangeCharacterTurn?.Invoke(activeCharacter);
            
            OnTurnOrderChanged?.Invoke(m_battlersQueue.ToList());
            
            //Return to end of Queue
            m_battlersQueue.Enqueue(nextInTurn);
        }

        #endregion
        
        
    }
}