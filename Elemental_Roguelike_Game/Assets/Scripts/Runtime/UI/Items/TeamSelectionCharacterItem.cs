using System;
using System.Collections.Generic;
using Data;
using Data.CharacterData;
using Project.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.UI.Items
{
    public class TeamSelectionCharacterItem: MonoBehaviour
    {

         #region Actions

        private Action<SavedMemberData, TeamSelectionCharacterItem> ItemPressedCallback;
        
        private Action<SavedMemberData, bool> ItemHighlightedCallback;
        
        #endregion

        #region Serialized Fields

        [SerializeField] private Image characterImage;
        
        [SerializeField] private bool m_isDisplayItem = true;
        
        [Header("Stats Display")]

        [SerializeField] private GameObject characterStatsHighlight;
        
        [SerializeField] private GameObject characterSelectedHighlight;

        #endregion

        #region Accessors
        
        public SavedMemberData m_assignedData { get; private set; }

        #endregion

        #region Class Implementation

        public void InitializeCharacterItem(SavedMemberData _memberData, Action<SavedMemberData, TeamSelectionCharacterItem> pressedCallback, Action<SavedMemberData, bool> highlightCallback = null)
        {
            if (_memberData.IsNull())
            {
                return;
            }
            
            m_assignedData = _memberData;

            AssignCharacterImage();

            AssignVisuals();

            if (!pressedCallback.IsNull())
            {
                ItemPressedCallback = pressedCallback;
            }

            if (!highlightCallback.IsNull())
            {
                ItemHighlightedCallback = highlightCallback;
            }
        }

        private void AssignCharacterImage()
        {
            if (characterImage.IsNull())
            {
                return;
            }

            if (m_assignedData.m_characterStatsBase.characterImage.IsNull())
            {
                return;
            }

            characterImage.sprite = m_assignedData.m_characterStatsBase.characterImage;
        }

        private void AssignVisuals()
        {
            if (!m_isDisplayItem)
            {
                return;
            }

            
        }

        public void OnPress()
        {
            ItemPressedCallback?.Invoke(m_assignedData, this);
        }

        public void OnUpdateSelectedIcon(bool _isSelected)
        {
            characterSelectedHighlight.SetActive(_isSelected);
        }

        public void MarkHighlight(bool _active)
        {
            ItemHighlightedCallback?.Invoke(m_assignedData, _active);
            characterStatsHighlight.SetActive(_active);
        }

        public void CleanUpItem()
        {
            MarkHighlight(false);
            ItemPressedCallback = null;
            ItemHighlightedCallback = null;
        }
        
        #endregion

    }
}