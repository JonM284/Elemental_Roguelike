using System.Collections;
using Runtime.GameControllers;
using UnityEngine;

namespace Runtime.Managers
{
    public class GameShowManager: MonoBehaviour
    {

        #region Serialized Fields

        [SerializeField] private TeamSelectionManager teamSelectionManager;

        [SerializeField] private MapController mapController;

        #endregion

        #region Unity Events

        private IEnumerator Start()
        {
            if (!MainController.Instance.allInitialized)
            {
                yield return new WaitUntil(() => MainController.Instance.allInitialized);
            }
            
            yield return new WaitForSeconds(0.5f);

            CheckOptions();

        }

        private void OnEnable()
        {
            MapDataController.RestartRealScene += CheckOptions;
        }

        private void OnDisable()
        {
            MapDataController.RestartRealScene -= CheckOptions;
        }

        #endregion
        
        
        #region Class Implementation

        private void CheckOptions()
        {
            if (TeamController.Instance.GetTeam().Count == 0)
            {
                if (mapController.mapIsShown)
                {
                    mapController.HideMap();
                }
                teamSelectionManager.OpenTeamWindow();
                return;
            }

            
            mapController.DisplayOneTime();

        }

        #endregion

    }
}