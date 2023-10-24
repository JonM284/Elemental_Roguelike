using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data.CharacterData;
using Data.DataSaving;
using Project.Scripts.Runtime.LevelGeneration;
using Project.Scripts.Utils;
using Runtime.GameControllers;
using Runtime.ScriptedAnimations;
using Runtime.Submodules;
using Runtime.UI.DataModels;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace Runtime.GameplayEvents
{
    public class LevelEventController: MonoBehaviour
    {

        #region Nested Classes
        
        [Serializable]
        public class EventByType
        {
            public GameplayEventType eventType;
            public  UnityEvent<GameplayEventType> triggeredEvent;
            public UnityEvent<GameplayEventType> endEvent;
            public AnimationListPlayer entranceAnimation;
            public AnimationListPlayer exitAnimation;
        }

        #endregion

        #region Actions

        public static event Action DisplayMap;

        public static event Action HideMap; 

        public static event Action<string, Vector3> MatchEventEnded;

        public static event Action<PoiLocation, GameplayEventType> EventEnded; 

        #endregion

        #region Private Fields

        private bool m_hasReceivedPlayerAction;

        private string m_currentEventIdentifier;

        private Vector3 m_lastPressedPOILocation;
        
        #endregion

        #region Serialized Fields
        
        [SerializeField]
        private List<EventByType> eventsByType = new List<EventByType>();

        #endregion

        #region Unity Events

        private void OnEnable()
        {
            PoiLocation.POILocationSelected += SetupEvent;
            TeamSelectionUIDataModel.TeamConfirmed += OnTeamMembersConfirmed;
            GameplayEventBase.GameplayEventEnded += OnGameplayEventEnded;
            WinConditionController.ReportMatchResults += WinConditionControllerOnReportMatchResults;
        }

        private void OnDisable()
        {
            PoiLocation.POILocationSelected -= SetupEvent;
            TeamSelectionUIDataModel.TeamConfirmed -= OnTeamMembersConfirmed;
            GameplayEventBase.GameplayEventEnded -= OnGameplayEventEnded;
            WinConditionController.ReportMatchResults -= WinConditionControllerOnReportMatchResults;
        }

        #endregion

        #region Class Implementation

        public void SetupEvent(PoiLocation poiLocation, GameplayEventType gameplayEventType)
        {
            if (poiLocation.IsNull() || gameplayEventType.IsNull())
            {
                return;
            }

            m_lastPressedPOILocation = poiLocation.savedLocation;
            m_currentEventIdentifier = gameplayEventType.eventGUID;
            
            MapDataController.Instance.SetLastPoint(m_lastPressedPOILocation);
            MapDataController.Instance.SetLastEventString(m_currentEventIdentifier);

            var foundEventByType = eventsByType.FirstOrDefault(ebt => ebt.eventType == gameplayEventType);

            if (foundEventByType.IsNull())
            {
                Debug.LogError("Event doesn't exist");
                return;
            }
            
            StartCoroutine(C_DoFullEventAction(poiLocation, foundEventByType));
        }

        public IEnumerator C_DoFullEventAction(PoiLocation _poiLocation, EventByType _eventByType)
        {
            if (_eventByType.IsNull())
            {
                Debug.LogError("Event Null");
                yield break;
            }
            
            HideMap?.Invoke();

            yield return new WaitForSeconds(0.3f);
            
            _eventByType.triggeredEvent?.Invoke(_eventByType.eventType);

            if (!_eventByType.entranceAnimation.IsNull())
            {
                _eventByType.entranceAnimation.Play();

                yield return new WaitUntil(() => !_eventByType.entranceAnimation.isPlaying);     
            }
            
            yield return new WaitUntil(() => m_hasReceivedPlayerAction);

            Debug.Log("EVENT COMPLETED");
            
            yield return new WaitForSeconds(1f);

            if (!_eventByType.exitAnimation.IsNull())
            {
                _eventByType.exitAnimation.Play();

                yield return new WaitUntil(() => !_eventByType.exitAnimation.isPlaying);   
            }

            _eventByType.endEvent?.Invoke(_eventByType.eventType);
            
            EventEnded?.Invoke(_poiLocation, _eventByType.eventType);
            
            m_hasReceivedPlayerAction = false;
            
            DisplayMap?.Invoke();

        }
        
        private void WinConditionControllerOnReportMatchResults(bool _playerHasWonMatch)
        {
            Debug.Log("LEVEL EVENT CONTROLLER: WIN CONDITION");
            m_currentEventIdentifier = MapDataController.Instance.GetLastEventString();
            m_lastPressedPOILocation = MapDataController.Instance.GetLastPoint();
            StartCoroutine(C_ReturnFromMatchScene());
        }

        private IEnumerator C_ReturnFromMatchScene()
        {
            var _eventByType = eventsByType.FirstOrDefault(ebt => ebt.eventType.eventGUID == m_currentEventIdentifier);
            
            UIUtils.FadeBlack(false);
            
            yield return new WaitForSeconds(1f);

            if (!_eventByType.exitAnimation.IsNull())
            {
                _eventByType.exitAnimation.Play();

                yield return new WaitUntil(() => !_eventByType.exitAnimation.isPlaying);   
            }

            _eventByType.endEvent?.Invoke(_eventByType.eventType);
            
            DisplayMap?.Invoke();
            MatchEventEnded?.Invoke(m_currentEventIdentifier, m_lastPressedPOILocation);

            m_hasReceivedPlayerAction = false;
            
        }
        
        private void OnGameplayEventEnded()
        {
            m_hasReceivedPlayerAction = true;
        }
        
        //--------- Team Selection Update ---------
        private void OnTeamMembersConfirmed(List<CharacterStatsBase> _characters, bool _isFirstTime, bool _isRandomTeam)
        {
            if (!_isRandomTeam)
            {
                return;
            }
            
            if (_isFirstTime)
            {
                DisplayMap?.Invoke();
                return;
            }
            
            m_hasReceivedPlayerAction = true;
        }



        #endregion
    }
}