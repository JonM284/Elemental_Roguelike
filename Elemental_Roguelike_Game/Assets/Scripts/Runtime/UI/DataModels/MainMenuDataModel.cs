using Data;
using Project.Scripts.Utils;
using UnityEditor;
using UnityEngine;
using Utils;

namespace Runtime.UI.DataModels
{
    public class MainMenuDataModel: MonoBehaviour
    {

        #region Serialized Fields

        [SerializeField] private UIWindowData tournamentUIWindowData;

        [SerializeField] private UIWindowData settingsUIWindowData;

        [SerializeField] private SceneName roguelikeSceneName;

        #endregion

        #region Class Implementation

        public void TournamentPressed()
        {
            if (tournamentUIWindowData.IsNull())
            {
                return;
            }
            
            UIUtils.OpenUI(tournamentUIWindowData);
        }

        public void VersusPressed()
        {
            //Nothing yet
        }

        public void SettingsPressed()
        {
            if (settingsUIWindowData.IsNull())
            {
                return;
            }
            
            UIUtils.OpenUI(settingsUIWindowData);
        }

        public void ExitPressed()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion


    }
}