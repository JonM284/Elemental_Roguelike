using Data;
using Runtime.GameControllers;
using UnityEngine;
using Utils;

namespace Runtime.Managers
{
    public class TeamSelectionManager: MonoBehaviour
    {

        #region Serialized Fields

        [SerializeField] private UIWindowData selectionUIData;

        #endregion

        #region Class Implementation

        public void OpenTeamWindow()
        {
            UIUtils.OpenUI(selectionUIData);
        }

        #endregion


    }
}