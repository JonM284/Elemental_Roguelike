using System.Collections.Generic;
using Data;
using Project.Scripts.Utils;
using Runtime.GameControllers;
using UnityEngine;
using Utils;

namespace Runtime.Submodules
{
    public class RandomTeamSelectionManager : MonoBehaviour
    {

        #region Serialized Fields

        private List<Transform> selectedTeamLocations = new List<Transform>();

        private List<Transform> randomGeneratedMeepleLocations = new List<Transform>();
        
        #endregion

        #region Private Fields

        private MeepleController m_meepleController;

        private TeamController m_teamController;

        #endregion

        #region Accessors

        private MeepleController meepleController => GameControllerUtils.GetGameController(ref m_meepleController);
        
        private TeamController teamController => GameControllerUtils.GetGameController(ref m_teamController);

        #endregion

        #region Unity Events

        

        #endregion

        #region Class Implementation

        private void OnTeamUpdated()
        {
            
        }

        private void OnNewCharactersGenerated()
        {
            
        }

        #endregion


    }
}
