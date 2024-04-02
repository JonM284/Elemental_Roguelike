using Data.Elements;
using Runtime.GameControllers;

namespace Utils
{
    public static class ElementUtils
    {

        #region Private Fields

        private static ElementController _elementController;

        #endregion

        #region Accessors

        private static ElementController elementController => GameControllerUtils.GetGameController(ref _elementController);

        #endregion

        #region Class Implementation

        public static ElementTyping GetElementTypeByGUID(string guid)
        {
            return elementController.GetElementByGUID(guid);
        }

        public static ElementTyping GetRandomElement()
        {
            return elementController.GetRandomElementTyping();
        }

        public static ElementTyping GetDefault()
        {
            return elementController.GetDefaultElement();
        }

        #endregion


    }
}