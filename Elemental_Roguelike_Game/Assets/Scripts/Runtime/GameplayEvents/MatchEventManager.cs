using Runtime.GameControllers;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.GameplayEvents
{
    public class MatchEventManager: MonoBehaviour
    {

        public void LoadMatch(GameplayEventType _gameplayEvent)
        {
            if (_gameplayEvent is MatchEventType matchEvent)
            {
                //ToDo: change turn controller's enemy ai team to be a random team from this.
                //ToDo: remember that it might be a meeple team
                SceneController.Instance.LoadScene(matchEvent.sceneName, true);
            }
        }


    }
}