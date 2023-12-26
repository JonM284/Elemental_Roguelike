using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Grid
{
    public class GridXZ<TGridObject>
    {

        #region Private Fields

        private const float GRID_CELL_OFFSET_MULTIPLIER = 0.75f;
        
        private int width;

        private int height;

        private float cellSize;

        private Vector3 originPosition;

        private TGridObject[,] gridArray;
        
        #endregion

        #region Constructor

        public GridXZ(int width, int height, float cellSize, Vector3 originPosition, Func<GridXZ<TGridObject>, int, int, TGridObject> createGridObject)
        {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;
            this.originPosition = originPosition;

            gridArray = new TGridObject[width, height];

            for (int x = 0; x < gridArray.GetLength(0); x++)
            {
                for (int z = 0; z < gridArray.GetLength(1); z++)
                {
                    gridArray[x, z] = createGridObject(this, x, z);
                }
            }
        }
        
        #endregion

        #region Class Implementation

        public int GetWidth()
        {
            return width;
        }

        public int GetHeight()
        {
            return height;
        }

        public float GetCellSize()
        {
            return cellSize;
        }

        public Vector3 GetWorldPosition(int x, int z)
        {
            return new Vector3(x, 0, 0) * cellSize + 
                   new Vector3(0,0, z) * (cellSize * GRID_CELL_OFFSET_MULTIPLIER) +
                   ((z % 2) == 1 ? new Vector3(1,0,0) * (cellSize * 0.5f) : Vector3.zero) +
                   originPosition;
        }

        public void GetXZ(Vector3 worldPosition, out int x, out int z)
        {
            int roughX = Mathf.RoundToInt((worldPosition - originPosition).x / cellSize);
            int roughZ = Mathf.RoundToInt((worldPosition - originPosition).z / cellSize / GRID_CELL_OFFSET_MULTIPLIER);

            Vector3Int roughXZ = new Vector3Int(roughX, 0, roughZ);

            bool oddRow = roughZ % 2 == 1;
            List<Vector3Int> neighbourXZList = new List<Vector3Int>
            {
                roughXZ + new Vector3Int(-1,0,0),
                roughXZ + new Vector3Int(+1,0,0),
                
                roughXZ + new Vector3Int(oddRow ? +1 : -1,0,+1),
                roughXZ + new Vector3Int(+0,0,+1),
                
                roughXZ + new Vector3Int(oddRow ? +1 : -1,0,-1),
                roughXZ + new Vector3Int(+0,0,-1),
            };

            Vector3Int closestXZ = roughXZ;
            foreach (var neighbourXZ in neighbourXZList)
            {
                if (Vector3.Distance(worldPosition, GetWorldPosition(neighbourXZ.x, neighbourXZ.z)) <
                    Vector3.Distance(worldPosition, GetWorldPosition(closestXZ.x, closestXZ.z)))
                {
                    closestXZ = neighbourXZ;
                }
            }
            
            
            x = closestXZ.x;
            z = closestXZ.z;
        }

        public TGridObject GetGridObject(int x, int z)
        {
            if (x >= 0 && z >= 0 && x < width && z < height)
            {
                return gridArray[x, z];
            }
            else
            {
                return default(TGridObject);
            }
        }

        public TGridObject GetGridObject(Vector3 worldPosition)
        {
            int x, z;
            GetXZ(worldPosition, out x, out z);
            return GetGridObject(x, z);
        }

        public void SetGridObject(int x, int z, TGridObject value)
        {
            if (x >= 0 && z >= 0 && x < width && z < height)
            {
                gridArray[x, z] = value;
            }
        }

        public void SetGridObject(Vector3 worldPosition, TGridObject value)
        {
            GetXZ(worldPosition, out int x, out int z);
            SetGridObject(x, z, value);
        }

        #endregion



    }
}