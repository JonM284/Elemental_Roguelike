using UnityEngine;

namespace Project.Scripts.Utils
{
    public static class VectorUtils
    {

        #region Class Implementation

        
        public static Vector3 FlattenVector3X(this Vector3 vector, float newX = 0f)
        {
            vector.x = newX;
            return vector;
        }
        
        public static Vector3 FlattenVector3Y(this Vector3 vector, float newY = 0f)
        {
            vector.y = newY;
            return vector;
        }

        public static Vector3 FlattenVector3Z(this Vector3 vector, float newZ = 0f)
        {
            vector.z = newZ;
            return vector;
        }

        public static bool IsNan(this Vector3 vector3)
        {
            return float.IsNaN(vector3.x) || float.IsNaN(vector3.y) || float.IsNaN(vector3.z);
        }

        public static Vector3 NormalizeTo(this Vector3 vector3, float multipliedValue)
        {
            return vector3.normalized * multipliedValue;
        }


        public static bool IsFastApproximate(Vector3 position1, Vector3 position2, float threshold = 1f)
        {
            return (position1 - position2).magnitude <= threshold;
        }

        #endregion
        
    }
}