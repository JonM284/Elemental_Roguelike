using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Project.Scripts.Runtime.LevelGeneration;
using Project.Scripts.Utils;
using Runtime.GameControllers;
using Runtime.GameplayEvents;
using Runtime.Selection;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Runtime.Managers
{
    public class MapController: MonoBehaviour, ISelectable
    {

        #region Nested Classes

        [Serializable]
        public class RowData
        {
            public int rowIndex;
            public List<PointData> rowPoints = new List<PointData>();
            public List<SaveablePointData> saveablePointDatas = new List<SaveablePointData>();
        }
        
        [Serializable]
        public class SaveablePointData
        {
            public Vector3 pointLocation;
            public string assignedEventGUID;
            public bool isCurrentPoint;
            public bool isCompleted;
            public bool isPassed;
            public List<Vector3> connectedLocationsAbove = new List<Vector3>();
        }
        
        //ToDo: change this to data that can be saved 
        //IE: instead of POILocation => VECTOR3 + Event
        [Serializable]
        public class PointData
        {
            public PoiLocation actualPoiLocation;
            public List<PoiLocation> connectedPointsAbove = new List<PoiLocation>();
        }

        #endregion

        #region Events

        public static event Action OnMapGenerated;

        #endregion
        
        #region Serialized Fields
        
        [SerializeField] private bool isLobbyScene;

        [SerializeField] private GameObject poiPrefab;

        //Connector Line is a Line Renderer, remember this when instantiating
        [SerializeField] private GameObject connectorPrefab;

        [SerializeField] private Transform mapGenParent;

        [SerializeField] private Transform starterPoint;
        
        [SerializeField] private Transform finalPoint;

        [SerializeField] private int maxAmountOfRows = 2;

        [SerializeField] private float columnBoundsHorizontal;
        
        [SerializeField] private float pointOffset = 1;
        
        [SerializeField] private List<GameplayEventType> allEventTypes = new List<GameplayEventType>();

        #endregion

        #region Events

        public UnityEvent onDisplayMap;

        public UnityEvent onHideMap;

        #endregion
        
        #region Private Fields

        private int m_selectionLevel = 0;
        
        private bool isGeneratingMap = false;

        private bool isGeneratingLines = false;

        private bool isGeneratedMap = false;

        private bool m_hasDoneOneTime;

        private int currentIterator = 0;

        private int maxColumns = 4;

        private int startingIterator = 0;

        private string m_lastEventIdentifier;

        private Vector3 m_lastPOILocation;

        private bool m_isReturnFromMatch;

        private bool m_currentEventEnded;

        private List<GameObject> m_cachedPoiLocations = new List<GameObject>();

        private List<GameObject> m_cachedConnectors = new List<GameObject>();
        
        private List<GameObject> m_activePointConnectors = new List<GameObject>();

        private PointData m_currentPoint;

        private Transform m_inactivePool;
        
        [SerializeField]
        private List<RowData> allRowsByLevel = new List<RowData>();

        private List<GameObject> m_cachedDataPointObjects = new List<GameObject>();

        private List<GameplayEventType> m_possibleEventTypes = new List<GameplayEventType>();

        #endregion

        #region Accessors

        public Transform inactivePool => CommonUtils.GetRequiredComponent(ref m_inactivePool, () =>
        {
            var pool = TransformUtils.CreatePool(this.transform, false);
            pool.RenameTransform("Map Inactive Pool");
            return pool;
        });

        public bool mapIsShown { get; private set; }

        #endregion

        #region Unity Events

        private void OnEnable()
        {
            LevelEventController.MatchEventEnded += OnMatchEventEnded;
            LevelEventController.EventEnded += OnEventEnded;
            LevelEventController.DisplayMap += DisplayMap;
            LevelEventController.HideMap += HideMap;
            MapDataController.RegenerateOriginalMap += MapDataControllerOnRegenerateOriginalMap;
        }

        private void OnDisable()
        {
            LevelEventController.MatchEventEnded -= OnMatchEventEnded;
            LevelEventController.EventEnded -= OnEventEnded;
            LevelEventController.DisplayMap -= DisplayMap;
            LevelEventController.HideMap -= HideMap;
            MapDataController.RegenerateOriginalMap -= MapDataControllerOnRegenerateOriginalMap;
        }

        #endregion

        #region Class Implementation

        public void DisplayMap()
        {
            mapIsShown = true;
            onDisplayMap?.Invoke();
        }

        public void DisplayOneTime()
        {
            if (m_hasDoneOneTime)
            {
                return;
            }
            
            DisplayMap();
            m_hasDoneOneTime = true;
        }

        public void HideMap()
        {
            mapIsShown = false;
            onHideMap?.Invoke();
        }

        public void DrawMap()
        {
            allRowsByLevel = MapDataController.Instance.GetGeneratedLevel();
            if (allRowsByLevel.IsNull() || allRowsByLevel.Count == 0)
            {
                CreateOverviewMap();
                return;
            }
            
            RecreateMap();
        }
        
        private void MapDataControllerOnRegenerateOriginalMap()
        {
            if (allRowsByLevel.IsNull() || allRowsByLevel.Count == 0)
            {
                return;
            }

            CacheAllPreviousItems();
            CreateOverviewMap();
        }
        
        //When this event is finished
        private void OnMatchEventEnded(string _lastPressedEventGUID, Vector3 _lastPressedPOILocation)
        {
            m_isReturnFromMatch = true;
            m_lastEventIdentifier = _lastPressedEventGUID;
            m_lastPOILocation = _lastPressedPOILocation;
            m_selectionLevel = MapDataController.Instance.GetSelectionLevel();
            allRowsByLevel = MapDataController.Instance.GetGeneratedLevel();
            Debug.Log("CALLING FROM MATCH EVENT ENDED");
            StartCoroutine(C_RemakeMap());
        }

        [ContextMenu("Generate Map")]
        private void CreateOverviewMap()
        {
            if (!isLobbyScene)
            {
                return;
            }

            isGeneratingMap = true;

            currentIterator = startingIterator;
            
            MapDataController.Instance.ResetAll();
            
            allRowsByLevel.Clear();

            for (int i = 0; i < maxAmountOfRows + 2; i++)
            {
                allRowsByLevel.Add(new RowData());
            }
            
            
            StartCoroutine(PointGeneration());

        }

        //Change Selected Point
        private void OnEventEnded(PoiLocation _pointLocation, GameplayEventType _event)
        {
            if (_pointLocation.IsNull())
            {
                return;
            }

            if (m_currentPoint.IsNull())
            {
                Debug.Log("Current Point Null");
            }

            #region Set Previous Inactive

            {
                //Mark current point as used in saveable data
                var _foundPoint = allRowsByLevel[m_selectionLevel].saveablePointDatas
                    .FirstOrDefault(spd => spd.pointLocation == m_currentPoint.actualPoiLocation.savedLocation);
                if (_foundPoint.isCurrentPoint)
                {
                    _foundPoint.isCompleted = true;
                    _foundPoint.isPassed = true;
                    _foundPoint.isCurrentPoint = false;
                }
            }
            
            foreach (var _saveablePointData in allRowsByLevel[m_selectionLevel].saveablePointDatas)
            {
                var _index = allRowsByLevel[m_selectionLevel].saveablePointDatas.IndexOf(_saveablePointData);
                allRowsByLevel[m_selectionLevel].rowPoints[_index].actualPoiLocation.SetPointInactive();
            }
            
            #endregion
            

            if (m_selectionLevel < maxAmountOfRows)
            {
                m_selectionLevel++; 
                MapDataController.Instance.NextSelectionLevel();
            }
            else
            {
                return;
            }

            #region Set Next Point

            var checkPoint = allRowsByLevel[m_selectionLevel].rowPoints.FirstOrDefault(pd => pd.actualPoiLocation == _pointLocation);

            {
                //Mark current point as current point in saveable data
                var _foundPoint = allRowsByLevel[m_selectionLevel].saveablePointDatas
                    .FirstOrDefault(spd => spd.pointLocation == _pointLocation.savedLocation && spd.assignedEventGUID == _event.eventGUID);

                if (_foundPoint.IsNull())
                {
                    Debug.LogError("FOUND POINT NULL");
                    _foundPoint = allRowsByLevel[m_selectionLevel].saveablePointDatas
                        .FirstOrDefault(spd => spd.isCurrentPoint);
                }

                if (_foundPoint.IsNull())
                {
                    Debug.LogError("FOUND POINT still null");
                }
                
                var _index = allRowsByLevel[m_selectionLevel].saveablePointDatas.IndexOf(_foundPoint);
                Debug.Log($"Selection Level:{m_selectionLevel}, Index:{_index}");
                allRowsByLevel[m_selectionLevel].rowPoints[_index].actualPoiLocation.SetPointSelected();
                
                _foundPoint.isCurrentPoint = true;
                
            }
            
            foreach (var _saveablePointData in allRowsByLevel[m_selectionLevel].saveablePointDatas)
            {
                if (!_saveablePointData.isCurrentPoint)
                {
                    var _index = allRowsByLevel[m_selectionLevel].saveablePointDatas.IndexOf(_saveablePointData);
                    _saveablePointData.isPassed = true;
                    allRowsByLevel[m_selectionLevel].rowPoints[_index].actualPoiLocation.SetPointInactive();
                }
            }

            #endregion
            
            
            if (checkPoint.IsNull())
            {
                Debug.LogError("No Point Found");
                return;
            }

            m_currentPoint = checkPoint;

            MapDataController.Instance.SetLevelChanges(allRowsByLevel);
            
            MarkConnectedActive(m_currentPoint);
        }

        private void RecreateMap()
        {
            if (!isLobbyScene)
            {
                return;
            }
            Debug.Log("CALLING FROM RECREATE MAP");

            m_selectionLevel = MapDataController.Instance.GetSelectionLevel();
            m_lastEventIdentifier = MapDataController.Instance.GetLastEventString();
            m_lastPOILocation = MapDataController.Instance.GetLastPoint();
            
            StartCoroutine(C_RemakeMap());
        }

        private IEnumerator C_RemakeMap()
        {
            yield return null;

            #region Generate And Connect Points

             foreach (var _row in allRowsByLevel)
            {
                yield return null;

                foreach (var _point in _row.saveablePointDatas)
                {
                    yield return null;
                    
                    var _index = _row.saveablePointDatas.IndexOf(_point);
                    var pointLocation = InstantiatePointAt(_point.pointLocation, _point.assignedEventGUID);
                    
                    _row.rowPoints[_index].actualPoiLocation = pointLocation;

                    if (_point.isPassed)
                    {
                        pointLocation.SetPointInactive();
                    }
                    
                    if (_point.connectedLocationsAbove.Count > 0)
                    {
                        foreach (var _connectedPointAbove in _point.connectedLocationsAbove)
                        {
                            yield return null;

                            ConnectPoints(_point.pointLocation, _connectedPointAbove);
                        }
                    }
                }
            }
            
            //After all rows are finished

            foreach (var rowData in allRowsByLevel)
            {
                var currentRowIndex = rowData.rowIndex;
                if (currentRowIndex == 0)
                {
                    continue;
                }
                
                var currentRow = rowData;
                var previousRow = allRowsByLevel[currentRow.rowIndex - 1];

                if (previousRow.saveablePointDatas.Count == 0)
                {
                    continue;
                }

                foreach (var _saveablePointData in previousRow.saveablePointDatas)
                {
                    var _index = previousRow.saveablePointDatas.IndexOf(_saveablePointData);
                    if (_saveablePointData.connectedLocationsAbove.Count <= 0)
                    {
                        continue;
                    }

                    foreach (var _savedConnectedPoint in _saveablePointData.connectedLocationsAbove)
                    {
                        var _connectedPointIndex = _saveablePointData.connectedLocationsAbove.IndexOf(_savedConnectedPoint);
                        foreach (var _point in currentRow.rowPoints)
                        {
                            if (_savedConnectedPoint == _point.actualPoiLocation.savedLocation)
                            {
                                previousRow.rowPoints[_index].connectedPointsAbove[_connectedPointIndex] =
                                    _point.actualPoiLocation;
                            }
                        }
                    }
                }
            }

            #endregion
            
            //Get Current Point

            {
                int _modifier = m_isReturnFromMatch ? 1 : 0;
                
                
                var currentFoundSaveablePointData = allRowsByLevel[m_selectionLevel + _modifier].saveablePointDatas
                    .FirstOrDefault(spd => spd.pointLocation == m_lastPOILocation && spd.assignedEventGUID == m_lastEventIdentifier);

                if (currentFoundSaveablePointData.IsNull())
                {
                    Debug.Log("Current Point NULL, retry");
                    currentFoundSaveablePointData = allRowsByLevel[m_selectionLevel].saveablePointDatas
                        .FirstOrDefault(spd => spd.isCurrentPoint);
                }

                if (currentFoundSaveablePointData.IsNull())
                {
                    Debug.Log("CURRENT point still NULL");
                }
                
                var _index = allRowsByLevel[m_selectionLevel + _modifier].saveablePointDatas.IndexOf(currentFoundSaveablePointData);

                Debug.Log($"Map Regen, SelectionLevel = Returning?{m_isReturnFromMatch}// {m_selectionLevel + _modifier}, Index: {_index}");
                
                m_currentPoint = allRowsByLevel[m_selectionLevel + _modifier].rowPoints[_index];
                m_currentPoint.actualPoiLocation.SetPointSelected();
            }

            if (m_isReturnFromMatch)
            {
                #region Set All Previous Points Inactive

                {
                    //Mark current point as used in saveable data
                    var _foundPoint = allRowsByLevel[m_selectionLevel].saveablePointDatas
                        .FirstOrDefault(spd => spd.isCurrentPoint);
                    
                    if (_foundPoint.isCurrentPoint)
                    {
                        _foundPoint.isCompleted = true;
                        _foundPoint.isPassed = true;
                        _foundPoint.isCurrentPoint = false;
                    }
                }
            
                foreach (var _saveablePointData in allRowsByLevel[m_selectionLevel].saveablePointDatas)
                {
                    var _index = allRowsByLevel[m_selectionLevel].saveablePointDatas.IndexOf(_saveablePointData);
                    allRowsByLevel[m_selectionLevel].rowPoints[_index].actualPoiLocation.SetPointInactive();
                }

                #endregion
                
                if (m_selectionLevel < maxAmountOfRows)
                {
                    m_selectionLevel++;  
                    MapDataController.Instance.NextSelectionLevel();
                }

                #region Set Next Point Active
                
                {
                    //Mark current point as current point in saveable data
                    var _foundPoint = allRowsByLevel[m_selectionLevel].saveablePointDatas
                        .FirstOrDefault(spd => spd.pointLocation == m_currentPoint.actualPoiLocation.savedLocation 
                                               && spd.assignedEventGUID == m_currentPoint.actualPoiLocation.AssignedEventType.eventGUID);

                    if (_foundPoint.IsNull())
                    {
                        Debug.LogError("FOUND POINT NULL");
                        _foundPoint = allRowsByLevel[m_selectionLevel].saveablePointDatas
                            .FirstOrDefault(spd => spd.isCurrentPoint);
                    }

                    if (_foundPoint.IsNull())
                    {
                        Debug.LogError("FOUND POINT still null");
                    }
                
                    var _index = allRowsByLevel[m_selectionLevel].saveablePointDatas.IndexOf(_foundPoint);
                    Debug.Log($"Selection Level:{m_selectionLevel}, Index:{_index}");

                    _foundPoint.isCurrentPoint = true;
                }
            
                foreach (var _saveablePointData in allRowsByLevel[m_selectionLevel].saveablePointDatas)
                {
                    if (!_saveablePointData.isCurrentPoint)
                    {
                        var _index = allRowsByLevel[m_selectionLevel].saveablePointDatas.IndexOf(_saveablePointData);
                        _saveablePointData.isPassed = true;
                        allRowsByLevel[m_selectionLevel].rowPoints[_index].actualPoiLocation.SetPointInactive();
                    }
                }

                #endregion

                MarkConnectedActive(m_currentPoint);
            }
            else
            {
                MarkConnectedActive(m_currentPoint);
            }

            MapDataController.Instance.SetLevelChanges(allRowsByLevel);

            m_isReturnFromMatch = false;
        }

        /// <summary>
        /// Generate map by these rules:
        /// 1. Find Point positions by percent between current row and full amount of rows
        /// 2. After placing all points, go back and choose which points can connect to each other
        /// 3. Instantiate line renderers to connect points
        /// </summary>
        /// <returns></returns>
        private IEnumerator PointGeneration()
        {
            yield return null;
            
            while (isGeneratingMap)
            {
                Debug.Log("Iterating");

                #region Placing Points
                
                if (currentIterator == 0)
                {
                    //first point is always starting point
                    var firstPoint = InstantiatePointAt(starterPoint.localPosition);
                    
                    if (firstPoint.IsNull())
                    {
                        Debug.Log("Doesn't have poiLocation Component");
                        break;
                    }
                    
                    var firstPointData = new PointData
                    {
                        actualPoiLocation = firstPoint,
                    };

                    var saveableData = new SaveablePointData
                    {
                        assignedEventGUID = firstPoint.AssignedEventType.eventGUID,
                        pointLocation = firstPoint.savedLocation,
                    };
                    
                    allRowsByLevel[currentIterator].rowIndex = currentIterator;
                    allRowsByLevel[currentIterator].rowPoints.Add(firstPointData);
                    allRowsByLevel[currentIterator].saveablePointDatas.Add(saveableData);
                    
                    currentIterator++;
                    continue;
                }
                
                if (currentIterator == maxAmountOfRows + 1)
                {
                    //Create Boss Token
                    var lastPoint = InstantiatePointAt(finalPoint.localPosition);
                    
                    if (lastPoint.IsNull())
                    {
                        Debug.Log("Doesn't have poiLocation Component");
                        break;
                    }
                    
                    var lastPointData = new PointData
                    {
                        actualPoiLocation = lastPoint,
                    };
                    
                    var saveableData = new SaveablePointData
                    {
                        assignedEventGUID = lastPoint.AssignedEventType.eventGUID,
                        pointLocation = lastPoint.savedLocation,
                    };
                    
                    allRowsByLevel[currentIterator].rowIndex = currentIterator;
                    allRowsByLevel[currentIterator].rowPoints.Add(lastPointData);
                    allRowsByLevel[currentIterator].saveablePointDatas.Add(saveableData);
                    
                    foreach (var point in allRowsByLevel[currentIterator - 1].rowPoints)
                    {
                        point.connectedPointsAbove.Add(lastPointData.actualPoiLocation);
                        ConnectPoints(point.actualPoiLocation.transform.localPosition,
                            lastPointData.actualPoiLocation.transform.localPosition);
                    }

                    foreach (var saveablePoint in allRowsByLevel[currentIterator - 1].saveablePointDatas)
                    {
                        saveablePoint.connectedLocationsAbove.Add(lastPointData.actualPoiLocation.savedLocation);
                    }
                    
                    isGeneratingMap = false;
                    isGeneratedMap = true;
                    Debug.Log("Finished Points");
                    OnMapGenerated?.Invoke();

                    m_currentPoint = allRowsByLevel[0].rowPoints.FirstOrDefault(); 
                    var m_firstSaved = allRowsByLevel[0].saveablePointDatas.FirstOrDefault();
                    m_firstSaved.isCompleted = true;
                    m_firstSaved.isCurrentPoint = true;
                    
                    MarkConnectedActive(m_currentPoint);
                    
                    MapDataController.Instance.SetLevelChanges(allRowsByLevel);

                    yield break;
                }

                //plan current row

                float percentage = (float)currentIterator / maxAmountOfRows;

                var previousRow = allRowsByLevel[currentIterator - 1];
                var currentRow = allRowsByLevel[currentIterator];

                var totalHeight = (finalPoint.transform.localPosition.y - starterPoint.transform.localPosition.y );
                
                var randomAmountOfColumns = previousRow.rowPoints.Count > 1 ? Random.Range(previousRow.rowPoints.Count - 1, maxColumns) : Random.Range(2,maxColumns);
                
                //row index
                
                currentRow.rowIndex = currentIterator;
                

                for (int i = 0; i < randomAmountOfColumns; i++)
                {
                    float horizontalPosByColumn = (float)i / randomAmountOfColumns + (columnBoundsHorizontal/randomAmountOfColumns);

                    float _Xposition = randomAmountOfColumns > 1 ? ((-columnBoundsHorizontal/1.5f) + horizontalPosByColumn) * pointOffset : 0;
                    float _Yposition = ((totalHeight * percentage) - (totalHeight/1.5f));
                    
                    Vector3 _pointPosition = new Vector3(_Xposition, _Yposition, 0);
                    
                    var rowPoint = InstantiatePointAt(_pointPosition);
                    
                    if (rowPoint.IsNull())
                    {
                        Debug.Log("Doesn't have poiLocation Component");
                        break;
                    }
                    
                    var rowPointData = new PointData
                    {
                        actualPoiLocation = rowPoint,
                    };
                    
                    var saveableData = new SaveablePointData
                    {
                        assignedEventGUID = rowPoint.AssignedEventType.eventGUID,
                        pointLocation = rowPoint.savedLocation,
                    };
                    
                    currentRow.rowPoints.Add(rowPointData);
                    currentRow.saveablePointDatas.Add(saveableData);
                    
                    yield return null;
                }
                
                yield return null;

                #endregion
                
                //At this point, the current row has been created.
                //We can go back to the previous row and connect them with the current row
                //If it is the first row: it connects to all other points.
                //If it is the last row: all other points connect to it.

                #region Connecting Points

                
                //Final check and connection

                if (previousRow.rowIndex == 0)
                {
                    foreach (var pointData in currentRow.rowPoints)
                    {
                        previousRow.rowPoints[0].connectedPointsAbove.Add(pointData.actualPoiLocation);
                        ConnectPoints(previousRow.rowPoints[0].actualPoiLocation.transform.localPosition,
                            pointData.actualPoiLocation.transform.localPosition);
                    }
                    
                    foreach (var saveablePoint in currentRow.saveablePointDatas)
                    {
                        previousRow.saveablePointDatas[0].connectedLocationsAbove.Add(saveablePoint.pointLocation);
                    }
                    
                    currentIterator++;
                    continue;
                }

                var maxColumnsCurrentRow = currentRow.rowPoints.Count;
                var maxColumnsPreviousRow = previousRow.rowPoints.Count;

                //only one output in this row, connect all previous to this one
                if (maxColumnsCurrentRow == 1)
                {
                    foreach (var previousPoint in previousRow.rowPoints)
                    {
                        previousPoint.connectedPointsAbove.Add(currentRow.rowPoints[0].actualPoiLocation);
                        ConnectPoints(previousPoint.actualPoiLocation.transform.localPosition, 
                            currentRow.rowPoints[0].actualPoiLocation.transform.localPosition);
                    }
                    
                    foreach (var previousSaveablePoint in previousRow.saveablePointDatas)
                    {
                        previousSaveablePoint.connectedLocationsAbove.Add(currentRow.saveablePointDatas[0].pointLocation);
                    }
                    currentIterator++;
                    continue;
                }

                if (maxColumnsPreviousRow == 1)
                {
                    for (int i = 0; i < currentRow.rowPoints.Count; i++)
                    {
                        previousRow.rowPoints[0].connectedPointsAbove.Add(currentRow.rowPoints[i].actualPoiLocation);
                        ConnectPoints(previousRow.rowPoints[0].actualPoiLocation.transform.localPosition,
                            currentRow.rowPoints[i].actualPoiLocation.transform.localPosition);
                    }
                    
                    for(int i = 0; i < currentRow.saveablePointDatas.Count; i++)
                    {
                        previousRow.saveablePointDatas[0].connectedLocationsAbove.Add(currentRow.saveablePointDatas[i].pointLocation);
                    }
                    currentIterator++;
                    continue;
                }

                for (int i = 0; i < previousRow.rowPoints.Count; i++)
                {
                    var prevRowCurrentPoint = previousRow.rowPoints[i];
                    var savedPrevRowCurrentPoint = previousRow.saveablePointDatas[i];
                    if (i == 0)
                    {
                        
                        prevRowCurrentPoint.connectedPointsAbove.Add(currentRow.rowPoints[i].actualPoiLocation);
                        
                        savedPrevRowCurrentPoint.connectedLocationsAbove.Add(currentRow.saveablePointDatas[i].pointLocation);
                        
                        ConnectPoints(prevRowCurrentPoint.actualPoiLocation.transform.localPosition,
                            currentRow.rowPoints[0].actualPoiLocation.transform.localPosition);

                        if (maxColumnsPreviousRow < maxColumnsCurrentRow)
                        {
                            prevRowCurrentPoint.connectedPointsAbove.Add(currentRow.rowPoints[1].actualPoiLocation);
                            savedPrevRowCurrentPoint.connectedLocationsAbove.Add(currentRow.saveablePointDatas[1].pointLocation);
                        
                            ConnectPoints(prevRowCurrentPoint.actualPoiLocation.transform.localPosition,
                                currentRow.rowPoints[1].actualPoiLocation.transform.localPosition);
                        }
                        continue;
                        
                    }else if (i == previousRow.rowPoints.Count - 1)
                    {
                        var lastPointCurrentRow = currentRow.rowPoints.LastOrDefault();
                        var saveableLastPointCurrentRow = currentRow.saveablePointDatas.LastOrDefault();
                       
                        prevRowCurrentPoint.connectedPointsAbove.Add(lastPointCurrentRow.actualPoiLocation);
                        savedPrevRowCurrentPoint.connectedLocationsAbove.Add(saveableLastPointCurrentRow.pointLocation);
                        
                        ConnectPoints(prevRowCurrentPoint.actualPoiLocation.transform.localPosition,
                            lastPointCurrentRow.actualPoiLocation.transform.localPosition);

                        if (maxColumnsPreviousRow < maxColumnsCurrentRow)
                        {
                            prevRowCurrentPoint.connectedPointsAbove.Add(currentRow.rowPoints[currentRow.rowPoints.Count - 2].actualPoiLocation);
                            savedPrevRowCurrentPoint.connectedLocationsAbove.Add(currentRow.saveablePointDatas[currentRow.saveablePointDatas.Count - 2].pointLocation);
                        
                            ConnectPoints(prevRowCurrentPoint.actualPoiLocation.transform.localPosition,
                                currentRow.rowPoints[currentRow.rowPoints.Count - 2].actualPoiLocation.transform.localPosition);
                        }

                        continue;
                    }
                    
                    //max has to start at 1 because final number is exclusive in random.range
                    List<PointData> availableConnects = new List<PointData>();

                    //if you are points in the middle, you usually have 3 options
                    if (!currentRow.rowPoints[i-1].IsNull())
                    {
                        availableConnects.Add(currentRow.rowPoints[i-1]);
                    }

                    if (!currentRow.rowPoints[i].IsNull())
                    {
                        availableConnects.Add(currentRow.rowPoints[i]);
                    }

                    if (maxColumnsCurrentRow >= maxColumnsPreviousRow && !currentRow.rowPoints[i+1].IsNull())
                    {
                        availableConnects.Add(currentRow.rowPoints[i+1]);
                    }
                    
                    int randomConnectsAmount = Random.Range(1,3);

                    for (int j = 0; j < randomConnectsAmount; j++)
                    {
                        if (j == 0)
                        {
                            var exactAbovePoint = currentRow.rowPoints[i];
                            var exactSavableAbovePoint = currentRow.saveablePointDatas[i];
                            availableConnects.Remove(exactAbovePoint);
                            prevRowCurrentPoint.connectedPointsAbove.Add(exactAbovePoint.actualPoiLocation);
                            savedPrevRowCurrentPoint.connectedLocationsAbove.Add(exactSavableAbovePoint.pointLocation);
                            ConnectPoints(prevRowCurrentPoint.actualPoiLocation.transform.localPosition,
                                exactAbovePoint.actualPoiLocation.transform.localPosition);
                            continue;
                        }

                        var randomOtherConnect = Random.Range(0, availableConnects.Count);
                        var randomPoint = availableConnects[randomOtherConnect];
                        availableConnects.Remove(randomPoint);
                        prevRowCurrentPoint.connectedPointsAbove.Add(randomPoint.actualPoiLocation);
                        savedPrevRowCurrentPoint.connectedLocationsAbove.Add(randomPoint.actualPoiLocation.savedLocation);
                        ConnectPoints(prevRowCurrentPoint.actualPoiLocation.transform.localPosition,
                            randomPoint.actualPoiLocation.transform.localPosition);

                    }

                    

                }
                

                yield return null;

                
                #endregion
                
                
                currentIterator++;

            }
            
        }

        private void MarkConnectedActive(PointData _currentPoint)
        {
            if (_currentPoint.IsNull())
            {
                Debug.LogError("Current Point null");
                return;
            }

            foreach (var point in _currentPoint.connectedPointsAbove)
            {
                point.TryGetComponent(out PoiLocation pointLocation);
                if (pointLocation)
                {
                    pointLocation.SetPointActive();
                }
            }
            
        }

        private void CacheAllPreviousItems()
        {
            foreach (var _row in allRowsByLevel)
            {
                foreach (var _point in _row.rowPoints)
                {
                    
                    m_cachedPoiLocations.Add(_point.actualPoiLocation.gameObject);
                    _point.actualPoiLocation.transform.parent = inactivePool;
                }
            }

            foreach (var _connector in m_activePointConnectors)
            {
                m_cachedConnectors.Add(_connector);
                _connector.transform.parent = inactivePool;
            }
            
            m_activePointConnectors.Clear();
        }


        //ToDo: This is set to completely random, this should be controlled randomness
        //Set rows, match row, item row, etc
        private GameplayEventType GetRandomEventType()
        {
            var randomInt = Random.Range(0, allEventTypes.Count);
            return allEventTypes[randomInt];
        }

        private GameplayEventType GetEventByGUID(string _searchGUID)
        {
            return allEventTypes.FirstOrDefault(get => get.eventGUID == _searchGUID);
        }

        private PoiLocation InstantiatePointAt(Vector3 _instPosition, string _eventType = "")
        {
            GameObject go;
            
            if (m_cachedPoiLocations.Count > 0)
            {
                go = m_cachedPoiLocations[0];
                go.transform.parent = mapGenParent;
                m_cachedPoiLocations.Remove(go);
            }
            else
            {
                go = Instantiate(poiPrefab, mapGenParent);
            }
            
            go.transform.localPosition = _instPosition;
            
            //Initialize Point
            go.TryGetComponent(out PoiLocation pointLocation);
            if (pointLocation)
            {
                var _event = string.IsNullOrEmpty(_eventType) ? GetRandomEventType() : GetEventByGUID(_eventType);
                pointLocation.Initialize(_event);
            }
            
            return pointLocation;
        }

        private void ConnectPoints(Vector3 _point1LocPos, Vector3 _point2LocPos)
        {
            GameObject lineGo;
            
            if (m_cachedConnectors.Count > 0)
            {
                lineGo = m_cachedConnectors[0];
                lineGo.transform.parent = mapGenParent;
                m_cachedConnectors.Remove(lineGo);
            }
            else
            {
                lineGo = Instantiate(connectorPrefab, mapGenParent);
            }
            
            var lineRend = lineGo.GetComponent<LineRenderer>();
            for (int i = 0; i < lineRend.positionCount; i++)
            {
                var corrPos = i == 0 ? _point1LocPos : _point2LocPos;
                lineRend.SetPosition(i, corrPos);
            }

            m_activePointConnectors.Add(lineGo);
        }

        #endregion

        #region ISelectable Inherited Methods

        public void OnSelect()
        {
            if (isGeneratedMap || isGeneratingMap)
            {
                return;
            }

            if (m_isReturnFromMatch)
            {
                return;
            }
            
            DrawMap();
        }

        public void OnUnselected()
        {
        }

        public void OnHover()
        {
        }

        public void OnUnHover()
        {
        }

        #endregion
    }
}