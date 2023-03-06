using Project.Scripts.Runtime.LevelGeneration;
using Runtime.GameControllers;

namespace Utils
{
    public static class LevelUtils
    {

        #region Private Fields

        private static LevelController _levelController;

        #endregion

        #region Accessors

        private static LevelController levelController => GameControllerUtils.GetGameController(ref _levelController);

        #endregion

        #region Class Implementation


        public static void ChangeRooms(RoomTracker _roomTracker)
        {
            if (_roomTracker == null)
            {
                return;
            }
            
            levelController.ChangeRoom(_roomTracker);
        }

        public static RoomTracker GetCurrentRoom()
        {
            return levelController.GetCurrentRoom();
        }

        
        #endregion


    }
}