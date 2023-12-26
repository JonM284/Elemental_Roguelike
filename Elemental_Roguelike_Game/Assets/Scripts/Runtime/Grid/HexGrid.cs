using System;
using System.Collections.Generic;
using Project.Scripts.Utils;
using UnityEngine;

namespace Runtime.Grid
{
    public class HexGrid: MonoBehaviour
    {

        #region Nested Classes

        public class GridObject
        {
            public Transform visualTransform;

            private GameObject m_selectedGO;

            private GameObject m_movementGO;

            private GameObject m_abilityUseGO;

            private GameObject m_normalGO;

            public GameObject selectedGO => CommonUtils.GetRequiredComponent(ref m_selectedGO, () =>
            {
                var go = visualTransform.Find("Selected");
                return go.gameObject;
            });
            
            public GameObject movementGO => CommonUtils.GetRequiredComponent(ref m_movementGO, () =>
            {
                var go = visualTransform.Find("Movement");
                return go.gameObject;
            });
            
            public GameObject abilityUseGO => CommonUtils.GetRequiredComponent(ref m_abilityUseGO, () =>
            {
                var go = visualTransform.Find("Ability");
                return go.gameObject;
            });
            
            public GameObject normalGO => CommonUtils.GetRequiredComponent(ref m_normalGO, () =>
            {
                var go = visualTransform.Find("Normal");
                return go.gameObject;
            });
            
            public void ShowSelected()
            {
                selectedGO.SetActive(true);
            }

            public void HideSelected()
            {
                selectedGO.SetActive(false);
            }

            public void MarkMovement(bool _isWalkable)
            {
                movementGO.SetActive(_isWalkable);
            }

            public void MarkAbility(bool _isUseablePoint)
            {
                abilityUseGO.SetActive(_isUseablePoint);
            }

            public void ClearAllMarkers()
            {
                MarkMovement(false);
                MarkAbility(false);
            }

            public void ShutOff()
            {
                MarkMovement(false);
                MarkAbility(false);
                normalGO.SetActive(false);
            }

        }

        #endregion
        
        #region Serialized Fields

        [SerializeField] private Camera camera;

        [SerializeField] private GameObject gridPrefab;

        [SerializeField] private int gridWidth = 10;

        [SerializeField] private int gridHeight = 6;

        [SerializeField] private float cellSize = 1f;

        [SerializeField] private LayerMask obstacleLayer;

        [SerializeField] private LayerMask selectableLayers;
        
        #endregion

        #region Private Fields
        
        private GridXZ<GridObject> gridXZ;

        private GridObject m_lastGridObj;

        private HexPathfindingXZ pathfindingXZ;

        private Transform m_gridObjPool;

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

        private void Awake()
        {

            gridXZ = new GridXZ<GridObject>(gridWidth, gridHeight, cellSize, transform.position,
                (GridXZ<GridObject> g, int x, int y) => new GridObject());

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

        private void Update()
        {
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
                List<Vector3> pathList = pathfindingXZ.FindPath(Vector3.zero, worldPos);

                if (!pathList.IsNull() && pathList.Count > 0)
                {
                    for (int i = 0; i < pathList.Count - 1; i++)
                    {
                        Debug.DrawLine(pathList[i], pathList[i + 1], Color.green, 5f);
                    }

                    gridXZ.GetXZ(worldPos, out int startX, out int startZ);
                    HighlightMovementArea(startX, startZ, 3);
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

        #endregion

    }
}