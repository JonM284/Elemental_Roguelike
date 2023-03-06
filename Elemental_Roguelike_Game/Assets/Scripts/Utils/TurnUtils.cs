using Runtime.Character;
using Runtime.GameControllers;

namespace Utils
{
    public static class TurnUtils
    {

        #region Private Fields

        private static TurnController m_turnController;

        #endregion

        #region Accessor

        private static TurnController turnController => GameControllerUtils.GetGameController(ref m_turnController);

        #endregion

        #region Class Implementation


        public static CharacterBase GetActiveCharacter()
        {
            return turnController.activeCharacter;
        }

        public static bool isInBattle()
        {
            return turnController.isInBattle;
        }

        public static void InitializeBattle()
        {
            turnController.StartBattle();
        }

        #endregion
    }
}