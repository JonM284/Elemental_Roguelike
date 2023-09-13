using Runtime.Submodules;
using UnityEngine;

namespace Runtime.GameplayEvents
{
    public class MemberEventManager: MonoBehaviour
    {

        #region Serialized Fields

        [SerializeField] private RandomTeamSelectionManager randomTeamSelectionManager;

        #endregion


        #region Class Implementation

        public void DisplayTeamSelectionManager()
        {
            StartCoroutine(randomTeamSelectionManager.ReopenTeamMenu());
        }

        #endregion
        
        
    }
}