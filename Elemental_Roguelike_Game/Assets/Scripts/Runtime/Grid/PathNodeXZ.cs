namespace Runtime.Grid
{
    public class PathNodeXZ
    {
        private GridXZ<PathNodeXZ> grid;
        public int x;
        public int z;

        public int gCost;
        public int hCost;
        public int fCost;

        public bool isWalkable;
        public PathNodeXZ cameFromNode;

        public PathNodeXZ(GridXZ<PathNodeXZ> grid, int x, int z)
        {
            this.grid = grid;
            this.x = x;
            this.z = z;
            isWalkable = true;
        }

        public void CalculateFCost()
        {
            fCost = gCost + hCost;
        }

        public void SetIsWalkable(bool isWalkable)
        {
            this.isWalkable = isWalkable;
        }

        public override string ToString()
        {
            return x + "," + z;
        }
    }
}