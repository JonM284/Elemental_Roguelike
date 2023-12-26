using System.Collections.Generic;
using System.Linq;
using Project.Scripts.Utils;
using Unity.VisualScripting;
using UnityEngine;

namespace Runtime.Grid
{
    public class HexPathfindingXZ
    {

        private const int MOVE_COST = 10;

        public static HexPathfindingXZ Instance { get; private set;}

        private GridXZ<PathNodeXZ> grid;

        private List<PathNodeXZ> openList;
        
        private List<PathNodeXZ> closedList;

        public HexPathfindingXZ(int width, int height, float cellSize, Vector3 gridStartPos)
        {
            Instance = this;
            grid = new GridXZ<PathNodeXZ>(width, height, cellSize, gridStartPos,
                (GridXZ<PathNodeXZ> g, int x, int z) => new PathNodeXZ(g, x, z));
        }

        public GridXZ<PathNodeXZ> GetGrid()
        {
            return grid;
        }

        public void CheckWalkable(LayerMask _obstacleLayer)
        {
            for (int x = 0; x < grid.GetWidth(); x++)
            {
                for (int z = 0; z < grid.GetHeight(); z++)
                {
                    var worldPos = grid.GetWorldPosition(x,z);
                    
                    var hitPoints = Physics.OverlapSphere(worldPos, 0.5f, _obstacleLayer);
                    if (hitPoints.Length > 0)
                    {
                        Debug.Log($"Found NOT WALKABLE: / {x}:{z}");
                        grid.GetGridObject(x, z).SetIsWalkable(false);
                    }
                }
            }
        }

        public bool IsWalkable(int x, int z)
        {
            var gridObj = grid.GetGridObject(x, z);
            
            if (gridObj.IsNull())
            {
                return false;
            }
            
            return gridObj.isWalkable;
        }

        public bool IsValidPosition(int x, int z)
        {
            return !grid.GetGridObject(x, z).IsNull();
        }

        public bool HasPath(int startX, int startZ, int endX, int endZ)
        {
            List<PathNodeXZ> path = FindPath(startX, startZ, endX, endZ);
            if (!path.IsNull())
            {
                return true;
            }

            return false;
        }

        public List<Vector3> FindPath(Vector3 startWorldPos, Vector3 endWorldPos)
        {
            grid.GetXZ(startWorldPos, out int startX, out int startZ);
            grid.GetXZ(endWorldPos, out int endX, out int endZ);

            List<PathNodeXZ> path = FindPath(startX, startZ, endX, endZ);
            if (path.IsNull())
            {
                return null;
            }

            List<Vector3> vectorPath = new List<Vector3>();

            foreach (PathNodeXZ pathNode in path)
            {
                vectorPath.Add(grid.GetWorldPosition(pathNode.x, pathNode.z));
            }

            return vectorPath;
        }

        public List<PathNodeXZ> FindPath(int startX, int startZ, int endX, int endZ)
        {
            PathNodeXZ startNode = grid.GetGridObject(startX, startZ);
            PathNodeXZ endNode = grid.GetGridObject(endX, endZ);

            if (startNode == null || endNode == null)
            {
                return null;
            }

            openList = new List<PathNodeXZ> { startNode };
            closedList = new List<PathNodeXZ>();

            for (int x = 0; x < grid.GetWidth(); x++)
            {
                for (int z = 0; z < grid.GetHeight(); z++)
                {
                    PathNodeXZ pathNode = grid.GetGridObject(x,z);
                    pathNode.gCost = 99999999;
                    pathNode.CalculateFCost();
                    pathNode.cameFromNode = null;
                }
            }

            startNode.gCost = 0;
            startNode.hCost = CalculateDistanceCost(startNode, endNode);
            startNode.CalculateFCost();

            while (openList.Count > 0)
            {
                PathNodeXZ currentNode = GetLowestFCostNode(openList);
                if (currentNode == endNode)
                {
                    return CalculatePath(endNode);
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                foreach (PathNodeXZ neighbourNode in GetNeighbourList(currentNode))
                {
                    if (closedList.Contains(neighbourNode))
                    {
                        continue;   
                    }

                    if (!neighbourNode.isWalkable)
                    {
                        closedList.Add(neighbourNode);
                    }

                    int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);

                    if (tentativeGCost < neighbourNode.gCost)
                    {
                        neighbourNode.cameFromNode = currentNode;
                        neighbourNode.gCost = tentativeGCost;
                        neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                        neighbourNode.CalculateFCost();

                        if (!openList.Contains(neighbourNode))
                        {
                            openList.Add(neighbourNode);
                        }
                    }
                }
            }

            return null;
        }

