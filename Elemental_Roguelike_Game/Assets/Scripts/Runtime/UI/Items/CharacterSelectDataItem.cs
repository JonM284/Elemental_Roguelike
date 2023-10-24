using System;
using System.Collections.Generic;
using Data.CharacterData;
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

        [SerializeField] private TMP_Text classTypeText;

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

        private Action<CharacterStatsBase> onDeleteCallback;

        private bool m_isCaptain;

        #endregion

        #region Accessor

        public CharacterStatsBase assignedData { get; private set; }

        public bool isConfirmed { get; private set; }

        #endregion

        #region Class Implementation

        public void Initialize(bool _isCaptain, Action<CharacterStatsBase> deleteCallback)
        {
            activeInfoHolder.SetActive(false);

            m_isCaptain = _isCaptain;

            onDeleteCallback = deleteCallback;

            characterNameText.text = "???";

            frameBackground.color = disabledColorLight;

            nameBackground.color = disabledColorDark;

            captainStar.SetActive(_isCaptain);
        }
        
        
        public void AssignData(CharacterStatsBase _character, bool _isConfirmed)
        {
            if (_character.IsNull())
            {
                return;
            }

            isConfirmed = _isConfirmed;
            
            activeInfoHolder.SetActive(true);

            assignedData = _character;

            frameBackground.color = assignedData.classTyping.darkColor;

            nameBackground.color = assignedData.classTyping.passiveColor;
            
            if (!_character.characterImage.IsNull() && !_character.characterImage.IsNull())
            {
                characterImage.sprite = _character.characterImage;
            }
            
            characterNameText.text = _character.characterName;
            
            classTypeText.text = $"<u>Class :</u> \n {_character.classTyping.classType}";

            
            agilityBar.fillAmount = _character.agilityScore / 100f;
            shootingBar.fillAmount = _character.shootingScore / 100f;
            passingBar.fillAmount = _character.passingScore / 100f;
            tackleBar.fillAmount = _character.tackleScore / 100f;

            abilities.ForEach(g => g.SetActive(false));
            
            for (int i = 0; i < assignedData.abilities.Count; i++)
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
            var highlightedAbility = assignedData.abilities[abilityIndex];
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
            classTypeText.text = string.Empty;
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