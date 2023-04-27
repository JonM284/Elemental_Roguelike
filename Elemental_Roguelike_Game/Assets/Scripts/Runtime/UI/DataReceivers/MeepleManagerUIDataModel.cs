using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Runtime.UI.Items;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Runtime.UI.DataReceivers
{
    public class MeepleManagerUIDataModel: MonoBehaviour
    {

        #region Serialized Fields

        [SerializeField] private List<MeepleManagerSelectionItem> meepleSelectorItems = new List<MeepleManagerSelectionItem>();

        [SerializeField] private UIButtonItem meepleAdditionItem;

        #endregion

        #region Private Fields

        private List<string> allOwnedMeepleGUIDs = new List<string>();

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
            allOwnedMeepleGUIDs = CharacterUtils.GetAllMeeples().Keys.ToList();
            
            meepleSelectorItems.ForEach(g => g.gameObject.SetActive(false));

            for (int i = 0; i < allOwnedMeepleGUIDs.Count; i++)
            {
                meepleSelectorItems[i].gameObject.SetActive(true);
                meepleSelectorItems[i].InitializeSelectionItem(allOwnedMeepleGUIDs[i], OnSelectorButtonPressed);
            }

            meepleAdditionItem.gameObject.SetActive(allOwnedMeepleGUIDs.Count < 10);
            
        }

        private void OnSelectorButtonPressed(string meepleID)
        {
            if (meepleID == String.Empty)
            {
                Debug.LogError("Meeple ID Invalid");
                return;
            }

            CharacterStatsData meepleStats = meepleID.GetMeepleFromGUID();

            if (meepleStats == null)
            {
                Debug.LogError("Meeple Invalid");
                return;
            }

            m_activeShownMeeple = meepleStats;

        }

        private void OnAdditionButtonPressed()
        {
            CharacterUtils.CreateNewMeeple();
            
            UpdateScreen();
        }

        #endregion

    }
}