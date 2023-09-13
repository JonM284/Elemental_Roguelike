using System.Collections.Generic;
using Data.CharacterData;
using Project.Scripts.Utils;
using Runtime.UI.Items;
using UnityEngine;
using Utils;

namespace Runtime.UI.DataModels
{
    public class MeepleManagerUIDataModel: MonoBehaviour
    {

        #region Serialized Fields

        [SerializeField] private List<MeepleManagerSelectionItem> meepleSelectorItems = new List<MeepleManagerSelectionItem>();

        [SerializeField] private UIButtonItem meepleAdditionItem;

        #endregion

        #region Private Fields

        private List<CharacterStatsData> allOwnedMeeples = new List<CharacterStatsData>();

        private CharacterStatsData m_activeShownMeeple;

        #endregion

        #region Accessors

        public CharacterStatsData activeMeeple => m_activeShownMeeple;

        #endregion

        #region Unity Events

        private void Start()
        {
            InitializeUI();
        }

        #endregion

        #region Class Implementation

        private void InitializeUI()
        {
            UpdateScreen();
            
            meepleAdditionItem.InitializeItem(OnAdditionButtonPressed);
        }

        private void UpdateScreen()
        {
            
        }

        private void OnSelectorButtonPressed(CharacterStatsData meeple)
        {
            if (meeple.IsNull())
            {
                Debug.LogError("Meeple Invalid");
                return;
            }
            
            m_activeShownMeeple = meeple;

        }

        private void OnAdditionButtonPressed()
        {
            CharacterUtils.CreateNewMeeple();
            
            UpdateScreen();
        }

        #endregion

    }
}