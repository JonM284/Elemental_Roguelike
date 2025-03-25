using System;
using System.Collections.Generic;
using Data;
using Project.Scripts.Utils;
using Runtime.GameControllers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.UI.Items
{
    public class CharacterSelectDataItem: MonoBehaviour
    {
       #region Serialized Fields
        
        [SerializeField] private GameObject activeInfoHolder;
        
        [SerializeField] private Image characterImage;
        
        [SerializeField] private Image nameBackground;
        
        [SerializeField] private TMP_Text characterNameText;
        
        [SerializeField] private Color disabledColorDark;

        [SerializeField] private TMP_Text healthText;
        
        [SerializeField] private TMP_Text shieldText;

        [SerializeField] private List<GameObject> abilities = new List<GameObject>();

        [SerializeField] private GameObject abilityDescriptionDisplay;
        
        [SerializeField] private TMP_Text abilityDescriptionText;

        [SerializeField] private List<Image> m_agilityBarImages = new List<Image>();
        [SerializeField] private List<Image> m_shootingBarImages = new List<Image>();
        [SerializeField] private List<Image> m_passingBarImages = new List<Image>();
        [SerializeField] private List<Image> m_tackleBarImages = new List<Image>();

        #endregion

        #region Private Fields

        private Action<SavedMemberData> onDeleteCallback;
        
        #endregion

        #region Accessor

        public SavedMemberData assignedData { get; private set; }

        public bool isConfirmed { get; private set; }

        #endregion

        #region Class Implementation

        public void Initialize()
        {
            activeInfoHolder.SetActive(false);
            
            characterNameText.text = "???";
            
            m_agilityBarImages.ForEach(img =>
            {
                img.gameObject.SetActive(false);
            });
            m_shootingBarImages.ForEach(img => 
            {
                img.gameObject.SetActive(false);
            });
            m_passingBarImages.ForEach(img => 
            {
                img.gameObject.SetActive(false);
            });
            m_tackleBarImages.ForEach(img => 
            {
                img.gameObject.SetActive(false);
            });
            
            nameBackground.color = disabledColorDark;
        }
        
        
        public void AssignData(SavedMemberData _characterData, bool _isConfirmed)
        {
            if (_characterData.IsNull())
            {
                return;
            }

            isConfirmed = _isConfirmed;
            
            activeInfoHolder.SetActive(true);

            assignedData = _characterData;
            
            nameBackground.color = assignedData.m_characterStatsBase.classTyping.passiveColor;
            
            if (!_characterData.m_characterStatsBase.characterImage.IsNull() && !_characterData.m_characterStatsBase.characterImage.IsNull())
            {
                characterImage.sprite = _characterData.m_characterStatsBase.characterImage;
            }
            
            characterNameText.text = _characterData.m_characterStatsBase.characterName;

            healthText.text = $"{_characterData.m_characterStatsBase.baseHealth} HP";
            
            shieldText.text = _characterData.m_characterStatsBase.baseShields.ToString();
            
            m_agilityBarImages.ForEach(img =>
            {
                img.color = _characterData.m_characterStatsBase.classTyping.barColor;
            });
            m_shootingBarImages.ForEach(img => 
            {
                img.color = _characterData.m_characterStatsBase.classTyping.barColor;
            });
            m_passingBarImages.ForEach(img => 
            {
                img.color = _characterData.m_characterStatsBase.classTyping.barColor;
            });
            m_tackleBarImages.ForEach(img => 
            {
                img.color = _characterData.m_characterStatsBase.classTyping.barColor;
            });

            for (int i = 0; i < CharacterGameController.Instance.GetStatMax(); i++)
            {
                m_agilityBarImages[i].gameObject.SetActive(i < _characterData.m_characterStatsBase.agilityScore/10);
                m_shootingBarImages[i].gameObject.SetActive(i < _characterData.m_characterStatsBase.shootingScore/10);
                m_passingBarImages[i].gameObject.SetActive(i < _characterData.m_characterStatsBase.passingScore/10);
                m_tackleBarImages[i].gameObject.SetActive(i < _characterData.m_characterStatsBase.tackleScore/10);
            }

            abilities.ForEach(g => g.SetActive(false));
            
            for (int i = 0; i < assignedData.m_characterStatsBase.abilities.Count; i++)
            {
               abilities[i].SetActive(true);
            }
        }

        public void OnHighlightAbility(int abilityIndex)
        {
            var highlightedAbility = assignedData.m_characterStatsBase.abilities[abilityIndex];
            abilityDescriptionDisplay.SetActive(true);
            abilityDescriptionText.text = $"{highlightedAbility.abilityName}: <br> {highlightedAbility.abilityDescription} <br> " +
                                                         $"Target Type: {highlightedAbility.targetType} <br> Cooldown: {highlightedAbility.roundCooldownTimer} Turn(s)";
        }

        public void OnUnhighlightAbility(int abilityIndex)
        {
            abilityDescriptionDisplay.SetActive(false);
        }

        public void ClearData()
        {
            assignedData = null;
            characterImage.sprite = null;
            
            characterNameText.text = "???";
            
            m_agilityBarImages.ForEach(img => img.gameObject.SetActive(false));
            m_shootingBarImages.ForEach(img => img.gameObject.SetActive(false));
            m_passingBarImages.ForEach(img => img.gameObject.SetActive(false));
            m_tackleBarImages.ForEach(img => img.gameObject.SetActive(false));

            isConfirmed = false;
            
            nameBackground.color = disabledColorDark;
            
            activeInfoHolder.SetActive(false);
        }
        
        
        #endregion
    }
}