        private List<PathNodeXZ> GetSquareNeighbourList(PathNodeXZ currentNode)
        {
            List<PathNodeXZ> neighbourList = new List<PathNodeXZ>();

            if (currentNode.x - 1 >= 0)
            {
                neighbourList.Add(GetNode(currentNode.x - 1, currentNode.z));

                if (currentNode.z - 1 >= 0)
                {
                    neighbourList.Add(GetNode(currentNode.x - 1, currentNode.z - 1));
                }

                if (currentNode.z + 1 < grid.GetHeight())
                {
                    neighbourList.Add(GetNode(currentNode.x - 1, currentNode.z + 1));
                }
            }

            if (currentNode.x + 1 < grid.GetWidth())
            {
                neighbourList.Add(GetNode(currentNode.x + 1, currentNode.z));

                if (currentNode.z - 1 >= 0)
                {
                    neighbourList.Add(GetNode(currentNode.x + 1, currentNode.z - 1));
                }

                if (currentNode.z + 1 < grid.GetHeight())
                {
                    neighbourList.Add(GetNode(currentNode.x + 1, currentNode.z + 1));
                }
            }

            if (currentNode.z - 1 >= 0)
            {
                neighbourList.Add(GetNode(currentNode.x, currentNode.z - 1));
            }

            if (currentNode.z + 1 < grid.GetHeight())
            {
                neighbourList.Add(GetNode(currentNode.x, currentNode.z + 1));
            }

            return neighbourList;
        }

        private List<PathNodeXZ> GetNeighbourList(PathNodeXZ currentNode)
        {
            List<PathNodeXZ> neighbourList = new List<PathNodeXZ>();

            bool oddRow = currentNode.z % 2 == 1;
            
            //LEFT
            if (currentNode.x - 1 >= 0)
            {
                neighbourList.Add(GetNode(currentNode.x - 1, currentNode.z));
            }

            //RIGHT
            if (currentNode.x + 1 < grid.GetWidth())
            {
                neighbourList.Add(GetNode(currentNode.x + 1, currentNode.z));
            }

            //DOWN
            if (currentNode.z - 1 >= 0)
            {
                neighbourList.Add(GetNode(currentNode.x, currentNode.z - 1));
            }
            
            //UP
            if (currentNode.z + 1 < grid.GetHeight())
            {
                neighbourList.Add(GetNode(currentNode.x, currentNode.z + 1));
            }

            if (oddRow)
            {
                if (currentNode.z + 1 < grid.GetHeight() && currentNode.x + 1 < grid.GetWidth())
                {
                    neighbourList.Add(GetNode(currentNode.x + 1, currentNode.z + 1));
                }

                if (currentNode.z - 1 >= 0 && currentNode.x + 1 < grid.GetWidth())
                {
                    neighbourList.Add(GetNode(currentNode.x + 1, currentNode.z - 1));
                }
            }
            else
            {
                if (currentNode.z + 1 < grid.GetHeight() && currentNode.x - 1 >= 0)
                {
                    neighbourList.Add(GetNode(currentNode.x - 1, currentNode.z + 1));
                }

                if (currentNode.z - 1 >= 0 && currentNode.x - 1 >= 0)
                {
                    neighbourList.Add(GetNode(currentNode.x - 1, currentNode.z - 1));
                }
            }

            return neighbourList;
        }

        public PathNodeXZ GetNode(int x, int z)
        {
            return grid.GetGridObject(x, z);
        }

        private List<PathNodeXZ> CalculatePath(PathNodeXZ endNode)
        {
            List<PathNodeXZ> path = new List<PathNodeXZ>();
            
            path.Add(endNode);

            PathNodeXZ currentNode = endNode;
            while (currentNode.cameFromNode != null)
            {
                path.Add(currentNode.cameFromNode);
                currentNode = currentNode.cameFromNode;
            }

            path.Reverse();
            return path;
        }

        private int CalculateDistanceCost(PathNodeXZ a, PathNodeXZ b)
        {
            return Mathf.RoundToInt(MOVE_COST * Vector3.Distance(grid.GetWorldPosition(a.x, a.z), grid.GetWorldPosition(b.x, b.z)));
        }

        private PathNodeXZ GetLowestFCostNode(List<PathNodeXZ> _pathNodeList)
        {
            PathNodeXZ lowestFCostNode = _pathNodeList[0];
            for (int i = 0; i < _pathNodeList.Count; i++)
            {
                if (_pathNodeList[i].fCost < lowestFCostNode.fCost)
                {
                    lowestFCostNode = _pathNodeList[i];
                }
            }

            return lowestFCostNode;
        }
        
    }
}