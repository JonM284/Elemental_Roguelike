using System;
using System.Collections.Generic;
using System.Linq;
using Project.Scripts.Runtime.LevelGeneration;
using Project.Scripts.Utils;
using Runtime.GameplayEvents;
using UnityEngine;
using UnityEngine.Events;

namespace Runtime.GameControllers
{
    public class LevelEventController: GameControllerBase
    {

        #region Nested Classes
        [SerializeField]
        public class EventByType
        {
            public GameplayEventType eventType;
            public  UnityEvent<GameplayEventType> triggeredEvent;
        }

        #endregion

        #region Private Fields

        private List<EventByType> eventsByType = new List<EventByType>();

        #endregion

        #region Unity Events

        private void OnEnable()
        {
            PoiLocation.POILocationSelected += SetupEvent;
        }

        private void OnDisable()
        {
            PoiLocation.POILocationSelected -= SetupEvent;
        }

        #endregion

        #region Class Implementation

        public void SetupEvent(PoiLocation poiLocation, GameplayEventType gameplayEventType)
        {
            if (poiLocation.IsNull() || gameplayEventType.IsNull())
            {
                return;
            }

            var foundEventByType = eventsByType.FirstOrDefault(ebt => ebt.eventType == gameplayEventType);

            if (!foundEventByType.IsNull())
            {
                foundEventByType.triggeredEvent?.Invoke(foundEventByType.eventType);
            }

        }

        public void MatchEvent(GameplayEventType _eventType)
        {
            if (_eventType.IsNull())
            {
                return;
            }

            if (_eventType is MatchEventType matchEvent)
            {
                SceneController.Instance.LoadScene(matchEvent.sceneName + matchEvent.arenaID);
            }
            
            
        }

        public void ItemEvent(GameplayEventType _eventType)
        {
            if (_eventType.IsNull())
            {
                return;
            }
        }

        public void UpgradeEvent(GameplayEventType _eventType)
        {
            if (_eventType.IsNull())
            {
                return;
            }
        }

        public void NewMemberEvent(GameplayEventType _eventType)
        {
            if (_eventType.IsNull())
            {
                return;
            }
        }

        #endregion
        
        
        
        
        
    }
}