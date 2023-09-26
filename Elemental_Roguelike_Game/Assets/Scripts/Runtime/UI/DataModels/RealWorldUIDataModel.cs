using System;
using System.Collections.Generic;
using Project.Scripts.Runtime.LevelGeneration;
using Runtime.GameControllers;
using Runtime.GameplayEvents;
using UnityEngine;

namespace Runtime.UI.DataModels
{
    public class RealWorldUIDataModel: MonoBehaviour
    {

        #region Serialized Fields

        [SerializeField]
        private List<GameObject> buttonGO;

        #endregion

        #region Unity Events

        private void OnEnable()
        {
            PoiLocation.POILocationSelected += EventStart;
            LevelEventController.EventEnded += LevelEventControllerOnEventEnded;
            LevelEventController.MatchEventEnded += LevelEventControllerOnMatchEventEnded;
        }

        private void OnDisable()
        {
            PoiLocation.POILocationSelected -= EventStart;
            LevelEventController.EventEnded -= LevelEventControllerOnEventEnded;
            LevelEventController.MatchEventEnded -= LevelEventControllerOnMatchEventEnded;
        }

        #endregion
        
        #region Class Implementation

        private void EventStart(PoiLocation _poi, GameplayEventType _event)
        {
            buttonGO.ForEach(g => g.SetActive(false));
        }
        
        private void LevelEventControllerOnMatchEventEnded(string _eventIdentifier, Vector3 _eventPOILocation)
        {
            buttonGO.ForEach(g => g.SetActive(true));
        }

        private void LevelEventControllerOnEventEnded(PoiLocation _poi, GameplayEventType _event)
        {
            buttonGO.ForEach(g => g.SetActive(true));
        }
        
        public void ResetMap()
        {
            MapDataController.Instance.RecreateMap();
        }

        public void ResetALL()
        {
            TeamController.Instance.RemoveAllTeamMembers();
            MapDataController.Instance.ResetAll();
            MapDataController.Instance.StartFromBeginning();
        }
        
        #endregion

        
    }
}