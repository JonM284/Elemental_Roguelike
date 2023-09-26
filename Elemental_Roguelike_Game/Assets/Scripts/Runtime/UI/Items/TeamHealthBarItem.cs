using System;
using System.Collections.Generic;
using Data.Sides;
using Project.Scripts.Utils;
using Runtime.Character;
using Runtime.GameControllers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.UI.Items
{
    public class TeamHealthBarItem: MonoBehaviour
    {

        #region Serialized Fields

        [SerializeField] private GameObject holder;

        [SerializeField] private Image frame;

        [SerializeField] private GameObject highlightObj;

        [SerializeField] private List<Color> m_healthStateColors = new List<Color>();

        [SerializeField] private Image primaryHealthBar;

        //[SerializeField] private Image secondaryHealthBar;

        [SerializeField] private Image m_characterImage;

        [SerializeField] private Image m_elementIcon;

        [SerializeField] private TMP_Text healthText;

        [SerializeField] private List<GameObject> shieldIcons = new List<GameObject>();

        [SerializeField] private List<GameObject> actionPointObj = new List<GameObject>();

        #endregion

        #region Private Fields

        private CharacterBase m_associatedCharacter;

        #endregion
        
        #region Accessors

        public int maxHealth => m_associatedCharacter.characterLifeManager.maxHealthPoints;

        public int currentHealth => m_associatedCharacter.characterLifeManager.currentHealthPoints;

        public int currentShield => m_associatedCharacter.characterLifeManager.currentShieldPoints;

        public float healthPercentage => (float)currentHealth / maxHealth;

        public bool hasShield => currentShield > 0;

        #endregion

        #region Unity Events

        private void OnEnable()
        {
            TurnController.OnChangeActiveCharacter += OnChangeActiveCharacter;
            TurnController.OnChangeActiveTeam += OnChangeActiveTeam;
            CharacterLifeManager.OnCharacterHealthChange += OnCharacterHealthChange;
            CharacterLifeManager.OnCharacterDied += OnCharacterDied;
            CharacterBase.CharacterReset += OnCharacterReset;
            CharacterBase.CharacterUsedActionPoint += OnCharacterUsedActionPoint;
        }

        private void OnDisable()
        {
            TurnController.OnChangeActiveCharacter -= OnChangeActiveCharacter;
            TurnController.OnChangeActiveTeam -= OnChangeActiveTeam;
            CharacterLifeManager.OnCharacterHealthChange -= OnCharacterHealthChange;
            CharacterLifeManager.OnCharacterDied -= OnCharacterDied;
            CharacterBase.CharacterReset -= OnCharacterReset;
            CharacterBase.CharacterUsedActionPoint -= OnCharacterUsedActionPoint;
        }

        #endregion
        
        #region Class Implementation

        public void Initialize(CharacterBase _character)
        {
            if (_character.IsNull())
            {
                return;
            }

            m_associatedCharacter = _character;

            var _element = _character.characterLifeManager.characterElementType;

            m_elementIcon.sprite = _element.elementSprite;

            m_characterImage.sprite = _element.meepleIcon;

            frame.color = _element.meepleColors[0];

            Debug.Log("Connected");
            
            OnCharacterHealthChange(m_associatedCharacter);
        }
        
        private void OnCharacterUsedActionPoint(CharacterBase _character, int _amountLeft)
        {
            if (m_associatedCharacter.IsNull())
            {
                return;
            }

            if (_character != m_associatedCharacter)
            {
                return;
            }

            for (int i = 0; i < actionPointObj.Count; i++)
            {
                if (i == _amountLeft)
                {
                    actionPointObj[i].SetActive(false);
                }
            }
        }
        
        private void OnCharacterHealthChange(CharacterBase _character)
        {
            if (m_associatedCharacter.IsNull())
            {
                return;
            }

            if (_character != m_associatedCharacter)
            {
                return;
            }

            healthText.text = $"{currentHealth}/{maxHealth}";

            primaryHealthBar.fillAmount = healthPercentage;
            
            shieldIcons.ForEach(g => g.SetActive(hasShield));

        }
        
        private void OnChangeActiveTeam(CharacterSide _side)
        {
            if (m_associatedCharacter.IsNull())
            {
                return;
            }

            if (_side != m_associatedCharacter.side)
            {
                actionPointObj.ForEach(g => g.SetActive(false));
                return;
            }
            
            actionPointObj.ForEach(g => g.SetActive(true));
        }
        
        private void OnChangeActiveCharacter(CharacterBase _character)
        {
            highlightObj.SetActive(_character == m_associatedCharacter);
        }
        
        private void OnCharacterDied(CharacterBase _character)
        {
            if (m_associatedCharacter.IsNull())
            {
                return;
            }

            if (_character != m_associatedCharacter)
            {
                return;
            }
            
            holder.SetActive(false);
        }
        
        private void OnCharacterReset(CharacterBase _character)
        {
            if (m_associatedCharacter.IsNull())
            {
                return;
            }

            if (_character != m_associatedCharacter)
            {
                return;
            }

            holder.SetActive(true);
        }
        
        public void OnPortraitPressed(){
            if (m_associatedCharacter.IsNull())
            {
                return;
            }
            
            m_associatedCharacter.OnSelect();
        }

        #endregion
        
       
        
        
    }
}