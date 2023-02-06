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

        public static TurnController turnController => GameControllerUtils.GetGameController(ref m_turnController);

        #endregion

        #region Class Implementation

        public static void SetActiveCharacter(CharacterBase _activeCharacter)
        {
            if (_activeCharacter == null)
            {
                return;
            }
            
            turnController.SetActiveCharacter(_activeCharacter);
        }

        public static CharacterBase GetActiveCharacter()
        {
            return turnController.activeCharacter;
        }

        #endregion
    }
}