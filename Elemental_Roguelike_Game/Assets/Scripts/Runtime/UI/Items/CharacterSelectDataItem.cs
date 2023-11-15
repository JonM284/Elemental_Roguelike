using System;
using System.Collections.Generic;
using Data;
using Project.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.UI.Items
{
    public class CharacterSelectDataItem: MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private GameObject highlightObject;

        [SerializeField] private GameObject activeInfoHolder;

        [SerializeField] private GameObject deleteCharacterDescription;

        [SerializeField] private GameObject captainStar;

        [SerializeField] private Image characterImage;

        [SerializeField] private Image frameBackground;

        [SerializeField] private Image nameBackground;
        
        [SerializeField] private TMP_Text characterNameText;
        
        [SerializeField] private Color disabledColorLight;

        [SerializeField] private Color disabledColorDark;

        [SerializeField] private List<GameObject> abilities = new List<GameObject>();

        [SerializeField] private GameObject abilityDescriptionDisplay;
        
        [SerializeField] private TMP_Text abilityDescriptionText;

        [SerializeField] private Image agilityBar;
        [SerializeField] private Image shootingBar;
        [SerializeField] private Image passingBar;
        [SerializeField] private Image tackleBar;

        #endregion

        #region Private Fields

        private Action<SavedMemberData> onDeleteCallback;

        private bool m_isCaptain;

        #endregion

        #region Accessor

        public SavedMemberData assignedData { get; private set; }

        public bool isConfirmed { get; private set; }

        #endregion

        #region Class Implementation

        public void Initialize(bool _isCaptain, Action<SavedMemberData> deleteCallback)
        {
            activeInfoHolder.SetActive(false);

            m_isCaptain = _isCaptain;

            onDeleteCallback = deleteCallback;

            characterNameText.text = "???";

            frameBackground.color = disabledColorLight;

            nameBackground.color = disabledColorDark;

            captainStar.SetActive(_isCaptain);
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

            frameBackground.color = assignedData.m_characterStatsBase.classTyping.darkColor;

            nameBackground.color = assignedData.m_characterStatsBase.classTyping.passiveColor;
            
            if (!_characterData.m_characterStatsBase.characterImage.IsNull() && !_characterData.m_characterStatsBase.characterImage.IsNull())
            {
                characterImage.sprite = _characterData.m_characterStatsBase.characterImage;
            }
            
            characterNameText.text = _characterData.m_characterStatsBase.characterName;
            
            agilityBar.color = _characterData.m_characterStatsBase.classTyping.barColor;
            shootingBar.color = _characterData.m_characterStatsBase.classTyping.barColor;
            passingBar.color = _characterData.m_characterStatsBase.classTyping.barColor;
            tackleBar.color = _characterData.m_characterStatsBase.classTyping.barColor;

            agilityBar.fillAmount = _characterData.m_characterStatsBase.agilityScore / 100f;
            shootingBar.fillAmount = _characterData.m_characterStatsBase.shootingScore / 100f;
            passingBar.fillAmount = _characterData.m_characterStatsBase.passingScore / 100f;
            tackleBar.fillAmount = _characterData.m_characterStatsBase.tackleScore / 100f;

            abilities.ForEach(g => g.SetActive(false));
            
            for (int i = 0; i < assignedData.m_characterStatsBase.abilities.Count; i++)
            {
               abilities[i].SetActive(true);
            }
        }

        public void OnHighlightDelete()
        {
            if (assignedData.IsNull())
            {
                return;
            }
            
            highlightObject.SetActive(true);
            deleteCharacterDescription.SetActive(true);
        }

        public void OnUnhighlightDelete()
        {
            if (assignedData.IsNull())
            {
                return;
            }
            
            highlightObject.SetActive(false);
            deleteCharacterDescription.SetActive(false);
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

        public void OnSelectItem()
        {
            onDeleteCallback?.Invoke(assignedData);
            ClearData();
            highlightObject.SetActive(false);
            deleteCharacterDescription.SetActive(false);
            //Open correct menu
        }

        public void ClearData()
        {
            assignedData = null;
            characterImage.sprite = null;
            characterNameText.text = "???";
            agilityBar.fillAmount = 0;
            shootingBar.fillAmount = 0;
            passingBar.fillAmount = 0;
            tackleBar.fillAmount = 0;

            isConfirmed = false;
            
            frameBackground.color = disabledColorLight;

            nameBackground.color = disabledColorDark;
            
            activeInfoHolder.SetActive(false);
        }
        
        
        #endregion
    }
}