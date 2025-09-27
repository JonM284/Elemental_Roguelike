using UnityEngine;

namespace Project.Scripts.Utils
{
    public static class VectorUtils
    {

        #region Class Implementation

        public static Vector3 FlattenVector3Y(this Vector3 vector)
        {
            vector.y = 0;
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

        #endregion
        
    }
}