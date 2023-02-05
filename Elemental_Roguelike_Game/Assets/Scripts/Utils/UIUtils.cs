using Runtime.GameControllers;
using Runtime.UI;

namespace Utils
{
    public static class UIUtils
    {
        
        #region Private Fields

        private static UIController _uiController;

        #endregion

        #region Accessors

        public static UIController uiController => GameControllerUtils.GetGameController(ref _uiController);

        #endregion

        #region Class Implementation

        public static void CloseUI(UIBase _uiWindow)
        {
            if (_uiWindow == null)
            {
                return;
            }
            
            uiController.ReturnUIToCachedPool(_uiWindow);
            
        }

        #endregion
        
    }
}