using Vector3 = UnityEngine.Vector3;

namespace Runtime.Gameplay.Ball
{
    public struct BallTrajectoryPoint
    {
        public Vector3 point;
        public bool isRechargeBall;

        public BallTrajectoryPoint( Vector3 _point, bool _isRechangeBall)
        {
            point = _point;
            isRechargeBall = _isRechangeBall;
        }
    }
}