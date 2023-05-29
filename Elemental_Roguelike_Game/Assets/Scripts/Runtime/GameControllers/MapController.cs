using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Project.Scripts.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Runtime.GameControllers
{
    public class MapController: GameControllerBase
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
            public Transform attachedTransform;
            public List<Transform> connectedPointsAbove = new List<Transform>();
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
        
        #endregion

        #region Private Fields

        private bool isGeneratingMap = false;

        private bool isGeneratingLines = false;

        private int currentIterator = 0;

        private int maxColumns = 5;

        private int startingIterator = 0;
        
        [SerializeField]
        private List<RowData> allRowsByLevel = new List<RowData>();

        private List<GameObject> m_cachedDataPointObjects = new List<GameObject>();

        #endregion

        #region Unity Events

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                CreateOverviewMap();
            }
        }

        #endregion

        #region GameControllerBase Inherited Methods

        public override void Initialize()
        {
            CreateOverviewMap();
            base.Initialize();
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
                    var firstPointData = new PointData
                    {
                        attachedTransform = firstPointTransform,
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
                    var lastPointData = new PointData
                    {
                        attachedTransform = lastPointTransform,
                    };
                    allRowsByLevel[currentIterator].rowIndex = currentIterator;
                    allRowsByLevel[currentIterator].rowPoints.Add(lastPointData);
                    foreach (var point in allRowsByLevel[currentIterator - 1].rowPoints)
                    {
                        point.connectedPointsAbove.Add(lastPointData.attachedTransform);
                        ConnectPoints(point.attachedTransform.localPosition,
                            lastPointData.attachedTransform.localPosition);
                    }
                    isGeneratingMap = false;
                    Debug.Log("Finished Points");
                    OnMapGenerated?.Invoke();
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
                    var rowPointData = new PointData
                    {
                        attachedTransform = rowPointTransform,
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
                        previousRow.rowPoints[0].connectedPointsAbove.Add(pointData.attachedTransform);
                        ConnectPoints(previousRow.rowPoints[0].attachedTransform.localPosition,
                            pointData.attachedTransform.localPosition);
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
                        previousPoint.connectedPointsAbove.Add(currentRow.rowPoints[0].attachedTransform);
                        ConnectPoints(previousPoint.attachedTransform.localPosition, 
                            currentRow.rowPoints[0].attachedTransform.localPosition);
                    }
                    currentIterator++;
                    continue;
                }

                if (maxColumnsPreviousRow == 1)
                {
                    for (int i = 0; i < currentRow.rowPoints.Count; i++)
                    {
                        previousRow.rowPoints[0].connectedPointsAbove.Add(currentRow.rowPoints[i].attachedTransform);
                        ConnectPoints(previousRow.rowPoints[0].attachedTransform.localPosition,
                            currentRow.rowPoints[i].attachedTransform.localPosition);
                    }
                    currentIterator++;
                    continue;
                }

                for (int i = 0; i < previousRow.rowPoints.Count; i++)
                {
                    var prevRowCurrentPoint = previousRow.rowPoints[i];
                    if (i == 0)
                    {
                        
                        prevRowCurrentPoint.connectedPointsAbove.Add(currentRow.rowPoints[i].attachedTransform);
                        
                        ConnectPoints(prevRowCurrentPoint.attachedTransform.localPosition,
                            currentRow.rowPoints[0].attachedTransform.localPosition);

                        if (maxColumnsPreviousRow < maxColumnsCurrentRow)
                        {
                            prevRowCurrentPoint.connectedPointsAbove.Add(currentRow.rowPoints[1].attachedTransform);
                        
                            ConnectPoints(prevRowCurrentPoint.attachedTransform.localPosition,
                                currentRow.rowPoints[1].attachedTransform.localPosition);
                        }
                        continue;
                        
                    }else if (i == previousRow.rowPoints.Count - 1)
                    {
                        var lastPointCurrentRow = currentRow.rowPoints.LastOrDefault();
                       
                        prevRowCurrentPoint.connectedPointsAbove.Add(lastPointCurrentRow.attachedTransform);
                        
                        ConnectPoints(prevRowCurrentPoint.attachedTransform.localPosition,
                            lastPointCurrentRow.attachedTransform.localPosition);

                        if (maxColumnsPreviousRow < maxColumnsCurrentRow)
                        {
                            prevRowCurrentPoint.connectedPointsAbove.Add(currentRow.rowPoints[currentRow.rowPoints.Count - 2].attachedTransform);
                        
                            ConnectPoints(prevRowCurrentPoint.attachedTransform.localPosition,
                                currentRow.rowPoints[currentRow.rowPoints.Count - 2].attachedTransform.localPosition);
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
                            prevRowCurrentPoint.connectedPointsAbove.Add(exactAbovePoint.attachedTransform);
                            ConnectPoints(prevRowCurrentPoint.attachedTransform.localPosition,
                                exactAbovePoint.attachedTransform.localPosition);
                            continue;
                        }

                        var randomOtherConnect = Random.Range(0, availableConnects.Count);
                        var randomPoint = availableConnects[randomOtherConnect];
                        availableConnects.Remove(randomPoint);
                        prevRowCurrentPoint.connectedPointsAbove.Add(randomPoint.attachedTransform);
                        ConnectPoints(prevRowCurrentPoint.attachedTransform.localPosition,
                            randomPoint.attachedTransform.localPosition);

                    }

                    

                }
                

                yield return null;

                
                #endregion
                
                
                currentIterator++;

            }
        }


        private Transform InstantiatePointAt(Vector3 _instPosition)
        {
            var go = Instantiate(poiPrefab, mapGenParent);
            go.transform.localPosition = _instPosition;
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