using System;
using System.Collections.Generic;
using Data;
using Project.Scripts.Utils;
using Runtime.GameControllers;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
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

        [SerializeField] private List<Image> agilityBarImages = new List<Image>();
        [SerializeField] private List<Image> throwingBarImages = new List<Image>();
        [SerializeField] private List<Image> tackleBarImages = new List<Image>();
        [SerializeField] private List<Image> influenceRangeImages = new List<Image>();
        [SerializeField] private List<Image> gravityBarImages = new List<Image>();

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
            
            agilityBarImages.ForEach(img =>
            {
                img.gameObject.SetActive(false);
            });
            throwingBarImages.ForEach(img => 
            {
                img.gameObject.SetActive(false);
            });
            tackleBarImages.ForEach(img => 
            {
                img.gameObject.SetActive(false);
            });
            influenceRangeImages.ForEach(img => 
            {
                img.gameObject.SetActive(false);
            });
            gravityBarImages.ForEach(img => 
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
            
            agilityBarImages.ForEach(img =>
            {
                img.color = _characterData.m_characterStatsBase.classTyping.barColor;
            });
            throwingBarImages.ForEach(img => 
            {
                img.color = _characterData.m_characterStatsBase.classTyping.barColor;
            });
            tackleBarImages.ForEach(img => 
            {
                img.color = _characterData.m_characterStatsBase.classTyping.barColor;
            });
            influenceRangeImages.ForEach(img => 
            {
                img.color = _characterData.m_characterStatsBase.classTyping.barColor;
            });
            gravityBarImages.ForEach(img => 
            {
                img.color = _characterData.m_characterStatsBase.classTyping.barColor;
            });

            for (int i = 0; i < CharacterGameController.Instance.GetStatMax(); i++)
            {
                agilityBarImages[i].gameObject.SetActive(i < _characterData.m_characterStatsBase.agilityScore/10);
                throwingBarImages[i].gameObject.SetActive(i < _characterData.m_characterStatsBase.throwScore/10);
                tackleBarImages[i].gameObject.SetActive(i < _characterData.m_characterStatsBase.tackleScore/10);
                influenceRangeImages[i].gameObject.SetActive(i < _characterData.m_characterStatsBase.influenceRange/10);
                gravityBarImages[i].gameObject.SetActive(i < _characterData.m_characterStatsBase.gravityScore/10);
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
            
            agilityBarImages.ForEach(img => img.gameObject.SetActive(false));
            throwingBarImages.ForEach(img => img.gameObject.SetActive(false));
            tackleBarImages.ForEach(img => img.gameObject.SetActive(false));
            influenceRangeImages.ForEach(img => img.gameObject.SetActive(false));
            gravityBarImages.ForEach(img => img.gameObject.SetActive(false));
            
            isConfirmed = false;
            
            nameBackground.color = disabledColorDark;
            
            activeInfoHolder.SetActive(false);
        }
        
        
        #endregion
    }
}