using System.Collections.Generic;
using Data.DataSaving;
using Project.Scripts.Utils;
using Rewired.Data.Mapping;
using Runtime.Managers;
using UnityEngine;

namespace Runtime.GameControllers
{
    public class MapDataController: GameControllerBase, ISaveableData
    {
        
        #region Static

        public static MapDataController Instance { get; private set; }

        #endregion

        #region Private Fields

        [SerializeField]
        private List<MapController.RowData> allRowsByLevel = new List<MapController.RowData>();

        private int m_selectionLevel;
        
        private string m_currentEventIdentifier;

        private Vector3 m_lastPressedPOILocation;

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

        public int GetSelectionLevel()
        {
            return m_selectionLevel;
        }

        public void NextSelectionLevel()
        {
            m_selectionLevel++;
        }

        public Vector3 GetLastPoint()
        {
            return m_lastPressedPOILocation;
        }

        public void SetLastPoint(Vector3 _location)
        {
            m_lastPressedPOILocation = _location;
        }

        public string GetLastEventString()
        {
            return m_currentEventIdentifier;
        }

        public void SetLastEventString(string _newString)
        {
            m_currentEventIdentifier = _newString;
        }

        public List<MapController.RowData> GetGeneratedLevel()
        {
            return allRowsByLevel.ToList();
        }

        public void SetLevelChanges(List<MapController.RowData> _newMap)
        {
            var copy = new List<MapController.RowData>(_newMap);
            allRowsByLevel = copy;
        }

        public void ChangeSavedPoint(int _selectionLevel, MapController.SaveablePointData _point)
        {
            
        }

        public void ResetAll()
        {
            m_selectionLevel = 0;
            m_currentEventIdentifier = "";
            m_lastPressedPOILocation = Vector3.zero;
            allRowsByLevel = new List<MapController.RowData>();
        }

        #endregion

        #region ISaveableData Inherited Methods

        public void LoadData(SavedGameData _savedGameData)
        {
            m_lastPressedPOILocation = _savedGameData.m_lastPressedPOIpoisiton;
            m_currentEventIdentifier = _savedGameData.m_currentEventIdetifier;
            allRowsByLevel = _savedGameData.savedMap;
            m_selectionLevel = _savedGameData.savedMapSelectionLevel;
        }

        public void SaveData(ref SavedGameData _savedGameData)
        {
            _savedGameData.m_currentEventIdetifier = m_currentEventIdentifier;
            _savedGameData.m_lastPressedPOIpoisiton = m_lastPressedPOILocation;
            _savedGameData.savedMap = allRowsByLevel;
            _savedGameData.savedMapSelectionLevel = m_selectionLevel;
        } 

        #endregion

       
    }
}