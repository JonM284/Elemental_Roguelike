using System.Collections.Generic;
using Data;
using Data.CharacterData;
using Project.Scripts.Utils;
using Runtime.Character;
using Runtime.GameControllers;

namespace Utils
{
    public static class TeamUtils
    {

        #region Private Fields

        private static TeamController m_teamController;

        #endregion

        #region Accessor

        private static TeamController teamController => GameControllerUtils.GetGameController(ref m_teamController);

        #endregion


        #region Class Implementation
        

        #endregion

    }
}