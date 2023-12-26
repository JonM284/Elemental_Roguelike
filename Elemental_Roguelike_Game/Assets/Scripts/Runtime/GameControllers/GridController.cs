using System;
using System.Collections.Generic;
using Data;
using Project.Scripts.Utils;
using Runtime.Character;
using Runtime.Grid;
using UnityEngine;
using Utils;

namespace Runtime.GameControllers
{
    public class GridController: GameControllerBase
    {
        
        
        #region Static

        public static GridController Instance { get; private set; }

        #endregion
        
        #region Serialized Fields

        [SerializeField] private Camera camera;

        [SerializeField] private GameObject gridPrefab;

        [SerializeField] private int gridWidth = 30;

        [SerializeField] private int gridHeight = 19;

        [SerializeField] private float cellSize = 1f;

        [SerializeField] private LayerMask obstacleLayer;

        [SerializeField] private LayerMask selectableLayers;
        
        #endregion
        
        #region Private Fields
        
        private GridXZ<HexGrid.GridObject> gridXZ;

        private HexGrid.GridObject m_lastGridObj;

        private HexPathfindingXZ pathfindingXZ;

        private Transform m_gridObjPool;

        private Vector3 m_currentPathStartPos;

        private bool m_isInMatch;
 
        #endregion

        #region Accessors

        private Transform gridObjPool => CommonUtils.GetRequiredComponent(ref m_gridObjPool, () =>
        {
            var p = TransformUtils.CreatePool(this.transform, true);
            p.RenameTransform("Grid_Pool");
            return p;
        });

        #endregion
        
        #region Unity Events

        private void OnEnable()
        {
            TurnController.OnChangeActiveCharacter += OnChangeActiveCharacter;
            SceneController.OnLevelPrefinishedLoading += OnLevelPreFinishedLoading;
        }

        private void OnDisable()
        {
            TurnController.OnChangeActiveCharacter -= OnChangeActiveCharacter;
            SceneController.OnLevelPrefinishedLoading -= OnLevelPreFinishedLoading;
        }

        private void Update()
        {
            if (!m_isInMatch)
            {
                return;
            }

            if (camera.IsNull())
            {
                return;
            }
            
            if (!m_lastGridObj.IsNull())
            {
                m_lastGridObj.HideSelected();
            }

            m_lastGridObj = gridXZ.GetGridObject(GetWorldPos());

            if (!m_lastGridObj.IsNull())
            {
                m_lastGridObj.ShowSelected();
            }

            if (Input.GetMouseButtonDown(0))
            {
                var worldPos = GetWorldPos();
                List<Vector3> pathList = pathfindingXZ.FindPath(m_currentPathStartPos, worldPos);

                if (!pathList.IsNull() && pathList.Count > 0)
                {
                    for (int i = 0; i < pathList.Count - 1; i++)
                    {
                        Debug.DrawLine(pathList[i], pathList[i + 1], Color.green, 5f);
                    }
                }
            }
        }

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

        private void OnChangeActiveCharacter(CharacterBase _character)
        {
            m_currentPathStartPos = _character.transform.position;
            gridXZ.GetXZ(m_currentPathStartPos, out int startX, out int startZ);
            HighlightMovementArea(startX, startZ, 3);
        }
        
        private void OnLevelPreFinishedLoading(SceneName _sceneName, bool _isMatchScene)
        {
            m_isInMatch = _isMatchScene;
            if (!_isMatchScene)
            {
                return;
            }

            camera = CameraUtils.GetMainCamera();
            InitializeGrid();
        }

        #endregion

        #region GridGeneration

        public void InitializeGrid()
        {
            gridXZ = new GridXZ<HexGrid.GridObject>(gridWidth, gridHeight, cellSize, (Vector3.zero - new Vector3(gridWidth/2f, 0, gridHeight/2f)) + (Vector3.up * 0.05f),
                (GridXZ<HexGrid.GridObject> g, int x, int y) => new HexGrid.GridObject());

            for (int x = 0; x < gridWidth; x++)
            {
                for (int z = 0; z < gridHeight; z++)
                {
                    var visualTransform = Instantiate(gridPrefab, gridXZ.GetWorldPosition(x, z), Quaternion.identity, gridObjPool);
                    gridXZ.GetGridObject(x, z).visualTransform = visualTransform.transform;
                    gridXZ.GetGridObject(x,z).HideSelected();
                }
            }

            pathfindingXZ = new HexPathfindingXZ(gridWidth, gridHeight, cellSize, transform.position);
            pathfindingXZ.CheckWalkable(obstacleLayer);
            
            //Turn Off Non-Walkable
            for (int x = 0; x < gridWidth; x++)
            {
                for (int z = 0; z < gridHeight; z++)
                {
                    
                    if (!pathfindingXZ.GetNode(x, z).isWalkable)
                    {
                        gridXZ.GetGridObject(x, z).ShutOff();
                    }
                    
                }
            }
        }
        
        private void HighlightMovementArea(int centerX, int centerZ, int maxDistance)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int z = 0; z < gridHeight; z++)
                {
                    gridXZ.GetGridObject(x,z).ClearAllMarkers();
                }
            }
            
            
            for (int x = centerX - maxDistance; x <= centerX + maxDistance; x++)
            {
                for (int z = centerZ - maxDistance; z <= centerZ + maxDistance; z++)
                {
                    if (!pathfindingXZ.IsValidPosition(x,z))
                    {
                        continue;
                    }
                    
                    if (pathfindingXZ.IsWalkable(x,z))
                    {
                        if (pathfindingXZ.HasPath(centerX, centerZ, x,z))
                        {
                            if (pathfindingXZ.FindPath(centerX, centerZ, x, z).Count <= maxDistance)
                            {
                                gridXZ.GetGridObject(x,z).MarkMovement(true);
                            }
                            else
                            {
                                
                            }
                        }
                        else
                        {
                            
                        }
                    }
                    else
                    {
                        
                    }
                }
            }
        }
        
        private Vector3 GetWorldPos()
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out RaycastHit hit, 999f, selectableLayers))
            {
                return hit.point;
            }
            else
            {
                return Vector3.zero;
            }
        }

        public PathNodeXZ GetPathNode(Vector3 _worldPos)
        {
            gridXZ.GetXZ(_worldPos, out int x, out int z);
            return pathfindingXZ.GetNode(x,z);
        }

        public List<Vector3> GetPath(Vector3 startPos, Vector3 endPos)
        {
            List<Vector3> pathList = pathfindingXZ.FindPath(startPos, endPos);
            return pathList;
        }

        #endregion
        
    }
}