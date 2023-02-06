using Project.Scripts.Utils;

namespace Utils
{
    public static class CameraUtils
    {

        #region Private Fields

        private static UnityEngine.Camera m_mainCamera;

        #endregion

        #region Accessors

        private static UnityEngine.Camera mainCamera => CommonUtils.GetRequiredComponent(ref m_mainCamera, () =>
        {
            var c = UnityEngine.Camera.main;
            return c;
        });

        #endregion

        #region Class Implementation

        public static UnityEngine.Camera GetMainCamera()
        {
            return mainCamera;
        }

        #endregion
        
        
    }
}