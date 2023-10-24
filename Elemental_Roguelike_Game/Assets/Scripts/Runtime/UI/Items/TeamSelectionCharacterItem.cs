using System;
using System.Collections.Generic;
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

        private Action<CharacterStatsBase> ItemPressedCallback;
        
        private Action<CharacterStatsBase, bool> ItemHighlightedCallback;
        
        #endregion

        #region Serialized Fields

        [SerializeField] private Image characterImage;

        [SerializeField] private TMP_Text title;

        [SerializeField] private bool m_isCaptain;

        [SerializeField] private bool m_isDisplayItem = true;
        
        [Header("Stats Display")]

        [SerializeField] private GameObject characterStatsHighlight;
        
        [SerializeField] private TMP_Text agilityStatText;
        
        [SerializeField] private TMP_Text shootingStatText;
        
        [SerializeField] private TMP_Text passingStatText;
        
        [SerializeField] private TMP_Text tackleStatText;
        
        [SerializeField] private TMP_Text classTypeText;

        [SerializeField] private Image agilityBar;
        
        [SerializeField] private Image shootingBar;
        
        [SerializeField] private Image passingBar;
        
        [SerializeField] private Image tackleBar;

        [Header("Abilities")] 
        [SerializeField] private List<GameObject> abilityDisplays;

        #endregion

        #region Accessors

        public CharacterStatsBase m_assignedStats { get; private set; }

        #endregion

        #region Class Implementation

        public void InitializeCharacterItem(CharacterStatsBase _characterStats, Action<CharacterStatsBase> pressedCallback, Action<CharacterStatsBase, bool> highlightCallback = null)
        {
            if (_characterStats.IsNull())
            {
                return;
            }
            
            m_assignedStats = _characterStats;

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

            if (m_assignedStats.characterImage.IsNull())
            {
                return;
            }

            characterImage.sprite = m_assignedStats.characterImage;
        }

        private void AssignVisuals()
        {
            if (!m_isDisplayItem)
            {
                return;
            }
            
            title.text = m_assignedStats.characterName;
            
            classTypeText.text = $"<u>Class Type:</u> {m_assignedStats.classTyping}";

            agilityStatText.text = $"<u>Agility</u>\r\n{m_assignedStats.agilityScore}";

            shootingStatText.text = $"<u>Shoot</u>\r\n{m_assignedStats.shootingScore}";
            
            passingStatText.text = $"<u>Pass</u>\r\n{m_assignedStats.passingScore}";
            
            tackleStatText.text = $"<u>Tackle</u>\r\n{m_assignedStats.tackleScore}";

            agilityBar.fillAmount = m_assignedStats.agilityScore / 100f;

            shootingBar.fillAmount = m_assignedStats.shootingScore / 100f;

            passingBar.fillAmount = m_assignedStats.passingScore / 100f;

            tackleBar.fillAmount = m_assignedStats.tackleScore / 100f;

            ClearAbilities();
            
            if (m_assignedStats.abilities.Count > 0)
            {
                SetupAbilityDisplays();
            }
        }

        public void OnPress()
        {
            ItemPressedCallback?.Invoke(m_assignedStats);
        }

        public void MarkHighlight(bool _active)
        {
            ItemHighlightedCallback?.Invoke(m_assignedStats, _active);
            characterStatsHighlight.SetActive(_active);
        }

        public void CleanUpItem()
        {
            MarkHighlight(false);
            ItemPressedCallback = null;
            ItemHighlightedCallback = null;
        }

        private void SetupAbilityDisplays()
        {
            for (int i = 0; i < m_assignedStats.abilities.Count; i++)
            {
                var _ability = m_assignedStats.abilities[i];
                if (_ability.IsNull())
                {
                    continue;
                }
                
                abilityDisplays[i].SetActive(true);

                if (_ability.abilityIcon.IsNull())
                {
                    continue;
                }
                
                abilityDisplays[i].TryGetComponent(out Image _abilityImageDisplay);
                if (_abilityImageDisplay)
                {
                    _abilityImageDisplay.sprite = _ability.abilityIcon;
                }
            }
        }

        private void ClearAbilities()
        {
            abilityDisplays.ForEach(g => g.SetActive(false));
        }

        #endregion
        

    }
}