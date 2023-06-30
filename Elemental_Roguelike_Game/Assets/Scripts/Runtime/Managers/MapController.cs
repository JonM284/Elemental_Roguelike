using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using Project.Scripts.Runtime.LevelGeneration;
using Project.Scripts.Utils;
using Runtime.GameplayEvents;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Runtime.Managers
{
    public class MapController: MonoBehaviour
    {

        #region Nested Classes

        [Serializable]
        public class RowData
        {
            public int rowIndex;
            public List<PointData> rowPoints = new List<PointData>();
        }
        
        [Serializable]
        public class PointData
        {
            public PoiLocation actualPoiLocation;
            public List<PoiLocation> connectedPointsAbove = new List<PoiLocation>();
        }
        
        public class GameplayEventsByType
        {
            
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

        [SerializeField] private float offsetMultiplier = 0.1f;
        
        [FormerlySerializedAs("matchEventType")]
        [Space(20)]
        [Header("POI Events")]
        [SerializeField] private GameplayEventType matchEventType;
        
        [SerializeField] private List<GameplayEventType> allEventTypes = new List<GameplayEventType>();

        #endregion

        #region Private Fields

        private int m_selectionLevel = 0;
        
        private bool isGeneratingMap = false;

        private bool isGeneratingLines = false;

        private int currentIterator = 0;

        private int maxColumns = 5;

        private int startingIterator = 0;

        private PointData m_currentPoint;
        
        private List<RowData> allRowsByLevel = new List<RowData>();

        private List<GameObject> m_cachedDataPointObjects = new List<GameObject>();

        private List<GameplayEventType> m_possibleEventTypes = new List<GameplayEventType>();

        #endregion

        #region Unity Events

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                CreateOverviewMap();
            }
            
        }

        private void OnEnable()
        {
            PoiLocation.POILocationSelected += OnPOILocationSelected;
        }

        private void OnDisable()
        {
            PoiLocation.POILocationSelected -= OnPOILocationSelected;
        }

        #endregion

        #region Class Implementation

        [ContextMenu("Generate Map")]
        private void CreateOverviewMap()
        {
            if (!isLobbyScene)
            {
                return;
            }

            isGeneratingMap = true;

            currentIterator = startingIterator;

            for (int i = 0; i < maxAmountOfRows + 2; i++)
            {
                allRowsByLevel.Add(new RowData());
            }
            
            
            StartCoroutine(PointGeneration());

        }
        
        private void OnPOILocationSelected(PoiLocation _pointLocation, GameplayEventType _event)
        {
            if (_pointLocation.IsNull())
            {
                return;
            }
            
            m_currentPoint.connectedPointsAbove.ForEach(t =>
            {
                t.TryGetComponent(out PoiLocation poiLocation);
                if (poiLocation)
                {
                    if (poiLocation != _pointLocation)
                    {
                        poiLocation.SetPointInactive();
                    }
                }
            });

            if (m_selectionLevel < maxAmountOfRows)
            {
                m_selectionLevel++;    
            }
            else
            {
                return;
            }
            
            //ToDo: get this to work correctly, after selecting a point, have it highlight the next points

            var checkPoint = allRowsByLevel[m_selectionLevel].rowPoints.FirstOrDefault(pd => pd.actualPoiLocation == _pointLocation);
            if (checkPoint.IsNull())
            {
                Debug.LogError("No Point Found");
                return;
            }

            m_currentPoint = checkPoint;

            MarkConnectedActive(m_currentPoint);

        }

        private void DoEventAction(GameplayEventType _event)
        {
            
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
                    var firstPointTransform = InstantiatePointAt(starterPoint.localPosition);
                    firstPointTransform.TryGetComponent(out PoiLocation poiLocation);
                    
                    if (poiLocation.IsNull())
                    {
                        Debug.Log("Doesn't have poiLocation Component");
                        break;
                    }
                    
                    var firstPointData = new PointData
                    {
                        actualPoiLocation = poiLocation,
                    };
                    allRowsByLevel[currentIterator].rowIndex = currentIterator;
                    allRowsByLevel[currentIterator].rowPoints.Add(firstPointData);
                    currentIterator++;
                    continue;
                }
                
                if (currentIterator == maxAmountOfRows + 1)
                {
                    //Create Boss Token
                    var lastPointTransform = InstantiatePointAt(finalPoint.localPosition);
                    
                    lastPointTransform.TryGetComponent(out PoiLocation poiLocation);
                    
                    if (poiLocation.IsNull())
                    {
                        Debug.Log("Doesn't have poiLocation Component");
                        break;
                    }
                    
                    var lastPointData = new PointData
                    {
                        actualPoiLocation = poiLocation,
                    };
                    allRowsByLevel[currentIterator].rowIndex = currentIterator;
                    allRowsByLevel[currentIterator].rowPoints.Add(lastPointData);
                    foreach (var point in allRowsByLevel[currentIterator - 1].rowPoints)
                    {
                        point.connectedPointsAbove.Add(lastPointData.actualPoiLocation);
                        ConnectPoints(point.actualPoiLocation.transform.localPosition,
                            lastPointData.actualPoiLocation.transform.localPosition);
                    }
                    isGeneratingMap = false;
                    Debug.Log("Finished Points");
                    OnMapGenerated?.Invoke();

                    m_currentPoint = allRowsByLevel[0].rowPoints.FirstOrDefault();
                    MarkConnectedActive(m_currentPoint);

                    yield break;
                }

                //plan current row

                float percentage = (float)currentIterator / maxAmountOfRows;

                var previousRow = allRowsByLevel[currentIterator - 1];
                var currentRow = allRowsByLevel[currentIterator];
                
                var randomAmountOfColumns = previousRow.rowPoints.Count > 1 ? Random.Range(previousRow.rowPoints.Count - 1, maxColumns) : Random.Range(2,maxColumns);
                
                //row index
                
                currentRow.rowIndex = currentIterator;
                

                for (int i = 0; i < randomAmountOfColumns; i++)
                {
                    float horizontalPosByColumn = randomAmountOfColumns > 1 ? (float)i / randomAmountOfColumns + (columnBoundsHorizontal/randomAmountOfColumns)
                        : columnBoundsHorizontal;
                    
                    float _Xposition = -columnBoundsHorizontal + horizontalPosByColumn;
                    float _Yposition = (starterPoint.transform.localPosition.y + percentage);
                    
                    Vector3 _pointPosition = new Vector3(_Xposition, _Yposition, 0);
                    
                    var rowPointTransform = InstantiatePointAt(_pointPosition);
                    
                    rowPointTransform.TryGetComponent(out PoiLocation poiLocation);
                    
                    if (poiLocation.IsNull())
                    {
                        Debug.Log("Doesn't have poiLocation Component");
                        break;
                    }
                    
                    var rowPointData = new PointData
                    {
                        actualPoiLocation = poiLocation,
                    };
                    currentRow.rowPoints.Add(rowPointData);
                    
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
                    currentIterator++;
                    continue;
                }

                for (int i = 0; i < previousRow.rowPoints.Count; i++)
                {
                    var prevRowCurrentPoint = previousRow.rowPoints[i];
                    if (i == 0)
                    {
                        
                        prevRowCurrentPoint.connectedPointsAbove.Add(currentRow.rowPoints[i].actualPoiLocation);
                        
                        ConnectPoints(prevRowCurrentPoint.actualPoiLocation.transform.localPosition,
                            currentRow.rowPoints[0].actualPoiLocation.transform.localPosition);

                        if (maxColumnsPreviousRow < maxColumnsCurrentRow)
                        {
                            prevRowCurrentPoint.connectedPointsAbove.Add(currentRow.rowPoints[1].actualPoiLocation);
                        
                            ConnectPoints(prevRowCurrentPoint.actualPoiLocation.transform.localPosition,
                                currentRow.rowPoints[1].actualPoiLocation.transform.localPosition);
                        }
                        continue;
                        
                    }else if (i == previousRow.rowPoints.Count - 1)
                    {
                        var lastPointCurrentRow = currentRow.rowPoints.LastOrDefault();
                       
                        prevRowCurrentPoint.connectedPointsAbove.Add(lastPointCurrentRow.actualPoiLocation);
                        
                        ConnectPoints(prevRowCurrentPoint.actualPoiLocation.transform.localPosition,
                            lastPointCurrentRow.actualPoiLocation.transform.localPosition);

                        if (maxColumnsPreviousRow < maxColumnsCurrentRow)
                        {
                            prevRowCurrentPoint.connectedPointsAbove.Add(currentRow.rowPoints[currentRow.rowPoints.Count - 2].actualPoiLocation);
                        
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
                            availableConnects.Remove(exactAbovePoint);
                            prevRowCurrentPoint.connectedPointsAbove.Add(exactAbovePoint.actualPoiLocation);
                            ConnectPoints(prevRowCurrentPoint.actualPoiLocation.transform.localPosition,
                                exactAbovePoint.actualPoiLocation.transform.localPosition);
                            continue;
                        }

                        var randomOtherConnect = Random.Range(0, availableConnects.Count);
                        var randomPoint = availableConnects[randomOtherConnect];
                        availableConnects.Remove(randomPoint);
                        prevRowCurrentPoint.connectedPointsAbove.Add(randomPoint.actualPoiLocation);
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


        //ToDo: This is set to completely random, this should be controlled randomness
        private GameplayEventType GetRandomEventType()
        {
            var randomInt = Random.Range(0, allEventTypes.Count);
            return allEventTypes[randomInt];
        }

        private Transform InstantiatePointAt(Vector3 _instPosition)
        {
            var go = Instantiate(poiPrefab, mapGenParent);
            go.transform.localPosition = _instPosition;
            
            //Initialize Point
            go.TryGetComponent(out PoiLocation pointLocation);
            if (pointLocation)
            {
                pointLocation.Initialize(GetRandomEventType());
            }
            
            return go.transform;
        }

        private void ConnectPoints(Vector3 _point1LocPos, Vector3 _point2LocPos)
        {
            var lineGo = Instantiate(connectorPrefab, mapGenParent);
            var lineRend = lineGo.GetComponent<LineRenderer>();
            for (int i = 0; i < lineRend.positionCount; i++)
            {
                var corrPos = i == 0 ? _point1LocPos : _point2LocPos;
                lineRend.SetPosition(i, corrPos);
            }
        }

        #endregion
        
        
        
    }
}