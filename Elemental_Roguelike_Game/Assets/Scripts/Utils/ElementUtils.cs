using System.Collections.Generic;
using Data.DataSaving;
using Data.Elements;
using Runtime.GameControllers;
using UnityEngine;

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

        public static ElementTyping GetElementTypeByName(string _name)
        {
            return elementController.GetElementByName(_name);
        }

        public static ElementTyping GetRandomElement()
        {
            return elementController.GetRandomElementTyping();
        }

        #endregion


    }
}